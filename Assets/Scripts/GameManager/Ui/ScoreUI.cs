using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TMP_Text totalScoreText;
    public TMP_Text distanceText;
    public TMP_Text swingText;
    public TMP_Text speedText;

    void Update()
    {
        if (!ScoreManager.Instance) return;

        totalScoreText.text = $"Score: {ScoreManager.Instance.GetTotalScore():0}";
        distanceText.text = $"Distance: {ScoreManager.Instance.DistanceScore:0}";
        swingText.text = $"Swings: {ScoreManager.Instance.SwingScore:0}";
        speedText.text = $"Speed Bonus: {ScoreManager.Instance.SpeedBonus:0}";
    }
}
