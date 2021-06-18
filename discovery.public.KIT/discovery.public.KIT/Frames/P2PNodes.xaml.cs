using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using discovery.KIT.ORACLE.Models;
using discovery.KIT.p2p.Models;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using discovery.KIT.Events;
using discovery.KIT.Internal;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace discovery.KIT.Frames
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class P2PNodes : Page, INotifyPropertyChanged
    {
        private ObservableCollection<DiscoveryFrame> _detectedAppareils;
        public P2PNodes()
        {
            DetectedAppareils = new ObservableCollection<DiscoveryFrame>();
            this.InitializeComponent();
            P2PManager.DiscoveredSystems.ForEach(data => DetectedAppareils.Add(data));

            EventManager.DataUpdatedHandler += (sender, args) =>
            {
                DetectedAppareils.Clear();
                P2PManager.DiscoveredSystems.ForEach(data => DetectedAppareils.Add(data));
            };
        }

        private bool _isFree = true;
        public bool IsFree
        {
            get => _isFree;
            private set => SetField(ref _isFree, value);
        }

        private bool _isSending = false;
        public bool IsSending
        {
            get => _isSending;
            private set => SetField(ref _isSending, value);
        }


        private int _barMaxItems = 0;
        public int BarMaxItems
        {
            get => _barMaxItems;
            private set => SetField(ref _barMaxItems, value);
        }

        private int _currentItem = 0;
        public int CurrentItem
        {
            get => _currentItem;
            private set => SetField(ref _currentItem, value);
        }



        private string _transfertTitle = "";
        public string TransfertTitle
        {
            get => _transfertTitle;
            private set => SetField(ref _transfertTitle, value);
        }

        private string _transfertContent = "";
        public string TransfertContent
        {
            get => _transfertContent;
            private set => SetField(ref _transfertContent, value);
        }

        public volatile bool _canceled = false;

        public ObservableCollection<DiscoveryFrame> DetectedAppareils
        {
            get => _detectedAppareils;
            private set => SetField(ref _detectedAppareils, value);
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

        private void PeerDeviceBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = P2PManager.DiscoveredSystems.Find(d => d.Address == (sender as Button)?.Tag.ToString());
            if (result.Equals(default(DiscoveryFrame))) return;
            Task.Run(() =>
            {
                P2PManager.PeerToSystem(result);
            });
            IsFree = false;
        }

        private void DFPBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelBtn_Click(null, null);
            Task.Run(P2PManager.DisconnectFromPeer);
            IsFree = true;
            DisplayClear();
        }

        private void SendDataBtn_Click(object sender, RoutedEventArgs e)
        {
            _canceled = false;
            IsSending = true;
            Task.Run(async() =>
            {
                if (_canceled)
                {
                    CancelBtn_Click(null, null);
                    return;
                }
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentItem = 0;
                    BarMaxItems = 200;
                    TransfertTitle = "Column Headers";
                });
                for (int i = 0; i < BarMaxItems; ++i)
                {
                    if (_canceled)
                    {
                        CancelBtn_Click(null, null);
                        return;
                    }
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { 
                        CurrentItem++;
                        TransfertContent = $"Header {CurrentItem} / {BarMaxItems}";
                    });
                    P2PManager.SendData(i);
                }
                if (_canceled)
                {
                    CancelBtn_Click(null, null);
                    return;
                }
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentItem = 0;
                    BarMaxItems = 2850;
                    TransfertTitle = "Column content";
                });
                for (int i = 0; i < BarMaxItems; ++i)
                {
                    if (_canceled)
                    {
                        CancelBtn_Click(null, null);
                        return;
                    }
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        CurrentItem++;
                        TransfertContent = $"Line {CurrentItem} / {BarMaxItems}";
                    });
                    P2PManager.SendData(i);
                }
            });
        }

        private async void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSending) return;
            _canceled = true;
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsSending = false;
                CurrentItem = 0;
                BarMaxItems = 0;
                TransfertTitle = "Operation Skipped";
            });
        }

        private async void DisplayClear()
        {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
              
                TransfertTitle = "";
                TransfertContent = "";
            });
        }
    }
}
