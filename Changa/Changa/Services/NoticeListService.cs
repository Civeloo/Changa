using System;
using Plugin.CloudFirestore;
using System.Collections.ObjectModel;
using Changa.Models;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Linq;
using Plugin.CloudFirestore.Extensions;
using Reactive.Bindings.Extensions;
using Reactive.Bindings;

namespace Changa.Services
{
    public class NoticeListService : INoticeListService
    {
        private const int Count = 20;
        private readonly IFirestore _firestore;
        private readonly ObservableCollection<Notice> _notices = new ObservableCollection<Notice>();
        private CompositeDisposable _disposables;
        private readonly IUserService _userService;    
        private long _lastTimestamp = long.MaxValue;
        private bool _isClosed;
        private readonly object _lock = new object();
        private readonly ReactivePropertySlim<Notice> _notice = new ReactivePropertySlim<Notice>();
        public ReadOnlyReactivePropertySlim<Notice> Notice { get; }
        public ReadOnlyObservableCollection<Notice> Notices { get; }

        public NoticeListService(IUserService userService)
        {
            _firestore = CrossCloudFirestore.Current.Instance;
            _userService = userService;//new UserService();
            Notices = new ReadOnlyObservableCollection<Notice>(_notices);
        }

        public async Task LoadAsync(string id)
        {
            try
            {
                var noticeCollection = _firestore.GetCollection(User.CollectionPath)
                                .GetDocument(id)
                                .GetCollection(Models.Notice.CollectionPath);
                var documents = await noticeCollection
                                //.OrderBy(nameof(Bid.Timestamp), true)
                                //.LimitTo(Count)
                                //.StartAfter(_lastTimestamp)
                                .GetDocumentsAsync()
                                .ConfigureAwait(false);

                if ((!documents.IsEmpty || _disposables == null) && !_isClosed)
                {
                    //lock (_lock)
                    {
                        if (!_isClosed)
                        {
                            if (!documents.IsEmpty)
                            {
                                var lastNotice = documents.Documents.Last().ToObject<Notice>();
                                _lastTimestamp = lastNotice.Timestamp;
                            }
                            else
                            {
                                _lastTimestamp = DateTime.Now.Ticks;
                            }

                            _disposables?.Dispose();
                            _disposables = new CompositeDisposable();

                            var query = _firestore.GetCollection(User.CollectionPath)
                                        .GetDocument(id)
                                        .GetCollection(Models.Notice.CollectionPath)
                                        .OrderBy(nameof(Models.Notice.Timestamp), true)
                                        .EndAt(_lastTimestamp);

                            query.ObserveAdded()
                                 .Where(d => _notices.FirstOrDefault(i => i.Id == d.Document.Id) == null)
                                 .Subscribe(async d => _notices.Insert(d.NewIndex, await GetNotice(d.Document)
                                ))
                                 .AddTo(_disposables);

                            query.ObserveModified()
                                 .Select(d => d.Document.ToObject<Notice>())
                                 .Subscribe(Notice =>
                                 {
                                     var targetNotice = _notices.FirstOrDefault(i => i.Id == Notice.Id);
                                     if (targetNotice != null)
                                     {
                                         Notice.CopyTo(targetNotice);
                                     }
                                 })
                                 .AddTo(_disposables);

                            query.ObserveRemoved()
                                 .Select(d => _notices.FirstOrDefault(i => i.Id == d.Document.Id))
                                 .Where(Notice => Notice != null)
                                 .Subscribe(Notice => _notices.Remove(Notice))
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

        public async Task<Notice> GetNotice(IDocumentSnapshot d)
        {
            var user = new User();

            if (_notice.Value == null) _notice.Value = d.ToObject<Notice>();
            if (_notice.Value.UserId != null)
                if (_notice.Value.User == null) user = await _userService.GetUserAsync(_notice.Value.UserId);

            if (user.Name != null)
            {
                var notice = _notice.Value; //new Notice();
                notice.User = user;
                _notice.Value = notice;
            }

            return _notice.Value;
        }

    }
}
