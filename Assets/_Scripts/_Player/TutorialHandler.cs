using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialHandler : MonoBehaviour
{

  [Serializable]
  public struct TutorialStep
  {
    public Dialogue text;
    public KeyCode action;
  }

  [SerializeField] private TutorialStep[] _tutorialSteps;

  private int _currentStep = 0;
  private bool _active = false;

  public void InitialDialogue() {
    DialogueHandler.Instance.PlayDialogue(_tutorialSteps[0].text, true);
    _active = true;
  }

  private void Update() {
    if (Input.GetKey(_tutorialSteps[_currentStep].action)) NextStep();
  }

  private void NextStep() {
    if (!_active) return;
    _currentStep++;
    if (_currentStep < _tutorialSteps.Length) {
      DialogueHandler.Instance.PlayDialogue(_tutorialSteps[_currentStep].text, true);
    }
    else {
      _currentStep--;
      this.enabled = false;
    }
  }

}
