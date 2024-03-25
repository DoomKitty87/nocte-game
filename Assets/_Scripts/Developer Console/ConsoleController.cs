using System;
using System.Collections.Generic;
using System.ComponentModel;
using Console.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Console
{
    public class ConsoleController : MonoBehaviour
    {
	    private InputHandler _input;

        [SerializeField] private GameObject _console;

        private ConsoleUI _consoleUI;

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
            BackgroundInfo._enableCheats = _enableCheats;
            
            SetupConsoleComponents();
        }

        private void OnEnable() {
            _exitConsole += CloseConsole;
        }

        private void Start() {
            _input = InputHandler.Instance;

            _input.PLAYER_consoleAction.started += SwapConsoleState;
            _input.DRIVING_consoleAction.started += SwapConsoleState;

            _input.PLAYER_noclipAction.started += TryNoclip;

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
            ConsoleUI consoleUI;
            try {
                consoleUI = _console.GetComponentInChildren<ConsoleUI>();
                consoleUI.Setup(GetCommands());
            }
            catch {
                Debug.LogWarning($"No consoleUI found on {this.name}.");
                this.enabled = false;
            }
        }

        private void ReadKeyInput() {
            if (_console.activeInHierarchy && _input.GENERAL_MoveVector.y > 0) 
                _consoleUI.GetPreviousMessage(1);
            
            if (_console.activeInHierarchy && _input.GENERAL_MoveVector.y < 0) 
                _consoleUI.GetPreviousMessage(-1);
        }

        private void TryNoclip(InputAction.CallbackContext context) {
            if (BackgroundInfo._enableCheats && !_console.activeInHierarchy) {
                _consoleUI.ApplyCommand("noclip toggle");
            }
        }
        
        private void SwapConsoleState(InputAction.CallbackContext context) {
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
            ConsoleOpened?.Invoke();
        }

        private void CloseConsole() {
            _console.SetActive(false);
            _consoleClosed?.Invoke();
            ConsoleClosed?.Invoke();
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