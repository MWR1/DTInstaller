using System;
using System.IO;
using System.Text.Json;
using DTInstaller.Utils;
using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Constants;

namespace DTInstaller
{
    static class LocalScriptDataManager
    {
        /**
         * <summary>Gets the script data from the local script data file, instead of the repository.</summary>
         * <returns>The script data object.</returns>
         */
        public static JsonScriptData GetLocalScriptData()
        {
            FileLoader fileLoader = new(path: Paths.localScriptDataFilePath);
            string localScriptData = fileLoader.ReadText();

            if (localScriptData == null) return null;

            try
            {
                return JsonSerializer.Deserialize<JsonScriptData>(localScriptData);
            }
            catch
            {
                Log(LogVariant.Warning, Texts.localScriptDataInvalidMessage);
                return null;
            }
        }

        /**
         * <summary>Updates the script data file in case a script change has occured.</summary>
         * <param name="scriptData">The script data to update the file with.</param>
         * <returns>True if the update has been successful, and false otherwise.</returns>
         */
        public static bool UpdateLocalScriptData(JsonScriptData scriptData)
        {
            return new FileLoader(path: Paths.localScriptDataFilePath)
                            .WriteText(JsonSerializer.Serialize(scriptData));
        }

        /**
         * <summary>
         * Creates the local script data directory, in which we also create the local script data file.
         * </summary>
         * <param name="fileContents">The script data object to be written to the local script data file.</param>
         * <returns>True if the creation of the directory and file was successful, and false otherwise.</returns>
         */
        public static bool CreateLocalScriptData(JsonScriptData fileContents)
        {
            if (!Directory.Exists(Paths.localScriptDataDirPath))
            {
                try
                {
                    Directory.CreateDirectory(Paths.localScriptDataDirPath);
                }
                catch (Exception error)
                {
                    Log(
                        LogVariant.Error,
                        $"There's been an error trying to create a directory for " +
                        $"holding data for this installer: {error.Message}"
                    );
                    DebugLog(LogVariant.Error, error.ToString());
                    return false;
                }
            }

            FileLoader fileLoader = new(path: Paths.localScriptDataFilePath);
            bool hasWritten = fileLoader.WriteText(JsonSerializer.Serialize(fileContents));

            return hasWritten;
        }
    }
}
