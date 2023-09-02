using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Models
{
    public class RecordsReceivedEventArgs : EventArgs
    {
        public readonly string OwnerId;

        public readonly int CurrentSize;

        public readonly int RetrievedSize;

        public RecordsReceivedEventArgs(string ownerId, int currentSize, int retrievedSize)
        {
            OwnerId = ownerId;
            CurrentSize = currentSize;
            RetrievedSize = retrievedSize;
        }
    }
}
