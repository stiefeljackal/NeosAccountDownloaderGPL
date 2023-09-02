namespace AccountDownloaderLibrary.NeosSearch.Exceptions;

public class NeosCloudBusyException : Exception
{
    public NeosCloudBusyException() : this(null) { }
    public NeosCloudBusyException(Exception innerException) : base("The Neos Cloud is currently too busy. Please try again later.", innerException) { }
}
