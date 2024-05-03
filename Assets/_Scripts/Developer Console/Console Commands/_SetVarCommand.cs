using System;
using System.Reflection;
using UnityEngine;

namespace Console.Commands
{
    public class _SetPlayerVarCommand : ConsoleCommand
    {
        private const string _wrongAmountMsg = "This command takes two inputs.";
        private string _currentWrongMessage = "";

        private string _currentSuccessMessage = "";
        public override string Command => "setvar";
        
        public override string WrongInputMessage => _currentWrongMessage;

        public override string SuccessMessage => _currentSuccessMessage;

        private Type _type;
        private FieldInfo _field;
        private object _parsedValue;
        
        public override bool Process(string[] args) {
            if (args.Length is not (0 or 1 or 2)) {
                _currentWrongMessage = _wrongAmountMsg;
                return false;
            }

            if (args.Length == 0) {
                _currentSuccessMessage = BuildVariableNameList();
                return true;
            }

            string varName = args[0];
            if (TryGetVariable(varName)) {
                if (args.Length == 1) {
                    _currentSuccessMessage = BuildVariableValueMessage(varName);
                    return true;
                }
            }
            else {
                _currentWrongMessage = BuildWrongVariableMessage(varName);
                return false;
            }

            string value = args[1];
            
            if (!TrySetValue(value)) {
                _currentWrongMessage = BuildWrongVarTypeMessage(varName, value);
                return false;
            }
            
            // Past all test cases
            _currentSuccessMessage = BuildSuccessMessage(varName, value);
            _field.SetValue(PlayerController.Instance, _parsedValue);
            return true;
        }
        
        private bool TryGetVariable(string varName) {
            _type = PlayerController.Instance.GetType();
            _field = _type.GetField(varName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (_field != null)
                return true;
            else
                return false;
        }

        private bool TrySetValue(string value) {
            try {
                _parsedValue = Convert.ChangeType(value, _field.FieldType);
                return true;
            }
            catch {
                return false;
            }
        }

        string BuildVariableNameList() {
            Type type = PlayerController.Instance.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            string _message = "";
            
            foreach (FieldInfo field in fields)
            {
                object value = field;
                _message += $"{value},\n";
            }
            
            _message = _message.Substring(0, _message.Length - 2); // Removes final comma and new line

            return _message;
        }
        
        string BuildWrongVariableMessage(string varName) =>
            $"No variable <color=red>{varName}</color> found.";
        
        string BuildVariableValueMessage(string varName) =>
            $"{varName} -> {_field.GetValue(PlayerController.Instance)}";

        string BuildWrongVarTypeMessage(string varName, string value) =>
            $"Variable {varName} can not be value {value}.";
        
        string BuildSuccessMessage(string varName, string value) =>
            $"{varName} = {value}";
    }
}