using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using DTInstaller.Utils;
using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Constants;

namespace DTInstaller.Commands
{
    class ScriptReinstaller : ICommand
    {
        public static (string name, string purpose, string alias) CommandDetails { get; } =
            ("Reinstall", "reinstall the script", "r");
        public (string name, string purpose, string alias) CommandDetailsInstance { get; } = CommandDetails;

        public async Task<bool> Execute()
        {
            Log(LogVariant.Information, Texts.reinstallingMessage);

            string archiveDirectoryPath = ArchiveDirectoryPathAssembler.AssembleArchiveDirectoryPath();
            if (archiveDirectoryPath == null) return false;

            string mainScreenFilePath = Path.Combine(archiveDirectoryPath, Paths.mainScreenFileWithinArchivePath);
            string sourceCodeCoreArchivePath = Path.Combine(archiveDirectoryPath, FileNames.archiveFileName);
            string archiveUnpackedDirectoryPath = Path.Combine(archiveDirectoryPath, FileNames.unpackedDirectoryName);

            Log(LogVariant.Information, Texts.scriptFetchingMessage);
            JsonScriptData scriptCode = await UpdatesManager.GetScript();
            if (scriptCode == null) return false;

            Log(LogVariant.Information, Texts.injectingProcessMessage);    
            Log(LogVariant.Warning, Texts.dontReopenClientMessage);
            ScriptInjector scriptInjector = new(
                targetFilePath: mainScreenFilePath,
                archiveFilePath: sourceCodeCoreArchivePath,
                archiveOutputDirectoryPath: archiveUnpackedDirectoryPath,
                scriptCode: Encoding.UTF8.GetString(Convert.FromBase64String(scriptCode.content))
            );

            bool hasInjected = scriptInjector.Inject();
            if (!hasInjected) return false;
            
            Log(LogVariant.Success, Texts.reinstallationSuccessMessage);
            return true;
        }
    }
}
