using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;

public class changeClothes : MonoBehaviour {


    // POSITIONS
    // for equipping: -220, -30
    // baby: -80, -45 

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void changeHat(GameObject clothes)
    {
        GameObject.Destroy(GameObject.FindGameObjectWithTag("hat"));

        GameObject test = Instantiate(clothes);
        test.transform.position = new Vector3(-220, -30, 0);
        test.transform.localRotation = Quaternion.identity;
        test.tag = "hat";
    }

    public void changeWeapon(GameObject clothes)
    {
        GameObject.Destroy(GameObject.FindGameObjectWithTag("weapon"));

        GameObject test = Instantiate(clothes);
        test.transform.position = new Vector3(-220, -30, 0);
        test.transform.localRotation = Quaternion.identity;
        test.tag = "weapon";
    }

    public void changeBaby(GameObject baby)
    {
        GameObject.Destroy(GameObject.FindGameObjectWithTag("baby"));

        GameObject test = Instantiate(baby);
        test.transform.position = new Vector3(-80, -45, 0);
        test.transform.localRotation = Quaternion.identity;
        test.tag = "baby";
    }

    public void equipItems()
    {
        string equipped = "";

        // get current config 
        foreach(Transform child in GameObject.Find("mole").transform) 
        {
            equipped += " " + child.name.Substring(0, child.name.Length - 10);
        }

        //this doesnt run??
        new LogEventRequest().SetEventKey("EQUIP_ITEMS")
        .SetEventAttribute("newEquip", equipped.Substring(1)) // change string to whatever configuration you need
        .Send((response) =>
        {
            if (!response.HasErrors)
            {
                Debug.Log("Equipped");
                SceneManager.LoadScene("PreMatch");
            }
            else
            {
                Debug.Log("Error Equipping");
            }
        });

    }

    public void cancelEquip()
    {
        SceneManager.LoadScene("PreMatch");
    }


}
