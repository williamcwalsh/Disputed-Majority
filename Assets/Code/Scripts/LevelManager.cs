using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    private static readonly Color CampaignRed = new Color(193f / 255f, 56f / 255f, 58f / 255f);
    private static readonly Color CampaignBlue = new Color(45f / 255f, 72f / 255f, 178f / 255f);
    private static readonly Color CampaignGreen = new Color(85f / 255f, 173f / 255f, 64f / 255f);

    public int RedHp = 3;
    public int BlueHp = 3;
    public int GreenHp = 3;

    [SerializeField] private GameObject[] redHpRects;
    [SerializeField] private GameObject[] blueHpRects;
    [SerializeField] private GameObject[] greenHpRects;

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

    private bool turnLocked = false;
    private bool hasDrawnThisTurn = false;
    private bool hasAccusedThisTurn = false;
    private bool accusationWindowOpen = true;

    private static readonly string TurnNameRed = "Red";
    private static readonly string TurnNameBlue = "Blue";
    private static readonly string TurnNameGreen = "Green";

    void Start()
    {
        if (deck != null)
        {
            for (int i = 0; i < deck.Length; i++)
                if (deck[i] != null) deck[i].SetActive(false);
        }

        ShuffleDeck();
        UpdateTurnUI();
        StartTurn();
        UpdateHpUI();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            EndTurn();

        if (accusationWindowOpen && !turnLocked)
            HandleAccuseClick();
    }

    private void UpdateHpUI()
    {
        SetHpRow(redHpRects, RedHp);
        SetHpRow(blueHpRects, BlueHp);
        SetHpRow(greenHpRects, GreenHp);
    }

    private void SetHpRow(GameObject[] row, int hp)
    {
        if (row == null) return;
        for (int i = 0; i < row.Length; i++)
            if (row[i] != null) row[i].SetActive(i < hp);
    }


    public void StartDraw()
    {
        if (turnLocked) return;
        if (hasDrawnThisTurn) return;
        if (hasAccusedThisTurn) return;

        hasDrawnThisTurn = true;
        accusationWindowOpen = false;

        DrawNextCard();
    }

    private void HandleAccuseClick()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("HandleAccuseClick: no Camera.main (tag your camera MainCamera)");
            return;
        }

        Vector3 w = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 p = new Vector2(w.x, w.y);

        Collider2D hit = Physics2D.OverlapPoint(p);
        if (hit == null) return;

        ProvStats prov = hit.GetComponent<ProvStats>();
        if (prov == null) prov = hit.GetComponentInParent<ProvStats>();
        if (prov == null) return;

        if (hasAccusedThisTurn) return;
        if (!prov.HasChallengeAvailable()) return;

        Debug.Log("Accuse click: " + prov.gameObject.name);

        hasAccusedThisTurn = true;
        ResolveChallenge(prov);

        turnLocked = true;
        HideActiveCard();
        Debug.Log("Accuse complete. Press Space for next turn.");
    }

    public Color VoteToColor(string v)
    {
        if (v == "red") return CampaignRed;
        if (v == "blue") return CampaignBlue;
        if (v == "green") return CampaignGreen;
        return Color.white;
    }

    public string TurnToName(int t)
    {
        if (t == 1) return TurnNameRed;
        if (t == 2) return TurnNameBlue;
        return TurnNameGreen;
    }

    private void StartTurn()
    {
        turnLocked = false;
        hasDrawnThisTurn = false;
        hasAccusedThisTurn = false;
        accusationWindowOpen = true;

        HideActiveCard();
    }

    private void EndTurn()
    {
        if (!turnLocked) return;

        turn++;
        if (turn > 3) turn = 1;

        UpdateTurnUI();
        StartTurn();
    }

    private void UpdateTurnUI()
    {
        if (redTurn != null) redTurn.SetActive(turn == 1);
        if (blueTurn != null) blueTurn.SetActive(turn == 2);
        if (greenTurn != null) greenTurn.SetActive(turn == 3);

        Debug.Log(TurnToName(turn) + " Turn");
    }

    private void HideActiveCard()
    {
        if (deck == null) return;
        if (currentCardIndex == -1) return;
        if (currentCardIndex >= deck.Length) return;
        if (deck[currentCardIndex] == null) return;

        deck[currentCardIndex].SetActive(false);
        currentCardIndex = -1;
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
            HideActiveCard();
            return;
        }

        HideActiveCard();

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
        if (ballot == null) return;

        Image img = ballot.GetComponent<Image>();
        if (img != null)
            img.color = VoteToColor(voteColor);

        Transform votesTextTf = ballot.transform.Find("Votes Text");
        if (votesTextTf == null) return;

        TMP_Text tmp = votesTextTf.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = "Votes: " + voteColor.ToUpper();
            return;
        }

        Text legacy = votesTextTf.GetComponent<Text>();
        if (legacy != null)
            legacy.text = "Votes: " + voteColor.ToUpper();
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

    private void PickProvince(string provinceName, Color color, string vote)
    {
        if (turnLocked) return;
        if (!hasDrawnThisTurn) return;

        GameObject p = GameObject.Find(provinceName);
        if (p == null) return;

        var sr = p.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;

        ProvStats stats = p.GetComponent<ProvStats>();
        if (stats != null)
            stats.setVote(vote, realVote, turn);

        HideActiveCard();
        turnLocked = true;
        Debug.Log("Vote placed. Press Space for next turn.");
    }

    public void ResolveChallenge(ProvStats prov)
    {
        if (prov == null) return;

        Debug.Log("ResolveChallenge called for " + prov.gameObject.name);

        if (!prov.TryConsumeChallenge())
        {
            Debug.Log("Challenge ignored (already used) for " + prov.gameObject.name);
            return;
        }

        bool conflict = prov.IsConflict();
        int liarTurn = prov.GetLiarTurn();

        Debug.Log($"Challenge check: conflict={conflict} liarTurn={liarTurn} realVote={prov.GetRealVote()} vote={prov.vote}");

        if (conflict)
        {
            if (liarTurn == 1) RedHp = Mathf.Max(0, RedHp - 1);
            else if (liarTurn == 2) BlueHp = Mathf.Max(0, BlueHp - 1);
            else if (liarTurn == 3) GreenHp = Mathf.Max(0, GreenHp - 1);

            Debug.Log($"HP updated => R:{RedHp} B:{BlueHp} G:{GreenHp}");
        }
        else
        {
            Debug.Log("No conflict. No HP change.");
        }
        UpdateHpUI();
        prov.RevealTruth(this);
    }

    public void setP1Red() { PickProvince("p1", CampaignRed, "red"); }
    public void setP1Blue() { PickProvince("p1", CampaignBlue, "blue"); }
    public void setP1Green() { PickProvince("p1", CampaignGreen, "green"); }

    public void setP2Red() { PickProvince("p2", CampaignRed, "red"); }
    public void setP2Blue() { PickProvince("p2", CampaignBlue, "blue"); }
    public void setP2Green() { PickProvince("p2", CampaignGreen, "green"); }

    public void setP3Red() { PickProvince("p3", CampaignRed, "red"); }
    public void setP3Blue() { PickProvince("p3", CampaignBlue, "blue"); }
    public void setP3Green() { PickProvince("p3", CampaignGreen, "green"); }

    public void setP4Red() { PickProvince("p4", CampaignRed, "red"); }
    public void setP4Blue() { PickProvince("p4", CampaignBlue, "blue"); }
    public void setP4Green() { PickProvince("p4", CampaignGreen, "green"); }

    public void setP5Red() { PickProvince("p5", CampaignRed, "red"); }
    public void setP5Blue() { PickProvince("p5", CampaignBlue, "blue"); }
    public void setP5Green() { PickProvince("p5", CampaignGreen, "green"); }

    public void setP6Red() { PickProvince("p6", CampaignRed, "red"); }
    public void setP6Blue() { PickProvince("p6", CampaignBlue, "blue"); }
    public void setP6Green() { PickProvince("p6", CampaignGreen, "green"); }

    public void setP7Red() { PickProvince("p7", CampaignRed, "red"); }
    public void setP7Blue() { PickProvince("p7", CampaignBlue, "blue"); }
    public void setP7Green() { PickProvince("p7", CampaignGreen, "green"); }

    public void setP8Red() { PickProvince("p8", CampaignRed, "red"); }
    public void setP8Blue() { PickProvince("p8", CampaignBlue, "blue"); }
    public void setP8Green() { PickProvince("p8", CampaignGreen, "green"); }
}
