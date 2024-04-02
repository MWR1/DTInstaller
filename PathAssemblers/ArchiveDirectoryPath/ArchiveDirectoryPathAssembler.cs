using DTInstaller.PathAssemblers.ArchiveDirectoryPath;

using static DTInstaller.Utils.Methods;
using static DTInstaller.Utils.Constants;
using static DTInstaller.Utils.Logger;
using System.Diagnostics;

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
            switch(OS)
            {
                case OperatingSystems.Windows: 
                    return WindowsPath.AssembleArchiveDirectoryPath(clientType);
                case OperatingSystems.Linux:
                    return LinuxPath.AssembleArchiveDirectoryPath(clientType);
                default:
                    Log(LogVariant.Error, "The OS could not be identified.");
                    return null;
            }
        }
    }
}
