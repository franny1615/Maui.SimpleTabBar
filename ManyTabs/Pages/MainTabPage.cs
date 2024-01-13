namespace ManyTabs.Pages;

public class MainTabPage : TabBarPage
{
    private readonly PageOne _PageOne;
    private readonly PageTwo _PageTwo;
    private readonly PageThree _PageThree;
    private readonly PageFour _PageFour;
    private readonly PageFive _PageFive;
    private readonly PageSix _PageSix;
    private readonly PageSeven _PageSeven;

    public SimpleTabModel _One { get; set; } = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page One",
        IsChecked = true,
    };
    private readonly SimpleTabModel _Two = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page Two",
        IsFlyoutVisible = false,
    };
    private readonly SimpleTabModel _Three = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page Three",
        IsFlyoutVisible = false,
    };
    private readonly SimpleTabModel _Four = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page Four",
    };   
    private readonly SimpleTabModel _Five = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page Five",
    };
    private readonly SimpleTabModel _Six = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page Six",
    };
    private readonly SimpleTabModel _Seven = new()
    {
        Icon = "dotnet_bot.png",
        Title = "Page Seven",
        IsTabVisible = false,
    };

	public MainTabPage() : base()
	{
        _PageOne = new(this);
        _PageTwo = new();
        _PageThree = new();
        _PageFour = new();
        _PageFive = new();
        _PageSix = new();
        _PageSeven = new(this);

        _One.PageContent = _PageOne;
        _Two.PageContent = _PageTwo;
        _Three.PageContent = _PageThree;
        _Four.PageContent = _PageFour;
        _Five.PageContent = _PageFive;
        _Six.PageContent = _PageSix;
        _Seven.PageContent = _PageSeven;

        FlyoutHeader.Content = new Grid
        {
            BackgroundColor = Colors.Green,
            Children =
            {
                new Label
                {
                    Text = "Some Title",
                    TextColor = Colors.White,
                    FontSize = 21,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
        FlyoutFooter.Content = new Grid
        {
            Padding = new Thickness(8, 0, 8, 0),
            Children =
            {
                new Button
                {
                    Text = "Sign Out",
                    TextColor = Colors.White,
                    BackgroundColor = Colors.Red
                }
            }
        };

        Tabs.Add(_One);
        Tabs.Add(_Two);
        Tabs.Add(_Three);
        Tabs.Add(_Four);
        Tabs.Add(_Five);
        Tabs.Add(_Six);
        Tabs.Add(_Seven);
    }
}

public class PageOne : ContentView
{
    private readonly MainTabPage _Parent;
    private readonly Button _OpenFlyoutButton = new()
    {
        Text = "Flyout",
        TextColor = Colors.White,
        BackgroundColor = Colors.Green,
    };

    private int _Count = 0;
    private readonly Label _Counter = new()
    {
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
        FontSize = 21,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.Red,
        Text = "0"
    };
    private readonly Button _AddToCounter = new()
    {
        BackgroundColor = Colors.Red,
        TextColor = Colors.White,
        Text = "Add To Counter"
    };

	public PageOne(MainTabPage parent)
	{
        _Parent = parent;
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Page One"
				},
                _OpenFlyoutButton,
                _Counter,
                _AddToCounter
			}
		};

        Loaded += HasLoaded;
        Unloaded += HasUnloaded;
	}

    private void HasUnloaded(object sender, EventArgs e)
    {
        _OpenFlyoutButton.Clicked -= OpenFlyout;
        _AddToCounter.Clicked -= Add;
    }

    private void HasLoaded(object sender, EventArgs e)
    {
        _OpenFlyoutButton.Clicked += OpenFlyout;
        _AddToCounter.Clicked += Add;
    }

    private void Add(object sender, EventArgs e)
    {
        _Count++;
        _Counter.Text = _Count.ToString();
    }

    private async void OpenFlyout(object sender, EventArgs e)
    {
        if (_Parent.IsFlyoutVisible)
        {
            await _Parent.CloseFlyoutDirect();
        }
        else
        {
            await _Parent.OpenFlyoutDirect();
        }
    }
}
public class PageTwo : ContentView
{
    public PageTwo()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Page Two"
                }
            }
        };
    }
}
public class PageThree : ContentView
{
    public PageThree()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Page Three"
                }
            }
        };
    }
}
public class PageFour : ContentView
{
    public PageFour()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Page Four"
                }
            }
        };
    }
}
public class PageFive : ContentView
{
    public PageFive()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Page Five"
                }
            }
        };
    }
}
public class PageSix : ContentView
{
    public PageSix()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Page Six"
                }
            }
        };
    }
}
public class PageSeven : ContentView
{
    private readonly MainTabPage _Parent;

    public PageSeven(MainTabPage parent)
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Page Seven"
                }
            }
        };
        _Parent = parent;

        Loaded += HasLoaded;
        Unloaded += HasUnloaded;
    }

    private void HasUnloaded(object sender, EventArgs e)
    {
        _Parent.OpenTabBarDirect();
    }

    private void HasLoaded(object sender, EventArgs e)
    {
        _Parent.CloseTabBarDirect();
    }
}