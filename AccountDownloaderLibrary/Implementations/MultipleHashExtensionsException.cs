namespace AccountDownloaderLibrary.Implementations;

[Serializable]
public class MultipleHashExtensionsException : Exception
{
    public MultipleHashExtensionsException(string hash) : base($"Multiple extensions found for hash '{hash}'.") { }
}
