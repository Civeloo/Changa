using System;
using Changa.Models;
using Reactive.Bindings;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using Changa.Services;

namespace Changa.ViewModels
{
    public class ItemViewModel : IDisposable
    {
        private readonly Item _item;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<string> Id { get; }
        public ReadOnlyReactivePropertySlim<string> Title { get; }
        //public ReadOnlyReactivePropertySlim<string> Image { get; }
        public ReadOnlyReactivePropertySlim<int> BidCount { get; }
        public ReadOnlyReactivePropertySlim<string> Comment { get; }
        public ReadOnlyReactivePropertySlim<double> Price { get; }
        public ReadOnlyReactivePropertySlim<int> Hours { get; }

        public ReadOnlyReactiveCollection<BidViewModel> Bids { get; }
        private readonly IBidListService _bidListService;

        public ItemViewModel(Item item)
        {
            _item = item;            

            Id = _item.ObserveProperty(x => x.Id)
                      .ToReadOnlyReactivePropertySlim()
                      .AddTo(_disposables);            

            Title = _item.ObserveProperty(x => x.Title)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            //Image = _item.ObserveProperty(x => x.Image)
            //             .ToReadOnlyReactivePropertySlim()
            //             .AddTo(_disposables);

            Comment = _item.ObserveProperty(x => x.Comment)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            BidCount = _item.ObserveProperty(x => x.BidCount)
                             .ToReadOnlyReactivePropertySlim()
                             .AddTo(_disposables);

            Price = _item.ObserveProperty(x => x.Price)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            Hours = _item.ObserveProperty(x => x.Hours)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
