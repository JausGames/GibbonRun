using UnityEngine;

public class Branch : MonoBehaviour
{ 
    [Header("Connector")]
    public string connectorName = "Connector";

    internal Transform Connector => transform.Find(connectorName); 
     
}
