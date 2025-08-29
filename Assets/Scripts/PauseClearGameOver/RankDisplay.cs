using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankDisplay : MonoBehaviour
{
    private TMPro.TextMeshProUGUI rankText;
    [SerializeField] float maxFontSize = 200;
    [SerializeField] float addFontSize = 10;
    [SerializeField] float accel = 0f;
    // Start is called before the first frame update
    void Start()
    {
        rankText = GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void InitTextSize()
    {
        rankText.fontSize = 10;
    }

    public void SetText(string Rank)
    {
        rankText.text = Rank;
        switch (Rank)
        {
            case "S":
                rankText.color = Color.yellow;
                break;
            case "A":
                rankText.color = Color.red;
                break;
            case "B":
                rankText.color = Color.blue;
                break;
            case "C":
                rankText.color = Color.black;
                break;
            default:
                rankText.color = Color.black;
                break;
        }
    }

    public void AnimateText()
    {
        if (rankText.fontSize < maxFontSize)
        {
            rankText.fontSize = rankText.fontSize + addFontSize;
            addFontSize += accel;
        }
    }
}
