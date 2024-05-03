namespace Console.Commands
{
    public class CloseCommand : ConsoleCommand
    {
        public override string Command => "close";

        public override string WrongInputMessage => "This command doesn't take any inputs.";

        public override string SuccessMessage => "Closed console.";

        public override bool Process(string[] args) {
            if (args.Length > 0) return false;
            ConsoleController.RaiseExitConsole();
            return true;
        }
    }
}
