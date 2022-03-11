﻿using System;
using Foundation;
using Prism;
using Prism.Ioc;
using UIKit;
using Changa.iOS.Renderers;
using Changa.Services;
using Firebase.Crashlytics;
using Prism.Events;


namespace Changa.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private App _app;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Firebase.Core.App.Configure();
            Crashlytics.Configure();

            AiForms.Renderers.iOS.CollectionViewInit.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            Xamarin.Auth.Presenters.XamarinIOS.AuthenticationConfiguration.Init();
            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();

            _app = new App(new iOSInitializer());
            LoadApplication(_app);

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            _app.Container.Resolve<IAuthService>()
                .OnPageLoading(new Uri(url.AbsoluteString));

            return true;
        }
    }

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}
