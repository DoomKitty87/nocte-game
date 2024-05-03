using System.Collections.Generic;
using System.Linq;

namespace Console
{
    public class ConsoleMain
    {
        public string Prefix { get; }
        public List<IConsoleCommand> Commands { get; }

        private const string _errorPrefix = "<color=\"red\">ERROR: </color>";
        private const string _notFoundMessage = "No such command found.";
        private const string _invalidCommandMessage = "Not a valid command syntax.";
        private const string _wrongInputFallbackMessage = "Wrong input args given.";
        private const string _successFallbackMessage = "Done.";

        public ConsoleMain(string prefix, IEnumerable<IConsoleCommand> commands) {
            this.Prefix = prefix;
            this.Commands = commands.ToList();
        }

        public string ProcessInput(string userInput) {
            var inputs = userInput.Split(' ');
            string commandInput = inputs[0];
            string[] args = inputs.Skip(1).ToArray();

            if (InputMatchesCommandPattern(commandInput)) {
                return TryExecuteUserCommand(commandInput, args);
            }
            else {
                return BuildErrorMessage(_invalidCommandMessage);
            }
        }

        private static bool InputMatchesCommandPattern(string userInput) => !string.IsNullOrWhiteSpace(userInput);

        private string TryExecuteUserCommand(string userCommandInput, string[] args) {
            IConsoleCommand targetCommand = FindMatchingCommand(userCommandInput);

            if (targetCommand != null)
                return CallCommand(targetCommand, args);
            else
                return BuildErrorMessage(_notFoundMessage);
        }

        private IConsoleCommand FindMatchingCommand(string userCommandInput) {
            foreach (IConsoleCommand consoleCommand in Commands) {
                if (userCommandInput.Equals(consoleCommand.Command, System.StringComparison.OrdinalIgnoreCase))
                    return consoleCommand;
            }

            return null;
        }

        private string CallCommand(IConsoleCommand command, string[] args) {
            if (command.Process(args)) {
                // Successful command execution
                if (command.SuccessMessage != null)
                    return command.SuccessMessage;
                else
                    return _successFallbackMessage;
            }
            else {
                // Correct command but wrong input args
                if (command.WrongInputMessage != null)
                    return BuildErrorMessage(command.WrongInputMessage);
                else
                    return BuildErrorMessage(_wrongInputFallbackMessage);
            }
        }

        private string BuildErrorMessage(string message) =>
            _errorPrefix + message;
    }
}
