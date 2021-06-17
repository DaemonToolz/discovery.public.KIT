using discovery.KIT.Models.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace discovery.KIT.Frames
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class CreateOrUpdateConnection : Page, INotifyPropertyChanged
    {
        private EventManager _eventManager = new EventManager();
        private Guid _guid;
        public Guid GUID
        {
            get => _guid;
            set => SetField(ref _guid, value);
        }
        private string _alias;
        public string Alias
        {
            get => _alias;
            set => SetField(ref _alias, value);
        }
        private AuthenticationType _auth = AuthenticationType.UserPassword;
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
        private DataSourceType _type = DataSourceType.Oracle;
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

        private SecureString _pwd;
        public SecureString Password
        {
            get => _pwd;
            set => SetField(ref _pwd, value);
        }
        public CreateOrUpdateConnection()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter == null)
            {
                GUID = Guid.NewGuid();
                return;
            };

            var summary = (DataSourceConnection)e.Parameter;
            Password = summary.AuthenticationData?.Password;
            User = summary.AuthenticationData?.Username;
            SID = summary.OracleContent.SID;
            Port = summary.Port.ToString();
            Server = summary.Server;
            Alias = summary.Alias;
            GUID = summary.ID;
            
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

        private void ValidateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (DatabaseInformationFlipView.SelectedIndex != DatabaseInformationFlipView.Items.Count -1)
            {
                DatabaseInformationFlipView.SelectedIndex += 1;
                return;
            }

            try
            {
                // On garde le mot de passe en SecureString
          

                _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
                {
                    NavigationEvent = NavigationEvent.Summary,
                    Data = new DataSourceConnection()
                    { 
                        Alias = Alias,
                        Authentication = _auth,
                        AuthenticationData = new Authentication()
                        {
                            Username = User,
                            Password = SettingsSecurity.StringToSecureString(UserPasswordBox.Password),
                        },
                        ID = GUID,
                        OracleContent = new OracleData()
                        {
                            SID = SID,
                        },
                        Server = Server,
                        Type = _type,
                        Port = int.Parse(Port),
                    }

                });
                UserPasswordBox.Password = string.Empty;
            } catch(Exception ex)
            {

            }
        }
    }
}
