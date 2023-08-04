﻿using AccountDownloaderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Abstractions;
using System.Text.Json;

namespace AccountDownloaderLibrary.Services;

using Models;

public class AppConfigLoader : IAppConfigLoader
{
    private readonly IFileSystem _fileSystem;

    public string DownloadFolderPath { get; }

    public AppConfigLoader(IFileSystem fileSystem, string downloadFolderPath)
    {
        DownloadFolderPath = downloadFolderPath;
        _fileSystem = fileSystem;
    }

    public IAccountDownloadUserConfig LoadAccountDownloadConfig(string userId)
    {
        var filePath = GetUserAccountDownloadConfigPath(userId);
        if (!_fileSystem.File.Exists(filePath)) { return null; }

        using var configFileReader = _fileSystem.File.OpenRead(filePath);

        return JsonSerializer.Deserialize<AccountDownloadUserConfig>(configFileReader);
    }

    public void SaveAccountDownloadConfig(string userId, IAccountDownloadUserConfig config)
    {
        if (!_fileSystem.Directory.Exists(DownloadFolderPath))
        {
            _fileSystem.Directory.CreateDirectory(DownloadFolderPath);
        }

        using var configFileWriter = _fileSystem.File.OpenWrite(GetUserAccountDownloadConfigPath(userId));
        JsonSerializer.Serialize(configFileWriter, config);
    }

    private string GetUserAccountDownloadConfigPath(string userId) => Path.Combine(DownloadFolderPath, $"{userId}_config.json");
}
