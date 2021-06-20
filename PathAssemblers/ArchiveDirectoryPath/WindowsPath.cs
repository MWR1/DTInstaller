using System.IO;
using DTInstaller.Utils;

using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Methods;
using static DTInstaller.Utils.Constants;

namespace DTInstaller.PathAssemblers.ArchiveDirectoryPath
{
    static class WindowsPath
    {

        /**
         * <summary>
         * Gets the desktop core directory path on Windows, because the directory structures differs there.
         * </summary>
         * <param name="clientType">The type of Discord client (Stable/PTB/Canary).</param>
         * <returns>The desktop core directory path if found, and null otherwise.</returns>
         */
        public static string AssembleArchiveDirectoryPath(string clientType)
        {
            string clientProcessPath = GetClientProcessPathAndKillIt(clientType); 
            if (clientProcessPath == null) return null;

            // The file that launched the client process is found, for now, in the same directory structure
            // where the archive directory is found.
            string appDirectoryPath = Path.GetDirectoryName(clientProcessPath);
            string modulesDirectoryPath = Path.Combine(appDirectoryPath, FileNames.modulesDirectoryName);
            string desktopCoreSingleDirectoryPath =
                AssembleDesktopCoreSingleDirectoryPart(currentPath: modulesDirectoryPath);

            if (desktopCoreSingleDirectoryPath != null)
                return Path.Combine(
                    desktopCoreSingleDirectoryPath,
                    FileNames.archiveDirectoryName
                );

            // Search for the directory path, if the known directory path is no longer valid. This is a fallback.
            DebugLog(
                LogVariant.Warning,
                $"The known directory path on platform {OS} has changed. Searching automatically."
            );

            // Search from the location of the process, then from the known directory where
            // the client is installed, then from another possible directory.
            return
                SearchForArchiveDirectoryPath(currentDirectoryPath: appDirectoryPath) ??
                SearchForArchiveDirectoryPath(currentDirectoryPath: Paths.clientInstallationDirectoryPath) ??
                SearchForArchiveDirectoryPath(
                    currentDirectoryPath:
                    Paths.otherPossibleClientInstallationDirectoryPath
                );

        }

        /**
         * <summary>Gets the directory "dicord_desktop_core-[number]".</summary>
         * <param name="currentPath">The existing path to which to attach the desktop core directory name.</param>
         * <returns>The discord_desktop_core-[number].</returns>
         */
        private static string AssembleDesktopCoreSingleDirectoryPart(string currentPath)
        {
            FileLoader fileLoader = new(path: currentPath);

            string[] directories = fileLoader.ReadDirectoryPaths(filterByNameLike: Constants.DirectoryNameFilters.archiveDirectory);
            if (directories == null || directories.Length == 0) return null;

            // For now, the first directory is also the only directory.
            return directories[0];
        }
    }
}