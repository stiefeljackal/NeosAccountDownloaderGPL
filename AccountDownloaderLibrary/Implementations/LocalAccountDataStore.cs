using CloudX.Shared;
using ConcurrentCollections;
using System.IO.Abstractions;
using System.Threading.Tasks.Dataflow;
using Medallion.Threading.FileSystem;

namespace AccountDownloaderLibrary.Implementations;

using Models;
using Mime;
using Mime.Interfaces;
using System.Text.Json;
using AccountDownloaderLibrary.Extensions;
using BaseX;

public class LocalAccountDataStore : IAccountDataStore, IDisposable
{
    private const short INIT_VERSION = 1;

    ActionBlock<AssetJob> DownloadProcessor;
    readonly ConcurrentHashSet<string> ScheduledAssets = new();

    public string Name => "Local Data Store";
    public string UserId { get; private set; }
    public string Username { get; private set; }

    public readonly string BasePath;
    public readonly string AssetsPath;
    public readonly string AssetsMetadataPath;
    private readonly AccountDownloadConfig Config;

    public event Action<string> ProgressMessage;

    private FileDistributedLockHandle DirectoryLock;

    public int FetchedGroupCount { get; private set; }

    readonly Dictionary<string, int> _fetchedRecords = new();

    private readonly IFileSystem _fileSystem;

    private readonly IMimeDetector _mimeDetector;

    private CancellationToken CancelToken;

    public int FetchedRecordCount(string ownerId)
    {
        _fetchedRecords.TryGetValue(ownerId, out var count);
        return count;
    }

    public LocalAccountDataStore(string userId, string basePath, string assetsPath, AccountDownloadConfig config) : this(userId, basePath, assetsPath, new FileSystem(), MimeDetector.Instance, config) { }

    public LocalAccountDataStore(string userId, string basePath, string assetsPath, IFileSystem fileSystem, IMimeDetector mimeDetector, AccountDownloadConfig config)
    {
        UserId = userId;
        BasePath = basePath;
        AssetsPath = assetsPath;
        AssetsMetadataPath = $"{assetsPath}Metadata";
        _fileSystem = fileSystem;
        _mimeDetector = mimeDetector;
        Config = config;
    }

    public async Task Prepare(CancellationToken token)
    {
        var lockFileDirectory = new DirectoryInfo(BasePath);
        CancelToken = token;

        InitDownloadProcessor(CancelToken);

        var myLock = new FileDistributedLock(lockFileDirectory, "AccountDownloader");
        try
        {
            DirectoryLock = await myLock.AcquireAsync(TimeSpan.FromSeconds(5), token);

            if (DirectoryLock != null)
                return;
        }
        catch
        {
            throw new DataStoreInUseException("Could not aquire a lock on LocalAccountStore is this path in use by another tool?");
        }
    }

    public async Task Complete()
    {
        DownloadProcessor.Complete();
        await DownloadProcessor.Completion.ConfigureAwait(false);

        ReleaseLocks();
    }

    void InitDownloadProcessor(CancellationToken token)
    {
        _fileSystem.Directory.CreateDirectory(AssetsPath);
        _fileSystem.Directory.CreateDirectory(AssetsMetadataPath);

        DownloadProcessor = new ActionBlock<AssetJob>(async job =>
        {
            var hash = job.asset.Hash;
            var ext = job.AssetExtension;
            AssetMetadata metadata;
            var path = GetAssetPath(string.IsNullOrEmpty(ext) ? hash : $"{hash}.{ext}");

            try
            {
                if (!_fileSystem.File.Exists(GetAssetMetadataPath(hash)))
                {
                    ProgressMessage?.Invoke($"Downloading asset metadata for {hash}");

                    metadata = await job.source.GetAssetMetadata(hash).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(ext))
                    {
                        metadata.Extension = ext;
                    }
                    await StoreAssetMetadata(metadata, hash).ConfigureAwait(false);
                }
                else
                {
                    metadata = await GetAssetMetadata(hash);
                }

                if (!_fileSystem.File.Exists(path) && _fileSystem.File.Exists(GetAssetPath(hash)))
                {
                    // If the extensionless asset file exists, then just rename the file.
                    ProgressMessage?.Invoke($"Renaming asset {hash} with extension");
                    job.callbacks.AssetSkipped(hash);
                    await MoveAsset(GetAssetPath(hash), path).ConfigureAwait(false);
                }
                else if(!DoesAssetFileExists(hash))
                {

                    ProgressMessage?.Invoke($"Downloading asset {hash}");

                    var stream = await job.source.GetAssetStream(hash).ConfigureAwait(false);
                    await StoreAsset(metadata, stream, hash).ConfigureAwait(false);

                    ProgressMessage?.Invoke($"Finished download {hash}");
                }

                job.callbacks.AssetUploaded?.Invoke();
            }
            catch (Exception ex)
            {
                ProgressMessage?.Invoke($"Exception in fetching asset with Hash: {hash}: {ex}");
                job.callbacks.AssetFailure(new AssetFailure(hash, ex.Message, job.forRecord));
            }

            job.callbacks.AssetJobCompleted?.Invoke();

        }, new ExecutionDataflowBlockOptions()
        {
            CancellationToken = token,
            MaxDegreeOfParallelism = Config.MaxDegreeOfParallelism,
        });
    }

    public User GetUserMetadata() => GetEntity<User>(UserMetadataPath(UserId));

    public Task<List<Friend>> GetContacts() => GetEntities<Friend>(ContactsPath(UserId));

    public async IAsyncEnumerable<Message> GetMessages(string contactId, DateTime? from)
    {
        var messages = await GetEntities<Message>(MessagesPath(UserId, contactId)).ConfigureAwait(false);

        foreach (var msg in messages)
        {
            if (from != null && msg.LastUpdateTime < from.Value)
                continue;

            yield return msg;
        }
    }

    public async Task<Record> GetRecord(string ownerId, string recordId)
    {
        return await Task.FromResult(GetEntity<Record>(Path.Combine(RecordsPath(ownerId), recordId)));
    }

    public async IAsyncEnumerable<Record> GetRecords(string ownerId, DateTime? from)
    {
        var records = await GetEntities<Record>(RecordsPath(ownerId)).ConfigureAwait(false);

        _fetchedRecords[ownerId] = records.Count;

        foreach (var record in records)
        {
            if (from != null && record.LastModificationTime < from.Value)
                continue;

            yield return record;
        }
    }

    public Task<List<CloudVariableDefinition>> GetVariableDefinitions(string ownerId) => GetEntities<CloudVariableDefinition>(VariableDefinitionPath(ownerId));

    public Task<List<CloudVariable>> GetVariables(string ownerId) => GetEntities<CloudVariable>(VariablePath(ownerId));

    public async Task<CloudVariable> GetVariable(string ownerId, string path)
    {
        return await Task.FromResult(GetEntity<CloudVariable>(Path.Combine(VariablePath(ownerId), path)));
    }

    public async IAsyncEnumerable<GroupData> GetGroups()
    {
        var path = GroupsPath(UserId);
        var groups = await GetEntities<Group>(path).ConfigureAwait(false);

        FetchedGroupCount = groups.Count;

        foreach (var group in groups)
        {
            var storage = GetEntity<Storage>(Path.Combine(path, group.GroupId + ".Storage"));

            yield return new GroupData(group, storage);
        }
    }

    public async Task<List<MemberData>> GetMembers(string groupId)
    {
        var path = MembersPath(UserId, groupId);
        var members = await GetEntities<Member>(path).ConfigureAwait(false);

        var list = new List<MemberData>();

        foreach (var member in members)
        {
            var storage = GetEntity<Storage>(Path.Combine(path, member.UserId + ".Storage"));

            list.Add(new MemberData(member, storage));
        }

        return list;
    }

    static Task<List<T>> GetEntities<T>(string path)
    {
        var list = new List<T>();

        if (Directory.Exists(path))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.json"))
            {
                var entity = JsonSerializer.Deserialize<T>( File.ReadAllText(file));

                list.Add(entity);
            }
        }

        return Task.FromResult(list);
    }

    static T GetEntity<T>(string path)
    {
        path += ".json";

        if (File.Exists(path))
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path));

        return default;
    }

    public async Task StoreDefinitions(List<CloudVariableDefinition> definitions)
    {
        foreach (var definition in definitions)
            await StoreEntity(definition, Path.Combine(VariableDefinitionPath(definition.DefinitionOwnerId), definition.Subpath)).ConfigureAwait(false);
    }

    public async Task StoreVariables(List<CloudVariable> variables)
    {
        foreach (var variable in variables)
            await StoreEntity(variable, Path.Combine(VariablePath(variable.VariableOwnerId), variable.Path)).ConfigureAwait(false);
    }

    public Task StoreUserMetadata(User user) => StoreEntity(user, Path.Combine(UserMetadataPath(user.Id)));

    public Task StoreContact(Friend cotnact) => StoreEntity(cotnact, Path.Combine(ContactsPath(cotnact.OwnerId), cotnact.FriendUserId));

    public Task StoreMessage(Message message) => StoreEntity(message, Path.Combine(MessagesPath(message.OwnerId, message.GetOtherUserId()), message.Id));

    public async Task<string> StoreRecord(Record record, IAccountDataGatherer source, RecordStatusCallbacks statusCallbacks, bool overwriteOnConflict)
    {
        await StoreEntity(record, Path.Combine(RecordsPath(record.OwnerId), record.RecordId)).ConfigureAwait(false);

        if (record.NeosDBManifest != null)
            foreach (var asset in record.NeosDBManifest)
                ScheduleAsset(record, asset, source, statusCallbacks);

        return null;
    }

    public async Task StoreGroup(Group group, Storage storage)
    {
        var path = Path.Combine(GroupsPath(group.AdminUserId), group.GroupId);

        await StoreEntity(group, path);
        await StoreEntity(storage, path + ".Storage");
    }

    public async Task StoreMember(Group group, Member member, Storage storage)
    {
        var path = Path.Combine(MembersPath(group.AdminUserId, member.GroupId), member.UserId);

        await StoreEntity(member, path);
        await StoreEntity(storage, path + ".Storage");
    }

    static Task StoreEntity<T>(T entity, string path)
    {
        // Don't write nulls to the file system
        if (entity == null)
            return Task.CompletedTask;

        var directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(entity);

        File.WriteAllText(path + ".json", json);

        return Task.CompletedTask;
    }

    string VariableDefinitionPath(string ownerId) => Path.Combine(BasePath, ownerId, "VariableDefinitions");
    string VariablePath(string ownerId) => Path.Combine(BasePath, ownerId, "Variables");
    string UserMetadataPath(string ownerId) => Path.Combine(BasePath, ownerId, "User");
    string ContactsPath(string ownerId) => Path.Combine(BasePath, ownerId, "Contacts");
    string MessagesPath(string ownerId, string contactId) => Path.Combine(BasePath, ownerId, "Messages", contactId);
    string RecordsPath(string ownerId) => Path.Combine(BasePath, ownerId, "Records");
    string GroupsPath(string ownerId) => Path.Combine(BasePath, ownerId, "Groups");
    string MembersPath(string ownerId, string groupId) => Path.Combine(BasePath, ownerId, "GroupMembers", groupId);
    string GetAssetPath(string hash) => Path.Combine(AssetsPath, hash);
    string GetAssetMetadataPath(string hash) => Path.Combine(AssetsMetadataPath, $"{hash}.metadata.json");

    public async Task<DateTime> GetLatestMessageTime(string contactId)
    {
        DateTime latest = new(2016, 1, 1);

        await foreach (var message in GetMessages(contactId, null).ConfigureAwait(false))
        {
            if (message.LastUpdateTime > latest)
                latest = message.LastUpdateTime;
        }

        return latest;
    }

    public async Task<DateTime?> GetLatestRecordTime(string ownerId)
    {
        DateTime? latest = null;

        await foreach (var record in GetRecords(ownerId, null).ConfigureAwait(false))
        {
            if (latest == null || record.LastModificationTime > latest)
                latest = record.LastModificationTime;
        }

        return latest;
    }

    void ScheduleAsset(Record record, NeosDBAsset asset, IAccountDataGatherer store, RecordStatusCallbacks recordStatusCallbacks)
    {
        if (!ScheduledAssets.Add(asset.Hash))
            return;

        var job = new AssetJob(record, asset, store, recordStatusCallbacks);

            var diff = new AssetDiff
            {
                State = AssetDiff.Diff.Added,
                Bytes = asset.Bytes
            };

        recordStatusCallbacks.AssetToUploadAdded(diff);

        DownloadProcessor.Post(job);
    }

    public Task DownloadAsset(string hash, string targetPath)
    {
        return Task.Run(() => _fileSystem.File.Copy(GetAssetPath(hash), targetPath));
    }

    /// <summary>
    /// Gets the file size of the asset from the file system.
    /// </summary>
    /// <param name="hash">The file hash of the asset.</param>
    /// <returns>A number to indicate the size of the assets in bytes.</returns>
    /// <exception cref="MultipleHashExtensionsException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<long> GetAssetSize(string hash)
    {
        var asset = await GetAssetMetadata(hash);
        return asset.Size;
    }

    /// <summary>
    /// Gets the file stream of the asset from the file system.
    /// </summary>
    /// <param name="hash">The file hash of the asset.</param>
    /// <returns>The stream of the asset that can be read.</returns>
    /// <exception cref="MultipleHashExtensionsException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public Task<Stream> GetAssetStream(string hash)
    {
        var assetFilename = GetAssetFilename(hash);

        Stream stream = _fileSystem.File.OpenRead(assetFilename);

        return Task.FromResult(stream);
    }

    /// <summary>
    /// Gets the asset metadata of the stored asset from the file system.
    /// </summary>
    /// <param name="hash">The file hash of the asset.</param>
    /// <returns>
    /// The mime type of the size of the asset as an AssetData struct
    /// from the metadata file or from reading the actual asset.
    /// </returns>
    /// <exception cref="MultipleHashExtensionsException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<AssetMetadata> GetAssetMetadata(string hash)
    {
        var fs = _fileSystem.File.OpenRead(GetAssetMetadataPath(hash));
        var assetMetadata = JsonSerializer.Deserialize<AssetMetadata>(fs);

        if (assetMetadata.MimeType != null) { return assetMetadata; }

        Stream stream = await GetAssetStream(hash);
        var size = stream.Length;
        var contentType = _mimeDetector.MostLikelyMimeType(stream);

        return new AssetMetadata(null, contentType, size);
    }

    /// <summary>
    /// Stores the asset on the file system.
    /// </summary>
    /// <param name="assetMetadata">The asset metadata to use to help retrieve the extension.</param>
    /// <param name="stream">The asset stream to read data from.</param>
    /// <param name="hash">The asset file hash to be used as the filename.</param>
    /// <returns>The completed task.</returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public async Task StoreAsset(AssetMetadata assetMetadata, Stream stream, string hash)
    {
        var ext = !string.IsNullOrEmpty(assetMetadata.Extension)
            ? assetMetadata.Extension
            : _mimeDetector.GetFileExtensionByMimeType(assetMetadata.MimeType);

        byte[] headerBytes = null;

        if (string.IsNullOrEmpty(ext))
        {
            headerBytes = new byte[Math.Min(MimeDetector.MAX_FILE_SIZE_TO_READ, stream.Length)];
            await stream.ReadAsync(headerBytes).ConfigureAwait(false);

            ext = _mimeDetector.MostLikelyFileExtension(headerBytes);
        }

        // If we cannot get the extension, then just write the file. We already store the mime type
        var assetPath = $"{GetAssetPath(hash)}{(string.IsNullOrEmpty(ext) ? string.Empty :  $".{ext}")}";
        using var fs = _fileSystem.File.OpenWrite(assetPath);

        if (headerBytes != null)
        {
            await fs.WriteAsync(headerBytes).ConfigureAwait(false);
        }
        await stream.CopyToAsync(fs).ConfigureAwait(false);
    }

    /// <summary>
    /// Stores the asset metadata on the file system.
    /// </summary>
    /// <param name="assetMetadata">The asset metadata.</param>
    /// <param name="hash">The asset file hash to be used as the filename.</param>
    /// <returns>The completed task.</returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public async Task StoreAssetMetadata(AssetMetadata assetMetadata, string hash)
    {
        using var fs = _fileSystem.File.OpenWrite(GetAssetMetadataPath(hash));
        await JsonSerializer.SerializeAsync(fs, assetMetadata);
    }

    /// <summary>
    /// Moves the asset to a new path.
    /// </summary>
    /// <param name="oldPath">The asset file that needs to be moved.</param>
    /// <param name="newPath">The new path to move the asset file to.</param>
    /// <returns>The completed task.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public Task MoveAsset(string oldPath, string newPath)
    {
        if (!_fileSystem.Path.HasExtension(newPath))
        {
            var fs = _fileSystem.File.OpenRead(oldPath);
            var ext = _mimeDetector.MostLikelyFileExtension(fs);

            fs.Dispose();

            // If we find the ext, then update the new path with it
            if (!string.IsNullOrEmpty(ext))
            {
                newPath = $"{newPath}.{ext}";
            }
        }

        // If old path does not equal the new path, then move the file.
        if (oldPath != newPath)
        {
            _fileSystem.File.Move(oldPath, newPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Perform any cleanup on the existing store due to a version upgrade of the tool.
    /// </summary>
    /// <param name="version">The current version of the account downloader config file.</param>
    /// <returns></returns>
    public Task<short> PerformCleanup(short version)
    {
        switch(version)
        {
            // Sometimes, we must cleanup our mistakes even if they are minor.
            case INIT_VERSION - 1:
                var assetFilePaths = _fileSystem.Directory.GetFiles(AssetsPath, "*.metadata.json");
                foreach(var assetFilePath in assetFilePaths)
                {
                    _fileSystem.File.Delete(assetFilePath);

                }
                return Task.FromResult(INIT_VERSION);
            default:
                return Task.FromResult(version);
        }
    }

    /// <summary>
    /// Returns the asset filename that is available on the file system.
    /// </summary>
    /// <param name="hash">The file hash id used to search for the filename.</param>
    /// <returns>The filename of the found of the asset file.</returns>
    /// <exception cref="MultipleHashExtensionsException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    private string GetAssetFilename(string hash)
    {
        var filenames = _fileSystem.Directory.GetFiles(AssetsPath, $"{hash}.*").ToArray();

        if (filenames.Length > 1) { throw new MultipleHashExtensionsException(hash); }
        else if (!filenames.Any()) { throw new FileNotFoundException($"File with hash '{hash}' was not found."); }

        return filenames.First();
    }

    /// <summary>
    /// Determines if the asset file exists or not.
    /// </summary>
    /// <param name="hash">The file hash id used to locate the asset file.</param>
    /// <returns>true if the asset file was found; otherwise, false.</returns>
    private bool DoesAssetFileExists(string hash)
    {
        try
        {
            return !string.IsNullOrEmpty(GetAssetFilename(hash));
        }
        catch
        {
            return false;
        }
    }


    private void ReleaseLocks()
    {
        DirectoryLock?.Dispose();
    }
    public void Dispose()
    {
        ReleaseLocks();
    }

    public Task Cancel()
    {
        ReleaseLocks();
        DownloadProcessor.Complete();
        return Task.CompletedTask;
    }

}
