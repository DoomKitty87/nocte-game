using System;
using UnityEngine;

namespace Console.Commands
{
    public class _TimeScaleCommand : ConsoleCommand
    {
        private const string _wrongAmountMsg = "This command takes one input.";
        
        private string _currentWrongMsg = "";
        private string _currentSuccessMessage = "";
        
        public override string Command => "timescale";

        public override string WrongInputMessage => _currentWrongMsg;

        public override string SuccessMessage => _currentSuccessMessage;

        public override bool Process(string[] args) {
            if (args.Length == 0) {
                _currentSuccessMessage = BuildCallbackMessage();
                return true;
            }
            if (args.Length != 1) {
                _currentWrongMsg = _wrongAmountMsg;
                return false;
            }

            string sceneArg = args[0];

            if (!TryType(sceneArg)) {
                _currentWrongMsg = BuildWrongType(sceneArg);
                return false;
            }

            float value = float.Parse(sceneArg);

            if (TryValue(value)) {
                _currentSuccessMessage = BuildSuccessMessage(value);
                UnityEngine.Time.timeScale = value;
                return true;
            }
            else {
                _currentWrongMsg = BuildWrongValue(value);
                return false;
            }
            
            
        }

        private bool TryType(string input) {
            if (float.TryParse(input, out float value)) 
                return true;

            return false;
        }

        private bool TryValue(float value) {
            if (value >= 0) 
                return true;
            else
                return false;
        }

        private string BuildWrongType(string arg) =>
            $"Input must be a float.";


        private string BuildWrongValue(float arg) =>
            $"Input must be equal or greater then zero.";

        private string BuildCallbackMessage() =>
            $"timescale -> {Time.timeScale}";
        
        private string BuildSuccessMessage(float arg) =>
            $"Time.timeScale = {arg}";
    }
}