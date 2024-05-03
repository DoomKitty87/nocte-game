using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class EquipMenuSlot
{
    public WeaponItem _weaponItem;
    public Image _iconImage;

}
public class EquipMenuHandler : MonoBehaviour
{

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _centerNameText;
    [SerializeField] private TextMeshProUGUI _centerDescText;
    
    

}
