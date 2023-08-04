using AccountDownloader.Services;
using AccountDownloaderLibrary;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using AccountDownloaderLibrary.Interfaces;

namespace AccountDownloader.ViewModels;

public class ProgressStatisticsViewModel : ReactiveObject
{
    [Reactive]
    public IAccountDownloadUserConfig Config { get; private set; }
    [Reactive]
    public AccountDownloadStatus Status { get; private set; }

    public ProgressStatisticsViewModel(IAccountDownloadUserConfig config, AccountDownloadStatus status)
    {
        Config = config;
        Status = status;
    }
}
