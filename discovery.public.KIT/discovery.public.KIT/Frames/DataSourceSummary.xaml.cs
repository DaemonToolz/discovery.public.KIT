using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using discovery.KIT.Events;
using discovery.KIT.Internal;
using discovery.KIT.Models;
using discovery.KIT.Models.DataSources;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace discovery.KIT.Frames
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class DataSourceSummary : Page, INotifyPropertyChanged
    {

        private DataSourceConnection _cachedData;

        private string _alias;
        public string Alias
        {
            get => _alias;
            set => SetField(ref _alias, value);
        }
        private string _server;
        public string Server
        {
            get => _server;
            set => SetField(ref _server, value);
        }
        private string _port;
        public string Port
        {
            get => _port;
            set => SetField(ref _port, value);
        }
        private string _sid;
        public string SID
        {
            get => _sid;
            set => SetField(ref _sid, value);
        }
        private string _user;
        public string User
        {
            get => _user;
            set => SetField(ref _user, value);
        }

        private EventManager _eventManager = new EventManager();
        public DataSourceSummary()
        {
            this.InitializeComponent();
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!(e.Parameter is DataSourceConnection data))
            {
                _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
                {
                    NavigationEvent = NavigationEvent.Welcome,
                });
                return;
            }

            Alias = data.Alias;
            Server = data.Server;
            SID = data.OracleContent.SID;
            Port = data.Port.ToString();
            User = data.AuthenticationData.Username;
            _cachedData = data;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void BookmarkMeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            Task.Run(async () =>
            {
                var myGUID = _cachedData.ID.ToString();
                SettingsSecurity.DeleteContainer(myGUID);
                await SettingsSecurity.WriteSettings(myGUID, "alias", _cachedData.Alias, false);
                await SettingsSecurity.WriteSettings(myGUID, "authentication", _cachedData.Authentication.ToString(),
                    false);
                await SettingsSecurity.WriteSettings(myGUID, "server", _cachedData.Server, false);
                await SettingsSecurity.WriteSettings(myGUID, "port", _cachedData.Port.ToString(), false);
                await SettingsSecurity.WriteSettings(myGUID, "type", _cachedData.Type.ToString(), false);
                await SettingsSecurity.WriteSettings(myGUID, "sid", _cachedData.OracleContent.SID, false);

                await SettingsSecurity.SaveCredentials(myGUID, _cachedData.AuthenticationData.Username, _cachedData.AuthenticationData.Password);


                _ = Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
                    {
                        NavigationEvent = NavigationEvent.Welcome,
                    });
                });
            });
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

        }
    }
}
