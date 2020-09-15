using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using discovery.KIT.Events;
using discovery.KIT.Frames;
using discovery.KIT.Models;


namespace discovery.KIT
{

    public sealed partial class MainPage : Page
    {
        private EventManager _manager;
        public MainPage()
        {
            this.InitializeComponent();
            _manager = new EventManager();

            EventManager.Handler += OnNavigation;
        }

        private void OnNavigation(object sender, NavigationEventArgs<object> args)
        {
            switch (args?.NavigationEvent)
            {
                case NavigationEvent.Back:
                    var toClear = (MainFrame.BackStack.LastOrDefault()?.GetType() == typeof(WelcomePage));
                    MainFrame.GoBack();
                    if (toClear)
                    {
                        MainFrame.ForwardStack.Clear();
                        MainFrame.BackStack.Clear();
                    }
                    break;
                case NavigationEvent.CreateOrUpdate:
                    MainFrame.Navigate(typeof(CreateOrUpdateConnection), args.Data);
                    break;
                case NavigationEvent.Summary:
                    MainFrame.Navigate(typeof(DataSourceSummary), args.Data);
                    break;
                case NavigationEvent.Welcome:
                    MainFrame.Navigate(typeof(WelcomePage));
                    MainFrame.ForwardStack.Clear();
                    MainFrame.BackStack.Clear();
                    break;
            }
        }
    }
}
