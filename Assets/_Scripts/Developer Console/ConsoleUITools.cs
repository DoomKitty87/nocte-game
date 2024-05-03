using System;
using Console;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleUITools : MonoBehaviour
{
    public void CloseUI() {
        ConsoleController.RaiseExitConsole();
    }
    
}
