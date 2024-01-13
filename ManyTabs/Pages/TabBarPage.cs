using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace ManyTabs.Pages;

public class TabBarPage : ContentPage
{
    #region Public Properties
    public List<SimpleTabModel> Tabs { get; private set; } = new();
    public readonly ContentView PageView = new();
    public readonly ContentView FlyoutHeader = new() { ZIndex = 0, IsEnabled = false };
    public readonly ContentView FlyoutFooter = new() { ZIndex = 0, IsEnabled = false };
    public bool IsFlyoutVisible { get; private set; } = false;
    #endregion

    #region Private Properties
    private readonly ScrollView _TabScroll = new()
    {
        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
        Orientation = ScrollOrientation.Horizontal,
        Padding = 0,
        Margin = 0,
    };
    private readonly HorizontalStackLayout _TabBarLayout = new()
    {
        Spacing = 0,
        VerticalOptions = LayoutOptions.Fill
    };
    private readonly Grid _ContainerLayout = new();
    private readonly Grid _FlyoutLayout = new()
    {
        ColumnDefinitions = Columns.Define(
            new GridLength(0.6, GridUnitType.Star),
            new GridLength(0.4, GridUnitType.Star)),
        RowDefinitions = Rows.Define(
            new GridLength(0.25, GridUnitType.Star), // Header
            Star,                                    // Flyout
            32,                                      // Version 
            50),                                     // Footer
        RowSpacing = 0,
        ColumnSpacing = 0,
        ZIndex = 0
    };
    private readonly ScrollView _FlyoutItemsScroll = new() 
    { 
        Orientation = ScrollOrientation.Vertical, 
        VerticalScrollBarVisibility = ScrollBarVisibility.Never, 
        ZIndex = 0, 
        IsEnabled = false 
    };
    private readonly VerticalStackLayout _FlyoutItemsLayout = new() 
    { 
        Spacing = 0, 
        Padding = new Thickness(8, 0, 8, 0) 
    };
    private readonly Grid _TabLayout = new()
    {
        RowDefinitions = Rows.Define(Star, 50),
        ZIndex = 1
    };
    private readonly VerticalStackLayout _CloseFlyoutBox = new()
    {
        BackgroundColor = Colors.DarkGray.WithAlpha(0.5f),
        Opacity = 0,
        ZIndex = 2
    };
    private readonly PanGestureRecognizer _FlyoutPanGesture = new();
    private readonly Label _AppVersionLabel = new()
    {
        FontSize = 16,
        FontAttributes = FontAttributes.Bold,
    };
    #endregion

    #region Constructor
    public TabBarPage()
    {
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);

        this.SetDynamicResource(BackgroundColorProperty, "TabBarBackgroundColor");
        PageView.SetDynamicResource(BackgroundColorProperty, "PageColor");
        _FlyoutLayout.SetDynamicResource(BackgroundColorProperty, "PageColor");

        _TabScroll.SetDynamicResource(ScrollView.BackgroundColorProperty, "TabBarBackgroundColor");
        _TabBarLayout.SetDynamicResource(HorizontalStackLayout.BackgroundColorProperty, "TabBarBackgroundColor");

        _TabScroll.Content = _TabBarLayout;

        _TabLayout.Children.Add(PageView.Row(0));
        _TabLayout.Children.Add(_TabScroll.Row(1));

        _FlyoutItemsScroll.Content = _FlyoutItemsLayout;

        _FlyoutLayout.Children.Add(FlyoutHeader.Row(0));
        _FlyoutLayout.Children.Add(_FlyoutItemsScroll.Row(1));
        _FlyoutLayout.Children.Add(_AppVersionLabel.Center().Row(2));
        _FlyoutLayout.Children.Add(FlyoutFooter.Row(3));

        _ContainerLayout.Children.Add(_FlyoutLayout);
        _ContainerLayout.Children.Add(_TabLayout);

        _ContainerLayout.GestureRecognizers.Add(_FlyoutPanGesture);

        Content = _ContainerLayout;

        _CloseFlyoutBox.TapGesture(async () =>
        {
            await CloseFlyoutDirect();
        });
    }

    private async void PageChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is SimpleTab tab && tab.SimpleTabModel is SimpleTabModel model)
        {
            PageView.Content = model.PageContent;
        }
        else if (sender is SimpleFlyout flyout && flyout.SimpleTabModel is SimpleTabModel flyoutModel)
        {
            await CloseFlyoutDirect();
            PageView.Content = flyoutModel.PageContent;
        }
    }
    #endregion

    #region Overrides
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _AppVersionLabel.Text = $"v.{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

        BindableLayout.SetItemsSource(_TabBarLayout, Tabs);
        BindableLayout.SetItemTemplate(_TabBarLayout, new DataTemplate(() =>
        {
            var view = new SimpleTab();
            view.SetBinding(SimpleTab.SimpleTabModelProperty, ".");
            view.SelectedColor = Application.Current.Resources["SelectedTabColor"] as Color;
            view.NonSelectedColor = Application.Current.Resources["NonSelectedTabColor"] as Color;
            view.CheckedChanged += PageChanged;

            return view;
        }));

        BindableLayout.SetItemsSource(_FlyoutItemsLayout, Tabs);
        BindableLayout.SetItemTemplate(_FlyoutItemsLayout, new DataTemplate(() =>
        {
            var view = new SimpleFlyout();
            view.SetBinding(SimpleFlyout.SimpleTabModelProperty, ".");
            view.SelectedColor = Application.Current.Resources["SelectedTabColor"] as Color;
            view.NonSelectedColor = Application.Current.Resources["NonSelectedTabColor"] as Color;
            view.CheckedChanged += PageChanged;

            return view;
        }));

        _FlyoutPanGesture.PanUpdated += OpenFlyout;
    }

    protected override void OnDisappearing()
    {
        _FlyoutPanGesture.PanUpdated -= OpenFlyout;
        base.OnDisappearing();
    }
    #endregion

    #region Helpers
    private async void OpenFlyout(object sender, PanUpdatedEventArgs e)
    {
        var displayWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        double maxX = displayWidth * 0.6;
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                if (!_TabLayout.Children.Contains(_CloseFlyoutBox))
                {
                    _TabLayout.Children.Add(_CloseFlyoutBox.RowSpan(2));
                }
                _CloseFlyoutBox.Opacity = e.TotalX / maxX;
                _TabLayout.TranslationX = Math.Clamp(e.TotalX, 0, maxX);

                break;
            case GestureStatus.Completed:
                double minXToTriggerFlyout = displayWidth * 0.15;
                if (_TabLayout.TranslationX >= minXToTriggerFlyout)
                {
                    await OpenFlyoutDirect();
                }
                else
                {
                    await CloseFlyoutDirect();
                }

                break;
        }
    }

    public async Task OpenFlyoutDirect()
    {
        var displayWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        double maxX = displayWidth * 0.6;

        if (!_TabLayout.Children.Contains(_CloseFlyoutBox))
        {
            _TabLayout.Children.Add(_CloseFlyoutBox.RowSpan(2));
        }

        _ = _CloseFlyoutBox.FadeTo(1, 300);
        await _TabLayout.TranslateTo(maxX, 0, 300, easing: Easing.CubicIn);

        IsFlyoutVisible = true;

        FlyoutHeader.IsEnabled = true;
        _FlyoutItemsScroll.IsEnabled = true;
        FlyoutFooter.IsEnabled = true;
    }

    public async Task CloseFlyoutDirect()
    {
        _ = _CloseFlyoutBox.FadeTo(0, 300);
        await _TabLayout.TranslateTo(0, 0, 300, easing: Easing.CubicIn);
        _TabLayout.Children.Remove(_CloseFlyoutBox);

        IsFlyoutVisible = false;

        FlyoutHeader.IsEnabled = false;
        _FlyoutItemsScroll.IsEnabled = false;
        FlyoutFooter.IsEnabled = false;
    }

    public void OpenTabBarDirect()
    {
        if (!_TabLayout.Children.Contains(_TabScroll))
        {
            _TabLayout.Children.Add(_TabScroll.Row(1));
            PageView.RowSpan(1);
        }
    }

    public void CloseTabBarDirect()
    {
        _TabLayout.Children.Remove(_TabScroll);
        PageView.RowSpan(2);
    }
    #endregion
}

#region SimpleTab
public class SimpleTab : RadioButton
{
    #region Public Properties
    public static readonly BindableProperty SimpleTabModelProperty = BindableProperty.Create(
        nameof(SimpleTabModelProperty),
        typeof(SimpleTabModel),
        typeof(SimpleTab),
        null);
    
    public SimpleTabModel SimpleTabModel
    {
        get => (SimpleTabModel)GetValue(SimpleTabModelProperty);
        set => SetValue(SimpleTabModelProperty, value);
    }

    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
        nameof(SelectedColorProperty),
        typeof(Color),
        typeof(SimpleTab),
        null);

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly BindableProperty NonSelectedColorProperty = BindableProperty.Create(
        nameof(NonSelectedColorProperty),
        typeof(Color),
        typeof(SimpleTab),
        null);

    public Color NonSelectedColor
    {
        get => (Color)GetValue(NonSelectedColorProperty);
        set => SetValue(NonSelectedColorProperty, value);
    }
    #endregion

    #region Private Properties
    private readonly VerticalStackLayout _ContentLayout = new() 
    { 
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.Fill,
    };
    private readonly IconTintColorBehavior _TintColorBehavior = new();
    private readonly Image _Icon = new() { Aspect = Aspect.AspectFit };
    private readonly Label _Title = new() { FontSize = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
    #endregion

    #region Constructors
    public SimpleTab() 
    {
        WidthCheck();

        _Icon.Behaviors.Add(_TintColorBehavior);
        ControlTemplate = new ControlTemplate(() =>
        {
            return _ContentLayout;
        });

        Loaded += HasLoaded;
        Unloaded += HasUnloaded;
    }

    private void HasUnloaded(object sender, EventArgs e)
    {
        DeviceDisplay.Current.MainDisplayInfoChanged -= DisplayInfoHasChanged;
    }

    private void HasLoaded(object sender, EventArgs e)
    {
        DeviceDisplay.Current.MainDisplayInfoChanged += DisplayInfoHasChanged;
    }
    #endregion

    #region Overrides
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SimpleTabModelProperty.PropertyName && 
            SimpleTabModel is not null)
        {
            BindingContext = SimpleTabModel;

            this.SetBinding(IsCheckedProperty, "IsChecked");
            _Icon.Source = SimpleTabModel.Icon;
            _Title.Text = SimpleTabModel.Title;

            LayoutUI();
            StyleUI();
            WidthCheck();
        }
        else if (propertyName == IsCheckedProperty.PropertyName)
        {
            StyleUI();
        }
    }
    #endregion

    #region Helpers
    private void LayoutUI()
    {
        _ContentLayout.Clear();
        _ContentLayout.Add(_Icon);

        if (!string.IsNullOrEmpty(SimpleTabModel.Title))
        {
            _Icon.WidthRequest = 25;
            _Icon.HeightRequest = 25;

            _ContentLayout.Add(_Title);
        }
        else
        {
            _Icon.WidthRequest = 35;
            _Icon.HeightRequest = 35;
        }
    }

    private void StyleUI()
    {
        if (IsChecked)
        {
            _Title.FontAttributes = FontAttributes.Bold;
            _TintColorBehavior.TintColor = SelectedColor;
            _ContentLayout.ScaleTo(1.05, 70);
        }
        else
        {
            _Title.FontAttributes = FontAttributes.None;
            _TintColorBehavior.TintColor = NonSelectedColor;
            _ContentLayout.ScaleTo(1.0, 70);
        }
    }

    private void DisplayInfoHasChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        WidthCheck();
    }

    private void WidthCheck()
    {
        if (SimpleTabModel is not null && !SimpleTabModel.IsTabVisible)
        {
            WidthRequest = 0;
            _ContentLayout.Children.Clear();
        }
        else
        {
            WidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density / 5;
        }
    }
    #endregion
}

public class SimpleFlyout : RadioButton
{
    #region Public Properties
    public static readonly BindableProperty SimpleTabModelProperty = BindableProperty.Create(
        nameof(SimpleTabModelProperty),
        typeof(SimpleTabModel),
        typeof(SimpleFlyout),
        null);

    public SimpleTabModel SimpleTabModel
    {
        get => (SimpleTabModel)GetValue(SimpleTabModelProperty);
        set => SetValue(SimpleTabModelProperty, value);
    }

    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
        nameof(SelectedColorProperty),
        typeof(Color),
        typeof(SimpleFlyout),
        null);

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly BindableProperty NonSelectedColorProperty = BindableProperty.Create(
        nameof(NonSelectedColorProperty),
        typeof(Color),
        typeof(SimpleFlyout),
        null);

    public Color NonSelectedColor
    {
        get => (Color)GetValue(NonSelectedColorProperty);
        set => SetValue(NonSelectedColorProperty, value);
    }
    #endregion

    #region Private Properties
    private readonly HorizontalStackLayout _ContentLayout = new()
    {
        HeightRequest = 50,
        VerticalOptions = LayoutOptions.Fill,
        HorizontalOptions = LayoutOptions.Fill,
        Spacing = 8
    };
    private readonly IconTintColorBehavior _TintColorBehavior = new();
    private readonly Image _Icon = new() 
    { 
        Aspect = Aspect.AspectFit,
        WidthRequest = 35,
        HeightRequest = 35,
        VerticalOptions = LayoutOptions.Center
    };
    private readonly Label _Title = new() 
    { 
        FontSize = 12, 
        HorizontalOptions = LayoutOptions.Start, 
        VerticalOptions = LayoutOptions.Center 
    };
    #endregion

    #region Constructors
    public SimpleFlyout()
    {
        _Icon.Behaviors.Add(_TintColorBehavior);
        ControlTemplate = new ControlTemplate(() =>
        {
            _ContentLayout.Add(_Icon);
            _ContentLayout.Add(_Title);

            return _ContentLayout;
        });

        Loaded += HasLoaded;
        Unloaded += HasUnloaded;
    }

    private void HasUnloaded(object sender, EventArgs e) { }

    private void HasLoaded(object sender, EventArgs e) { }
    #endregion

    #region Overrides
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SimpleTabModelProperty.PropertyName &&
            SimpleTabModel is not null)
        {
            BindingContext = SimpleTabModel;

            this.SetBinding(IsCheckedProperty, "IsChecked");
            _Icon.Source = SimpleTabModel.Icon;
            _Title.Text = SimpleTabModel.Title;

            StyleUI();
            HeightCheck();
        }
        else if (propertyName == IsCheckedProperty.PropertyName)
        {
            StyleUI();
        }
    }
    #endregion

    #region Helpers
    private void StyleUI()
    {
        if (IsChecked)
        {
            _Title.FontAttributes = FontAttributes.Bold;
            _TintColorBehavior.TintColor = SelectedColor;
            _ContentLayout.ScaleTo(1.05, 70);
        }
        else
        {
            _Title.FontAttributes = FontAttributes.None;
            _TintColorBehavior.TintColor = NonSelectedColor;
            _ContentLayout.ScaleTo(1.0, 70);
        }
    }

    private void HeightCheck()
    {
        if (SimpleTabModel is not null && !SimpleTabModel.IsFlyoutVisible)
        {
            _ContentLayout.HeightRequest = 0;
            _ContentLayout.Children.Clear();
        }
        else
        {
            _ContentLayout.HeightRequest = 50;
        }
    }
    #endregion
}

#region Model
public partial class SimpleTabModel : ObservableObject
{
    [ObservableProperty]
    public string icon = string.Empty;

    [ObservableProperty]
    public string title = string.Empty;

    [ObservableProperty]
    public bool isChecked = false;

    [ObservableProperty]
    public bool isFlyoutVisible = true;

    [ObservableProperty]
    public bool isTabVisible = true;

    public ContentView PageContent { get; set; } = null;
}
#endregion

#endregion

