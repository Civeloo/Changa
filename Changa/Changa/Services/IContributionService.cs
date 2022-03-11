using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Reactive.Bindings;
using Xamarin.Forms.GoogleMaps;

namespace Changa.Services
{
    public interface IContributionService
    {
        ReactivePropertySlim<Stream> ItemImage { get; }
        ReactivePropertySlim<string> ItemTitle { get; }
        ReactivePropertySlim<string> ItemComment { get; }
        ReactivePropertySlim<double> ItemPrice { get; }
        ReactivePropertySlim<string> ItemLocation { get; }
        ReactivePropertySlim<string> ItemPostalCode { get; }
        ReactivePropertySlim<int> ItemHours { get; }
        ReactivePropertySlim<string> ItemStatus { get; }

        ReadOnlyReactivePropertySlim<Map> MyMap { get; }

        ReadOnlyReactivePropertySlim<bool> CanContribute { get; }
        ReadOnlyReactivePropertySlim<bool> IsContributing { get; }
        IObservable<string> ContributeErrorNotifier { get; }
        IObservable<Unit> ContributeCompletedNotifier { get; }
        
        Task SelectImage();
        Task Contribute();
    }
}
