using DTInstaller.PathAssemblers.ArchiveDirectoryPath;

using static DTInstaller.Utils.Methods;
using static DTInstaller.Utils.Constants;

namespace DTInstaller
{
    static class ArchiveDirectoryPathAssembler
    {
        /**
         * <summary>
         * Assembles the path to the directory where the archive file with the source code is located.
         * </summary>
         * <returns>The path to the archive's directory if found, and null otherwise.</returns>
         */
        public static string AssembleArchiveDirectoryPath()
        {
            string clientType = GetClientType();
            return OS switch
            {
                OperatingSystems.Windows => WindowsPath.AssembleArchiveDirectoryPath(clientType),
                OperatingSystems.Linux => LinuxPath.AssembleArchiveDirectoryPath(clientType),
                _ => null,
            };
        }
    }
}
