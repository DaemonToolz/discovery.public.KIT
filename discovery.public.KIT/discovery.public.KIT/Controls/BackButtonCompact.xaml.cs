
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using discovery.KIT.Events;
using discovery.KIT.Models;


// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page https://go.microsoft.com/fwlink/?LinkId=234236

namespace discovery.KIT.Controls
{
    public sealed partial class BackButtonCompact : UserControl
    {
        private EventManager _eventManager = new EventManager();
        public BackButtonCompact()
        {
            this.InitializeComponent();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
     
            _eventManager.OnNavigationEvent(new NavigationEventArgs<object>(){
                NavigationEvent =  NavigationEvent.Back,
            });
        }
    }
}
