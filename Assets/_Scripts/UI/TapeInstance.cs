using UnityEngine;

public class TapeInstance : MonoBehaviour
{

  public TapesMenu tapesMenu;

  public void Select() {
    tapesMenu.SelectTape(transform.GetSiblingIndex());
  }
  
}