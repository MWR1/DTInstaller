using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace DTInstaller.Utils
{
    static class Logger
    {
        private readonly static Dictionary<
            LogVariant, 
            (ConsoleColor backgroundColor, ConsoleColor textColor, string logTypeTitle)
       > logVariants = new()
        {
            [LogVariant.Error] = (ConsoleColor.Black, ConsoleColor.Red, "Error"),
            [LogVariant.Warning] = (ConsoleColor.Black, ConsoleColor.Yellow, "Warning"),
            [LogVariant.Success] = (ConsoleColor.Black, ConsoleColor.Green, "Success"),
            [LogVariant.Information] = (ConsoleColor.Black, ConsoleColor.Cyan, "Info"),
            [LogVariant.Question] = (ConsoleColor.Black, ConsoleColor.White, "Question")
        };

        public enum LogVariant
        {
            Error = 1,
            Warning = 2,
            Success = 3,
            Information = 4,
            Question = 5
        }

        /**
         * <summary>Logs given text, based on a given variant.</summary>
         */
        public static void Log(LogVariant logVariant, string text)
        {
            (ConsoleColor backgroundColor, ConsoleColor textColor, string logTypeTitle) colorPreferences;
            bool gotValueSucessfully = logVariants.TryGetValue(logVariant, out colorPreferences);
            if (!gotValueSucessfully)
                colorPreferences = logVariants.GetValueOrDefault(LogVariant.Information);

            Console.BackgroundColor = colorPreferences.backgroundColor;
            Console.ForegroundColor = colorPreferences.textColor;
            Console.WriteLine($"[{colorPreferences.logTypeTitle}]: {text}");

            Console.ResetColor();
        }

        /**
         * <summary>
         * Is used for additional information, when debugging. This method disappears
         * when the project gets built for release.
         * </summary>
         */
        [Conditional("DEBUG")]
        public static void DebugLog(LogVariant logVariant, string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[DEBUG] ");
            Log(logVariant, text); 
        }

        public static void LogDefault(string text) => Console.WriteLine(text);
        
        public static void PromptToClose()
        {
            LogDefault("Press any key to close...");
            Console.ReadKey();
        }
    }
}
