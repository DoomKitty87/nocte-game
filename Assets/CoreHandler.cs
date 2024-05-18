using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreHandler : MonoBehaviour
{
	public void AddCore() {
		PlayerMetaProgression.Instance.AddCore();
		Debug.Log("Added a core");
		Destroy(this.gameObject);
	}
}
