using System;
using System.Reactive;
using System.Threading.Tasks;
using Reactive.Bindings;
using Changa.Models;
using Xamarin.Forms.GoogleMaps;
using Changa.ViewModels;

namespace Changa.Services
{
    public interface IBidService
    {
        ReadOnlyReactivePropertySlim<Bid> Bid { get; }
        ReadOnlyReactivePropertySlim<User> Owner { get; }

        ReadOnlyReactivePropertySlim<bool> IsLoaded { get; }
        ReadOnlyReactivePropertySlim<bool> IsDeleting { get; }

        IObservable<string> LoadErrorNotifier { get; }
        IObservable<string> DeleteErrorNotifier { get; }
        IObservable<Unit> DeleteCompletedNotifier { get; }
        Task LoadAsync(string id);
        //Task DeleteAsync();
    }
}