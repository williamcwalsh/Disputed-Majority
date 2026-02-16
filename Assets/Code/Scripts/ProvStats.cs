using UnityEngine;

public class ProvStats : MonoBehaviour
{
    public string vote = "";
    private string realVote = "";
    public bool conflict = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void setVote(string vote, string RealVote){
        this.vote = vote;
        this.realVote = RealVote;
    }

    // Update is called once per frame
    void Update()
    {
        if (vote != realVote) {
            conflict = true;
        }else{
            conflict = false;
        }
    }
}
