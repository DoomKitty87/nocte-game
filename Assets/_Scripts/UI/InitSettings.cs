using UnityEngine;

public class InitSettings : MonoBehaviour
{

  [SerializeField] private InGameMenus _menus;

  private void Start() {
    InitSettingsMenuKey();
  }

  // Fill this out later, you get the point

  private void InitSettingsMenuKey() {
    // _menus.SetKeybind(0, PlayerPrefs.GetInt("SettingsKeybind")); int to keycode this
  }
}