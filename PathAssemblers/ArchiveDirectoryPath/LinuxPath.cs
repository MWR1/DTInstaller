using System.IO;

using DTInstaller.Utils;
using static DTInstaller.Utils.Methods;
using static DTInstaller.Utils.Constants;
using static DTInstaller.Utils.Logger;


namespace DTInstaller.PathAssemblers.ArchiveDirectoryPath
{
    static class LinuxPath
    {

        /**
         * <summary>Assembles the desktop core directory path on Linux.</summary> 
         * <param name="clientType">The client type for Discord (Stable, Canary, etc.)</param> 
         */
        public static string AssembleArchiveDirectoryPath(string clientType)
        {
            // We can get the app version directly here, because on Linux, the path we want is like 
            // [Discord_client_type]/[app_version].

            string clientDirectoryPath = Path.Combine(Paths.clientInstallationDirectoryPath, clientType);
            string appVersionDirectoryPath = GetAppVersionDirectoryPath(clientDirectoryPath);
            if (appVersionDirectoryPath == null) return null;

            string archiveDirectoryPath = Path.Combine(
                clientDirectoryPath,
                appVersionDirectoryPath,
                FileNames.modulesDirectoryName,
                FileNames.archiveDirectoryName
            );

            if (Directory.Exists(archiveDirectoryPath)) return archiveDirectoryPath;

            return
                SearchForArchiveDirectoryPath(currentDirectoryPath: appVersionDirectoryPath) ??
                SearchForArchiveDirectoryPath(currentDirectoryPath: Paths.clientInstallationDirectoryPath) ??
                SearchForArchiveDirectoryPath(
                    currentDirectoryPath:
                    Paths.otherPossibleClientInstallationDirectoryPath
                );
        }

        /**
          * <param name="clientDirectoryPath">The path to the directory where the client is found.</param>
          * <returns>The path to the app version directory if found, and null otherwise.</returns>
         */
        private static string GetAppVersionDirectoryPath(string clientDirectoryPath)
        {
            FileLoader fileLoader = new(path: clientDirectoryPath);
            string[] directoryPaths = fileLoader.ReadDirectoryPaths();

            if (directoryPaths == null) return null;

            // Get the first directory. Normally, directories are sorted alphabetically. And for now,
            // the first directory represents an app version (e.g. 0.0.4), which is the one we want.
            foreach (string directoryPath in directoryPaths)
            {
                // Check if this is indeed the directory we need, by checking if it has a "modules" directory.
                string modulesDirectoryPath = Path.Combine(directoryPath, FileNames.modulesDirectoryName);
                if (Directory.Exists(modulesDirectoryPath))
                    return directoryPath;
            }

            Log(LogVariant.Error, "Could not find the app version directory path.");
            return null;
        }
    }
}
