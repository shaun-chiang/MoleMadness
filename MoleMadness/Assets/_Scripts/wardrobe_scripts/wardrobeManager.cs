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

    public int hatsinitialized = 0;
    public int weaponsinitialized = 0;
    public int babyinitialized = 0; 

    public List<GameObject> tabsList = new List<GameObject>();

    public List<string> inventoryList;


    //POSITIONS 
    // for buttons: 37.5, + 75 (x); 82.5, -75 (y)

    // somehow need to get current configuration of accessories? 

    // Use this for initialization


    void Start () {
        
        string inventory = PlayerPrefs.GetString("virtualgoods");
        initializeInventory(inventory);

        initializeEquipped();
       
        getItemsList("Hat", hats_list, "hats");
        getItemsList("Weapon", weapons_list, "weapons");
        getItemsList("Baby", baby_list, "baby");
        
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
        if (hatsinitialized == 0)
        {
            if (hats_list.Count > 0)
            {
                initializeLists(hats, hats_list);
                hatsinitialized = 1;

            }
        }
        if (weaponsinitialized == 0)
        {
            if (weapons_list.Count > 0)
            {
                initializeLists(weapons, weapons_list);
                weaponsinitialized = 1;

            }
        }
        if (babyinitialized == 0)
        {
            if (baby_list.Count > 0)
            {
                initializeLists(baby, baby_list);
                babyinitialized = 1;

            }
        }
    }

    void initializeInventory(string inventory)
    {
        inventory = inventory.Substring(1, inventory.Length - 2);
        string[] list = inventory.Split(',');
        for (int i = 0; i<list.Length; i++)
        {
            inventoryList.Add(list[i].Substring(1, list[i].Length - 4));
            Debug.Log(inventoryList[i]);
        }

    }

    void getItemsList(string tag, List<GameObject> list, string folder)
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
                    List<GameObject> item_list = new List<GameObject>();
                    Debug.Log("Got virtual goods details");
                    jsonmessage = new JSONObject(response.JSONString);
                    for(int i = 0; i<jsonmessage["virtualGoods"].Count;i++)
                    {
                        string item = jsonmessage["virtualGoods"][i]["shortCode"].ToString();
                        string price = jsonmessage["virtualGoods"][i]["currency1Cost"].ToString();
                        item = item.Substring(1, item.Length - 2);
                        // check if item is in inventory list or not; if yes: load as per normal; else: load as shop
                        if(inventoryList.Contains(item))
                        {
                            list.Add(Resources.Load(folder + "/" + item, typeof(GameObject)) as GameObject);
                        } else
                        {
                            list.Add(Resources.Load(folder + "/" + item + "_SHOP", typeof(GameObject)) as GameObject);
                        }
                    }
                    

                }
                else
                {
                    Debug.Log("Error Receiving virtual goods details");
                }
            });
    }

    void initializeLists(GameObject name, List<GameObject> list)
    {

        int index = 0;
        for (int j = 0; j < System.Math.Ceiling(list.Count / 4.0); j++)
        {
            for (int k = 0; k < 4; k++)
            {
                if (index != list.Count)
                {
                    Debug.Log(list[index].name);
                    GameObject test = Instantiate(list[index]);
                    
                    test.transform.SetParent(name.transform);
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
        hat.transform.SetParent(GameObject.Find("mole").transform);
        hat.transform.localRotation = Quaternion.identity;
        hat.tag = "hat";

        GameObject weapon = Instantiate(Resources.Load("weapons/" + equippedList[1], typeof(GameObject))) as GameObject;
        weapon.transform.position = new Vector3(-220, -30, 0);
        weapon.transform.SetParent(GameObject.Find("mole").transform);
        weapon.transform.localRotation = Quaternion.identity;
        weapon.tag = "weapon";

        GameObject baby = Instantiate(Resources.Load("baby/" + equippedList[2], typeof(GameObject))) as GameObject;
        baby.transform.position = new Vector3(-80, -45, 0);
        baby.transform.SetParent(GameObject.Find("mole").transform);
        baby.transform.localRotation = Quaternion.identity;
        baby.tag = "baby";
    }
}
