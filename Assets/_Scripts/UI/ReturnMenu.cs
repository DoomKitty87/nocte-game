using UnityEngine;
using UnityEngine.UI;

public class ReturnMenu : MonoBehaviour
{

  [SerializeField] private GameObject _toDisable;
  [SerializeField] private GameObject _toEnable;

  [SerializeField] private Button _returnButton;

  private void Start() {
    _returnButton.onClick.AddListener(Return);
  }

  public void Return() {
    _toEnable.SetActive(true);
    _toDisable.SetActive(false);
  }

}