
using UnityEngine;

public class InGameMenus : MonoBehaviour
{
}
/*

// Each menu specified has its own manager script
// Currently 0 settings, 1 progression, and 2 tapes
[System.Serializable]
private struct Menu {

  public GameObject object;
  public KeyCode keybind;

}

[SerializeField] private Menu[] _menus;
[SerializeField] private MusicManager _music;

private void Update() {
  for (int i = 0; i < _menus.Lengths; i++) {
    if (Input.GetKeyDown(_menus[i].keybind)) ActivateMenu(i);
  }
}

private void ActivateMenu(int index) {
  for (int i = 0; i < _menus.Lengths; i++) {
    if (i == index) continue;
    _menus[i].object.SetActive(false);
  }
  if (_menus[index].object.activeSelf) {
    _menus[index].object.SetActive(false);
    _music.ResumeMusic();
  }
  else {
    _menus[index].object.SetActive(true);
    _music.PauseMusic();
  }
}

public void SetKeybind(int index, KeyCode keybind) {
  _menus[i].keybind = keybind;
}
}
*/