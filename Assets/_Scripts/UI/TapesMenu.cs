using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TapesMenu : MonoBehaviour
{

  [SerializeField] private InventoryManager _inventory;
  [SerializeField] private Transform _tapeHolder;
  [SerializeField] private GameObject _tapePrefab;
  [SerializeField] private AudioSource _tapeAudioSource;

  private InventoryManager.AudioTape[] _tapes;

  private void OnEnable() {
    _tapeAudioSource.Stop();
    _tapes = _inventory.GetOwnedTapes();
    for (int i = _tapeHolder.childCount; i >= 0; i--) {
      Destroy(_tapeHolder.GetChild(i));
    }
    for (int i = 0; i < _tapes.Length; i++) {
      GameObject tape = Instantiate(_tapePrefab);
      tape.transform.parent = _tapeHolder;
      tape.GetComponent<Image>().sprite = _tapes[i].icon;
      tape.GetComponent<TextMeshProUGUI>().text = _tapes[i].name;
    }
  }

  public void SelectTape(int index) {
    _tapeAudioSource.clip = _tapes[index].clip;
    _tapeAudioSource.Play();
  }

}