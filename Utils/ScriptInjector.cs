using System;
using System.IO;
using System.Text;

using AsarSharp;

using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Methods;
using static DTInstaller.Utils.Constants;

namespace DTInstaller
{
    class ScriptInjector
    {
        private readonly string _archiveFilePath;
        private readonly string _archiveOutputDirectoryPath;
        private readonly string _scriptCode;
        private readonly string _targetFilePath;

        public ScriptInjector(
            string targetFilePath,
            string archiveFilePath,
            string archiveOutputDirectoryPath,
            string scriptCode
        )
        {
            _targetFilePath = targetFilePath;
            _archiveFilePath = archiveFilePath;
            _archiveOutputDirectoryPath = archiveOutputDirectoryPath;
            _scriptCode = scriptCode;
        }

        /**
         * <returns>True if the injection has been successful, and false otherwise.</returns>
         */
        public bool Inject()
        {
            bool didFileUnlock = WaitForFileToUnlock(
                _archiveFilePath,
                maxTimeToWait: Durations.timeToWaitForFileUnlock
            );
            if (!didFileUnlock) return false;

            bool hasExtractedArchive = TryExtractArchive();
            if (!hasExtractedArchive) return false;

            bool hasWrittenScriptToTarget = WriteScriptToTarget();
            if (!hasWrittenScriptToTarget) return false;

            bool hasPackedArchive = TryPackArchive();
            if (!hasPackedArchive) return false;

            return true;
        }

        /**
         * <returns>True if the extraction has been successful, and false otherwise.</returns> 
         */
        private bool TryExtractArchive()
        {
            try
            {
                using AsarExtractor sourceCodeExtractor = new(
                    archiveFilePath: _archiveFilePath,
                    extractInto: _archiveOutputDirectoryPath
                    );
                sourceCodeExtractor.Extract();
                return true;
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"The following error occurred when trying to unpack the source code asar file: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return false;
            }
        }

        /**
         * <returns>True if the file has been written to without errors, and false otherwise.</returns>
         */
        private bool WriteScriptToTarget()
        {
            try
            {
                string modifiedSourceCode = null;
                bool didFileUnlock = WaitForFileToUnlock(_targetFilePath, maxTimeToWait: Durations.timeToWaitForFileUnlock);
                if (!didFileUnlock) return false;

                using (StreamReader targetFileStreamReader = new(_targetFilePath))
                {
                    // Our sourceCode array is going to follow this structure: [source_code, script_code].
                    // The script is delimited by Constants.scriptPresenceDelimiter, inside the source code
                    // file.

                    // If the script_code is not there, the Split method below is going to return just one
                    // element (which happens, usually, when it's the first time using this installer).
                    // So then, we resize the sourceCode array to make space for script_code as well.
                    string[] sourceCode = targetFileStreamReader.ReadToEnd().Split(Miscellaneous.scriptPresenceDelimiter);

                    if (sourceCode.Length < 2) Array.Resize(ref sourceCode, 2);

                    sourceCode[1] = CreateScriptCodeAdder();

                    modifiedSourceCode = string.Join(Miscellaneous.scriptPresenceDelimiter, sourceCode);
                }

                // We overwrite the entire file in the case that the modified code length is somehow smaller than 
                // the existing code in the file, so we don't have remnants.
                using StreamWriter targetFileStreamWriter = new(_targetFilePath, append: false);
                targetFileStreamWriter.Write(modifiedSourceCode);

                return true;
            }
            catch (IOException error)
            {
                Log(LogVariant.Error, $"The following IO related error occurred while trying to inject the script: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return false;
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"The following error occurred while trying to inject the script: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return false;
            }
        }


        /**
         * <returns>True if the archive has been archived, and false otherwise.</returns>
         */
        private bool TryPackArchive()
        {
            try
            {
                using AsarArchiver sourceCodeArchiver = new(_archiveOutputDirectoryPath, _archiveFilePath);
                sourceCodeArchiver.Archive();
                return true;
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"The following error occurred while trying to pack everything back together: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return false;
            }
        }

        /**
         * <summary>Creates the script code that has to be injected inside the target file.</summary>
         * <remarks>
         * We don't just simply inject the script code. We also inject some handler code that ensures
         * the script code is going to execute.
         * </remarks>
         * <returns>The script code to be injected.</returns>
         */
        private string CreateScriptCodeAdder()
        {
            // We don't add Constants.scriptPresenceDelimiter to this string, because string.Join does that
            // for us. We just add \n at the beginning so it won't all just be a comment lol.
            return
            // Declare the interval that checks if we can execute the script code.
            "\nconst __DT_INTERVAL__=setInterval(() => {" +
            // This will check for mainWindow, because the script code uses a mainWindow method to attach
            // itself to the load event of Discord. If we hadn't done this, we would've got a JS runtime error.
            "if(mainWindow) { clearInterval(__DT_INTERVAL__);" +
            // Append the actual script code, and close the brackets of the if statement and the setInterval,
            // and add the interval in which we should if mainWindow is defined
            // (by convention, every 500ms).
            _scriptCode + "} }, 500);";
        }

    }
}
