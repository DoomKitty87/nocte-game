using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UpgradeSystem {
    public class UpgradeHandler : MonoBehaviour
    {
        public static UpgradeHandler Instance { get; private set; }

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }
    }
}
