using UnityEngine;

public class MoveTo : MonoBehaviour
{
    public Vector3 origin;
    public Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = transform.localPosition;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position;
    }
    private void OnDisable()
    {
        transform.localPosition = origin; 
    }
}
