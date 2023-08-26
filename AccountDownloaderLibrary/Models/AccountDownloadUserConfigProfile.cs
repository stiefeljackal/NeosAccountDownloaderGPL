using AccountDownloaderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Models;

public class AccountDownloadUserConfig : IAccountDownloadUserConfig
{
    [JsonPropertyName("version")]
    public short Version { get; set; }

    [JsonPropertyName("migrateUserMetadata")]
    public bool UserMetadata { get; set; }

    [JsonPropertyName("migrateContacts")]
    public bool Contacts { get; set; }

    [JsonPropertyName("migrateMessageHistory")]
    public bool MessageHistory { get; set; }

    [JsonPropertyName("migrateInventoryAndWorlds")]
    public bool InventoryWorlds { get; set; }

    [JsonPropertyName("migrateCloudVarDefinitions")]
    public bool CloudVariableDefinitions { get; set; }

    [JsonPropertyName("migrateCloudVarValues")]
    public bool CloudVariableValues { get; set; }

    [JsonPropertyName("groupsToMigrate")]
    public IEnumerable<string> Groups { get; set; } = new List<string>();

    [JsonPropertyName("migrateFilePath")]
    public string FilePath { get; set; }
}
