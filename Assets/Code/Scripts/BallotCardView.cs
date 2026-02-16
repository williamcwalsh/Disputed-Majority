using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallotCardView : MonoBehaviour
{
    [SerializeField] TMP_Text provinceNameText;
    [SerializeField] Image provinceImage;
    [SerializeField] TMP_Text votesText;

    public void Set(string provinceName, Sprite provinceSprite, string voteColorName)
    {
        provinceNameText.text = provinceName;
        provinceImage.sprite = provinceSprite;
        votesText.text = $"VOTES: {voteColorName}";
    }
}
