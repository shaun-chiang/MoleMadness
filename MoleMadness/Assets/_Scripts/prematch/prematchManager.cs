using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class prematchManager : MonoBehaviour {

    public Font font;

    public int equippedAlr = 0;

	// Use this for initialization
	void Start () {

        GameObject nameGO = new GameObject("name");
        Text name = nameGO.AddComponent<Text>();
        RectTransform trans = name.GetComponent(typeof(RectTransform)) as RectTransform;
        trans.sizeDelta = new Vector2(200, 100);
        name.text = PlayerPrefs.GetString("PlayerName");
        name.transform.SetParent(GameObject.Find("Canvas").transform);
        name.color = Color.black;
        name.font = font;
        name.transform.position = new Vector3(-115, 97, 0);
        name.transform.localRotation = Quaternion.identity;

    }
	
	// Update is called once per frame
	void Update () {
		if(equippedAlr ==0 )
        {
            Debug.Log("EQUIPPED = " + PlayerPrefs.GetString("equipped"));
            if (PlayerPrefs.GetString("equipped") != null)
            {
                equipItems();
                equippedAlr = 1;
            }
        }
	}

    void equipItems()
    {
        string equipped = PlayerPrefs.GetString("equipped");

        string[] equippedList;
        Debug.Log(equipped);
        equipped = equipped.Substring(10, equipped.Length - 12);
        equippedList = equipped.Split(' ');
        for (int i = 0; i < equippedList.Length; i++)
        {
            equippedList[i] += "_ON";
        }

        GameObject hat = Instantiate(Resources.Load("hats/" + equippedList[0], typeof(GameObject))) as GameObject;
        hat.transform.position = new Vector3(-220, -70, 0);
        hat.transform.localRotation = Quaternion.identity;

        GameObject weapon = Instantiate(Resources.Load("weapons/" + equippedList[1], typeof(GameObject))) as GameObject;
        weapon.transform.position = new Vector3(-220, -70, 0);
        weapon.transform.localRotation = Quaternion.identity;

        GameObject baby = Instantiate(Resources.Load("baby/" + equippedList[2], typeof(GameObject))) as GameObject;
        baby.transform.position = new Vector3(-80, -85, 0);
        baby.transform.localRotation = Quaternion.identity;
    }
}
