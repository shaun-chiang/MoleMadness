using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;

public class wardrobeManager : MonoBehaviour {

    // preferably can use a getAccessories() method or something to get these lists? 
    // so like.. hats_list will be accessoriesList[0] and weapons_list will be accessoriesList[1] etc... 
    public List<GameObject> hats_list = new List<GameObject>();
    public List<GameObject> weapons_list = new List<GameObject>();
    public List<GameObject> baby_list = new List<GameObject>();

    public GameObject hats;
    public GameObject weapons;
    public GameObject baby;

    public List<GameObject> tabsList = new List<GameObject>();

    //POSITIONS 
    // for buttons: 37.5, + 75 (x); 82.5, -75 (y)

    // somehow need to get current configuration of accessories? 

    // Use this for initialization


    void Start () {

        initializeEquipped();

        hats_list = getItemsList("Hat");
        weapons_list = getItemsList("Weapon");
        baby_list = getItemsList("Baby");


        initializeLists("hats", hats_list);
        initializeLists("weapons", weapons_list);
        initializeLists("baby", baby_list);

        tabsList.Add(hats);
        tabsList.Add(weapons);
        tabsList.Add(baby);

        foreach (GameObject i in tabsList)
        {
            i.SetActive(false);
        }
        tabsList[0].SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {

    }

    List<GameObject> getItemsList(string tag)
    {
        JSONObject jsonmessage = new JSONObject(); 
        // Sample: How to get shop items. Categories are "Hat", "Baby", "Weapon" //
        List<string> tags = new List<string>();
        tags.Add(tag);
        new ListVirtualGoodsRequest()
            .SetIncludeDisabled(false)
            .SetTags(tags) // set this to Tag above^
            .Send((response) =>
            {
                Debug.Log("Requesting Virtual Goods...");
                if (!response.HasErrors)
                {
                    Debug.Log("Got virtual goods details");
                    Debug.Log(response.JSONString);
                    jsonmessage = new JSONObject(response.JSONString);
                    Debug.Log(jsonmessage["virtualGoods"]);
                    Debug.Log("End virtual goods details");
                }
                else
                {
                    Debug.Log("Error Receiving virtual goods details");
                }
            });
        Debug.Log("MEOWMEOW");
        Debug.Log("ITS HERE: " + jsonmessage["virtualGoods"][1]["shortCode"]);
        Debug.Log("ITS HERE 2: " + jsonmessage["virtualGoods"].Count);

        return null;
    }

    void initializeLists(string name, List<GameObject> list)
    {
        int index = 0;
        for (int j = 0; j < System.Math.Ceiling(list.Count / 4.0); j++)
        {
            for (int k = 0; k < 4; k++)
            {
                if (index != list.Count)
                {
                    GameObject test = Instantiate(list[index]);
                    test.transform.SetParent(GameObject.Find("Canvas/" + name).transform);
                    test.transform.position = new Vector3(37.5f + (k * 75), 82.5f + (j * -75), 0);
                    test.transform.localRotation = Quaternion.identity;
                    index += 1;

                }
            }
        }
    }

    void initializeEquipped()
    {
        string equipped = PlayerPrefs.GetString("equipped");
        //Debug.Log(equippedList2);

        string[] equippedList;

        equipped = equipped.Substring(10, equipped.Length - 12);
        equippedList = equipped.Split(' ');
        for (int i = 0; i < equippedList.Length; i++)
        {
            equippedList[i] += "_ON";
        }

        GameObject hat = Instantiate(Resources.Load("hats/" + equippedList[0], typeof(GameObject))) as GameObject;
        hat.transform.position = new Vector3(-220, -30, 0);
        hat.transform.localRotation = Quaternion.identity;
        hat.tag = "hat";

        GameObject weapon = Instantiate(Resources.Load("weapons/" + equippedList[1], typeof(GameObject))) as GameObject;
        weapon.transform.position = new Vector3(-220, -30, 0);
        weapon.transform.localRotation = Quaternion.identity;
        weapon.tag = "weapon";

        GameObject baby = Instantiate(Resources.Load("baby/" + equippedList[2], typeof(GameObject))) as GameObject;
        baby.transform.position = new Vector3(-80, -45, 0);
        baby.transform.localRotation = Quaternion.identity;
        baby.tag = "baby";
    }
}
