using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Console
{
    public class ConsoleUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _logArea;
        [SerializeField] private TMP_InputField _inputArea;
        [SerializeField] private string _prefix = "";

        public static Action _consoleClear;
        public static Action _consoleHelp;

        public static Action _reloadCommands;

        private ConsoleMain _console;

        private ConsoleScroller _scroller;

        private bool _isClosing;

        private bool _showStartUp = true;

        private void Awake() {
            _scroller = GetComponentInChildren<ConsoleScroller>();
        }

        private void OnEnable() {
            _isClosing = false;
            if (_showStartUp) {
                ClearLog();
                _showStartUp = false;
            }

            ClearInput();
            SubEvents();
            SetUpInputField();
        }

        private void OnDisable() {
            _isClosing = true;
            UnsubEvents();
        }

        public void Setup(IEnumerable<IConsoleCommand> commands) => 
            _console = new ConsoleMain(_prefix, commands);

        private void SubEvents() {
            _inputArea.onSubmit.AddListener(ProcessInput);
            _consoleClear += ClearLog;
            _consoleHelp += PrintConsoleHelp;
        }

        private void UnsubEvents() {
            _inputArea.onSubmit.RemoveAllListeners();
            _consoleClear -= ClearLog;
            _consoleHelp -= PrintConsoleHelp;
        }

        private void SetUpInputField() {
            _inputArea.caretWidth = 10;
            StartCoroutine(SelectInputField());
        }

        private void ProcessInput(string userInput) {
            LogText(GetFormattedUserInput(userInput));

            bool cheats = BackgroundInfo._enableCheats;
            
            string processedMessage = _console.ProcessInput(userInput);
            LogText(processedMessage);

            if (cheats != BackgroundInfo._enableCheats) 
                _reloadCommands?.Invoke();

            ResetInputField();
        }

        private void LogText(string message) {
            _logArea.text += message + "<br>";
            _scroller.MoveDown();
        }

        private void ClearLog() {
            _logArea.text = "";
            PrintInitialInfo();
        }

        private void ResetInputField() {
            ClearInput();

            if (!_isClosing) 
                StartCoroutine(SelectInputField());
        }
        
        private void ClearInput() =>
            _inputArea.text = "";

        private void PrintInitialInfo() {
            LogText($"Type <color=green>help</color> to get a list of commands.");
            LogText($"Type <color=red>close</color> to close developer menu.");
            // Put start code here
        }

        private void PrintConsoleHelp() {
            var builder = new StringBuilder();
            int counter = 0;
            foreach (var comm in _console.Commands) {
                counter++;
                string commandWord = comm.Command;
                if (counter % 3 == 0) {
                    builder.Append(commandWord + "<br>");
                }
                else {
                    int fillingSpaceAmount = 15 - commandWord.Length;
                    builder.Append(commandWord + GetFillingSpace(fillingSpaceAmount));
                }
            }
            LogText(builder.ToString().TrimEnd(' '));
        }

        private string GetFillingSpace(int count) {
            StringBuilder builder = new();
            for (int i = 0; i < count; i++) {
                builder.Append(' ');
            }

            return builder.ToString();
        }

        private IEnumerator SelectInputField() {
            yield return new WaitForEndOfFrame();
            _inputArea.ActivateInputField();
            _inputArea.Select();
        }

        string GetFormattedUserInput(string userInput) =>
            $"<color=#808080>{_console.Prefix}</color> <color=white>{userInput}</color>";
    }
}
