using System;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Reactive;
using Xamarin.Forms.GoogleMaps;

namespace Changa.Services
{
    public interface IAccountService
    {
        ReadOnlyReactivePropertySlim<bool> IsInitialized { get; }
        ReadOnlyReactivePropertySlim<bool> IsLoggedIn { get; }
        ReadOnlyReactivePropertySlim<string> UserId { get; }
        ReactivePropertySlim<string> UserName { get; }
        ReactivePropertySlim<int> UserReputation { get; }
        ReactivePropertySlim<string> UserLocation { get; }
        ReactivePropertySlim<string> UserPostalCode { get; }
        ReactivePropertySlim<string> UserPhoneNumber { get; }
        ReactivePropertySlim<string> UserEmail { get; }
        ReadOnlyReactivePropertySlim<string> UserImage { get; }
        ReadOnlyReactivePropertySlim<int> ContributionCount { get; }
        ReadOnlyReactivePropertySlim<Map> MyMap { get; }

        Task Initialize();
        Task LoginWithGoogleAsync(string idToken, string accessToken);
        Task LoginWithEmailAndPasswordAsync(string email, string password);
        Task SignupAsync(string email, string password, string name, string image = null);
        void Logout();
        Task IncrementContributionCountAsync(int delta);
        Task SaveAsync();
        void GetMap();
        void Close();

        ReadOnlyReactivePropertySlim<bool> Can { get; }
        ReadOnlyReactivePropertySlim<bool> IsNotifier { get; }
        IObservable<string> ErrorNotifier { get; }
        IObservable<Unit> CompletedNotifier { get; }
    }
}
