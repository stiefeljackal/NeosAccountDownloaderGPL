using System.Net;

namespace AccountDownloaderLibrary.NeosSearch.Exceptions;

public class UnexpectedCloudRecordSearchErrorException : Exception
{
    public UnexpectedCloudRecordSearchErrorException(string message, HttpStatusCode code) : this(message, code, null) { }

    public UnexpectedCloudRecordSearchErrorException(string message, HttpStatusCode code, Exception innerException) : base($"An unexpected error was encountered when contacting the Cloud: ({code}) {message}", innerException) { }
}
