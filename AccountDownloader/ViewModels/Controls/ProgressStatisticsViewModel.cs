using AccountDownloader.Services;
using AccountDownloaderLibrary;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using AccountDownloaderLibrary.Interfaces;

namespace AccountDownloader.ViewModels;

public class ProgressStatisticsViewModel : ReactiveObject
{
    [Reactive]
    public IAccountDownloadUserConfigProfile Config { get; private set; }
    [Reactive]
    public AccountDownloadStatus Status { get; private set; }

    public ProgressStatisticsViewModel(IAccountDownloadUserConfigProfile config, AccountDownloadStatus status)
    {
        Config = config;
        Status = status;
    }
}
