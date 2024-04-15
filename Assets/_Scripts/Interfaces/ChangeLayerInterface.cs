using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLayerInterface : MonoBehaviour
{
    public void ChangeLayerTo(int layerIndex)
	{
		gameObject.layer = layerIndex;
		// foreach (Transform child in gameObject)
		// {
		// 	ChangeLayer(child, layerName);
		// }
	}
}
