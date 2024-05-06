using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{

  [SerializeField] private Image _loadingBar;

  private void Update() {
    _loadingBar.fillAmount = WorldGenInfo._loadingProgress;
  }
  
}