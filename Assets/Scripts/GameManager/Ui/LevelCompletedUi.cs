using System; 
using UnityEngine.UnitEvents;
using UnityEngine.UI;

public class LevelCompletedUi : MonoBehaviour
{  
    private UnityEvent NextLevelEvent { get; } = new UnityEvent(); 
    [SerializedField] private Button nextButton; 

    private void Awake = nextButton.OnClick.AddListener(NextLevelEvent.Invoke); 

    internal void Display(bool value) => gameObject.SetActive(value);
}
