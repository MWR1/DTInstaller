using System;
using System.IO;

namespace DTInstaller.Utils
{
    class Constants
    {
        public enum OperatingSystems
        {
            Windows = 1,
            Linux = 2,
            Mac = 3,
            Other = 4
        }

        public static OperatingSystems OS =
            OperatingSystem.IsWindows() ? OperatingSystems.Windows :
            OperatingSystem.IsLinux() ? OperatingSystems.Linux :
            OperatingSystem.IsMacOS() ? OperatingSystems.Mac :
            OperatingSystems.Other;

        public static class Paths
        {

            // The path where the "Discord" or "DiscordCanary" or "DiscordPTB" directories are located.
            // They differ from Windows to Linux.
            public static readonly string clientInstallationDirectoryPath = Environment.GetFolderPath(
                OS == OperatingSystems.Windows ?
                  Environment.SpecialFolder.LocalApplicationData :
                  Environment.SpecialFolder.ApplicationData
            );

            // Basically the opposite of the path above. Mainly used for searching for the archive directory path.
            public static readonly string otherPossibleClientInstallationDirectoryPath = Environment.GetFolderPath(
                OS == OperatingSystems.Windows ?
                  Environment.SpecialFolder.ApplicationData :
                  Environment.SpecialFolder.LocalApplicationData
            );


            // The rest of the path to the file we inject the script into. This is attached to the
            // main path after the archive has been extracted.
            public static readonly string mainScreenFileWithinArchivePath =
                @$"{FileNames.unpackedDirectoryName}/app/{FileNames.fileNameToInjectInto}";

            // These are the paths where we keep the last downloaded script. We can use this to decide whether
            // there are new updates, or to reinstall the script without pulling it from the repository.
            public static string localScriptDataDirPath = Path.Combine(clientInstallationDirectoryPath, "DTInstaller");
            public static string localScriptDataFilePath = Path.Combine(localScriptDataDirPath, "scriptData.json");
        }

        public static class FileNames
        {
            public const string modulesDirectoryName = "modules";
            public const string archiveFileName = "core.asar";
            public const string unpackedDirectoryName = "__unpacked";
            public const string fileNameToInjectInto = "mainScreen.js";
            public const string archiveDirectoryName = "discord_desktop_core";
        }

        public static class URLs
        {
            #if !DEBUG
            public const string scriptFileURL = "https://api.github.com/repos/MWR1/discord-transparency/contents/main/startup-with-discord.js";
            #else
            public const string scriptFileURL = "http://localhost:6900";
            #endif
        }

        public static class Texts
        {

            public const string updatesCheckMessage = "Checking for updates...";
            public const string newUpdatesMessage = "There's a new update available! Type 'i' to install";
            public const string upToDateMessage = "You're up to date!";
            public const string noUpdatesMessage = "There are no updates yet.";

            public const string installingMessage = "Installing...";
            public const string installationSuccessMessage = "The script has been installed successfully.";

            public const string reinstallingMessage = "Reinstalling...";
            public const string reinstallationSuccessMessage = "The script has been reinstalled successfully.";

            public const string scriptFetchingMessage = "Getting the script...";
            public const string injectingProcessMessage = "Injecting the script...";
            public const string localScriptDataInvalidMessage = "The data inside the file needed to see if the script is up to date is invalid. Have you modified it? Overwriting...";

            public const string invalidCommandMessage = "That command does not exist.";
            public const string errorExecutingCommandMessage = "The command could not be executed.";

            public const string clientTypeQuestion = "What type of Discord client do you have?\nDefault (press 1) | PTB (press 2) | Canary (press 3)";
            public const string invalidClientType = "That's not a valid client type. Please try again.";

            public const string clientNotOpenWarn = "Could not find where Discord is located. Make sure your Discord app is open.";
            public const string clientNotClosedWarn = "Your Discord client is open. Please close it, and try again.";
            public const string abortedClientProcessKillWarn = "Discord has to be stopped in order for the installation to continue. Aborting.";

            public const string dontReopenClientMessage = "Don't reopen Discord during this process, otherwise it won't work.";
        }

        public static class Miscellaneous
        {
            // The depth at which the asar archive directory is normally found at. This is used for 
            // searching for the archive directory.
            public const short archiveDirectoryDepth = 4;
            public const string scriptPresenceDelimiter = "//---";
            public const string installerHeaderRequest = "dtinstaller-request";
            public static readonly string[] clientTypeVariants = { "Discord", "DiscordPTB", "DiscordCanary" };
        }

        public static class Durations
        {
            public const short timeoutForProcessExit = 5_000;
            public const short timeToWaitForFileUnlock = 10_000;
            public const short threadSleepTimeForInjection = 1_000;
        }

        public static class DirectoryNameFilters
        {
            public const string appVersion = "*app-*";
            public static readonly string archiveDirectory = $"*{FileNames.archiveDirectoryName}-*";
        }
    }
}