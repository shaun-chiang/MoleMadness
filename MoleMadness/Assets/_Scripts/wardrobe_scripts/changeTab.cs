using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeTab : MonoBehaviour {

    public GameObject hats;
    public GameObject clothes;
    public GameObject baby;

    public List<GameObject> tabsList = new List<GameObject>();

    // Use this for initialization
    void Start () {
        tabsList.Add(hats);
        tabsList.Add(clothes);
        tabsList.Add(baby);
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void changeTabs(int index)
    {
        foreach(GameObject i in tabsList)
        {
            i.SetActive(false);
        }
        tabsList[index].SetActive(true);
    }
}
