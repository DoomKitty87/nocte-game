using System;
using System.Collections.Generic;
using Console.Commands;
using UnityEngine;

namespace Console
{
    public class ConsoleController : MonoBehaviour
    {
        [SerializeField] private GameObject _console;

        public KeyCode _consoleKey = KeyCode.BackQuote;

        private static Action _consoleOpen;
        private static Action _consoleClosed;

        public delegate void OnConsoleOpen();
        public delegate void OnConsoleClose();
        
        public static event OnConsoleOpen ConsoleOpened;
        public static event OnConsoleClose ConsoleClosed;

        private static Action _exitConsole;
        public static void RaiseExitConsole() => _exitConsole?.Invoke();

        List<IConsoleCommand> GetCommands() {
            return new List<IConsoleCommand> {
                new HelpCommand(),
                new QuitCommand(),
                new CloseCommand(),
                new ClearCommand(),
                new ReloadCommand(),
                new LoadSceneCommand(),
                new TimeScaleCommand()
                // Commands here
            };
        }

        private void Awake() {
            SetupConsoleComponents();
        }

        private void OnEnable() {
            _exitConsole += CloseConsole;
        }

        private void Start() {
            CloseConsole();
        }

        private void Update() {
            ReadKeyInput();
        }

        private void OnDisable() {
            _exitConsole -= CloseConsole;
        }

        private void SetupConsoleComponents() {
            var consoleUI = _console.GetComponentInChildren<ConsoleUI>();
            consoleUI.Setup(GetCommands());
        }

        private void ReadKeyInput() {
            if (Input.GetKeyDown(_consoleKey))
                SwapConsoleState();
        }
        
        private void SwapConsoleState() {
            if (_console == null) throw new NullReferenceException("No console object set in ConsoleController.");
            
            bool isOpen = _console.activeInHierarchy;
            if (isOpen)
                CloseConsole();
            else
                OpenConsole();
        }

        private void OpenConsole() {
            _console.SetActive(true);
            _consoleOpen?.Invoke();
            if (ConsoleOpened != null)
                ConsoleOpened();
        }

        private void CloseConsole() {
            _console.SetActive(false);
            _consoleClosed?.Invoke();
            if (ConsoleOpened != null)
                ConsoleClosed();
        }

        public void Action() {
             
        }
    }
}