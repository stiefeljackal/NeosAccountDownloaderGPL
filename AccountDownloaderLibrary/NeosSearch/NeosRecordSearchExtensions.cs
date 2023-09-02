using AccountDownloaderLibrary.Models;
using CloudX.Shared;

namespace AccountDownloaderLibrary.NeosSearch;

public static class NeosRecordSearchExtensions
{
    public static SearchParameters Clone(this SearchParameters searchParameters) =>
        new SearchParameters
        {
            ByOwner = searchParameters.ByOwner,
            Count = searchParameters.Count,
            ExcludedTags = searchParameters.ExcludedTags,
            ExtraSignatures = searchParameters.ExtraSignatures,
            MaxDate = searchParameters.MaxDate,
            MinDate = searchParameters.MinDate,
            Offset = searchParameters.Offset,
            OnlyFeatured = searchParameters.OnlyFeatured,
            OptionalTags = searchParameters.OptionalTags,
            OwnerType = searchParameters.OwnerType,
            Private = searchParameters.Private,
            RecordType = searchParameters.RecordType,
            RequiredTags = searchParameters.RequiredTags,
            SortBy = searchParameters.SortBy,
            SortDirection = searchParameters.SortDirection,
            SubmittedTo = searchParameters.SubmittedTo
        };
}
