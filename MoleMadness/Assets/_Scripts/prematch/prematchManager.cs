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
        name.color = Color.white;
        name.font = font;
        name.transform.position = new Vector3(-115, 90, 0);
        name.transform.localRotation = Quaternion.identity;

        if(PlayerPrefs.GetString("Achievement")!="")
        {
            JSONObject jsonmessage = new JSONObject(PlayerPrefs.GetString("Achievement"));
            initializeAchievementPopup(jsonmessage);
        }

    }
	
	// Update is called once per frame
	void Update () {
		if(equippedAlr ==0 )
        {
            //Debug.Log("EQUIPPED = " + PlayerPrefs.GetString("equipped"));
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

    void initializeAchievementPopup(JSONObject jsonmessage)
    {
        Debug.Log("ACHIEVEMENT initializing");

        GameObject popup = Instantiate(Resources.Load("achievements/achpopup", typeof(GameObject)) as GameObject, gameObject.transform);

        popup.transform.SetParent(GameObject.Find("Canvas").transform);
        popup.transform.position = new Vector3(50, 0, 0);
        popup.transform.localRotation = Quaternion.identity;
        popup.SetActive(true);

        GameObject msgGO = new GameObject("msg");
        Text msg = msgGO.AddComponent<Text>();
        RectTransform trans = msg.GetComponent(typeof(RectTransform)) as RectTransform;
        trans.sizeDelta = new Vector2(200, 50);
        msg.text = jsonmessage["summary"].ToString().Substring(1, jsonmessage["summary"].ToString().Length-2) ;
        msg.transform.SetParent(popup.transform);
        msg.color = Color.black;
        msg.font = font;
        msg.transform.position = new Vector3(100, 20, 0);
        msg.transform.localRotation = Quaternion.identity;

        string achShort = jsonmessage["achievementShortCode"].ToString();
        achShort = achShort.Substring(1, achShort.Length - 2);
        GameObject img = Instantiate(Resources.Load("achievements/" + achShort + "_IMG", typeof(GameObject)) as GameObject);
        img.transform.SetParent(popup.transform);
        img.transform.position = new Vector3(-70, 0, 0);
        img.transform.localRotation = Quaternion.identity;

        PlayerPrefs.SetString("Achievement", "");
    }

}
