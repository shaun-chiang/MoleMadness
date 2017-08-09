using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class buyClothes : MonoBehaviour {
    
    public Font font;
    public int gems;
    public string itemname;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void buy(int cost)
    {

        gems = int.Parse(PlayerPrefs.GetString("gems"));

        GameObject popup = Instantiate(Resources.Load("popup", typeof(GameObject)) as GameObject);
        popup.transform.SetParent(GameObject.Find("Canvas").transform);
        popup.transform.position = new Vector3(150, 10, 0);
        popup.transform.localRotation = Quaternion.identity;
        popup.SetActive(true);

        //0,63,0 (text) 
        GameObject nameGO = new GameObject("name");
        Text name = nameGO.AddComponent<Text>();
        RectTransform trans = name.GetComponent(typeof(RectTransform)) as RectTransform;
        trans.sizeDelta = new Vector2(200, 100);
        name.text = this.name.Substring(0, this.name.Length - 12).Replace('_', ' ');
        name.transform.SetParent(popup.transform);
        name.color = Color.black;
        name.font = font; 
        name.transform.position = new Vector3(150, 55, 0);
        name.transform.localRotation = Quaternion.identity;

        GameObject img = Instantiate(Resources.Load(this.name.Substring(0, this.name.Length - 12) + "_IMG", typeof(GameObject)) as GameObject);
        img.transform.SetParent(popup.transform);
        img.transform.position = new Vector3(100, 48, 0);
        img.transform.localRotation = Quaternion.identity;

        GameObject yesButtonGO = Instantiate(Resources.Load("yesButton", typeof(GameObject)) as GameObject);
        yesButtonGO.transform.SetParent(popup.transform);
        yesButtonGO.transform.position = new Vector3(80, -55, 0);
        yesButtonGO.transform.localRotation = Quaternion.identity;
        yesButtonGO.GetComponentInChildren<Text>().text = "Yes (" + cost + " gems)";

        GameObject numGemsGO = new GameObject("numGems");
        Text numGems = numGemsGO.AddComponent<Text>();
        RectTransform trans2 = numGems.GetComponent(typeof(RectTransform)) as RectTransform;
        trans2.sizeDelta = new Vector2(200, 100);
        numGems.text = "You have " + gems + " gems.";
        numGems.transform.SetParent(popup.transform);
        numGems.color = Color.black;
        numGems.font = font;
        numGems.fontSize = 12;
        numGems.transform.position = new Vector3(150, -125, 0);
        numGems.transform.localRotation = Quaternion.identity;



    }

    public void yes()
    {
        GameObject popupGO = GameObject.Find("Canvas/popup(Clone)");
        foreach (Transform child in popupGO.transform)
        {
            if(child.name.Contains("IMG"))
            {
                itemname = child.name.Substring(0, child.name.Length - 11);
            }
        }
        new BuyVirtualGoodsRequest()
            .SetCurrencyType(1) //Using Gems,
            .SetQuantity(1) //Buy 1 of...
            .SetShortCode(itemname) //Change to shortcode of item (see table)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Bought Item");
                    PlayerPrefs.SetString("virtualgoods", PlayerPrefs.GetString("virtualgoods") + " " + itemname);
                    SceneManager.LoadScene("Wardrobe");
                }
                else
                {
                    Debug.Log("Error Buying Item");
                }
            });

    }

    public void no()
    {
        GameObject.Destroy(this.gameObject);
    }
}
