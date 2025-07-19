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

        totalScoreText.text = $"{ScoreManager.Instance.GetTotalScore():0}";
        distanceText.text = $"{ScoreManager.Instance.TimeScore:0}";
        swingText.text = $"{ScoreManager.Instance.SwingScore:0}";
        speedText.text = $"{ScoreManager.Instance.SpeedBonus:0}";
    }
}
