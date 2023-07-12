using System.Net;
using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;

namespace AccountDownloaderLibrary
{
    public class CloudAccountDataStore : IAccountDataGatherer
    {
        public readonly CloudXInterface Cloud;

        public string Name => Cloud.UserAgentProduct + " " + Cloud.UserAgentVersion;
        public string UserId => Cloud.CurrentUser.Id;
        public string Username => Cloud.CurrentUser.Username;

#pragma warning disable CS0067 // The event 'CloudAccountDataStore.ProgressMessage' is never used
        public event Action<string> ProgressMessage;
#pragma warning restore CS0067 // The event 'CloudAccountDataStore.ProgressMessage' is never used

        public int FetchedGroupCount { get; private set; }

        readonly Dictionary<string, int> _fetchedRecords = new();

        public static DateTime EARLIEST_API_TIME = new(2016, 1, 1);

        private CancellationToken CancelToken;

        private const string DB_PREFIX = "neosdb:///";
        private static readonly TimeSpan WEB_CLIENT_DEFAULT_TIMEOUT = new TimeSpan(0, 3, 0);
        private HttpClient WebClient;

        private readonly AccountDownloadConfig Config;

        public int FetchedRecordCount(string ownerId)
        {
            _fetchedRecords.TryGetValue(ownerId, out var count);
            return count;
        }

        public CloudAccountDataStore(CloudXInterface cloud, AccountDownloadConfig config) : this(cloud, new HttpClient { Timeout = WEB_CLIENT_DEFAULT_TIMEOUT  }, config) { }

        public CloudAccountDataStore(CloudXInterface cloud, HttpClient client, AccountDownloadConfig config)
        {
            this.Cloud = cloud;
            this.Config = config;
            this.WebClient = client;
        }

        public virtual async Task Prepare(CancellationToken token)
        {
            CancelToken = token;

            await Cloud.UpdateCurrentUserMemberships().ConfigureAwait(false);

            FetchedGroupCount = Cloud.CurrentUserGroupInfos.Where(g => g.AdminUserId == Cloud.CurrentUser.Id).Count();
        }

        public virtual async Task Complete()
        {
            await Task.CompletedTask;
        }

        public virtual async Task<List<Friend>> GetContacts()
        {
            CloudResult<List<Friend>> result = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                result = await Cloud.GetFriends().ConfigureAwait(false);

                if (result.IsOK)
                    return result.Entity;

                await Task.Delay(TimeSpan.FromSeconds(attempt * 1.5), CancelToken).ConfigureAwait(false);
            }

            throw new Exception("Could not fetch contacts after several attempts. Result: " + result);
        }

        public virtual async IAsyncEnumerable<Message> GetMessages(string contactId, DateTime? from)
        {
            List<Message> messages = null;
            var start = from ?? EARLIEST_API_TIME;

            var processed = new HashSet<string>();

            for (; ; )
            {
                messages = (await Cloud.GetMessages(start, 100, contactId, false).ConfigureAwait(false)).Entity;

                messages?.RemoveAll(m => processed.Contains(m.Id));

                if (messages == null || messages.Count == 0)
                    break;

                foreach (var m in messages)
                {
                    yield return m;

                    if (m.SendTime >= start)
                        start = m.SendTime;

                    processed.Add(m.Id);
                }
            }
        }

        public virtual async IAsyncEnumerable<GroupData> GetGroups()
        {
            var memberships = await Cloud.GetUserGroupMemeberships().ConfigureAwait(false);

            foreach (var membership in memberships.Entity)
            {
                var group = await Cloud.GetGroup(membership.GroupId).ConfigureAwait(false);

                var storage = await Cloud.GetStorage(membership.GroupId).ConfigureAwait(false);

                yield return new GroupData(group.Entity, storage.Entity);
            }
        }

        public virtual async Task<List<MemberData>> GetMembers(string groupId)
        {
            var members = await Cloud.GetGroupMembers(groupId).ConfigureAwait(false);

            var data = new List<MemberData>();

            foreach (var member in members.Entity)
            {
                var storage = await Cloud.GetMemberStorage(groupId, member.UserId).ConfigureAwait(false);

                data.Add(new MemberData(member, storage.Entity));
            }

            return data;
        }

        public virtual async Task<Record> GetRecord(string ownerId, string recordId)
        {
            var result = await Cloud.GetRecord<Record>(ownerId, recordId).ConfigureAwait(false);

            return result.Entity;
        }

        public virtual async IAsyncEnumerable<Record> GetRecords(string ownerId, DateTime? from)
        {
            var searchParams = new SearchParameters
            {
                ByOwner = ownerId,
                Private = true,

                SortBy = SearchSortParameter.LastUpdateDate,
                SortDirection = SearchSortDirection.Descending
            };

            if (from != null)
                searchParams.MinDate = from.Value;

            var search = new RecordSearch<Record>(searchParams, Cloud);

            var count = 0;

            while (search.HasMoreResults)
            {
                count += 100;
                await search.EnsureResults(count);
            }

            _fetchedRecords[ownerId] = search.Records.Count;

            foreach (var r in search.Records)
            {
                string lastError = null;

                // Must get the actual record, which will include manifest
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    var result = await Cloud.GetRecord<Record>(ownerId, r.RecordId).ConfigureAwait(false);

                    if (result.Entity == null)
                    {
                        // it was deleted in the meanwhile, just ignore
                        if (result.State == HttpStatusCode.NotFound)
                            break;

                        lastError = $"Could not get record: {ownerId}:{r.RecordId}. Result: {result}";

                        // try again
                        continue;
                    }

                    yield return result.Entity;
                    break;
                }

                if (lastError != null)
                    throw new Exception(lastError);
            }
        }

        public virtual User GetUserMetadata() => Cloud.CurrentUser;

        public virtual async Task<List<CloudVariableDefinition>> GetVariableDefinitions(string ownerId)
        {
            var definitions = await Cloud.ListVariableDefinitions(ownerId).ConfigureAwait(false);
            return definitions.Entity;
        }

        public virtual async Task<List<CloudVariable>> GetVariables(string ownerId)
        {
            var variables = await Cloud.GetAllVariables(ownerId).ConfigureAwait(false);
            return variables.Entity;
        }

        public virtual async Task<CloudVariable> GetVariable(string ownerId, string path)
        {
            var result = await Cloud.ReadVariable<CloudVariable>(ownerId, path).ConfigureAwait(false);

            return result?.Entity;
        }

        public virtual async Task<DateTime> GetLatestMessageTime(string contactId)
        {
            int delay = 50;

            CloudResult lastResult = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                var messages = await Cloud.GetMessages(null, 1, contactId, false).ConfigureAwait(false);

                lastResult = messages;

                if (!messages.IsOK)
                {
                    await Task.Delay(delay);
                    delay *= 2;

                    continue;
                }

                if (messages.Entity.Count > 0)
                    return messages.Entity[0].LastUpdateTime;

                return EARLIEST_API_TIME;
            }

            throw new Exception($"Failed to determine latest message time after several attempts for contactId: {contactId}. Result: {lastResult}");
        }

        public virtual async Task<DateTime?> GetLatestRecordTime(string ownerId)
        {
            var search = new SearchParameters
            {
                ByOwner = ownerId,
                Private = true,
                SortBy = SearchSortParameter.LastUpdateDate,
                SortDirection = SearchSortDirection.Descending,
                Count = 1
            };

            var records = await Cloud.FindRecords<Record>(search).ConfigureAwait(false);

            if (records.IsOK && records.Entity.Records.Count == 1)
                return records.Entity.Records[0].LastModificationTime;

            return null;
        }

        /// <summary>
        /// Gets the asset size from CloudX
        /// </summary>
        /// <param name="hash">The file hash of the asset.</param>
        /// <returns>A number that represents the size of the asset.</returns>
        public virtual async Task<long> GetAssetSize(string hash)
        {
            var result = await Cloud.GetGlobalAssetInfo(hash).ConfigureAwait(false);
            return result.IsOK ? result.Entity?.Bytes ?? 0 : 0;
        }

        /// <summary>
        /// Gets the asset stream from CloudX.
        /// </summary>
        /// <param name="hash">The file hash of the asset.</param>
        /// <returns>The asset as a readable stream from CloudX.</returns>
        /// <exception cref="CloudXAssetResponseErrorException"></exception>
        public virtual async Task<Stream> GetAssetStream(string hash)
        {
            var response = await WebClient.GetAsync(GetAssetUri(hash)).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new CloudXAssetResponseErrorException(hash, response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return stream;
        }

        /// <summary>
        /// Gets the CloudX Uri of the asset.
        /// </summary>
        /// <param name="hash">The file hash of the asset.</param>
        /// <returns></returns>
        private Uri GetAssetUri(string hash) => CloudXInterface.NeosDBToHttp(new Uri($"{DB_PREFIX}{hash}"), NeosDB_Endpoint.Default);

        /// <summary>
        /// Gets the asset metadata of the asset from CloudX.
        /// </summary>
        /// <param name="hash">The file hash of the asset.</param>
        /// <returns>The mime type and the size of the asset as an AssetMetadata struct.</returns>
        public virtual async Task<AssetMetadata> GetAssetMetadata(string hash)
        {
            var assetUri = GetAssetUri(hash);
            var sizeTask = GetAssetSize(hash);
            var mimeTask = GetAssetMime(hash);
            await Task.WhenAll(sizeTask, mimeTask).ConfigureAwait(false);

            return new AssetMetadata(assetUri, mimeTask.Result, sizeTask.Result);
        }

        public Task Cancel()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the mime type of the asset.
        /// </summary>
        /// <param name="hash">The file hash associated with this asset.</param>
        /// <returns>The mime type string from the available mime endpoint or from the header Content-Type in an asset blob call.</returns>
        private async Task<string> GetAssetMime(string hash)
        {
            var mimeTask = await Cloud.GetAssetMime(hash);

            if (mimeTask.IsOK) { return mimeTask.Content?.Replace("\"", string.Empty); }

            // We should be able to get it at the endpoint above, but if the endpoint fails,
            // then we will call the asset endpoint and get the Content-Type header. (Worst case)

            var assetResponseTask = WebClient.GetAsync(GetAssetUri(hash));

            var assetContentResponse = assetResponseTask.Result.Content;
            var headers = assetContentResponse.Headers;
            return headers.Contains("Content-Type") ? headers.GetValues("Content-Type").FirstOrDefault() : string.Empty;
        }
    }
}
