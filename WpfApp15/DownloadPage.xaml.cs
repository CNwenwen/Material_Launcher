using System.Windows;
using System.Windows.Controls;
using StarLight_Core.Installer;
using StarLight_Core.Utilities;
using MaterialDesignThemes.Wpf;
using StarLight_Core.Enum;
using StarLight_Core.Models.Downloader;
using StarLight_Core.Models.Installer;

namespace WpfApp15;

public partial class DownloadPage : Page
{
    public DownloadPage()
    {
        InitializeComponent();
        DownloadAPIs.SwitchDownloadSource(DownloadSource.BmclApi);
        DownloaderConfig.MaxThreads = 128;
    }

    public class DownloadMCData
    {
        public string DisplayId { get; set; }
        public string Id { get; set; }
    }
    
    public class DownloadFabData
    {
        public string DisplayVersion { get; set; }
        public string Version { get; set; }
    }
    private async void DownloadPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            List<DownloadMCData> downloadDatas = new List<DownloadMCData>();
            foreach (var downloaddata in await InstallUtil.GetGameCoresAsync())
            {
                if (downloaddata.Type == "release")
                {
                    downloadDatas.Add(new DownloadMCData()
                    {
                        Id = downloaddata.Id,
                        DisplayId = downloaddata.Id + "，" + "正式版"
                    });
                }
                else if (downloaddata.Type == "snapshot")
                {
                    downloadDatas.Add(new DownloadMCData()
                    {
                        Id = downloaddata.Id,
                        DisplayId = downloaddata.Id + "，" + "快照版"
                    });
                }
                else
                {
                    downloadDatas.Add(new DownloadMCData()
                    {
                        Id = downloaddata.Id,
                        DisplayId = downloaddata.Id + "，" + "远古版"
                    });
                }
            }
            CoreVerBox.ItemsSource = downloadDatas;
            CoreVerBox.DisplayMemberPath = "DisplayId";
            CoreVerBox.SelectedValuePath = "Id";
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private async void CoreVerBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            List<DownloadFabData> downloadFabDatas = new List<DownloadFabData>();
            downloadFabDatas.Add(new DownloadFabData()
            {
                DisplayVersion = "不下载Fabric",
                Version = null
            });
            foreach (var downloadfabdata in await FabricInstaller.FetchFabricVersionsAsync(CoreVerBox.SelectedValue.ToString()))
            {
                downloadFabDatas.Add(new DownloadFabData()
                {
                    DisplayVersion = downloadfabdata.Version + "（" + downloadfabdata.GameVersion + "）",
                    Version = downloadfabdata.Version
                });
            }

            FabVerBox.ItemsSource = downloadFabDatas;
            FabVerBox.DisplayMemberPath = "DisplayVersion";
            FabVerBox.SelectedValuePath = "Version";
            FabVerBox.IsEnabled = true;
            HintAssist.SetHint(FabVerBox,"请选择Fabric版本");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            MinecraftInstaller installer = new MinecraftInstaller(CoreVerBox.SelectedValue.ToString(), ".minecraft");
            installer.OnProgressChanged += (status, progress) =>
            {
                ProgressBar.Value = progress;
                TextBlock.Text = status;
            };
            installer.OnSpeedChanged += (speed) =>
            {
                Console.WriteLine($"{speed}/s");
            };
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            await installer.InstallAsync(null,true,token);
            if (FabVerBox.SelectedValue != null)
            {
                FabricInstaller installerfab = new FabricInstaller(CoreVerBox.SelectedValue.ToString(),
                    FabVerBox.SelectedValue.ToString(),
                    default);
                installerfab.OnProgressChanged += (status, progress) =>
                {
                    ProgressBar.Value = progress;
                    TextBlock.Text = status;
                };
                installerfab.OnSpeedChanged += (speed) =>
                {
                    Console.WriteLine($"{speed}/s");
                };
                await installerfab.InstallAsync();
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}