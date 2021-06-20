using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using DTInstaller.Utils;
using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Methods;
using static DTInstaller.Utils.Constants;

namespace DTInstaller.Commands
{
    class UpdateInstaller : ICommand
    {
        public static (string name, string purpose, string alias) CommandDetails { get; } =
            ("Install", "install the update, if there's one", "i");
        public (string name, string purpose, string alias) CommandDetailsInstance { get; } = CommandDetails;

        public async Task<bool> Execute()
        {
            Log(LogVariant.Information, Texts.installingMessage);

            // We check again for an update because otherwise, the user could proceed to install an update
            // when there isn't one to begin with.
            var (isNewUpdateAvailable, didError) = await UpdatesManager.CheckForUpdates();
            if (didError) return false;

            if (!isNewUpdateAvailable)
            {
                Log(LogVariant.Information, Texts.noUpdatesMessage);
                return true;
            }

            bool didUpdateLocalScriptData = LocalScriptDataManager
                .UpdateLocalScriptData(UpdatesManager.FetchedScriptData);
            // While this may not stop the installation of the update itself, when the user wants to reinstall 
            // the script, they'll actually install an old update, so rather than doing that, just fail.
            if (!didUpdateLocalScriptData) return false;
 
            string archiveDirectoryPath = ArchiveDirectoryPathAssembler.AssembleArchiveDirectoryPath();
            if (archiveDirectoryPath == null) return false;

            string mainScreenFilePath = Path.Combine(archiveDirectoryPath, Paths.mainScreenFileWithinArchivePath);
            string sourceCodeCoreArchivePath = Path.Combine(archiveDirectoryPath, FileNames.archiveFileName);
            string archiveUnpackedDirectory = Path.Combine(archiveDirectoryPath, FileNames.unpackedDirectoryName);

            Log(LogVariant.Information, Texts.injectingProcessMessage);    
            Log(LogVariant.Warning, Texts.dontReopenClientMessage);    
            ScriptInjector scriptInjector = new(
                targetFilePath: mainScreenFilePath,
                archiveFilePath: sourceCodeCoreArchivePath,
                archiveOutputDirectoryPath: archiveUnpackedDirectory,
                scriptCode: Encoding.UTF8.GetString(Convert.FromBase64String(UpdatesManager.FetchedScriptData.content))
            );

            bool hasInjected = scriptInjector.Inject();
            if (!hasInjected) return false;

            Log(LogVariant.Success, Texts.installationSuccessMessage);
            return true;
        }

    }
}
