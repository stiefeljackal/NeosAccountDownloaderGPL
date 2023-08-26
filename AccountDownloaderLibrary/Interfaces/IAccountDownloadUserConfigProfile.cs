using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountDownloaderLibrary.Interfaces;

public interface IAccountDownloadUserConfigProfile
{
    [JsonPropertyName("version")]
    public short Version { get; set; }

    [JsonPropertyName("migrateUserMetadata")]
    public bool UserMetadata { get; }

    [JsonPropertyName("migrateContacts")]
    public bool Contacts { get; }

    [JsonPropertyName("migrateMessageHistory")]
    public bool MessageHistory { get; }

    [JsonPropertyName("migrateInventoryAndWorlds")]
    public bool InventoryWorlds { get; }

    [JsonPropertyName("migrateCloudVarDefinitions")]
    public bool CloudVariableDefinitions { get; }

    [JsonPropertyName("migrateCloudVarValues")]
    public bool CloudVariableValues { get; }

    [JsonPropertyName("groupsToMigrate")]
    public IEnumerable<string> Groups { get; }

    [JsonPropertyName("migrateFilePath")]
    public string FilePath { get; }
}
