using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankLoseChange : MonoBehaviour {
    private int rank;
    private int stars;

    public Text oldRankStarText, newRankStarText;
    // Use this for initialization
    void Start()
    {
        rank = int.Parse(PlayerPrefs.GetString("rank"));
        stars = int.Parse(PlayerPrefs.GetString("stars"));
        Debug.Log("Old Rank: " + rank);
        Debug.Log("Old stars: " + stars);
        oldRankStarText.text = "Rank " + rank.ToString() + " Stars " + stars.ToString();
        if (stars>0)
        {
            stars--;
        }
        else if (rank<25)
        {
            rank--;
            if (rank >= 1 && rank <= 9)
            {
                stars = 5;
            } else if (rank >= 10 && rank <= 14)
            {
                stars = 4;
            }
            else if (rank >= 15 && rank <= 19)
            {
                stars = 3;
            }
            else {
                stars = 2;
            }
        }
        Debug.Log("New Rank: " + rank);
        Debug.Log("New Stars: " + stars);
        PlayerPrefs.SetString("rank", rank.ToString());
        PlayerPrefs.SetString("stars", stars.ToString());
        newRankStarText.text = "Rank " + rank.ToString() + " Stars " + stars.ToString();
    }
}
