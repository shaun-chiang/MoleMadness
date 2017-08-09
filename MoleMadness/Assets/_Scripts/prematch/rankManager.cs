using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rankManager : MonoBehaviour {

    public Text rankText;
    public GameObject star1, star2, star3, star4, star5;

    public string rank, stars;

    public Sprite star;

    public int starAlr = 0; 

	// Use this for initialization
	void Start () {
        stars = PlayerPrefs.GetString("stars");
        rank = PlayerPrefs.GetString("rank");
        
	}
	
	// Update is called once per frame
	void Update () {
        if(starAlr ==0)
        {
            if(PlayerPrefs.GetString("stars")!="0")
            {
                rankText.text = rank;
                initializeStars();
                starAlr = 1; 
            }
        }
    }

    void initializeStars()
    {

        List<GameObject> starList = new List<GameObject>();
        starList.Add(star1);
        starList.Add(star2);
        starList.Add(star3);
        starList.Add(star4);
        starList.Add(star5);

        if(int.Parse(rank)>=11)
        {
            star5.SetActive(false); 
            if(int.Parse(rank) >= 16)
            {
                star4.SetActive(false); 
                if(int.Parse(rank) >= 20)
                {
                    star3.SetActive(false);
                }
            }
        } 

        for(int i = 0; i<int.Parse(stars); i++)
        {
            Image img = starList[i].GetComponentInChildren<Image>();
            img.sprite = star;
        }


    }
}
