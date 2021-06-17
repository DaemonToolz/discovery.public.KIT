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
using discovery.KIT.Events;
using discovery.KIT.Internal;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace discovery.KIT.Frames
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class P2PNodes : Page
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
            P2PManager.PeerToSystem(result);

            for (int i = 0; i < 5; ++i)
            {

                P2PManager.SendData("testestesttest");

            }
        }
    }
}
