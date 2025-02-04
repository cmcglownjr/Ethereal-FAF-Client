﻿using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : INavigationAware
    {
        private readonly IOAuthService OAuthService;
        private readonly ILobbySessionService _LobbySessionService;
        private INavigationManager NavigationManager;

        /// <summary>
        /// Raising event on navigating, give us a NavigationManager to move to next view
        /// </summary>
        /// <param name="navigationManager"></param>
        /// <returns></returns>
        async Task INavigationAware.OnViewChanged(INavigationManager navigationManager)
        {
            NavigationManager = navigationManager;

            // getting user setting for auto join
            if (Settings.Default.AutoJoin)
            {
                // looping view controls and hide them
                for (int i = 0; i < Canvas.Children.Count; i++)
                    Canvas.Children[i].Visibility = Visibility.Collapsed;
                
                ProgressRing.Visibility = Visibility.Visible;

                // launch OAuth2 authorization in service
                await OAuthService.Auth();
            }
        }

        public AuthView()
        {
            InitializeComponent();

            // loading required services
            OAuthService = App.Services.GetRequiredService<IOAuthService>();
            _LobbySessionService = App.Services.GetRequiredService<ILobbySessionService>();

            // starting listening events

            // raising up after finishing OAuth2 authorization
            OAuthService.Result += OnOAuthAuthorizationFinish;
            
            // raising up after finishing lobby session authorization
            _LobbySessionService.Authorization += OnLobbySessionServiceAuthorizationFinish;

            // load user setting of auto join ot local checkbox
            AutoJoinCheckBox.IsChecked = Settings.Default.AutoJoin;
        }
            
        private void OnLobbySessionServiceAuthorizationFinish(object sender, Infrastructure.EventArgs<bool> e) =>
            Dispatcher.Invoke(() =>
            {
                // release used resources
                Canvas.Children.Clear();
                Canvas = null;

                // remove listeners of events
                OAuthService.Result -= OnOAuthAuthorizationFinish;
                _LobbySessionService.Authorization -= OnLobbySessionServiceAuthorizationFinish;
                // call UI thread and invoke our instructions
                // events coming from different thread and can raise exception
                return NavigationManager.Navigate(new MainView());
            });

        private void OnCheckBoxClick(object sender, RoutedEventArgs e) => Settings.Default.AutoJoin = ((CheckBox)sender).IsChecked.Value;

        private void OnAuthorization(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string loginrOrEmail = LoginInput.Text;
                string password = PasswordInput.Password;

                for (int i = 0; i < Canvas.Children.Count; i++)
                    Canvas.Children[i].Visibility = Visibility.Collapsed;

                ProgressRing.Visibility = Visibility.Visible;

                OAuthService.Auth(loginrOrEmail, password);
            });
        }

        private readonly ContentDialog WarnDialog = new()
        {
            Title = "Warning",
            CloseButtonText = "Ok"
        };

        private void OnOAuthAuthorizationFinish(object sender, Infrastructure.EventArgs<OAuthStates> e)
        {
            switch (e.Arg)
            {
                case OAuthStates.AUTHORIZED:
                    for (int i = 0; i < Canvas.Children.Count; i++)
                        Canvas.Children[i].Visibility = Visibility.Collapsed;
                    ProgressRing.Visibility = Visibility.Visible;
                    return;
                case OAuthStates.INVALID:
                    WarnDialog.Content = "Invalid authorization parameters";
                    break;
                case OAuthStates.NO_CONNECTION:
                    WarnDialog.Content = "No connection.\nPlease check your internet connection";
                    break;
                case OAuthStates.NO_TOKEN:
                    WarnDialog.Content = "Something went wrong on auto-join.\nPlease use authorization form again";
                    break;
                case OAuthStates.TIMED_OUT:
                    WarnDialog.Content = "Server is not responses";
                    break;
                case OAuthStates.EMPTY_FIELDS:
                    WarnDialog.Content = "Empty fields";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < Canvas.Children.Count; i++)
                Canvas.Children[i].Visibility = Visibility.Visible;

            ProgressRing.Visibility = Visibility.Collapsed;

            WarnDialog.ShowAsync();
        }
    }
}
