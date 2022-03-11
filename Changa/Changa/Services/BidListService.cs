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
using System.Threading;

namespace Changa.Services
{
    public class BidListService : IBidListService
    {
        private const int Count = 20;
        private readonly IFirestore _firestore;
        
        private CompositeDisposable _disposables;
        private readonly IUserService _userService;

        private long _lastTimestamp = long.MaxValue;
        private bool _isClosed;
        private readonly object _lock = new object();

        private readonly ReactivePropertySlim<Bid> _bid = new ReactivePropertySlim<Bid>();
        public ReadOnlyReactivePropertySlim<Bid> Bid { get; }

        private string _assigned;

        private readonly ObservableCollection<Bid> _bids = new ObservableCollection<Bid>();
        public ReadOnlyObservableCollection<Bid> Bids { get; }

        // public string _itemId { get; set; }

        public BidListService(IUserService userService)
        {
            try
            {           
            
            _firestore = CrossCloudFirestore.Current.Instance;

            _userService = userService;// new UserService();
            Bids = new ReadOnlyObservableCollection<Bid>(_bids);

                //Owner = _owner.ToReadOnlyReactivePropertySlim();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public async Task LoadAsync(string itemId)
        {
            try
            {
                var document = _firestore.GetCollection(Item.CollectionPath).GetDocument(itemId);

                var itemD = await document.GetDocumentAsync().ConfigureAwait(false);
                var item = itemD.ToObject<Item>();
                _assigned = item.Assigned;

                var collection = document.GetCollection(Models.Bid.CollectionPath);
                var documents = await collection
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
                                var last = documents.Documents.Last().ToObject<Bid>();
                                _lastTimestamp = last.Timestamp;
                            }
                            else
                            {
                                _lastTimestamp = DateTime.Now.Ticks;
                            }

                            _disposables?.Dispose();
                            _disposables = new CompositeDisposable();                            

                            var query = _firestore.GetCollection(Item.CollectionPath).GetDocument(itemId).GetCollection(Models.Bid.CollectionPath)
                                        .OrderBy(nameof(Models.Bid.Timestamp), true)
                                        //.EndAt(_lastTimestamp)
                                        ;

                            query.ObserveAdded()
                                 .Where(d => _bids.FirstOrDefault(i => i.Id == d.Document.Id) == null)
                                 .Subscribe( d =>
                                    {
                                        //Thread.Sleep(2000);
                                        //var l = GetBidder(d.Document.ToObject<Bid>());
                                        //_bids.Insert(d.NewIndex, l);                                           
                                        _bids.Insert(d.NewIndex, d.Document.ToObject<Bid>() );
                                    }
                                 )                                 
                                 .AddTo(_disposables)
                                 ;                            

                            query.ObserveModified()
                                 .Select(d => d.Document.ToObject<Bid>())
                                 .Subscribe(Bid =>
                                 {
                                     var target = _bids.FirstOrDefault(i => i.Id == Bid.Id);
                                     if (target != null)
                                     {
                                         Bid.CopyTo(target);
                                     }
                                 })
                                 .AddTo(_disposables);

                            query.ObserveRemoved()
                                 .Select(d => _bids.FirstOrDefault(i => i.Id == d.Document.Id))
                                 .Where(Bid => Bid != null)
                                 .Subscribe(Bid => _bids.Remove(Bid))
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

//        public Bid GetBidder(Bid l)
//        {            
//try
//{
//                var u = new User();
//                if (!string.IsNullOrEmpty(l.Id))
//                {
                    
//                    u = _userService
//                        .GetUser(l.Id)
//                        ;
//                }
//                if (!string.IsNullOrEmpty(u.Name))
//                {
//                    l.BidderName = u.Name;
//                    l.BidderImage = u.Image;
//                    l.BidderReputation = u.Reputation;
//                }
                
//            }
//catch (Exception e)
//{
//    System.Diagnostics.Debug.WriteLine(e);
//}
//            return l;
//        }        

    }
}
