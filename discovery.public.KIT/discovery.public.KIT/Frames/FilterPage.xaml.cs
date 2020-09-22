using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using discovery.KIT.ORACLE.Models;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace discovery.KIT.Frames
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class FilterPage : Page, INotifyPropertyChanged
    {
        private EventManager _eventManager = new EventManager();

        public FilterPage()
        {
            this.InitializeComponent();
        }

        private ObservableCollection<QueryFilter> _filters = new ObservableCollection<QueryFilter>();
        private ObservableCollection<QueryOrderBy> _orderBys = new ObservableCollection<QueryOrderBy>();

        public ObservableCollection<QueryOrderBy> OrderBys
        {
            get => _orderBys;
            set => SetField(ref _orderBys, value);
        }


        public ObservableCollection<QueryFilter> Filters
        {
            get => _filters;
            set => SetField(ref _filters, value);
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

        private void AddSearchClauseBtn_Click(object sender, RoutedEventArgs e)
        {
            Filters.Add(new QueryFilter());
        }

        private void AddOrderClauseBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderBys.Add(new QueryOrderBy());
        }

        private void RemSearchClauseBtn_Click(object sender, RoutedEventArgs e)
        {
            Filters.Clear();
        }

        private void RemOrderClauseBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderBys.Clear();
        }

        private void DeleteFilterLineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button deleteBtn) || string.IsNullOrEmpty(deleteBtn?.Tag?.ToString()))
            {
                return;
            }

            try
            {
                Filters.Remove(Filters.Single(data => data.GUID.Equals(deleteBtn.Tag.ToString())));
            } catch
            {

            }
        }

        private void ValidateBtn_Click(object sender, RoutedEventArgs e)
        {
            var filters = Filters;

            Task.Run(() =>
            {
                ActiveConnectionHandler.Filters = filters.Select(data => $"{data.Column ?? string.Empty}{data.Filter?.Sign ?? string.Empty}{data.Value ?? string.Empty}").ToList<string>();
            });

            var orders = OrderBys;
            Task.Run(() =>
            {
                ActiveConnectionHandler.OrderBy = orders.Select(data => $"{data.Column ?? string.Empty}{data.Direction?.Sign ?? string.Empty}").ToList<string>();
            });

            _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
            {
                NavigationEvent = NavigationEvent.LogIn,
                Data = null
            });
        }
    }
}
