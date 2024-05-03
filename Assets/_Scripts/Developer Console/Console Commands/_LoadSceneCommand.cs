using System;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class _LoadSceneCommand : ConsoleCommand
    {
        private const string _wrongAmountMsg = "This command takes one input.";
        private const string _notFoundMsg = "No scene with that input found.";
        private string _currentWrongMessage = "";

        private string _currentSuccessMessage = "";
        
        public override string Command => "loadscene";

        public override string WrongInputMessage => _currentWrongMessage;

        public override string SuccessMessage => _currentSuccessMessage;

        public override bool Process(string[] args) {
            if (args.Length != 1) {
                _currentWrongMessage = _wrongAmountMsg;
                return false;
            }

            string sceneArg = args[0];

            if (TryLoadByIndex(sceneArg)) {
                _currentSuccessMessage = BuildSuccessMessage(sceneArg);
                return true;
            }

            if (TryLoadByName(sceneArg)) {
                _currentSuccessMessage = BuildSuccessMessage(sceneArg);
                return true;
            }

            _currentWrongMessage = _notFoundMsg;
            return false;
        }
        
        private bool TryLoadByIndex(string sceneArg) {
            if (Int32.TryParse(sceneArg, out int index)
                && SceneManager.GetSceneByBuildIndex(index) != null) {
                SceneManager.LoadScene(index);
                return true;
            }

            return false;
        }

        private bool TryLoadByName(string sceneArg) {
            if (SceneManager.GetSceneByName(sceneArg) != null) {
                SceneManager.LoadScene(sceneArg);
                return true;
            }

            return false;
        }

        string BuildSuccessMessage(string sceneName) =>
            $"scene {sceneName} loaded.";
    }
}