using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Implementations;

public class CloudXAssetResponseErrorException : Exception
{
    public CloudXAssetResponseErrorException(string hash, HttpStatusCode code)
        : base($"Unable to fetch asset '{hash}' due to an HTTP error on CloudX: {code}") { }
}
