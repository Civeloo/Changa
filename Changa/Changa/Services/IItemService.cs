using System;
using System.Reactive;
using System.Threading.Tasks;
using Reactive.Bindings;
using Changa.Models;
using Xamarin.Forms.GoogleMaps;
using Changa.ViewModels;
using Plugin.CloudFirestore;

namespace Changa.Services
{
    public interface IItemService
    {
        ReadOnlyReactivePropertySlim<Item> Item { get; }
        ReadOnlyReactivePropertySlim<User> Owner { get; }
        ReadOnlyReactivePropertySlim<User> Assigned { get; }        

        ReadOnlyReactivePropertySlim<string> TbItemName { get; }

        ReadOnlyReactivePropertySlim<string> UserName { get; }
        ReadOnlyReactivePropertySlim<int> UserReputation { get; }
        ReadOnlyReactivePropertySlim<string> UserPhoneNumber { get; }

        ReadOnlyReactivePropertySlim<bool> IsBid { get; }
        ReadOnlyReactivePropertySlim<bool> IsOwner { get; }
        //ReadOnlyReactivePropertySlim<bool> IsAssigned { get; }
        ReadOnlyReactivePropertySlim<bool> IsLoaded { get; }
        ReadOnlyReactivePropertySlim<bool> IsDeleting { get; }

        ReadOnlyReactivePropertySlim<Map> MyMap { get; }
        ReadOnlyReactiveCollection<BidViewModel> Bids { get; }

        IObservable<string> LoadErrorNotifier { get; }
        IObservable<string> DeleteErrorNotifier { get; }
        IObservable<Unit> DeleteCompletedNotifier { get; }
        //IObservable<string> ErrorNotifier { get; }
        //IObservable<Unit> CompletedNotifier { get; }
        Task LoadAsync(string id);
        Task BidOrUnBidAsync();
        Task BidAsync();
        Task UnBidAsync();
        Task DeleteAsync();
        Task AssignAsync(string id);
        Task CalificateWorkAsync(bool good);
    }
}
