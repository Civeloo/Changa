﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Reactive.Bindings.Extensions;
using Changa.Models;

namespace Changa.Services
{
    public class UserItemListService : IUserItemListService
    {
        private const int Count = 20;

        private readonly IFirestore _firestore;
        private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
        private CompositeDisposable _disposables;
        private long _lastTimestamp = long.MaxValue;
        private bool _isClosed;
        private string _userId;
        private readonly object _lock = new object();

        public ReadOnlyObservableCollection<Item> Items { get; }

        public UserItemListService()
        {
            _firestore = CrossCloudFirestore.Current.Instance;

            Items = new ReadOnlyObservableCollection<Item>(_items);
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
            _disposables?.Dispose();
            _items.Clear();
        }

        public async Task LoadAsync()
        {
            try
            {
                var documents = await _firestore.GetCollection(Item.CollectionPath)
                                                .WhereEqualsTo(nameof(Item.OwnerId), _userId)
                                                .OrderBy(nameof(Item.Timestamp), true)
                                                .LimitTo(Count)
                                                .StartAfter(_lastTimestamp)
                                                .GetDocumentsAsync()
                                                .ConfigureAwait(false);

                if ((!documents.IsEmpty || _disposables == null) && !_isClosed)
                {
                    lock (_lock)
                    {
                        if (!_isClosed)
                        {
                            if (!documents.IsEmpty)
                            {
                                var lastItem = documents.Documents.Last().ToObject<Item>();
                                _lastTimestamp = lastItem.Timestamp;
                            }
                            else
                            {
                                _lastTimestamp = DateTime.Now.Ticks;
                            }

                            _disposables?.Dispose();
                            _disposables = new CompositeDisposable();

                            var query = _firestore.GetCollection(Item.CollectionPath)
                                                  .WhereEqualsTo(nameof(Item.OwnerId), _userId)
                                                  .OrderBy(nameof(Item.Timestamp), true)
                                                  .EndAt(_lastTimestamp);

                            query.ObserveAdded()
                                 .Where(d => _items.FirstOrDefault(i => i.Id == d.Document.Id) == null)
                                 .Subscribe(d => _items.Insert(d.NewIndex, d.Document.ToObject<Item>()))
                                 .AddTo(_disposables);

                            query.ObserveModified()
                                 .Select(d => d.Document.ToObject<Item>())
                                 .Subscribe(item =>
                                 {
                                     var targetItem = _items.FirstOrDefault(i => i.Id == item.Id);
                                     if (targetItem != null)
                                     {
                                         item.CopyTo(targetItem);
                                     }
                                 })
                                 .AddTo(_disposables);

                            query.ObserveRemoved()
                                 .Select(d => _items.FirstOrDefault(i => i.Id == d.Document.Id))
                                 .Where(item => item != null)
                                 .Subscribe(item => _items.Remove(item))
                                 .AddTo(_disposables);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                _disposables?.Dispose();
                _isClosed = true;
            }
        }
    }
}
