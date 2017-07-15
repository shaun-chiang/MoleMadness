using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeTab : MonoBehaviour {

    //public Image hats;
    //public Image clothes;
    //public Image shoes;
    //public Image baby;

    public GameObject hats;
    public GameObject clothes;
    public GameObject shoes;
    public GameObject baby;

    public List<GameObject> tabsList = new List<GameObject>(); 

	// Use this for initialization
	void Start () {
        tabsList.Add(hats);
        tabsList.Add(clothes);
        tabsList.Add(shoes);
        tabsList.Add(baby);

        foreach (GameObject i in tabsList)
        {
            //i.enabled = false;
            i.SetActive(false);
        }
        //tabsList[0].enabled = true; 
        tabsList[0].SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void changeTabs(int index)
    {
        foreach(GameObject i in tabsList)
        {
            //i.enabled = false; 
            i.SetActive(false);
        }
        //tabsList[index].enabled = true;
        tabsList[index].SetActive(true);
    }
}
