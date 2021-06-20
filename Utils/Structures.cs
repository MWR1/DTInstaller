using System.Threading.Tasks;

namespace DTInstaller.Utils
{
    /**
     * <summary>Interface for commands to follow.</summary>
     */
    public interface ICommand
    {
        /**
         * <summary>Provides details about each command.</summary>
         */
        public static (string name, string purpose, string alias) CommandDetails { get; }

        /**
         * <summary>Provides details about each command, accessible by instances.</summary>
         */
        public (string name, string purpose, string alias) CommandDetailsInstance { get; }
        
        /**
         * <summary>Executes the code associated with each command.</summary>
         * <returns>True if the execution of the command was successful, and false otherwise.</returns>
         */
        public Task<bool> Execute();
    }

    /**
     * <summary>Interface for things that work with text.</summary>
     */
    public interface ITextLoader
    {
        /**
         * <returns>The text read.</returns>
         */
        string ReadText();
        /**
         * <returns>True if the write operation was successful, and false otherwise.</returns>
         */
        bool WriteText(string text);
    }

    class JsonScriptData
    {
        // There's no real reason for these to be properties, other than the fact that for some reason, 
        // it won't set the data received from the repository to instances of this class. Why? I has no idea!

        #pragma warning disable IDE1006 // Naming Styles - shut up
        public string content { get; set; }
        public string sha { get; set; }
    }
}


