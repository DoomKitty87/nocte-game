using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TapesMenu : MonoBehaviour
{

  [SerializeField] private InventoryManager _inventory;
  [SerializeField] private Transform _tapeHolder;
  [SerializeField] private GameObject _tapePrefab;
  [SerializeField] private AudioSource _tapeAudioSource;

  [SerializeField] private Image _currentTapeImage;
  [SerializeField] private TextMeshProUGUI _currentTapeName;
  [SerializeField] private TextMeshProUGUI _currentTapeText;

  private InventoryManager.AudioTape[] _tapes;

  private void OnEnable() {
    UpdateTapes();
  }

  public void UpdateTapes() {
    _tapeAudioSource.Stop();
    _tapes = _inventory.GetOwnedTapes();
    for (int i = _tapeHolder.childCount; i > 0; i--) {
      Destroy(_tapeHolder.GetChild(i - 1).gameObject);
    }
    for (int i = 0; i < _tapes.Length; i++) {
      GameObject tape = Instantiate(_tapePrefab);
      tape.transform.parent = _tapeHolder;
      tape.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = _tapes[i].icon;
      tape.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = _tapes[i].name;
      tape.gameObject.GetComponent<TapeInstance>().tapesMenu = this;
    }
    _currentTapeImage.sprite = null;
    _currentTapeName.text = "";
    _currentTapeText.text = "";
  }

  public void SelectTape(int index) {
    Debug.Log("Selected tape: " + index);
    _tapeAudioSource.clip = _tapes[index].clip;
    _tapeAudioSource.Play();
    _currentTapeImage.sprite = _tapes[index].icon;
    _currentTapeName.text = _tapes[index].name;
    _currentTapeText.text = _tapes[index].dialogue;
  }

}