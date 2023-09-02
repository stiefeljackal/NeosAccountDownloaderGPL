using AccountDownloaderLibrary.Models;
using CloudX.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Interfaces
{
    public interface IRecordSearcher
    {
        short BatchSize { get; set; }

        event EventHandler<RecordsReceivedEventArgs> SearchResultSizeUpdate;

        IAsyncEnumerable<string> PerformSearch(SearchParameters searchParams);
    }
}
