using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using discovery.KIT.Events;
using discovery.KIT.Internal;
using discovery.KIT.Models;
using discovery.KIT.Models.DataSources;


namespace discovery.KIT.Frames
{

    public sealed partial class WelcomePage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<DataSourceConnection> _data = new ObservableCollection<DataSourceConnection>();

        private ObservableCollection<DataSourceConnection> Data
        {
            get => _data;
            set => SetField(ref _data, value);
        }


        public WelcomePage()
        {
            this.InitializeComponent();
            _=DatabaseDiscovery();


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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Task DatabaseDiscovery()
        {
            return Task.Run(async () =>
            {
                var containers = SettingsSecurity.FetchAllContainers();
                
                foreach (var container in containers)
                {
                    try
                    {

                        var summary = new DataSourceConnection()
                        {
                            ID = Guid.Parse(container),
                            Alias = await SettingsSecurity.ReadSettings(container, "alias", false),
                            Server = await SettingsSecurity.ReadSettings(container, "server", false),
                            Port = int.Parse(await SettingsSecurity.ReadSettings(container, "port", false)),
                        };

                        if(summary.Type == DataSourceType.Oracle)
                        {
                            summary.OracleContent = new OracleData()
                            {
                                SID = await SettingsSecurity.ReadSettings(container, "sid", false),
                            };
                        }

                        var myCredentials = SettingsSecurity.GetCredentials(container).FirstOrDefault();

                        myCredentials.RetrievePassword();
                        
                        summary.AuthenticationData = new Authentication(){
                            Username = myCredentials.UserName,
                            Password = SettingsSecurity.StringToSecureString(myCredentials.Password)
                        };
                        myCredentials = null;
                        _ = Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { Data.Add(summary); });
                    } catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            });
        }


        private EventManager _eventManager = new EventManager();
 
        private void CreateNewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
            {
                NavigationEvent = NavigationEvent.CreateOrUpdate,
            });
        }

        private void DeleteDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button DelButton))
            {
                return;
            }

            try
            {
                var guid = DelButton.Tag.ToString();
                var myCredentials = SettingsSecurity.GetCredentials(guid).FirstOrDefault();
                if (myCredentials != null)
                {
                    _ = Task.Run(async() =>
                    {
                        await SettingsSecurity.DeleteCredentials(guid, myCredentials);
                        SettingsSecurity.DeleteContainer(guid);

                        await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            try
                            {
                                var data = Data.Where(summary => summary.ID.ToString() == guid);
                                Data.Remove(data.First());
                            } catch
                            {

                            }
                         
                        });
                    });
                }
            }
            catch
            {

            }
        }

        private void EditSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button EditBtn))
            {
                return;
            }

            try
            {
                _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
                {
                    NavigationEvent = NavigationEvent.CreateOrUpdate,
                    Data = Data.First(summary => summary.ID.ToString() == EditBtn.Tag.ToString())
                });
            } catch
            {

            }
        }
    }
}
