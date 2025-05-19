using UOMacroMobile.Pages;

namespace UOMacroMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
            Routing.RegisterRoute(nameof(ActionsPage), typeof(ActionsPage));
        }



    }
}
