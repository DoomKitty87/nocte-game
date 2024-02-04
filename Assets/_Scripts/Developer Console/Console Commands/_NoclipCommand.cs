using System;
using UnityEngine;

namespace Console.Commands
{
    public class _NoclipCommand : ConsoleCommand
    {
        private const string _wrongAmountMsg = "This command takes one input.";
        private const string _wrongInputMsg = "Must be a value of either 1 or 0.";
        
        private string _currentWrongMsg = "";
        private string _currentSuccessMessage = "";
        
        public override string Command => "noclip";

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

            string arg = args[0];

            if (!TryType(arg)) {
                _currentWrongMsg = _wrongInputMsg;
                return false;
            }
            
            if (arg == "toggle") {
                ToggleNoclip();
                return true;
            } 
            if (TryValue(arg)) {
                int value = int.Parse(arg);
                if (value == 1) {
                    _currentSuccessMessage = BuildSuccessMessage("enabled");
                    PlayerController.Instance.State = PlayerController.PlayerStates.Noclip;
                }
                else {
                    _currentSuccessMessage = BuildSuccessMessage("disabled");
                    PlayerController.Instance.State = PlayerController.PlayerStates.Idle;
                }

                return true;
            }
            else {
                _currentWrongMsg = _wrongInputMsg;
                return false;
            }
        }

        private bool TryType(string input) {
            if (int.TryParse(input, out int value)) 
                return true;
            if (input == "toggle") 
                return true;
            return false;
        }

        private bool TryValue(string arg) {
            int value = int.Parse(arg);
            if (value is 0 or 1) 
                return true;
            else
                return false;
        }

        private void ToggleNoclip() {
            if (PlayerController.Instance.State == PlayerController.PlayerStates.Noclip) {
                PlayerController.Instance.State = PlayerController.PlayerStates.Idle;
                BuildSuccessMessage("disabled");
            }
            else {
                PlayerController.Instance.State = PlayerController.PlayerStates.Noclip;
                BuildSuccessMessage("enabled");
            }
        }
        
        private string BuildCallbackMessage() =>
            // TODO: Fix this
            $"noclip -> {(PlayerController.Instance.State == PlayerController.PlayerStates.Noclip ? 1 : 0)}";
        
        private string BuildSuccessMessage(string arg) =>
            $"{arg} noclip.";
        
    }
}