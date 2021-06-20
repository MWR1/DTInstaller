using System;
using System.IO;

using static DTInstaller.Utils.Logger;

namespace DTInstaller.Utils
{
    class FileLoader : ITextLoader
    {
        private readonly string _path;

        public FileLoader(string path)
        {
            _path = path;
        }

        public string ReadText()
        {
            try
            {
                if (!File.Exists(_path)) return null;

                return File.ReadAllText(_path);
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"There's been an error trying to read the text from a file: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return null;
            }
        }

        public bool WriteText(string text)
        {
            try
            {
                File.WriteAllText(_path, text);
                return true;
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"There's been an error trying to write text to a file: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return false;
            }

        }

        /**
         * <summary>Checks if a given file is in use.</summary>
         * <remarks>It's not entirely reliable but it's good enough for this usecase.</remarks>
         * <returns>True if the file is locked, and false otherwise.</returns>
         */
        public bool IsInUse()
        {
            try
            {
                using FileStream fileStream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.None);
            } catch (IOException) // There's no guarantee that IOExceptions always result from locked files, but it's good enough.
            {
                return true;
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"There's been an error trying to see if the file is open: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return false;
            }

            return false;
        }
    
        public string[] ReadDirectoryPaths(string filterByNameLike = "*")
        {
            try
            {
                return Directory.GetDirectories(_path, filterByNameLike, SearchOption.TopDirectoryOnly);
            }
            catch (Exception error)
            {
                Log(LogVariant.Error, $"There's been an error trying to read directory paths: {error.Message}");
                DebugLog(LogVariant.Error, error.ToString());
                return null;
            }
        }
    }
}
