using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class DevConsole : MonoBehaviour
{

  [SerializeField] private GameObject _consoleWindow;
  [SerializeField] private TMP_InputField _commandInput;
  [SerializeField] private TextMeshProUGUI _commandHistory;

  [SerializeField] private WorldGenerator _worldGenerator;
  [SerializeField] private GameObject _fpsDisplay;
  
  private void Update() {
    if (Input.GetKeyDown(KeyCode.Slash)) ToggleConsole();
  }

  private void ToggleConsole() {
    _consoleWindow.SetActive(!_consoleWindow.activeSelf);
    if (_consoleWindow.activeSelf) {
      _commandInput.ActivateInputField();
    }
    else {
      _commandInput.DeactivateInputField();
    }
  }

  private void Awake() {
    _consoleWindow.SetActive(false);
    _commandInput.DeactivateInputField();
    _commandInput.onEndEdit.AddListener(delegate { SubmitCommand(_commandInput.text); });
  }

  public void SubmitCommand(string command) {
    _commandHistory.text += "\n" + command;
    string value = "";
    if (command.Split(' ').Length > 1) value = command.Split(' ')[1];
    string response = "";
    switch (command.Split(' ')[0]) {
      case "regenerate":
        _worldGenerator.Regenerate();
        response = "Regenerating terrain";
        break;
      case "scene":
        if (value == null) {
          response = "Unrecognized command";
          break;
        }
        SceneManager.LoadScene(value);
        response = "Loaded scene " + value;
        break;
      case "quit":
        Application.Quit();
        break;
      case "timescale":
        if (value == null) {
          response = "Unrecognized command";
          break;
        }
        Time.timeScale = float.Parse(value);
        response = "Timescale = " + value;
        break;
      case "showfps":
        if (value == 1) {
          _fpsDisplay.SetActive(true);
          response = "Enabled fps display";
        }
        else if (value == 0) {
          _fpsDisplay.SetActive(false);
          response = "Disabled fps display";
        }
        else {
          response = "Unrecognized command";
          break;
        }
      default:
        response = "Unrecognized command";
        break;
    }
    _commandHistory.text += "\n" + response;
  }
}