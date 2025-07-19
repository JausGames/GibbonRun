using UnityEngine;

abstract public class MapObject : MonoBehaviour
{ 
    public MapObject Instanciate(Vector3 position, Quaternion rotation, Transform parent) => Instantiate(this, position, rotation, parent); 
}
