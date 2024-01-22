using System;
using System.Collections.Generic;
using Console.Commands;
using UnityEngine;

namespace Console
{
    public class ConsoleController : MonoBehaviour
    {
        [SerializeField] private GameObject _console;

        private ConsoleUI _consoleUI;

        public KeyCode _consoleKey = KeyCode.BackQuote;

        private static Action _consoleOpen;
        private static Action _consoleClosed;

        public delegate void OnConsoleOpen();
        public delegate void OnConsoleClose();
        
        public static event OnConsoleOpen ConsoleOpened;
        public static event OnConsoleClose ConsoleClosed;

        public bool _enableCheats;

        private static Action _exitConsole;
        public static void RaiseExitConsole() => _exitConsole?.Invoke();

        List<IConsoleCommand> GetCommands() {
            List<IConsoleCommand> commands = new List<IConsoleCommand> {
                new HelpCommand(),
                new QuitCommand(),
                new CloseCommand(),
                new ClearCommand(),
                new SeedCommand(),
                new ShowFPSCommand(),
                new CheatsCommand() // Comment out this script to disable player cheats
                // Commands here
            };

            List<IConsoleCommand> cheatCommands = new List<IConsoleCommand>() {
                new _ReloadCommand(),
                new _LoadSceneCommand(),
                new _TimeScaleCommand(),
                new _MaxUpdatesCommand(),
                new _NoclipCommand(),
                new _SetPlayerVarCommand()
                // Cheat commands here
            };
            
            if (BackgroundInfo._enableCheats) 
                commands.AddRange(cheatCommands);

            return commands;
        }

        private void Awake() {
            SetupConsoleComponents();
        }

        private void OnEnable() {
            _exitConsole += CloseConsole;
        }

        private void Start() {
            _consoleUI = _console.GetComponentInChildren<ConsoleUI>();
            
            CloseConsole();

            ConsoleUI._reloadCommands += SetupConsoleComponents;
            
            ConsoleOpened += ShowMouse;
            ConsoleClosed += HideMouse;
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

            if (_console.activeInHierarchy && Input.GetKeyDown(KeyCode.UpArrow)) 
                _consoleUI.GetPreviousMessage(1);
            
            if (_console.activeInHierarchy && Input.GetKeyDown(KeyCode.DownArrow)) 
                _consoleUI.GetPreviousMessage(-1);
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
            if (ConsoleClosed != null)
                ConsoleClosed();
        }

        private void ShowMouse() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HideMouse() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}