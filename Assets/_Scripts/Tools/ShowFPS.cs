using System.Collections;
using UnityEngine;
using TMPro;

public class ShowFPS : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _text;
  [Range(0.1f, 5)][SerializeField] private float _updateAfterSec = 0.25f;
  private int frames;
  
  private void Start() {
    StartCoroutine(Counter());
  }

  private IEnumerator Counter() {
    int startFramesPassed;
    int framesPassed;
    while (true) {
      startFramesPassed = frames;
      yield return new WaitForSeconds(_updateAfterSec);
      framesPassed = frames - startFramesPassed;
      _text.text = $"{framesPassed * (1 / _updateAfterSec)} FPS - {_updateAfterSec / framesPassed * 1000}ms";
    }
  }

  private void Update() {
    frames++;
  }
  
}