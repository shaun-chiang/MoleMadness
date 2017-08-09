using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankWinChange : MonoBehaviour
{
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
        if (rank <= 25 && rank >= 21)
        {
            if (stars >= 2)
            {
                rank -= 1;
                stars = 0;
            } else
            {
                stars += 1;
            }
        }
        else if (rank <= 20 && rank >= 16)
        {
            if (stars >= 3)
            {
                rank -= 1;
                stars = 0;
            }
            else
            {
                stars += 1;
            }
        }
        else if (rank <= 15 && rank >= 11)
        {
            if (stars >= 4)
            {
                rank -= 1;
                stars = 0;
            }
            else
            {
                stars += 1;
            }
        }
        else
        {
            if (rank > 1)
            {
                if (stars >= 3)
                {
                    rank -= 1;
                    stars = 0;
                }
                else
                {
                    stars += 1;
                }
            }
        }
        Debug.Log("New Rank: " + rank);
        Debug.Log("New Stars: " + stars);
        PlayerPrefs.SetString("rank", rank.ToString());
        PlayerPrefs.SetString("stars", stars.ToString());
        newRankStarText.text = "Rank " + rank.ToString() + " Stars " + stars.ToString();
    }
}
