using Prism.Navigation;
using Prism.Services;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using Xamarin.Forms.GoogleMaps;
using Changa.Services;

namespace Changa.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public MapViewModel(INavigationService navigationService) : base(navigationService)
        {

        }

        public override void Destroy()
        {
            base.Destroy();

            _disposables.Dispose();
        }
    }
}
