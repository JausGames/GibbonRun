using System; 
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelCompletedUi : MonoBehaviour
{  
    internal UnityEvent NextLevelEvent { get; } = new UnityEvent(); 
    [SerializeField] private Button nextButton; 

    private void Awake() => nextButton.onClick.AddListener(NextLevelEvent.Invoke); 

    internal void Display(bool value) => gameObject.SetActive(value);
}
