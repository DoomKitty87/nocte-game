using UnityEngine;

public class SettingsMenu : MonoBehaviour
{

  public void ExitToMenuButton() {
    // Exit to menu somehow
  }

  public void CloseSettingsButton() {
    gameObject.SetActive(false);
  }

  public void ResetSettingsButton() {
    PlayerPrefs.DeleteAll();
  }

  // Can't fill these all out without wifi and a list of what we want

  public void ChangeVsync(bool enabled) {
    // Enable/disable vsync
    PlayerPrefs.SetInt("Vsync", enabled ? 1 : 0);
  }
}