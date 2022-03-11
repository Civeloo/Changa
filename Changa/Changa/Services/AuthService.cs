﻿using System;
using Xamarin.Auth;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Auth.Presenters;
using Changa.Helpers;

namespace Changa.Services
{
    [Singleton]
    public class AuthService : IAuthService
    {
        private WebRedirectAuthenticator _authenticator;

        public Task<(string IdToken, string AccessToken)> LoginWithGoogle()
        {
            string clientId = null;
            string redirectUri = null;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientId = Variables.IosGoogleClientId;
                    redirectUri = Variables.IosReversedGoogleClientId;
                    break;
                case Device.Android:
                    clientId = Variables.AndroidGoogleClientId;
                    redirectUri = Variables.AndroidReversedGoogleClientId;
                    break;
            }
            redirectUri += ":/oauth2redirect";

            _authenticator = new OAuth2Authenticator(clientId,
                                                     null,
                                                     "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile",
                                                     new Uri("https://accounts.google.com/o/oauth2/auth"),
                                                     new Uri(redirectUri),
                                                     new Uri("https://www.googleapis.com/oauth2/v4/token"),
                                                     null,
                                                     true);

            var tcs = new TaskCompletionSource<(string IdToken, string AccessToken)>();

            _authenticator.Completed += (sender, e) =>
            {
                if (e.IsAuthenticated && e.Account != null && e.Account.Properties != null)
                {
                    var properties = e.Account.Properties;

                    tcs.TrySetResult((IdToken: properties["id_token"], AccessToken: properties["access_token"]));
                }
                else
                {
                    tcs.TrySetResult((null, null));
                }
            };

            _authenticator.Error += (sender, e) =>
            {
                tcs.TrySetException(e.Exception ?? new Exception(e.Message));
            };

            var presenter = new OAuthLoginPresenter();
            presenter.Login(_authenticator);

            return tcs.Task;
        }

        public void OnPageLoading(Uri uri)
        {
            _authenticator?.OnPageLoading(uri);
        }
    }
}
