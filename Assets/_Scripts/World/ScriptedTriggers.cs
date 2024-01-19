using System;
using UnityEngine;

public class ScriptedTriggers : MonoBehaviour
{

  [System.Serializable]
  private struct Trigger
  {

    public Action onTriggerFunction;
    public bool canRepeat;
    [HideInInspector] public bool hasExecuted;

  }

  [SerializeField] private Trigger[] _triggers;

  public void Activate(int index) {
    if (_triggers[index].canRepeat || !_triggers[index].hasExecuted) {
      _triggers[index].onTriggerFunction();
      _triggers[index].hasExecuted = true;
    }
  }

}