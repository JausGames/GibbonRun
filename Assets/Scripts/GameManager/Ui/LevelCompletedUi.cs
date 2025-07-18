using System; 
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public TMP_Text finalScoreText;

public class LevelCompletedUi : MonoBehaviour
{  
    internal UnityEvent NextLevelEvent { get; } = new UnityEvent(); 
    [SerializeField] private Button nextButton; 

    private void Awake() => nextButton.onClick.AddListener(NextLevelEvent.Invoke); 

    internal void Display(bool value) => gameObject.SetActive(value);
}

public void ShowFinalScore()
{
    finalScoreText.text = $"Final Score: {ScoreManager.Instance.GetTotalScore():0}";
}
