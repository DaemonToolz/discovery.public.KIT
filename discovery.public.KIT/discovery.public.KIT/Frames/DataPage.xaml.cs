using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class DataPage : Page, INotifyPropertyChanged
    {

        private EventManager _eventManager = new EventManager();

        private bool _offlineMode = true;
        public bool OfflineMode
        {
            get => _offlineMode;
            set => SetField(ref _offlineMode, value);
        }

        private DataSourceConnection _cached;

        public DataPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!(e.Parameter is DataSourceConnection data))
            {
                return;
            }

            _cached = data;
            OfflineMode = _cached.OfflineMode;

            if (!OfflineMode)
            {
                OfflineMode = false;
                /*
               _ = ActiveConnectionHandler.ConnectAsync(_cached).ContinueWith(task =>
               {
                   var ok = false;
                   try  {
                       ok = task.Result;
                   } catch
                   {
                       ok = false;
                   }

                   _ = Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { OfflineMode = ok; });
               });
                */
            }
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

    }
}
