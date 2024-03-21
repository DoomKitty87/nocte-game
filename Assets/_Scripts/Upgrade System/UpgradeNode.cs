using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UpgradeSystem
{
    
    [System.Serializable]
    public class UpgradeNode : MonoBehaviour
    {
        [HideInInspector] public UpgradeNode _parent;
        public List<UpgradeNode> _children;

        public TextMeshProUGUI text;
        private Image image;
        public Color _enabledColor;
        public Color _disableColor;
        
        [SerializeField] private UpgradeScriptable _data;
        [SerializeField] private int _maxLevel;

        [HideInInspector] public bool _enabled = false;
        private int _currentLevel = 0;


        private void OnEnable() {
            foreach (UpgradeNode node in _children) node._parent = this;
            image = GetComponent<Image>();

            image.color = _disableColor;
            text.text = " ";
        }

        public void EnableNode() {
            _enabled = true;
            image.color = _enabledColor;

            text.text = $"0 / {_maxLevel}";
        }

        public bool IncreaseLevel() {
            if (_currentLevel >= _maxLevel) return false;
            _currentLevel++;

            text.text = $"{_currentLevel} / {_maxLevel}";

            if (_currentLevel == 1) {
                foreach (UpgradeNode child in _children) child.EnableNode();
            }

            return true;
        }

        public void ResetNode() {
            _enabled = false;
            _currentLevel = 0;
            text.text = " ";
            image.color = _disableColor;
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

    }
}