using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using StarLight_Core.Authentication;
using StarLight_Core.Enum;
using StarLight_Core.Launch;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;
using StarLight_Core.Utilities;

namespace WpfApp15;

public partial class StartPage : Page
{
    public StartPage()
    {
        InitializeComponent();
        GetJavas();
        GetGameCores();
    }

    public class Item
    {
        public string Id { get; set; }
    }
    private void GetGameCores()
    {
        List<Item> items = new List<Item>();
        foreach (var item in GameCoreUtil.GetGameCores(".minecraft"))
        {
            if (item.Exception == null)
            {
                items.Add(new Item 
                    { Id = item.Id,
                    });
            }
            else
            {
                items.Add(new Item 
                { Id = $"错误的版本：{item.Id}"
                });
            }
        }
        CoreBox.ItemsSource = items;
    }


    public class Item2
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
    private void GetJavas()
    {
        List<Item2> items = new List<Item2>();
        //items.Add(new Item2
        //{
            //Name = "让SMCL自动选择Java",
            //Path = ""
        //});
        foreach (var item in JavaUtil.GetJavas())
        {
            items.Add(new Item2
            {
                Name = $"Java {item.JavaSlugVersion}, {item.JavaLibraryPath}",
                Path = item.JavaPath
            });
        }
        JavaBox.ItemsSource = items;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        StartButton.IsEnabled = true;
    }

    private async void StartButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            BaseAccount account;
            account = new OfflineAuthentication(NameBox.Text).OfflineAuth();
            LaunchConfig args = new() // 配置启动参数
            {
                Account = new()
                {
                    BaseAccount = account // 账户
                },
                GameCoreConfig = new()
                {
                    Root = ".minecraft", // 游戏根目录(可以是绝对的也可以是相对的,自动判断)
                    Version = CoreBox.Text, // 启动的版本
                    IsVersionIsolation = true, //版本隔离
                    //Nide8authPath = ".minecraft\\nide8auth.jar", // 只有统一通行证需要
                    //UnifiedPassServerId = "xxxxxxxxxxxxxxxxxx" // 同上
                },
                JavaConfig = new()
                {
                    JavaPath = JavaBox.SelectedValue.ToString(), // Java 路径(绝对路径)
                    MaxMemory = 16384,
                    MinMemory = 1000
                },
                GameWindowConfig = new()
                {
                    Width = 854,
                    Height = 480,
                    IsFullScreen = false
                }
            };
            var launch = new MinecraftLauncher(args); // 实例化启动器
            var la = await launch.LaunchAsync(ReportProgress); // 启动
                
// 日志输出
            la.ErrorReceived += (output) => Console.WriteLine($"{output}");
            la.OutputReceived += (output) => Console.WriteLine($"{output}");
                
            if (la.Status == Status.Succeeded)
            {
                SnackbarOne.IsActive = true;
                await Task.Delay(1000);
                SnackbarOne.IsActive = false;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    private void ReportProgress(ProgressReport obj)
    {
        ProgressBar.Value = obj.Percentage;
    }

    private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        StartButton.IsEnabled = true;
    }
}