using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class buyClothes : MonoBehaviour {

    public GameObject popup;

    public Font font;

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void buy(GameObject sprite)
    {
        Debug.Log(this.name);
        Debug.Log("buy");
        popup = Instantiate(Resources.Load("popup", typeof(GameObject)) as GameObject);
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
        name.transform.position = new Vector3(150, 65, 0);
        name.transform.localRotation = Quaternion.identity;

        GameObject img = Instantiate(sprite);
        img.transform.SetParent(popup.transform);
        img.transform.position = new Vector3(100, 48, 0);
        img.transform.localRotation = Quaternion.identity;
        
        
        

    }

    public void yes()
    {
        Debug.Log("BUYBUY"); 
    }

    public void no()
    {
        popup = this.gameObject;
        GameObject.Destroy(popup);
    }
}
