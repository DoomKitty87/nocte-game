using UnityEngine;

public class SetPosition : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private void OnValidate() {
        transform.position = _target.transform.position;
    }

    private void Update() {
        transform.position = _target.transform.position;
    }
}
