using Avalonia;
using Splat;
using AccountDownloader.Services;
using AccountDownloader.ViewModels;
using CloudX.Shared;
using Serilog.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Diagnostics;
using Serilog.Events;
using AccountDownloaderLibrary.Interfaces;
using AccountDownloaderLibrary;
using AccountDownloaderLibrary.Services;
using System.IO.Abstractions;

namespace AccountDownloader
{
    public class Config
    {
        public string LogFolder { get; }

        public string DownloadConfigFolder { get; }

        public LogEventLevel LogLevel { get; }
        
        public Config(IAssemblyInfoService? info) {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            var appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);

            LogFolder = Path.Join(appFolder, info.CompanyName, info.Name);

            DownloadConfigFolder = Path.Join(appFolder, info.CompanyName, info.Name, "DownloadConfigs");

#if DEBUG
            LogLevel = LogEventLevel.Debug;
#else
            LogLevel = LogEventLevel.Information;
#endif

        }
    }

    public class Boostrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolve)
        {
            services.RegisterConstant<IAssemblyInfoService>(new AssemblyInfoService());
            services.RegisterLazySingleton(() => new Config(resolve.GetService<IAssemblyInfoService>()));
            services.RegisterLazySingleton<ILogger>(() =>
            {
                var machine = Environment.MachineName;

                var info = resolve.GetService<IAssemblyInfoService>();
                var config = resolve.GetService<Config>();
                var folder = config!.LogFolder;
                var level = config!.LogLevel;

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(folder + $"/{timestamp}-{machine}-{info!.Version}-.log", restrictedToMinimumLevel:level )
                    .CreateLogger();

                var listener = new SerilogTraceListener.SerilogTraceListener(logger);
                Trace.Listeners.Add(listener);

                var factory = new SerilogLoggerFactory(logger);

                return factory.CreateLogger("Default");
            });

            services.RegisterConstant<IFileSystem>(new FileSystem());
            services.RegisterConstant<ILocaleService>(new LocaleService(resolve.GetService<ILogger>()));
            services.RegisterConstant<IAppConfigLoader>(new AppConfigLoader(resolve.GetService<IFileSystem>(), resolve.GetService<Config>()!.DownloadConfigFolder));

            var version = resolve.GetService<IAssemblyInfoService>();
            // Registering this as non-lazy because it is quite slow to init.
            services.RegisterConstant(new CloudXInterface(null, version?.NameNoSpaces, version?.Version));

            services.RegisterLazySingleton<IAppCloudService>(() => new AppCloudService(resolve.GetService<CloudXInterface>(), resolve.GetService<ILogger>()));

            services.RegisterLazySingleton(() => new MainWindowViewModel());
            services.Register<IAccountDownloader>(() => new AccountDownloadManager(resolve.GetService<CloudXInterface>(), resolve.GetService<ILogger>()));
            services.Register<IStorageService>(() => new CloudStorageService(resolve.GetService<CloudXInterface>(), resolve.GetService<ILogger>()));
            services.Register<IGroupsService>(() => new GroupsService(resolve.GetService<CloudXInterface>(), resolve.GetService<IStorageService>(), resolve.GetService<ILogger>()));
            services.RegisterLazySingleton(() => new ContributionsService(resolve.GetService<ILogger>()));
        }
    }
}
