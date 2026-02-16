using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    private static readonly Color CampaignRed = new Color(193f / 255f, 56f / 255f, 58f / 255f);
    private static readonly Color CampaignBlue = new Color(45f / 255f, 72f / 255f, 178f / 255f);
    private static readonly Color CampaignGreen = new Color(85f / 255f, 173f / 255f, 64f / 255f);

    public float players;
    private int turn = 1;
    public float currentProvidence = -1;

    public string realVote = "";

    [SerializeField] private GameObject[] deck;
    [SerializeField] private GameObject redTurn;
    [SerializeField] private GameObject blueTurn;
    [SerializeField] private GameObject greenTurn;


    private int currentCardIndex = -1;
    private int drawIndex = 0;

    public void SetSpriteColor(string objectName, Color color)
    {
        var go = GameObject.Find(objectName);
        if (go == null)
        {
            Debug.LogError($"SetSpriteColor: No GameObject named '{objectName}' found in scene.");
            return;
        }

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError($"SetSpriteColor: GameObject '{objectName}' has no SpriteRenderer.");
            return;
        }

        sr.color = color;
    }

    void Start()
    {
        if (deck != null)
        {
            for (int i = 0; i < deck.Length; i++)
            {
                if (deck[i] != null)
                    deck[i].SetActive(false);
            }
        }

        ShuffleDeck();
        DrawNextCard();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            turn++;
            Debug.Log("Turn: " + turn);
            DrawNextCard();
        }
        if (turn>3){
            turn =1;
        }
        if (turn == 1){
            Debug.Log("Red Turn");
            redTurn.SetActive(true);
            blueTurn.SetActive(false);
            greenTurn.SetActive(false);
        }
        if (turn == 2){
            Debug.Log("Blue Turn");
            redTurn.SetActive(false);
            blueTurn.SetActive(true);
            greenTurn.SetActive(false);
        }
        if (turn == 3){
            Debug.Log("Green Turn");
            redTurn.SetActive(false);
            blueTurn.SetActive(false);
            greenTurn.SetActive(true);
        }
    }

    private void DrawNextCard()
    {
        if (deck == null || deck.Length == 0)
        {
            Debug.LogWarning("Deck is empty or not assigned.");
            return;
        }

        if (drawIndex >= deck.Length)
        {
            Debug.Log("Deck empty. No more cards to draw.");
            if (currentCardIndex != -1 && deck[currentCardIndex] != null)
                deck[currentCardIndex].SetActive(false);
            return;
        }

        if (currentCardIndex != -1 && deck[currentCardIndex] != null)
            deck[currentCardIndex].SetActive(false);

        if (deck[drawIndex] != null)
            deck[drawIndex].SetActive(true);

        currentCardIndex = drawIndex;
        drawIndex++;

        int roll = Random.Range(0, 3);
        if (roll == 0) realVote = "red";
        else if (roll == 1) realVote = "blue";
        else realVote = "green";

        SetBallotUI(deck[currentCardIndex], realVote);

        Debug.Log("Drew card index: " + currentCardIndex + " | realVote: " + realVote);
    }

    private void SetBallotUI(GameObject cardRoot, string voteColor)
    {
        if (cardRoot == null) return;

        Transform ballot = cardRoot.transform.Find("Ballot1");
        if (ballot == null)
        {
            Debug.LogError($"SetBallotUI: '{cardRoot.name}' has no child named 'Ballot1'");
            return;
        }

        Image img = ballot.GetComponent<Image>();
        if (img != null)
        {
            if (voteColor == "red") img.color = CampaignRed;
            else if (voteColor == "blue") img.color = CampaignBlue;
            else if (voteColor == "green") img.color = CampaignGreen;
        }
        else
        {
            Debug.LogError($"SetBallotUI: 'Ballot1' on '{cardRoot.name}' has no Image component");
        }

        Transform votesTextTf = ballot.transform.Find("Votes Text");
        if (votesTextTf == null)
        {
            Debug.LogError($"SetBallotUI: 'Ballot1' on '{cardRoot.name}' has no child named 'Votes Text'");
            return;
        }

        TMP_Text tmp = votesTextTf.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = "Votes: " + voteColor.ToUpper();
            return;
        }

        Text legacy = votesTextTf.GetComponent<Text>();
        if (legacy != null)
        {
            legacy.text = "Votes: " + voteColor.ToUpper();
            return;
        }

        Debug.LogError($"SetBallotUI: 'Votes Text' on '{cardRoot.name}' has no TMP_Text or Text component");
    }

    private void ShuffleDeck()
    {
        if (deck == null) return;

        for (int i = 0; i < deck.Length; i++)
        {
            int randomIndex = Random.Range(i, deck.Length);
            GameObject temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void setP1Red()
    {
        SetSpriteColor("p1", CampaignRed);

        GameObject p1 = GameObject.Find("p1");
        if (p1 == null) return;

        ProvStats stats = p1.GetComponent<ProvStats>();
        if (stats != null)
            stats.setVote("red", realVote);
    }

    public void setP1Blue() { SetSpriteColor("p1", CampaignBlue); }
    public void setP1Green() { SetSpriteColor("p1", CampaignGreen); }

    public void setP2Red() { SetSpriteColor("p2", CampaignRed); }
    public void setP2Blue() { SetSpriteColor("p2", CampaignBlue); }
    public void setP2Green() { SetSpriteColor("p2", CampaignGreen); }

    public void setP3Red() { SetSpriteColor("p3", CampaignRed); }
    public void setP3Blue() { SetSpriteColor("p3", CampaignBlue); }
    public void setP3Green() { SetSpriteColor("p3", CampaignGreen); }

    public void setP4Red() { SetSpriteColor("p4", CampaignRed); }
    public void setP4Blue() { SetSpriteColor("p4", CampaignBlue); }
    public void setP4Green() { SetSpriteColor("p4", CampaignGreen); }

    public void setP5Red() { SetSpriteColor("p5", CampaignRed); }
    public void setP5Blue() { SetSpriteColor("p5", CampaignBlue); }
    public void setP5Green() { SetSpriteColor("p5", CampaignGreen); }

    public void setP6Red() { SetSpriteColor("p6", CampaignRed); }
    public void setP6Blue() { SetSpriteColor("p6", CampaignBlue); }
    public void setP6Green() { SetSpriteColor("p6", CampaignGreen); }

    public void setP7Red() { SetSpriteColor("p7", CampaignRed); }
    public void setP7Blue() { SetSpriteColor("p7", CampaignBlue); }
    public void setP7Green() { SetSpriteColor("p7", CampaignGreen); }

    public void setP8Red() { SetSpriteColor("p8", CampaignRed); }
    public void setP8Blue() { SetSpriteColor("p8", CampaignBlue); }
    public void setP8Green() { SetSpriteColor("p8", CampaignGreen); }
}
