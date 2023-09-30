using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReferenceRedirector : MonoBehaviour
{
  [SerializeField] Dictionary<string, UnityEngine.Object> _references;
  public List<string> ListKeys() {
    var referencesKeys = _references.Keys;
    var keyList = referencesKeys.ToList();
    return keyList;
  }

  public UnityEngine.Object GetObject(string key) {
    return _references[key];
  }
}