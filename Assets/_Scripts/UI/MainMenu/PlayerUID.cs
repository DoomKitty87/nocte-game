using UnityEngine;
using TMPro;

public class PlayerUID : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _playerUID;

  private void Start() {
    if (!PlayerPrefs.HasKey("UID")) {
      PlayerPrefs.SetInt("UID", Random.Range(0, 1000000));
    }
    _playerUID.text = "User ID: " + PlayerPrefs.GetInt("UID").ToString();
  }

}