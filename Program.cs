using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DTInstaller.Utils;
using DTInstaller.Commands;
using static DTInstaller.Utils.Logger;
using static DTInstaller.Utils.Constants;

namespace DTInstaller
{

    static class Program
    {
        private static readonly Dictionary<string, ICommand> _commands = new()
        {
            [UpdateInstaller.CommandDetails.alias] = new UpdateInstaller(),
            [ScriptReinstaller.CommandDetails.alias] = new ScriptReinstaller(),
        };

        private static void LogAvailableCommands()
        {
            Log(LogVariant.Information, "Available list of commands:");
            foreach (var command in _commands.Values)
            {
                var (name, purpose, alias) = command.CommandDetailsInstance;
                LogDefault($"{name} -> {alias} : {purpose}");
            }

            LogDefault("Quit -> q : quit the installer");
        }

        static async Task Main()
        {
            if (OS == OperatingSystems.Windows) ConsoleQuickEdit.Disable();

            LogAvailableCommands();

            Log(LogVariant.Information, Texts.updatesCheckMessage);
            var (isNewUpdateAvailable, didError) = await UpdatesManager.CheckForUpdates();
            if (didError)
            {
                PromptToClose();
                return;
            }

            if (isNewUpdateAvailable)
                Log(LogVariant.Information, Texts.newUpdatesMessage);
            else
                Log(LogVariant.Success, Texts.upToDateMessage);


            await InitConsoleCommandsListener();
        }


        /**
         * <summary>
         * Creates an infinite loop that reads commands from the user until a quit command is executed.
         * </summary>
         */
        private static async Task InitConsoleCommandsListener()
        {
            string command;
            while (true)
            {
                command = Console.ReadLine();
                if (command == "q") return;

                ICommand ReceivedCommand;
                bool doesCommandExist = _commands.TryGetValue(command, out ReceivedCommand);

                if (!doesCommandExist)
                {
                    Log(LogVariant.Warning, Texts.invalidCommandMessage);
                    continue;
                }

                bool hasCommandExecuted = await ReceivedCommand.Execute();
                if (!hasCommandExecuted)
                    Log(LogVariant.Error, Texts.errorExecutingCommandMessage);
            }
        }
    }
}
