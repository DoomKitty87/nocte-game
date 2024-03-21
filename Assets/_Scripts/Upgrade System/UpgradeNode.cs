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
        [SerializeField] private UpgradeScriptable _data;

        public List<UpgradeNode> _children;

        [SerializeField] private TextMeshProUGUI _upgradeName;
        [SerializeField] private TextMeshProUGUI _upgradeDiscription;
        [SerializeField] private TextMeshProUGUI _levelText;

        private Image image;
        [SerializeField] private  Color _enabledColor;
        [SerializeField] private  Color _disableColor;
        
        private int _maxLevel;
        private int _upgradeCost;

        [HideInInspector] public bool _enabled = false;
        private int _currentLevel = 0;

        private void Awake() {
            _maxLevel = _data._upgradeMaxLevel;
            _upgradeCost = _data._upgradeCost;

            image = GetComponent<Image>();

            image.color = _disableColor;
            _levelText.text = " ";
            _upgradeName.text = "Locked";
            _upgradeDiscription.text = "This upgrade is currently locked.";
        }

        public void EnableNode() {
            _enabled = true;
            image.color = _enabledColor;
            _upgradeName.text = _data._upgradeName;
            _upgradeDiscription.text = _data._upgradeDescription;
            _levelText.text = $"0 / {_maxLevel}";

        }

        public bool IncreaseLevel(int upgradeLevels, ref int upgradeCostRef) {
            if (_currentLevel >= _maxLevel) return false;
            if (upgradeLevels < _upgradeCost) return false;

            _currentLevel++;
            upgradeCostRef = _upgradeCost;

            _levelText.text = $"{_currentLevel} / {_maxLevel}";

            if (_currentLevel == 1) {
                foreach (UpgradeNode child in _children) child.EnableNode();
            }

            return true;
        }

        public void ResetNode() {
            _enabled = false;
            _currentLevel = 0;
            _upgradeName.text = "Locked";
            _upgradeDiscription.text = "This upgrade is currently locked.";
            _levelText.text = " ";
            image.color = _disableColor;
        }

        public void AssignButton(UpgradeTree parentTree) {
            GetComponent<Button>().onClick.AddListener(() => parentTree.OnClick(this));
        }

        public void Traverse(Action<UpgradeNode> action)
        {
            action(this);  // Call the function on the current node

            foreach (var child in _children)
            {
                child.Traverse(action);  // Recursive call for child nodes
            }
        }

        public void ResetAllNodes()
        {
            Traverse(node => node.ResetNode());  // Call Traverse with a delegate calling Reset()
        }

        public void AssignAllButtons(UpgradeTree parentTree)
        {
            Traverse(node => node.AssignButton(parentTree));
        }

    }
}