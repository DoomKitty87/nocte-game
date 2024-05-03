namespace Console.Commands
{
    public class QuitCommand : ConsoleCommand
    {
        public override string Command => "quit";

        public override string WrongInputMessage => "This command doesn't take any inputs.";

        public override string SuccessMessage => "quitting...";

        public override bool Process(string[] args) {
            if (args.Length > 0) return false;
            UnityEngine.Application.Quit();
            return true;
        }
    }
}
