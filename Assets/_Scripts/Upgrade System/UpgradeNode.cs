using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UpgradeSystem
{
    [System.Serializable]
    public class UpgradeNode : MonoBehaviour
    {
        public UpgradeNode _parent;
        public List<UpgradeNode> _children;
        
        [SerializeField] private UpgradeScriptable _data;
        [SerializeField] private int _maxLevel = 0;

        [HideInInspector] public bool _enabled = false;
        private int _currentLevel = 0;
        
        public void EnableNode() =>
            _enabled = true;

        public bool IncreaseLevel() {
            if (_currentLevel ! < _maxLevel) return false;
            _currentLevel++;
            return true;
        }

    }
}