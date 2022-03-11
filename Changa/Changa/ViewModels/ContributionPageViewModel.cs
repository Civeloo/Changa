using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Changa.Services;
using Reactive.Bindings;
using Xamarin.Forms;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Reactive.Disposables;
using System.Diagnostics;
using Xamarin.Forms.GoogleMaps;
using Changa.Resx;

namespace Changa.ViewModels
{
    public class ContributionPageViewModel : ViewModelBase
    {
        private readonly IContributionService _contributionService;
        private readonly IPageDialogService _pageDialogService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<Map> MyMap { get; }

        //public ReadOnlyReactivePropertySlim<ImageSource> ItemImage { get; }
        public ReactivePropertySlim<string> ItemTitle { get; }
        public ReactivePropertySlim<string> ItemComment { get; }
        public ReactivePropertySlim<double> ItemPrice { get; }
        public ReactivePropertySlim<string> ItemLocation { get; }
        public ReactivePropertySlim<string> ItemPostalCode { get; }
        public ReactivePropertySlim<int> ItemHours { get; }
        public ReactivePropertySlim<string> ItemStatus { get; }

        //public ReadOnlyReactivePropertySlim<bool> ItemImageNotExists { get; }

        public AsyncReactiveCommand SelectImageCommand { get; }
        public AsyncReactiveCommand ContributeCommand { get; }

        public ContributionPageViewModel(INavigationService navigationService, IContributionService contributionService, IPageDialogService pageDialogService) : base(navigationService)
        {
            _contributionService = contributionService;
            _pageDialogService = pageDialogService;
            
            Title = AppResources.Work;

            //ItemImage = _contributionService.ItemImage
            //    .Select(source =>
            //    {
            //        if (source == null) return null;

            //        var stream = new MemoryStream();
            //        source.CopyTo(stream);
            //        source.Seek(0, SeekOrigin.Begin);
            //        stream.Seek(0, SeekOrigin.Begin);
            //        return stream;
            //    })
            //    .Select(s => s == null ? null : ImageSource.FromStream(() => s))
            //    .ToReadOnlyReactivePropertySlim()
            //    .AddTo(_disposables);

            ItemTitle = _contributionService.ItemTitle;
            ItemComment = _contributionService.ItemComment;
            ItemPrice = _contributionService.ItemPrice;
            ItemLocation = _contributionService.ItemLocation;
            ItemPostalCode = _contributionService.ItemPostalCode;
            MyMap = _contributionService.MyMap
                .ToReadOnlyReactivePropertySlim()
                .AddTo(_disposables);

            ItemHours = _contributionService.ItemHours;
            ItemStatus = _contributionService.ItemStatus;

            //ItemImageNotExists = _contributionService.ItemImage.Select(s => s == null)
            //                                         .ToReadOnlyReactivePropertySlim()
            //                                         .AddTo(_disposables);

            _contributionService.ContributeCompletedNotifier
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => _pageDialogService.DisplayAlertAsync(null, AppResources.Post_has_been_completed, AppResources.Ok).ToObservable())
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => GoBackAsync())
                                .Subscribe()
                                .AddTo(_disposables);

            _contributionService.ContributeErrorNotifier
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => _pageDialogService.DisplayAlertAsync("error", AppResources.Post_failed, AppResources.Ok).ToObservable())
                                .Subscribe()
                                .AddTo(_disposables);

            _contributionService.IsContributing
                                .Skip(1)
                                .Where(b => b)
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => NavigateAsync<LoadingPageViewModel>())
                                .Subscribe()
                                .AddTo(_disposables);

            _contributionService.IsContributing
                                .Skip(1)
                                .Where(b => !b)
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => GoBackAsync())
                                .Subscribe()
                                .AddTo(_disposables);

            SelectImageCommand = new AsyncReactiveCommand();
            ContributeCommand = _contributionService.CanContribute
                                                    .ToAsyncReactiveCommand()
                                                    .AddTo(_disposables);

            SelectImageCommand.Subscribe(async () => await _contributionService.SelectImage());
            ContributeCommand.Subscribe(async () => await _contributionService.Contribute());
            
        }

        public override void Destroy()
        {
            base.Destroy();

            _disposables.Dispose();
        }
    }
}
