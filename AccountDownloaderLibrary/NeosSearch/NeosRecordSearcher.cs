using System.Net;
using CloudX.Shared;

namespace AccountDownloaderLibrary.NeosSearch;

using AccountDownloaderLibrary.Interfaces;
using AccountDownloaderLibrary.Models;
using Exceptions;

public sealed class NeosRecordSearcher<R> : IRecordSearcher, IDisposable where R : class, IRecord, new()
{
    public const byte DEFAULT_BATCH_SIZE = 125;

    public const byte MAX_RETRY_COUNT = 10;

    public short BatchSize { get; set; } = DEFAULT_BATCH_SIZE;

    private CloudXInterface _cloud;

    public event EventHandler<RecordsReceivedEventArgs> SearchResultSizeUpdate;

    public NeosRecordSearcher(CloudXInterface cloud)
    {
        _cloud = cloud;
    }

    public async IAsyncEnumerable<string> PerformSearch(SearchParameters searchParameters)
    {
        CleanseSearchParameters(ref searchParameters);

        var lastRecordIdBatch = new HashSet<string>();
        var currentSearchCount = 0;
        var hasMoreResults = true;

        while (hasMoreResults)
        {
            CloudResult<SearchResults<R>> cloudResult = null;
            for (var retryCount = 1; retryCount <= MAX_RETRY_COUNT; retryCount++)
            {
                cloudResult = await _cloud.FindRecords<R>(searchParameters).ConfigureAwait(false);

                if (cloudResult.IsOK) { break; }
                else if ((cloudResult.State != HttpStatusCode.TooManyRequests && cloudResult.State != HttpStatusCode.InternalServerError))
                {
                    throw new UnexpectedCloudRecordSearchErrorException(cloudResult.Content, cloudResult.State);
                }
                else if (retryCount >= MAX_RETRY_COUNT)
                {
                    throw new NeosCloudBusyException();
                }
            }

            var resultRecords = cloudResult.Entity.Records;
            if (!resultRecords.Any()) { break; }

            var recentMinDate = resultRecords.OrderBy(r => r.LastModificationTime).Last().LastModificationTime.AddMicroseconds(1);

            if (searchParameters.MinDate == null || recentMinDate > searchParameters.MinDate)
            {
                searchParameters.MinDate = recentMinDate;
                searchParameters.Offset = 0;
            }
            else
            {
                searchParameters.Offset += searchParameters.Count;
            }

            lastRecordIdBatch = resultRecords.Select(r => r.RecordId).Except(lastRecordIdBatch).ToHashSet();
            currentSearchCount += lastRecordIdBatch.Count;
            OnSearchResultSizeUpdate(searchParameters.ByOwner, currentSearchCount, lastRecordIdBatch.Count);

            foreach (var recordId in lastRecordIdBatch)
            {
                yield return recordId;
            }

            hasMoreResults = cloudResult.Entity.HasMoreResults;
        }
    }

    private void CleanseSearchParameters(ref SearchParameters searchParameters)
    {
        searchParameters = searchParameters.Clone();
        searchParameters.SortBy = SearchSortParameter.LastUpdateDate;
        searchParameters.SortDirection = SearchSortDirection.Ascending;
        searchParameters.Count = BatchSize;
    }

    private void OnSearchResultSizeUpdate(string ownerId, int currentSize, int retrievedSize)
    {
        SearchResultSizeUpdate?.Invoke(this, new RecordsReceivedEventArgs(ownerId, currentSize, retrievedSize));
    }

    public void Dispose()
    {
        foreach(var @delegate in SearchResultSizeUpdate.GetInvocationList().Cast<EventHandler<RecordsReceivedEventArgs>>())
        {
            SearchResultSizeUpdate -= @delegate;
        }
    }
}
