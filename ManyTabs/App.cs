namespace ManyTabs
{
    public class App : Application
    {
        public App()
        {
            Resources.MergedDictionaries.Add(new Resources.Styles.Colors());
            Resources.MergedDictionaries.Add(new Resources.Styles.Styles());

            MainPage = new AppShell();
        }
    }
}
