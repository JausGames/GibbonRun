using UnityEngine;
using UnityEngine.UI;

public class GrappleInfoUi : MonoBehaviour
{
    public Gradient colorGradient;
    private Image image;
    private TMPro.TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        Destroy(gameObject, 1f);
    }

    public void PlayAnim(float v)
    {
        var t = ""; 
        if (v == 0f) t = "GAY";
        else if (v <= .3f) t = "...";
        else if (v <= .6f) t = "MEH";
        else if (v <= .8f) t = "OK";
        else if (v < 1f) t = "GOOD";
        else if (v == 1f) t = "DAMN";
        text.text = t;
         

        image.color = colorGradient.Evaluate(v);
    }
}
