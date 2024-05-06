using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{

  [SerializeField] public Sprite[] _loadingScreens;

  [SerializeField] private Image _screen;

  private void Awake() {
    _screen.sprite = _loadingScreens[DateTime.Now.Millisecond % _loadingScreens.Length];
  }
}