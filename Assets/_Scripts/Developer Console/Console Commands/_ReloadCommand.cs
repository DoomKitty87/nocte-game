using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class _ReloadCommand : ConsoleCommand
    {
        public override string Command => "reload";

        public override string WrongInputMessage => "This command doesn't take any inputs.";

        public override string SuccessMessage => "Reloaded current scene.";

        public override bool Process(string[] args) {
            if (args.Length > 0) return false;
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
            return true;
        }
    }
}
