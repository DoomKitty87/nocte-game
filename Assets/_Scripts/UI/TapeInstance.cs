using UnityEngine;

public class TapeInstance : MonoBehaviour
{

  public TapesMenu tapesMenu;

  private void OnMouseDown() {
    tapesMenu.SelectTape(transform.GetSiblingIndex());
  }
  
}