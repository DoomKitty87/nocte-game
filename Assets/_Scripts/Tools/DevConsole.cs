using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DevConsole : MonoBehaviour
{

  [SerializeField] private GameObject _consoleWindow;
  [SerializeField] private TMP_InputField _commandInput;
  [SerializeField] private TextMeshProUGUI _commandHistory;

  [SerializeField] private WorldGenerator _worldGenerator;
  
  private void Update() {
    if (Input.GetKeyDown(KeyCode.Slash)) ToggleConsole();
  }

  private void ToggleConsole() {
    _consoleWindow.SetActive(!_consoleWindow.activeSelf);
  }

  private void SubmitCommand(string command) {
    _commandHistory.text += "\n" + command;
    string response = "";
    switch (command.Split(' ')[0]) {
      case "regenerate":
        _worldGenerator.Regenerate();
        response = "Regenerating terrain.";
        break;
      case default:
        response = "Unrecognized command.";
        break;
    }
    _commandHistory.text += "\n" + response;
  }
  
}