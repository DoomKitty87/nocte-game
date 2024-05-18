using UnityEngine;

public class TapeSelect : MonoBehaviour
{

  public int tapeIndex;

  public void SelectTape() {
    TapesMenu.Instance.SelectTape(tapeIndex);
  }

}