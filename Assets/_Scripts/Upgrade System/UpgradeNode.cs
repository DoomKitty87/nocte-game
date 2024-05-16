using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UpgradeSystem
{

  [System.Serializable]
  public class UpgradeNode : MonoBehaviour
  {
    private PlayerMetaProgression instance;

    [SerializeField] private int _index;
    [SerializeField] private UpgradeScriptable _data;

    [SerializeField] private TextMeshProUGUI _upgradeName;
    [SerializeField] private TextMeshProUGUI _upgradeDiscription;

    [SerializeField] private Animator _animator;

    [Header("Colors")]
    private Image image;
    [SerializeField] private Color _lockedColor;
    [SerializeField] private Color _unlockedColor;
    [SerializeField] private Color _boughtColor;

    [SerializeField] private Color _lockedTextNameColor;
    [SerializeField] private Color _lockedTextDiscriptionColor;

    [SerializeField] private Color _unlockedTextNameColor;
    [SerializeField] private Color _unlockedTextDiscriptionColor;

    [SerializeField] private Color _boughtTextNameColor;
    [SerializeField] private Color _boughtTextDiscriptionColor;

    private void Start()
    {
      instance = PlayerMetaProgression.Instance;
    }

    private void OnEnable()
    {
      int upgradeLevel = instance.CheckUpgrade(_index);
      switch (upgradeLevel)
      {
        case 0:
          LockNodeVisual();
          break;
        case 1:
          UnlockNodeVisual();
          break;
        case 2:
          BuyNodeVisual();
          break;
      }
    }

    private void LockNode()
    {
      instance.Lock(_index);
      _animator.SetInteger("UpgradeLevel", 0);
      LockNodeVisual();
    }

    private void UnlockNode()
    {
      instance.Unlock(_index);
      _animator.SetInteger("UpgradeLevel", 1);
      UnlockNodeVisual();
    }

    private void BuyNode()
    {
      instance.Buy(_index);
      _animator.SetInteger("UpgradeLevel", 2);
      BuyNodeVisual();
    }

    public void LockNodeVisual()
    {
      image = GetComponent<Image>();

      image.color = _lockedColor;
      _upgradeName.text = "Locked";
      _upgradeName.color = _lockedTextNameColor;
      _upgradeDiscription.text = "This upgrade is currently locked.";
      _upgradeDiscription.color = _lockedTextDiscriptionColor;

      PlayerMetaProgression.Instance.Lock(_index);
    }

    public void UnlockNodeVisual()
    {
      image.color = _unlockedColor;
      _upgradeName.text = _data._upgradeName;
      _upgradeName.color = _unlockedTextNameColor;
      _upgradeDiscription.text = _data._upgradeDescription;
      _upgradeDiscription.color = _unlockedTextDiscriptionColor;
    }

    public void BuyNodeVisual()
    {
      image.color = _boughtColor;
      _upgradeName.text = _data._upgradeName;
      _upgradeName.color = _boughtTextNameColor;
      _upgradeDiscription.text = _data._upgradeDescription;
      _upgradeDiscription.color = _boughtTextDiscriptionColor;
    }
  }
}