using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPositionAsGizmo : MonoBehaviour
{
	[SerializeField] private float _gizmoSize = 1;
	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, _gizmoSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * _gizmoSize * 10);
	}
}
