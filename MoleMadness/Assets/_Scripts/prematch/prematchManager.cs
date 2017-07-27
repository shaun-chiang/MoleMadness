using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prematchManager : MonoBehaviour {



	// Use this for initialization
	void Start () {
        string equipped = PlayerPrefs.GetString("equipped");
        //Debug.Log(equippedList2);

        string[] equippedList;

        equipped = equipped.Substring(1, equipped.Length-2);
        equippedList = equipped.Split(' ');
        for(int i= 0; i < equippedList.Length; i++) 
        {
            equippedList[i] += "_ON";
        }

        GameObject hat = Instantiate(Resources.Load("hats/" + equippedList[0], typeof(GameObject))) as GameObject;
        hat.transform.position = new Vector3(-220, -30, 0);
        hat.transform.localRotation = Quaternion.identity;

        GameObject weapon = Instantiate(Resources.Load("weapons/" + equippedList[1], typeof(GameObject))) as GameObject;
        weapon.transform.position = new Vector3(-220, -30, 0);
        weapon.transform.localRotation = Quaternion.identity;

        GameObject baby = Instantiate(Resources.Load("baby/" + equippedList[2], typeof(GameObject))) as GameObject;
        baby.transform.position = new Vector3(-80, -45, 0);
        baby.transform.localRotation = Quaternion.identity;

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
