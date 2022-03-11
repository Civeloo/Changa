using System;
using Changa.Models;
using Reactive.Bindings;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using Changa.Services;
using System.Reactive.Linq;

namespace Changa.ViewModels
{
    public class BidViewModel : IDisposable
    {
        private readonly IBidService _bidService;
        private readonly Bid _bid;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<string> Id { get; }
        public ReadOnlyReactivePropertySlim<string> BidderName { get; }
        public ReadOnlyReactivePropertySlim<int> BidderReputation { get; }
        public ReadOnlyReactivePropertySlim<string> BidderImage { get; }

        public BidViewModel(Bid bid)
        {
            _bidService = new BidService();
            _bid = bid;

            Id = _bid.ObserveProperty(x => x.Id)
                      .ToReadOnlyReactivePropertySlim()
                      .AddTo(_disposables);

            BidderName = _bid.ObserveProperty(x => x.BidderName)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            BidderReputation = _bid.ObserveProperty(x => x.BidderReputation)
                             .ToReadOnlyReactivePropertySlim()
                             .AddTo(_disposables);

            BidderImage = _bid.ObserveProperty(x => x.BidderImage)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }        

    }
}