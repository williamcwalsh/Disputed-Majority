using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private static readonly Color CampaignRed = new Color(193f/255f, 56f/255f, 58f/255f);
    private static readonly Color CampaignBlue = new Color(45f/255f, 72f/255f, 178f/255f);
    private static readonly Color CampaignGreen = new Color(85f/255f, 173f/255f, 64f/255f);

    public float players;
    private float turn = 1;
    public float currentProvidence = -1;
    [SerializeField] private GameObject card1;
    [SerializeField] private GameObject card2;
    [SerializeField] private GameObject card3;
    [SerializeField] private GameObject card4;
    [SerializeField] private GameObject card5;
    [SerializeField] private GameObject card6;
    [SerializeField] private GameObject card7;
    [SerializeField] private GameObject card8;

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

    void Start(){
    SetSpriteColor("p1", CampaignRed);
    SetSpriteColor("p2", CampaignBlue);
    SetSpriteColor("p3", CampaignRed);
    SetSpriteColor("p4", CampaignBlue);
    SetSpriteColor("p5", CampaignRed);
    SetSpriteColor("p6", CampaignBlue);
    SetSpriteColor("p7", CampaignBlue);
    SetSpriteColor("p8", CampaignBlue);


    card6.SetActive(true);
    
    }

    public void setP1Red(){
        SetSpriteColor("p1", CampaignRed);
    }
    public void setP1Blue(){
        SetSpriteColor("p1", CampaignBlue);
    }
     public void setP2Red(){
        SetSpriteColor("p2", CampaignRed);
    }
    public void setP2Blue(){
        SetSpriteColor("p2", CampaignBlue);
    }
    public void setP3Red(){
        SetSpriteColor("p3", CampaignRed);
    }
    public void setP3Blue(){
        SetSpriteColor("p3", CampaignBlue);
    }
    public void setP4Red(){
        SetSpriteColor("p4", CampaignRed);
    }
    public void setP4Blue(){
        SetSpriteColor("p4", CampaignBlue);
    }
    public void setP5Red(){
        SetSpriteColor("p5", CampaignRed);
    }
    public void setP5Bue(){
        SetSpriteColor("p5", CampaignBlue);
    }
    public void setP6Red(){
        SetSpriteColor("p6", CampaignRed);
    }
    public void setP6Bue(){
        SetSpriteColor("p6", CampaignBlue);
    }
    public void setP7Red(){
        SetSpriteColor("p7", CampaignRed);
    }
    public void setP7Bue(){
        SetSpriteColor("p7", CampaignBlue);
    }
    public void setP8Red(){
        SetSpriteColor("p8", CampaignRed);
    }
    public void setP8Bue(){
        SetSpriteColor("p8", CampaignBlue);
    }
}
