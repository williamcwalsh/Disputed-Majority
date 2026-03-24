using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private class ProvinceOdds
    {
        public int red;
        public int blue;
        public int green;
    }

    private static readonly Color CampaignRed = new Color(193f / 255f, 56f / 255f, 58f / 255f);
    private static readonly Color CampaignBlue = new Color(45f / 255f, 72f / 255f, 178f / 255f);
    private static readonly Color CampaignGreen = new Color(85f / 255f, 173f / 255f, 64f / 255f);

    [SerializeField] private GameObject redWin;
    [SerializeField] private GameObject blueWin;
    [SerializeField] private GameObject greenWin;
    [SerializeField] private GameObject tie;
    [SerializeField] private GameObject resetBtn;

    private bool gameOver = false;

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

    private readonly Dictionary<string, ProvinceOdds> provinceOdds = new Dictionary<string, ProvinceOdds>();
    private TMP_Text pollText;
    private GameObject pollBG;
    private ProvStats selectedProvince;

    void Start()
    {
        GenerateProvinceOdds();
        CachePollUI();
        UpdatePollUI(null);

        if (deck != null)
        {
            for (int i = 0; i < deck.Length; i++)
                if (deck[i] != null) deck[i].SetActive(false);
        }

        ShuffleDeck();
        UpdateTurnUI();
        StartTurn();
        UpdateHpUI();

        if (redWin != null) redWin.SetActive(false);
        if (blueWin != null) blueWin.SetActive(false);
        if (greenWin != null) greenWin.SetActive(false);
        if (tie != null) tie.SetActive(false);
        if (resetBtn != null) resetBtn.SetActive(false);
    }

    void Update()
    {
        if (gameOver) return;

        HandleProvinceSelectionClick();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            EndTurn();

        if (accusationWindowOpen && !turnLocked)
            HandleAccuseClick();
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private int VoteStringToTurn(string v)
    {
        if (v == "red") return 1;
        if (v == "blue") return 2;
        if (v == "green") return 3;
        return 0;
    }

    private string GetProvinceDisplayName(string provinceName)
    {
        if (provinceName == "p1") return "Pomirek";
        if (provinceName == "p2") return "Tonali";
        if (provinceName == "p3") return "Quirich";
        if (provinceName == "p4") return "Licatia";
        if (provinceName == "p5") return "Eritrea";
        if (provinceName == "p6") return "Fornia";
        if (provinceName == "p7") return "Leafswick";
        if (provinceName == "p8") return "Cuddinham";
        return provinceName;
    }

    private void ShowWinUI(int winnerTurn, bool isTie)
    {
        if (redWin != null) redWin.SetActive(false);
        if (blueWin != null) blueWin.SetActive(false);
        if (greenWin != null) greenWin.SetActive(false);
        if (tie != null) tie.SetActive(false);
        if (resetBtn != null) resetBtn.SetActive(true);

        if (isTie)
        {
            if (tie != null) tie.SetActive(true);
            return;
        }

        if (winnerTurn == 1 && redWin != null) redWin.SetActive(true);
        else if (winnerTurn == 2 && blueWin != null) blueWin.SetActive(true);
        else if (winnerTurn == 3 && greenWin != null) greenWin.SetActive(true);
        else if (tie != null) tie.SetActive(true);
    }

    private void EndGame()
    {
        gameOver = true;
        turnLocked = true;
        accusationWindowOpen = false;
        hasDrawnThisTurn = true;
        hasAccusedThisTurn = true;

        if (redTurn != null) redTurn.SetActive(false);
        if (blueTurn != null) blueTurn.SetActive(false);
        if (greenTurn != null) greenTurn.SetActive(false);

        GetTerritoryCounts(out int redTerr, out int blueTerr, out int greenTerr);

        int winnerTurn = GetWinnerByScore(redTerr, blueTerr, greenTerr);

        LogScoreSummary("END GAME SUMMARY", redTerr, blueTerr, greenTerr);

        if (winnerTurn == 0)
        {
            ShowWinUI(0, true);
            return;
        }

        ShowWinUI(winnerTurn, false);
    }

    private int GetWinnerByScore(int redTerr, int blueTerr, int greenTerr)
    {
        int maxTerr = Mathf.Max(redTerr, Mathf.Max(blueTerr, greenTerr));

        bool redTopTerr = redTerr == maxTerr;
        bool blueTopTerr = blueTerr == maxTerr;
        bool greenTopTerr = greenTerr == maxTerr;

        int topTerrCount = (redTopTerr ? 1 : 0) + (blueTopTerr ? 1 : 0) + (greenTopTerr ? 1 : 0);
        if (topTerrCount == 1)
        {
            if (redTopTerr) return 1;
            if (blueTopTerr) return 2;
            return 3;
        }

        int redScoreHp = redTopTerr ? RedHp : int.MinValue;
        int blueScoreHp = blueTopTerr ? BlueHp : int.MinValue;
        int greenScoreHp = greenTopTerr ? GreenHp : int.MinValue;

        int maxHp = Mathf.Max(redScoreHp, Mathf.Max(blueScoreHp, greenScoreHp));

        bool redTopHp = redScoreHp == maxHp;
        bool blueTopHp = blueScoreHp == maxHp;
        bool greenTopHp = greenScoreHp == maxHp;

        int topHpCount = (redTopHp ? 1 : 0) + (blueTopHp ? 1 : 0) + (greenTopHp ? 1 : 0);
        if (topHpCount == 1)
        {
            if (redTopHp) return 1;
            if (blueTopHp) return 2;
            return 3;
        }

        return 0;
    }

    private void GetTerritoryCounts(out int redTerr, out int blueTerr, out int greenTerr)
    {
        redTerr = 0;
        blueTerr = 0;
        greenTerr = 0;

        var provs = FindObjectsOfType<ProvStats>();
        for (int i = 0; i < provs.Length; i++)
        {
            var p = provs[i];
            if (p == null) continue;

            int ownerTurn = VoteStringToTurn(p.vote);
            if (ownerTurn == 1) redTerr++;
            else if (ownerTurn == 2) blueTerr++;
            else if (ownerTurn == 3) greenTerr++;
        }
    }

    private void LogScoreSummary(string prefix, int redTerr, int blueTerr, int greenTerr)
    {
        Debug.Log($"{prefix} | Red: {redTerr} territories, {RedHp} HP | Blue: {blueTerr} territories, {BlueHp} HP | Green: {greenTerr} territories, {GreenHp} HP");
    }

    private void GenerateProvinceOdds()
    {
        provinceOdds.Clear();

        var provs = FindObjectsOfType<ProvStats>();
        for (int i = 0; i < provs.Length; i++)
        {
            var prov = provs[i];
            if (prov == null) continue;

            ProvinceOdds odds = CreateRandomOdds();
            provinceOdds[prov.gameObject.name] = odds;

            Debug.Log($"START GAME ODDS | {GetProvinceDisplayName(prov.gameObject.name)} | Red: {odds.red}% | Blue: {odds.blue}% | Green: {odds.green}%");
        }
    }

    private void CachePollUI()
    {
        GameObject pollUI = GameObject.Find("PollUI");
        if (pollUI == null) return;

        Transform bgTransform = pollUI.transform.Find("PollBG");
        if (bgTransform == null)
            bgTransform = pollUI.transform.Find("Image");

        if (bgTransform != null)
            pollBG = bgTransform.gameObject;

        Transform textTransform = pollUI.transform.Find("PollText");
        if (textTransform == null)
            textTransform = pollUI.transform.Find("Text (TMP)");

        if (textTransform == null) return;

        pollText = textTransform.GetComponent<TMP_Text>();
    }

    private void HandleProvinceSelectionClick()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        ProvStats prov = GetClickedProvince();
        if (prov == null) return;

        selectedProvince = prov;
        UpdatePollUI(prov);
    }

    private ProvStats GetClickedProvince()
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        Vector3 w = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 p = new Vector2(w.x, w.y);

        Collider2D hit = Physics2D.OverlapPoint(p);
        if (hit == null) return null;

        ProvStats prov = hit.GetComponent<ProvStats>();
        if (prov == null) prov = hit.GetComponentInParent<ProvStats>();
        return prov;
    }

    private void UpdatePollUI(ProvStats prov)
    {
        if (pollText == null) return;

        if (pollBG != null)
            pollBG.SetActive(true);

        if (prov == null)
        {
            pollText.text = "Poll\nR:\nB:\nG:\nResult: --";
            return;
        }

        ProvinceOdds odds = GetProvinceOdds(prov.gameObject.name);
        string resultText = string.IsNullOrEmpty(prov.vote) ? "--" : prov.vote.ToUpper();

        pollText.text =
            $"{GetProvinceDisplayName(prov.gameObject.name)}\n" +
            "Poll\n" +
            $"R: {odds.red}%\n" +
            $"B: {odds.blue}%\n" +
            $"G: {odds.green}%\n" +
            $"Result: {resultText}";
    }

    private ProvinceOdds GetProvinceOdds(string provinceName)
    {
        if (provinceOdds.TryGetValue(provinceName, out ProvinceOdds odds))
            return odds;

        ProvinceOdds fallback = new ProvinceOdds();
        fallback.red = 0;
        fallback.blue = 0;
        fallback.green = 0;
        return fallback;
    }

    private ProvinceOdds CreateRandomOdds()
    {
        int cutA = Random.Range(0, 101);
        int cutB = Random.Range(0, 101);

        if (cutA > cutB)
        {
            int temp = cutA;
            cutA = cutB;
            cutB = temp;
        }

        ProvinceOdds odds = new ProvinceOdds();
        odds.red = cutA;
        odds.blue = cutB - cutA;
        odds.green = 100 - cutB;
        return odds;
    }

    private string GetProvinceNameForCard(GameObject card)
    {
        if (card == null) return "";

        string cardName = card.name;
        int number = 0;

        for (int i = 0; i < cardName.Length; i++)
        {
            char c = cardName[i];
            if (c < '0' || c > '9') continue;

            number = (number * 10) + (c - '0');
        }

        if (number <= 0) return "";
        return "p" + number;
    }

    private string RollVoteForProvince(string provinceName)
    {
        if (!provinceOdds.TryGetValue(provinceName, out ProvinceOdds odds))
        {
            int fallbackRoll = Random.Range(0, 3);
            if (fallbackRoll == 0) return "red";
            if (fallbackRoll == 1) return "blue";
            return "green";
        }

        int roll = Random.Range(0, 100);
        if (roll < odds.red) return "red";
        if (roll < odds.red + odds.blue) return "blue";
        return "green";
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
        if (gameOver) return;
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

        ProvStats prov = GetClickedProvince();
        if (prov == null) return;

        if (hasAccusedThisTurn) return;
        if (!prov.HasChallengeAvailable()) return;

        hasAccusedThisTurn = true;
        ResolveChallenge(prov);

        turnLocked = true;
        HideActiveCard();
    }

    private bool IsPlayerAlive(int playerTurn)
    {
        if (playerTurn == 1) return RedHp > 0;
        if (playerTurn == 2) return BlueHp > 0;
        if (playerTurn == 3) return GreenHp > 0;
        return false;
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

        GetTerritoryCounts(out int redTerr, out int blueTerr, out int greenTerr);
        LogScoreSummary("TURN SUMMARY", redTerr, blueTerr, greenTerr);

        int safety = 0;

        do
        {
            turn++;
            if (turn > 3) turn = 1;

            safety++;
            if (safety > 3) break;
        }
        while (!IsPlayerAlive(turn));

        if (!IsPlayerAlive(turn))
        {
            EndGame();
            return;
        }

        UpdateTurnUI();
        StartTurn();
    }

    private void UpdateTurnUI()
    {
        if (redTurn != null) redTurn.SetActive(turn == 1);
        if (blueTurn != null) blueTurn.SetActive(turn == 2);
        if (greenTurn != null) greenTurn.SetActive(turn == 3);
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
            return;

        if (drawIndex >= deck.Length)
        {
            HideActiveCard();
            EndGame();
            return;
        }

        HideActiveCard();

        if (deck[drawIndex] != null)
            deck[drawIndex].SetActive(true);

        currentCardIndex = drawIndex;
        drawIndex++;

        string provinceName = GetProvinceNameForCard(deck[currentCardIndex]);
        realVote = RollVoteForProvince(provinceName);

        SetBallotUI(deck[currentCardIndex], realVote);
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
        {
            stats.setVote(vote, realVote, turn);
            if (selectedProvince == stats)
                UpdatePollUI(stats);
        }

        HideActiveCard();
        turnLocked = true;

        if (drawIndex >= deck.Length)
        {
            EndGame();
            return;
        }
    }

    public void ResolveChallenge(ProvStats prov)
    {
        if (prov == null) return;

        if (!prov.TryConsumeChallenge())
            return;

        bool conflict = prov.IsConflict();
        int liarTurn = prov.GetLiarTurn();

        if (conflict)
        {
            if (liarTurn == 1) RedHp = Mathf.Max(0, RedHp - 1);
            else if (liarTurn == 2) BlueHp = Mathf.Max(0, BlueHp - 1);
            else if (liarTurn == 3) GreenHp = Mathf.Max(0, GreenHp - 1);
        }

        UpdateHpUI();
        prov.RevealTruth(this);
        if (selectedProvince == prov)
            UpdatePollUI(prov);
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
