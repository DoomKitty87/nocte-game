using UnityEngine;
using TMPro;

public class ShowFPS : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _text;

  private void Update() {
    _text.text = $"{Mathf.Round(1f / Time.deltaTime)} FPS - {Mathf.Round(Time.deltaTime * 1000)}ms";
  }
}