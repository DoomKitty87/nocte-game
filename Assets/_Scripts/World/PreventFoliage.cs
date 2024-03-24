using UnityEngine;
using Foliage;

public class PreventFoliage : MonoBehaviour
{

  [SerializeField] private Transform _corner1;
  [SerializeField] private Transform _corner2;
  
  private void Start() {
    FoliagePool._structureBounds.Add((new Vector2(_corner1.position.x, _corner1.position.z), new Vector2(_corner2.position.x, _corner2.position.z)));
  }

}