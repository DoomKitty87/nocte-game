namespace Console.Commands
{

  public class SeedCommand : ConsoleCommand
  {

    private string _currentWrongMsg = "";
    private string _currentSuccessMessage = "";

    public override string Command => "seed";

    public override string WrongInputMessage => _currentWrongMsg;

    public override string SuccessMessage => _currentSuccessMessage;

    public override bool Process(string[] args) {
      _currentSuccessMessage = "Current seed is " + WorldGenInfo._seed;
      return true;
    }
  }
}