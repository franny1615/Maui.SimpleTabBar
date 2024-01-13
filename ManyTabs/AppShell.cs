using ManyTabs.Pages;

namespace ManyTabs
{
    public class AppShell : Shell
    {
        public AppShell()
        {
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
            Shell.SetTabBarIsVisible(this, false);

            Items.Add(new ShellContent
            {
                Title = "Page One",
                ContentTemplate = new DataTemplate(typeof(MainTabPage)),
                Route = nameof(MainTabPage)
            });           
        }
    }
}
