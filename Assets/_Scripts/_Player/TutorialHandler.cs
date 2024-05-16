using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialHandler : MonoBehaviour
{

  [Serializable]
  public struct TutorialStep {
    public string text;
    public InputActionReference action;
  }

  [SerializeField] private TutorialStep[] _tutorialSteps;

  [SerializeField] private TextMeshProUGUI _tutorialText;

  private int _currentStep = 0;

  private void Start() {
    _tutorialText.text = _tutorialSteps[_currentStep].text;
    _tutorialSteps[_currentStep].action.action.performed += NextStep;
  }

  private void NextStep(InputAction.CallbackContext context) {
    _tutorialSteps[_currentStep].action.action.performed -= NextStep;
    _currentStep++;
    if (_currentStep < _tutorialSteps.Length) {
      _tutorialText.text = _tutorialSteps[_currentStep].text;
      _tutorialSteps[_currentStep].action.action.performed += NextStep;
    }
    else {
      _tutorialText.gameObject.SetActive(false);
    }
  }

}
