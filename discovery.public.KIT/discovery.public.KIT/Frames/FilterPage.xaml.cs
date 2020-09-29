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
            DataContext = this;

            this.InitializeComponent();
        }

        private ObservableCollection<QueryFilter> _filters;
        private ObservableCollection<QueryOrderBy> _orderBys;

        public List<string> AvailableOrderBy { get; } = new List<string>() { SQLOrderingEnum.Ascendant.Sign, SQLOrderingEnum.Descendant.Sign, };


        public List<string> AvailableFilters { get; } = new List<string>(){ SQLComparatorsEnum.Equal.Sign,
            SQLComparatorsEnum.GreaterThan.Sign,
            SQLComparatorsEnum.GreaterThanOrEqual.Sign,
            SQLComparatorsEnum.LIKE.Sign,
            SQLComparatorsEnum.LowerThan.Sign,
            SQLComparatorsEnum.LowerThanOrEqual.Sign,
            SQLComparatorsEnum.OracleDifferent.Sign,
            SQLComparatorsEnum.SQLDifferent.Sign };

        public ObservableCollection<QueryOrderBy> OrderBys
        {
            get => _orderBys;
            private set => SetField(ref _orderBys, value);
        }

        public ObservableCollection<QueryFilter> Filters
        {
            get => _filters;
            private set => SetField(ref _filters, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Filters = new ObservableCollection<QueryFilter>(ActiveConnectionHandler.Filters);
            OrderBys = new ObservableCollection<QueryOrderBy>(ActiveConnectionHandler.OrderBy);
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

            Filters.Add(new QueryFilter(){ Filter = new SQLComparator(string.Empty)});
        }

        private void AddOrderClauseBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderBys.Add(new QueryOrderBy(){Direction = new SQLComparator(string.Empty) });
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
                ActiveConnectionHandler.Filters = filters.ToList();
            });

            var orders = OrderBys;
            Task.Run(() =>
            {
                ActiveConnectionHandler.OrderBy = orders.ToList();
            });

            _eventManager.OnNavigationEvent(new NavigationEventArgs<object>()
            {
                NavigationEvent = NavigationEvent.LogIn,
                Data = null
            });
        }

        private void DeleterOrderBy_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button deleteBtn) || string.IsNullOrEmpty(deleteBtn?.Tag?.ToString()))
            {
                return;
            }

            try
            {
                OrderBys.Remove(OrderBys.Single(data => data.GUID.Equals(deleteBtn.Tag.ToString())));
            }
            catch
            {

            }
        }
    }
}
