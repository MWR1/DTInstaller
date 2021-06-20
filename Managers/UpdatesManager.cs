using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Constants;

namespace DTInstaller.Utils
{
    static class UpdatesManager
    {
        private static readonly HttpClient _client = new();
        // We use a nullable bool so we can differentiate between unset and error (null/false).
        private static bool? _isNewUpdateAvailable = null;
        public static JsonScriptData FetchedScriptData { get; private set; } = null;

        /**
         * <summary>Checks for updates by pulling data from the repository.</summary>
         * <param name="force">
         * Forces to ping the GitHub servers for a new update, rather than getting the update availability
         * from an internal field. In other words, it "invalidates the cache".
         * </param>
         * <returns>A tuple that holds the update availability, and error presences.</returns>
         */
        public static async Task<(bool isNewUpdateAvailable, bool didError)> CheckForUpdates(bool force = false)
        {
            if (_isNewUpdateAvailable != null && !force)
                return ((bool)_isNewUpdateAvailable, didError: false);

            FetchedScriptData = await GetRepositoryScript();
            if (FetchedScriptData == null) return (isNewUpdateAvailable: false, didError: true);

            JsonScriptData localScriptData = LocalScriptDataManager.GetLocalScriptData();

            // If true, it might be that it's the first time they get the installer,
            // or they removed the local script directory, or the file inside.
            if (localScriptData == null)
            {
                DebugLog(LogVariant.Information,
                    $"({nameof(CheckForUpdates)}) Local data directory is not defined. Creating...");

                bool didCreateScriptDataPlace = LocalScriptDataManager.CreateLocalScriptData(fileContents: FetchedScriptData);
                if (!didCreateScriptDataPlace)
                    return (isNewUpdateAvailable: true, didError: true);

                // Assuming it's the first time they get the installer, say there's an update.
                _isNewUpdateAvailable = true;

                return (isNewUpdateAvailable: true, didError: false);
            }
                
            _isNewUpdateAvailable = localScriptData.sha != FetchedScriptData.sha;
            
            return ((bool)_isNewUpdateAvailable, didError: false);
        }

        /**
         * <summary>
         * Gets the script from the local script data file. If said file doesn't exist,
         * it gets the script data from the repository, and creates a new script data file.
         * </summary>
         * <returns>The script data object.</returns>
         */
        public static async Task<JsonScriptData> GetScript()
        {
            JsonScriptData localScriptData = LocalScriptDataManager.GetLocalScriptData();
            if (localScriptData == null)
            {
                DebugLog(LogVariant.Information, $"({nameof(GetScript)}) Local data directory is not defined. Pulling script from repo...");
                FetchedScriptData = await GetRepositoryScript();
                if (FetchedScriptData == null) return null;

                DebugLog(LogVariant.Information, $"({nameof(GetScript)}) Creating local script data place...");
                bool didCreateScriptDataPlace = LocalScriptDataManager.CreateLocalScriptData(fileContents: FetchedScriptData);
                if (!didCreateScriptDataPlace) return null;

                return FetchedScriptData;
            }

            return localScriptData;
        }

        /**
         * <summary>Makes a request to the servers where the script is hosted, and gets it.</summary>
         * <returns>The script data object.</returns>
         */
        public static async Task<JsonScriptData> GetRepositoryScript()
        {
            try
            {
                HttpRequestMessage requestMessage = new(HttpMethod.Get, URLs.scriptFileURL);
                
                // GitHub (the current host) requires an "User-Agent" header.
                requestMessage.Headers.Add("User-Agent", Miscellaneous.installerHeaderRequest);

                var response = await _client.SendAsync(requestMessage);
                FetchedScriptData = JsonSerializer.Deserialize<JsonScriptData>(await response.Content.ReadAsStringAsync());

                return FetchedScriptData;
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"There's been an error fetching the script data: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return null;
            }
        }
    }
}
