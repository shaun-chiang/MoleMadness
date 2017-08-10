using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankWinChange : MonoBehaviour
{
    private int rank;
    private int stars;

    public Sprite star;

    public GameObject star1, star2, star3, star4, star5;
    public GameObject star6, star7, star8, star9, star10; 

    public Text oldRankStarText, newRankStarText;
    // Use this for initialization
    void Start()
    {
        rank = int.Parse(PlayerPrefs.GetString("rank"));
        stars = int.Parse(PlayerPrefs.GetString("stars"));
        Debug.Log("Old Rank: " + rank);
        Debug.Log("Old stars: " + stars);
        oldRankStarText.text = rank.ToString();


        List<GameObject> starList = new List<GameObject>();
        starList.Add(star1);
        starList.Add(star2);
        starList.Add(star3);
        starList.Add(star4);
        starList.Add(star5);


        if (rank >= 11)
        {
            star5.SetActive(false);
            if (rank >= 16)
            {
                star4.SetActive(false);
                if (rank >= 20)
                {
                    star3.SetActive(false);
                }
            }
        }

        for (int i = 0; i < stars; i++)
        {
            Image img = starList[i].GetComponentInChildren<Image>();
            img.sprite = star;
        }



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
        newRankStarText.text = rank.ToString();

        List<GameObject> starList2 = new List<GameObject>();
        starList2.Add(star6);
        starList2.Add(star7);
        starList2.Add(star8);
        starList2.Add(star9);
        starList2.Add(star10);


        if (rank >= 11)
        {
            star10.SetActive(false);
            if (rank >= 16)
            {
                star9.SetActive(false);
                if (rank >= 20)
                {
                    star8.SetActive(false);
                }
            }
        }

        for (int i = 0; i < stars; i++)
        {
            Image img = starList2[i].GetComponentInChildren<Image>();
            img.sprite = star;
        }

    }

}
