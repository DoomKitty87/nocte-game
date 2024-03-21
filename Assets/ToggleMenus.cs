using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleMenus : MonoBehaviour
{
    [SerializeField] private List<GameObject> _menus;
    [SerializeField] private List<GameObject> _menuButtons;

    private void Start() {
        foreach (var menuButton in _menuButtons) {
            menuButton.GetComponent<Button>().onClick.AddListener(() => ToggleMenu(_menuButtons.IndexOf(menuButton)));
        }
    }

    public void ToggleMenu(int index) {
        foreach (var menu in _menus) {
            menu.SetActive(false);
        }
        _menus[index].SetActive(true);
    }
}
