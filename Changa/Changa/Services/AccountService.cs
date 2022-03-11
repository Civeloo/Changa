using System;
using System.Threading.Tasks;
using Plugin.FirebaseAuth;
using Changa.Models;
using Plugin.CloudFirestore;
using System.Reactive.Subjects;
using Reactive.Bindings;
using Changa.Helpers;
using Xamarin.Forms;
using Prism.Mvvm;
using System.Reactive.Linq;
using Plugin.CloudFirestore.Extensions;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using Reactive.Bindings.Notifiers;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Essentials;
using System.Linq;
using System.Reactive;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace Changa.Services
{
    [Singleton]
    public class AccountService : IAccountService
    {
        private readonly IAuth _firebaseAuth;
        private readonly IFirestore _firestore;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        IMapService _mapService;

        IUser _authUser;
        IDocumentSnapshot _userDocumentSnapshot;
        IDocumentReference _userDocumentReference;
        User _user;
        Place _place;

        private readonly ReactivePropertySlim<bool> _isInitialized = new ReactivePropertySlim<bool>();
        public ReadOnlyReactivePropertySlim<bool> IsInitialized { get; }

        private readonly ReactivePropertySlim<bool> _isLoggedIn = new ReactivePropertySlim<bool>();
        public ReadOnlyReactivePropertySlim<bool> IsLoggedIn { get; }

        private readonly ReactivePropertySlim<string> _userId = new ReactivePropertySlim<string>();
        public ReadOnlyReactivePropertySlim<string> UserId { get; }

        //private readonly ReactivePropertySlim<string> _userName = new ReactivePropertySlim<string>();
        //public ReadOnlyReactivePropertySlim<string> UserName { get; }
        public ReactivePropertySlim<string> UserName { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<int> UserReputation { get; } = new ReactivePropertySlim<int>();

        //private readonly ReactivePropertySlim<string> _userLocation = new ReactivePropertySlim<string>();
        //public ReadOnlyReactivePropertySlim<string> UserLocation { get; }
        public ReactivePropertySlim<string> UserLocation { get; } = new ReactivePropertySlim<string>();

        //private readonly ReactivePropertySlim<string> _userPostalCode = new ReactivePropertySlim<string>();
        //public ReadOnlyReactivePropertySlim<string> UserPostalCode { get; }
        public ReactivePropertySlim<string> UserPostalCode { get; } = new ReactivePropertySlim<string>();

        //private readonly ReactivePropertySlim<string> _userPhoneNumber = new ReactivePropertySlim<string>();
        //public ReadOnlyReactivePropertySlim<string> UserPhoneNumber { get; }
        public ReactivePropertySlim<string> UserPhoneNumber { get; } = new ReactivePropertySlim<string>();

        //private readonly ReactivePropertySlim<string> _userEmail = new ReactivePropertySlim<string>();
        //public ReadOnlyReactivePropertySlim<string> UserEmail { get; }
        public ReactivePropertySlim<string> UserEmail { get; } = new ReactivePropertySlim<string>();

        private readonly ReactivePropertySlim<string> _userImage = new ReactivePropertySlim<string>();
        public ReadOnlyReactivePropertySlim<string> UserImage { get; }

        private readonly ReactivePropertySlim<int> _contributionCount = new ReactivePropertySlim<int>();
        public ReadOnlyReactivePropertySlim<int> ContributionCount { get; }

        private readonly ReactivePropertySlim<Xamarin.Forms.GoogleMaps.Map> _myMap = new ReactivePropertySlim<Xamarin.Forms.GoogleMaps.Map>();
        public ReadOnlyReactivePropertySlim<Xamarin.Forms.GoogleMaps.Map> MyMap { get; }

        public ReadOnlyReactivePropertySlim<bool> Can { get; }

        private readonly BusyNotifier _notifier = new BusyNotifier();
        public ReadOnlyReactivePropertySlim<bool> IsNotifier { get; }

        private readonly Subject<string> _errorNotifier = new Subject<string>();
        public IObservable<string> ErrorNotifier => _errorNotifier;

        private readonly Subject<Unit> _completedNotifier = new Subject<Unit>();
        public IObservable<Unit> CompletedNotifier => _completedNotifier;

        public AccountService()
        {
            _mapService = new MapService();

            _firebaseAuth = CrossFirebaseAuth.Current.Instance;
            _firestore = CrossCloudFirestore.Current.Instance;

            IsInitialized = _isInitialized.ToReadOnlyReactivePropertySlim();
            IsLoggedIn = _isLoggedIn.ToReadOnlyReactivePropertySlim();

            UserId = _userId.ToReadOnlyReactivePropertySlim();
            //UserName = _userName.ToReadOnlyReactivePropertySlim();
            //UserLocation = _userLocation.ToReadOnlyReactivePropertySlim();
            //UserPostalCode = _userPostalCode.ToReadOnlyReactivePropertySlim();
            //UserPhoneNumber = _userPhoneNumber.ToReadOnlyReactivePropertySlim();
            //UserEmail = _userEmail.ToReadOnlyReactivePropertySlim();
            UserImage = _userImage.ToReadOnlyReactivePropertySlim();
            ContributionCount = _contributionCount.ToReadOnlyReactivePropertySlim();

            UserId.Select(userId => string.IsNullOrEmpty(userId) ?
                                    Observable.Return<User>(null) :
                                    _firestore.GetCollection(User.CollectionPath)
                                              .GetDocument(userId)
                                              .AsObservable()
                                              .Select(d => d.ToObject<User>()))
                  .Switch()
                  .Subscribe(user =>
                  {
                      if (user != null)
                      {
                          //_userName.Value = user.Name;
                          //_userImage.Value = user.Image;
                          SetUser(user);
                          _contributionCount.Value = user.ContributionCount;
                      }
                      else
                      {
                          //_userName.Value = null;
                          //_userImage.Value = null;
                          SetUser(new User());
                          _contributionCount.Value = 0;
                      }
                  })
                  .AddTo(_disposables);

            MyMap = _mapService.MyMap//_myMap.ToReadOnlyReactivePropertySlim()
                ;

            Can = new[]
            {
                //_accountService.UserId.Select(s => !string.IsNullOrEmpty(s)),
                UserPhoneNumber.Select(s => !string.IsNullOrEmpty(s)),
                UserPostalCode.Select(s => !string.IsNullOrEmpty(s))
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim();

            IsNotifier = _notifier.ToReadOnlyReactivePropertySlim();

            //GetLocationAsync();
        }

        public async Task Initialize()
        {
            if (IsInitialized.Value) return;

            Plugin.FirebaseAuth.IListenerRegistration registration = null;
            try
            {
                var tcs = new TaskCompletionSource<string>();

                registration = _firebaseAuth.AddAuthStateChangedListener(auth =>
                {
                    tcs.TrySetResult(auth?.CurrentUser?.Uid);
                });

                var userId = await tcs.Task.ConfigureAwait(false);

                if (userId != null)
                {
                    _userDocumentReference = _firestore.GetCollection(User.CollectionPath)
                                              .GetDocument(userId);

                    _userDocumentSnapshot = await _userDocumentReference.GetDocumentAsync().ConfigureAwait(false);

                    if (_userDocumentSnapshot.Exists)
                    {
                        _user = _userDocumentSnapshot.ToObject<User>();

                        //_userId.Value = user.Id;
                        //_userName.Value = user.Name;
                        //_userImage.Value = user.Image;
                        SetUser(_user);
                        _contributionCount.Value = _user.ContributionCount;
                        _isLoggedIn.Value = true;
                    }
                    else
                    {
                        //_userId.Value = null;
                        //_userName.Value = null;
                        //_userImage.Value = null;
                        SetUser(new User());
                        _contributionCount.Value = 0;
                        _isLoggedIn.Value = false;
                    }
                }
                else
                {
                    //_userId.Value = null;
                    //_userName.Value = null;
                    //_userImage.Value = null;
                    SetUser(new User());
                    _contributionCount.Value = 0;
                    _isLoggedIn.Value = false;
                }                
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            finally
            {
                registration?.Remove();
                _isInitialized.Value = true;
            }
        }

        public async Task LoginWithGoogleAsync(string idToken, string accessToken)
        {
            var credential = CrossFirebaseAuth.Current
                                              .GoogleAuthProvider
                                              .GetCredential(idToken, accessToken);

            var result = await _firebaseAuth.SignInWithCredentialAsync(credential).ConfigureAwait(false);

            _authUser = result.User;

            _userDocumentReference = _firestore.GetCollection(User.CollectionPath)
                                      .GetDocument(_authUser.Uid);

            _userDocumentSnapshot = await _userDocumentReference.GetDocumentAsync().ConfigureAwait(false);

            _user = _userDocumentSnapshot.ToObject<User>();

            await GetLocationAsync();

            if (_user == null)
            {
                //user = new User()
                //{
                //    Id = authUser.Uid,
                //    Name = authUser.DisplayName,
                //    PhoneNumber = authUser.PhoneNumber,
                //    Image = authUser.PhotoUrl?.AbsoluteUri
                //};
                _user = NewUser(_authUser);

                await _userDocumentReference.SetDataAsync(_user).ConfigureAwait(false);
            }

            //_userId.Value = user.Id;
            //_userName.Value = user.Name;
            //_userImage.Value = user.Image;
            SetUser(_user);
            _contributionCount.Value = _user.ContributionCount;
            _isLoggedIn.Value = true;
            
        }

        public async Task LoginWithEmailAndPasswordAsync(string email, string password)
        {

            var result = await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);

            _authUser = result.User;

            _userDocumentReference = _firestore.GetCollection(User.CollectionPath)
                                      .GetDocument(_authUser.Uid);

            _userDocumentSnapshot = await _userDocumentReference.GetDocumentAsync().ConfigureAwait(false);

            var user = _userDocumentSnapshot.ToObject<User>();

            await GetLocationAsync();

            if (user == null)
            {
                //var place = await GetLocationAsync();
                //user = new User()
                //{
                //    Id = authUser.Uid,
                //    Name = authUser.DisplayName,                    
                //    Location = place.Id,
                //    PostalCode = place.PostalCode,
                //    PhoneNumber = authUser.PhoneNumber,
                //    Email = authUser.Email,
                //    Image = authUser.PhotoUrl?.AbsoluteUri
                //};
                user = NewUser(_authUser);

                await _userDocumentReference.SetDataAsync(user).ConfigureAwait(false);
            }                  

            //_userId.Value = user.Id;
            //_userName.Value = user.Name;
            //_userImage.Value = user.Image;
            SetUser(user);
            _contributionCount.Value = user.ContributionCount;
            _isLoggedIn.Value = true;
        }

        public async Task SignupAsync(string email, string password, string name, string image = null)
        {

            var result = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password).ConfigureAwait(false);

            _authUser = result.User;

            _userDocumentReference = _firestore.GetCollection(User.CollectionPath)
                                      .GetDocument(_authUser.Uid);

            //var place = await GetLocationAsync();
            //var user = new User
            //{
            //    Id = authUser.Uid,
            //    Name = name,
            //    Location = place.Id,
            //    PostalCode = place.PostalCode,
            //    PhoneNumber = authUser.PhoneNumber,
            //    Email = authUser.Email,
            //    Image = image
            //};
            _user = NewUser(_authUser);

            await _userDocumentReference.SetDataAsync(_user).ConfigureAwait(false);

            //_userId.Value = user.Id;
            //_userName.Value = user.Name;
            //_userImage.Value = user.Image;
            SetUser(_user);
            _contributionCount.Value = _user.ContributionCount;
            _isLoggedIn.Value = true;
        }

        public void Logout()
        {
            _firebaseAuth.SignOut();

            //_userId.Value = null;
            //_userName.Value = null;
            //_userImage.Value = null;
            SetUser(new User());
            _contributionCount.Value = 0;
            _isLoggedIn.Value = false;
        }

        public Task IncrementContributionCountAsync(int delta)
        {
            return _firestore.RunTransactionAsync(transaction =>
            {
                //var document = _firestore.GetCollection(User.CollectionPath).GetDocument(UserId.Value);
                //_user = transaction.GetDocument(_userDocumentReference).ToObject<User>();

                if (_user != null)
                {
                    _user.ContributionCount += delta;
                    transaction.UpdateData(_userDocumentReference, _user);
                }
            });
        }

        public void Close()
        {
            _disposables.Dispose();
        }

        public async Task<Place> GetLocationAsync()
        {
            var l = "";
            try
            {
                var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (permissionStatus != PermissionStatus.Granted)
                    await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                //if (placemark != null)
                //{
                //    // Combine those string to built full address... placemark.AdminArea ,placemark.CountryCode , placemark.Locality , placemark.SubAdminArea , placemark.SubLocality , placemark.PostalCode
                //    string GeoCountryName = placemark.CountryName;

                //}

                //Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                l = ($"{(location.Latitude.ToString())}; {(location.Longitude.ToString())}");

                _place = new Place
                {
                    Id = l,
                    Latitude = location.Latitude.ToString(),
                    Longitude = location.Longitude.ToString(),
                    AdminArea = placemark.AdminArea,
                    CountryCode = placemark.CountryCode,
                    Locality = placemark.Locality,
                    SubAdminArea = placemark.SubAdminArea,
                    SubLocality = placemark.SubLocality,
                    PostalCode = placemark.PostalCode
                };
                return _place;
            }
        }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

            return null;
        }

        public void SetUser(User user)
        {
            _userId.Value = user.Id;
            UserName.Value = user.Name;
            UserReputation.Value = user.Reputation;
            UserLocation.Value = user.Location;
            UserPostalCode.Value = user.PostalCode;
            UserPhoneNumber.Value = user.PhoneNumber;
            UserEmail.Value = user.Email;
            _userImage.Value = user.Image;        
        }

        public User NewUser(IUser authUser)
        {
            //var place =  GetLocationAsync().Result;
            return new User()
            {
                Id = authUser.Uid,
                Name = authUser.DisplayName,
                Location = _place.Id,
                PostalCode = _place.PostalCode,
                PhoneNumber = authUser.PhoneNumber,
                Email = authUser.Email,
                Image = authUser.PhotoUrl?.AbsoluteUri
            };
        }

        public async Task SaveAsync()
        {
            if (!Can.Value)
                return;

            try
            {
                using (_notifier.ProcessStart())
                {
                    if (_place != null)
                    {
                        _user.Location = _place.Id;
                        _user.PostalCode = _place.PostalCode;
                    } else _user.PostalCode = UserPostalCode.Value;

                    _user.PhoneNumber = UserPhoneNumber.Value;
                    //await _userDocumentReference.SetDataAsync(_user).ConfigureAwait(false);
                    await _userDocumentReference.UpdateDataAsync(_user).ConfigureAwait(false);
                }
                _completedNotifier.OnNext(Unit.Default);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                _errorNotifier.OnNext(e.Message);
            }
        }

        public void GetMap()
        {
            //if (_user != null) _myMap.Value = _mapService.MapToLocation(_user.Location);
            //_myMap.Value = 
                _mapService.MapToLocation(_user.Location);
        }

    }
}
