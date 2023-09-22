using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAreaSwitcher : MonoBehaviour
{
  [SerializeField] private Collider _switcherTrigger;
  [SerializeField] private List<WeaponItem> _offeredItems = new();
  [SerializeField] private Vector3 _weaponDisplayPosition;
}