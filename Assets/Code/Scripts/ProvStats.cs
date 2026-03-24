using UnityEngine;

public class ProvStats : MonoBehaviour
{
    public string vote = "";
    private string realVote = "";
    public bool conflict = false;

    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private float questionScale = 0.35f;
    [SerializeField] private Vector2 questionOffset = Vector2.zero;
    [SerializeField] private int sortingOrderOffset = 10;

    private SpriteRenderer provSR;
    private GameObject questionGO;
    private SpriteRenderer questionSR;

    private int liarTurn = 0;
    private bool challengeAvailable = false;

    void Awake()
    {
        provSR = GetComponent<SpriteRenderer>();

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            gameObject.AddComponent<PolygonCollider2D>();
    }

    public void setVote(string vote, string RealVote, int turn)
    {
        this.vote = vote;
        this.realVote = RealVote;
        liarTurn = turn;

        conflict = this.vote != this.realVote;

        challengeAvailable = true;
        ShowQuestionMark();
    }

    public bool IsConflict()
    {
        return vote != realVote;
    }

    public int GetLiarTurn()
    {
        return liarTurn;
    }

    public string GetRealVote()
    {
        return realVote;
    }

    public bool TryConsumeChallenge()
    {
        if (!challengeAvailable) return false;
        challengeAvailable = false;
        return true;
    }

    public void RevealTruth(LevelManager lm)
    {
        HideQuestionMark();

        vote = realVote;

        if (provSR == null || lm == null) return;
        provSR.color = lm.VoteToColor(realVote);
    }

    private void ShowQuestionMark()
    {
        if (questionMarkSprite == null) return;
        if (provSR == null) return;

        if (questionGO == null)
        {
            questionGO = new GameObject("QuestionMarkOverlay");
            questionGO.transform.SetParent(transform, false);
            questionGO.transform.localPosition = (Vector3)questionOffset;
            questionGO.transform.localScale = Vector3.one * questionScale;

            questionSR = questionGO.AddComponent<SpriteRenderer>();
            questionSR.sprite = questionMarkSprite;

            questionSR.sortingLayerID = provSR.sortingLayerID;
            questionSR.sortingOrder = provSR.sortingOrder + sortingOrderOffset;
        }

        questionGO.SetActive(true);
    }
    public bool HasChallengeAvailable()
    {
        return challengeAvailable;
    }


    public void HideQuestionMark()
    {
        if (questionGO != null)
            questionGO.SetActive(false);
    }
}
