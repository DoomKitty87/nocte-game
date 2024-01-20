namespace Console.Commands
{

  public class MaxUpdatesCommand : ConsoleCommand
  {
    private const string _wrongAmountMsg = "This command takes one input.";
    private const string _wrongInputMsg = "Must be a positive float.";

    private string _currentWrongMsg = "";
    private string _currentSuccessMessage = "";

    public override string Command => "maxupdates";

    public override string WrongInputMessage => _currentWrongMsg;

    public override string SuccessMessage => _currentSuccessMessage;

    public override bool Process(string[] args) {
      if (args.Length != 1) {
                _currentWrongMsg = _wrongAmountMsg;
        return false;
      }           
      string arg = args[0];
      if (!TryType(arg)) {
        _currentWrongMsg = _wrongInputMsg;
        return false;
      }
      float value = float.Parse(arg);
      if (TryValue(value)) {
                _currentSuccessMessage = BuildSuccessMessage(value.ToString());
                WorldGenInfo._maxUpdatesPerFrame = value;
                return true;
            }
            else {
                _currentWrongMsg = _wrongInputMsg;
                return false;
            }
    }

    private bool TryType(string input) {
      if (float.TryParse(input, out float value)) 
          return true;

      return false;
    }

    private bool TryValue(float value) {
      if (value > 0) 
          return true;
      else
          return false;
    }

    private string BuildSuccessMessage(string arg) {
            return $"Max updates per frame set to {arg}.";
    }
  }
}