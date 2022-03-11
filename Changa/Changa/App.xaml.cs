﻿using Prism;
using Prism.Ioc;
using Changa.ViewModels;
using Changa.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.NavigationEx;
using System.Linq;
using System;
using Xamarin.Forms.Internals;
using Changa.Helpers;
using System.Reflection;
using Prism.Events;
using Changa.Events;
using Changa.Services;
using Prism.Plugin.Popups;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Changa
{
    public partial class App
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync<SplashScreenPageViewModel>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterPopupNavigationService();

            containerRegistry.RegisterForNavigation(this);

            GetType().Assembly.GetTypes()
                     .Where(t => t.Namespace?.EndsWith(".Services", StringComparison.Ordinal) ?? false && !t.IsAbstract && !t.IsInterface)
                     .Select(t => (Interface: t.GetInterface("I" + t.Name), Type: t))
                     .Where(t => t.Interface != null)
                     .ForEach(t =>
                     {
                         if (Attribute.GetCustomAttribute(t.Type, typeof(SingletonAttribute)) != null)
                         {
                             containerRegistry.RegisterSingleton(t.Interface, t.Type);
                         }
                         else
                         {
                             containerRegistry.Register(t.Interface, t.Type);
                         }
                     });
        }

        public void Destroy()
        {
            Container.Resolve<IEventAggregator>()
                     .GetEvent<DestoryEvent>()
                     .Publish();

            Container.Resolve<IAccountService>().Close();
        }
    }
}
