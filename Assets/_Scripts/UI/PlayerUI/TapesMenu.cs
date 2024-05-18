using TMPro;
using Unity.VisualScripting;
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
  [SerializeField] private TextMeshProUGUI _currentTapeTimestamp;
  [SerializeField] private TextMeshProUGUI _currentTapeText;

  [SerializeField] private TextMeshProUGUI _currentTapeCurrentTime;
  [SerializeField] private TextMeshProUGUI _currentTapeMaxTime;
  [SerializeField] private Transform _playerHead;

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
    //_currentTapeImage.sprite = null;
    _currentTapeName.text = "";
    _currentTapeText.text = "";
    _currentTapeTimestamp.text = "";
  }

  public void SelectTape(int index) {
    //Debug.Log("Selected tape: " + index);
    if (_tapes[index].clip != null) {
      _tapeAudioSource.clip = _tapes[index].clip;
      _tapeAudioSource.Play();
      _currentTapeCurrentTime.text = "00:00";
      _currentTapeMaxTime.text = _tapes[index].clip.length / 60 + ":" + _tapes[index].clip.length % 60;
      StartCoroutine(UpdateCurrentTime());
    } else {
      _currentTapeCurrentTime.text = "00:00";
      _currentTapeMaxTime.text = "00:00";
    }

    //_currentTapeImage.sprite = _tapes[index].icon;
    _currentTapeName.text = _tapes[index].name;
    _currentTapeText.text = _tapes[index].dialogue;
    _currentTapeTimestamp.text = _tapes[index].timestamp;

  }

  private System.Collections.IEnumerator UpdateCurrentTime() {
    while (_tapeAudioSource.isPlaying) {
      _currentTapeCurrentTime.text = Mathf.Floor(_tapeAudioSource.time / 60).ToString("00") + ":" + Mathf.Floor(_tapeAudioSource.time % 60).ToString("00");
      _playerHead.localPosition = Vector3.right * (_tapeAudioSource.time / _tapeAudioSource.clip.length) * _playerHead.localScale.x;
      yield return null;
    }
  }

}