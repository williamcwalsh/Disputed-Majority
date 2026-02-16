using System.Collections.Generic;
using UnityEngine;

public class BallotDeckUI : MonoBehaviour
{
    [SerializeField] BallotCardView ballotCardPrefab;
    [SerializeField] Transform handParent;
    [SerializeField] List<ProvinceData> deck;

    readonly List<ProvinceData> drawPile = new();
    int topIndex;

    void Awake()
    {
        ResetDeck();
    }

    public void ResetDeck()
    {
        drawPile.Clear();
        drawPile.AddRange(deck);

        for (int i = 0; i < drawPile.Count; i++)
        {
            int j = Random.Range(i, drawPile.Count);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }

        topIndex = 0;
    }

    public ProvinceData Draw()
    {
        if (topIndex >= drawPile.Count) ResetDeck();
        return drawPile[topIndex++];
    }

    public BallotCardView DealToUI(string voteColorName)
    {
        var province = Draw();
        var card = Instantiate(ballotCardPrefab, handParent);
        card.Set(province.provinceName, province.provinceSprite, voteColorName);
        return card;
    }
}
