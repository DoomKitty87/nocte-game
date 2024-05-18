using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradeSystem
{

  [System.Serializable]
  public class UpgradeNode : MonoBehaviour
  {
    private PlayerMetaProgression instance;

    [SerializeField] private ShipUpgradeTableAnimationHandler _shipUpgradeTableAnimationHandler;

		[SerializeField] private Image _lockImage;
    
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
    [SerializeField] private Color _lockedTextDescriptionColor;

    [SerializeField] private Color _unlockedTextNameColor;
    [SerializeField] private Color _unlockedTextDescriptionColor;

    [SerializeField] private Color _boughtTextNameColor;
    [SerializeField] private Color _boughtTextDescriptionColor;

    private void Awake()
    {

      image = GetComponent<Image>();
		}

		private void Start()
    {
	    instance = PlayerMetaProgression.Instance;

			var upgradeLevel = instance._progression.upgrades[_index];

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

			if (PlayerMetaProgression.Instance.AvailableCores > 0) {
        _animator.SetBool("Have Cores", true);
			}
    }

		public void Lock() {
			_animator.SetBool("Have Cores", false);
		}

		private void LockNode() {
      instance.Lock(_index);
      instance.SaveData();
      LockNodeVisual();
    }

    private void UnlockNode()  {
      instance.Unlock(_index);
      instance.SaveData();
			UnlockNodeVisual();
		}

    private void BuyNode()
    {
      instance.Buy(_index);
      instance.UseCore();
			instance.SaveData();
			_shipUpgradeTableAnimationHandler.UpdateCoreCounter();
			BuyNodeVisual();
		}

    public void LockNodeVisual()
    {
      image.color = _lockedColor;
      _upgradeName.text = _data._upgradeName;
      _upgradeName.color = _lockedTextNameColor;
      _upgradeDiscription.text = "This upgrade is\ncurrently locked.";
      _upgradeDiscription.color = _lockedTextDescriptionColor;

      _lockImage.transform.gameObject.SetActive(true);
      
      _animator.SetBool("Locked", true);
      _animator.SetBool("Unlocked", false);
      _animator.SetBool("Bought", false);
    }

    public void UnlockNodeVisual()
    {
      image.color = _unlockedColor;
      _upgradeName.text = _data._upgradeName;
      _upgradeName.color = _unlockedTextNameColor;
      _upgradeDiscription.text = _data._upgradeDescription;
      _upgradeDiscription.color = _unlockedTextDescriptionColor;

      _lockImage.transform.gameObject.SetActive(true);
      
      _animator.SetBool("Locked", false);
      _animator.SetBool("Unlocked", true);
      _animator.SetBool("Bought", false);
		}

    public void BuyNodeVisual()
    {
      image.color = _boughtColor;
      _upgradeName.text = _data._upgradeName;
      _upgradeName.color = _boughtTextNameColor;
      _upgradeDiscription.text = _data._upgradeDescription;
      _upgradeDiscription.color = _boughtTextDescriptionColor;

      _lockImage.transform.gameObject.SetActive(false);
      
      _animator.SetBool("Locked", false);
      _animator.SetBool("Unlocked", false);
      _animator.SetBool("Bought", true);
		}
  }
}