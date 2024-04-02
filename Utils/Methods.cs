using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Constants;

namespace DTInstaller.Utils
{
    static class Methods
    {
        /**
         * <summary>
         * Gets the Discord process path, and kills the process. If it can't kill the process
         * in the first try, it will create an infinite loop that will ask the user if they want to retry 
         * to kill the process, until they refuse to keep going, or until the process terminates.
         * </summary>
         * <param name="clientName">The name of the client.</param>
         * <returns>The client process path if found, and null otherwise.</returns>
         */
        public static string GetClientProcessPathAndKill(string clientName)
        {
            Process[] clientProcesses = Process.GetProcessesByName(clientName);
            if (clientProcesses.Length == 0) return null;


            // As an Electron app, Discord spawns multiple processes that all lead back to the main process. Therefore,
            // any child process will point to the main executable's path.
            string clientProcessPath = clientProcesses[0].MainModule.FileName;

            foreach (Process clientProcess in clientProcesses)
            {
                string currentProcessName = clientProcess.ProcessName;

                try
                {
                    clientProcess.Kill();
                    while (!clientProcess.WaitForExit(Durations.timeoutForProcessExit))
                    {
                        Log(LogVariant.Warning, $"Could not stop {currentProcessName}.");
                        Log(LogVariant.Question, $"Do you want to retry to close {currentProcessName}? (y/n)");

                        string answer = Console.ReadLine();
                        if (answer == "n") return null;

                        clientProcess.Kill();
                        Log(LogVariant.Information, $"Stopping {currentProcessName}...");
                    }
                }
                catch (Exception error)
                {
                    Log(LogVariant.Error,
                        $"Could not stop {currentProcessName ?? "<unknown process name>"}. Reason: " + error.Message);

                    return null;
                }
            }

            return clientProcessPath;
        }

        /**
         * <summary>Waits for a given file to unlock in an interval of time.</summary>
         * <param name="filePath">The path to the file that is locked.</param>
         * <param name="maxTimeToWait">The maximum time to wait, after which we give up.</param>
         * <returns>True if the file unlocked in the given window of time, and false otherwise.</returns>
         */
        public static bool WaitForFileToUnlock(string filePath, short maxTimeToWait = 10_000)
        {
            short fileUsageChecks = 0;
            short fileUsageCheckProtection = (short)(maxTimeToWait / Durations.threadSleepTimeForInjection);
            FileLoader targetFile = new(filePath);

            while (fileUsageChecks < fileUsageCheckProtection && targetFile.IsInUse())
            {
                fileUsageChecks++;
                Log(
                    LogVariant.Information,
                    $"{filePath} is in use. Checks performed so far: " +
                    $"{fileUsageChecks} out of {fileUsageCheckProtection}. Retrying."
                );
                Thread.Sleep(Durations.threadSleepTimeForInjection);
            }

            if (fileUsageChecks == fileUsageCheckProtection) return false;

            return true;
        }


        /**
         * <summary>
         * Gets the client type of Discord (Stable/PTB/Canary).
         * If the input the user gave is not valid, this method will keep asking for input until 
         * it is valid.
         * </summary> 
         * <returns>The client type.</returns>
         */
        public static string GetClientType()
        {
            Log(LogVariant.Question, Texts.clientTypeQuestion);
            string clientType = GetClientTypeFromUser();
            while (clientType == null)
            {
                Log(LogVariant.Warning, Texts.invalidClientType);
                clientType = GetClientTypeFromUser();
            }

            return clientType;
        }

        /**
         * <summary>Gets, and validates the user input for the client type.</summary>
         * <returns>The client type.</returns>
         */
        public static string GetClientTypeFromUser()
        {
            bool hasParsedSuccessfully = short.TryParse(Console.ReadLine(), out short buildTypeVariantIndex);
            bool isInputValid = hasParsedSuccessfully &&
                (buildTypeVariantIndex > 0 && buildTypeVariantIndex <= Miscellaneous.clientTypeVariants.Length);

            if (!isInputValid) return null;

            return OS == OperatingSystems.Windows ?
             Miscellaneous.clientTypeVariants[buildTypeVariantIndex - 1] :
             Miscellaneous.clientTypeVariants[buildTypeVariantIndex - 1].ToLower();
        }

        /**
        * <summary>
        * Rescursively searches for the directory where the source code archive is located,
        * by checking every directory starting from a given path.
        * </summary>
        * <param name="currentDirectoryPath">The directory path where the search is currently performed.</param>
        * <param name="depth">The depth at which the currentDirectory is located.</param>
        * <returns>The archive directory path if found, and null otherwise.</returns> 
        *
        */
        public static string SearchForArchiveDirectoryPath(string currentDirectoryPath, short depth = 0)
        {
            try
            {
                foreach (string directoryPath in Directory.EnumerateDirectories(currentDirectoryPath))
                {
                    if (directoryPath.EndsWith(FileNames.archiveDirectoryName))
                        return directoryPath;

                    if (depth >= Miscellaneous.archiveDirectoryDepth) return null;

                    DebugLog(LogVariant.Information, $"Searching {directoryPath}");

                    string archiveDirectoryPath = SearchForArchiveDirectoryPath(directoryPath, depth: (short)(depth + 1));
                    if (archiveDirectoryPath == null) continue; // Don't return because there might be other directories.

                    // Maybe the subsequent directory traversals can give the correct path, which is why we 
                    // check again.
                    if (archiveDirectoryPath.EndsWith(FileNames.archiveDirectoryName))
                        return archiveDirectoryPath;
                }
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"Error while trying to find the directory path automatically. Reason: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
            }

            return null;
        }
    }
}
