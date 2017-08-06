using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;

public class buyClothes : MonoBehaviour {

    public GameObject popup; 

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void buy(GameObject accessory)
    {
        Debug.Log("buy");
        popup = Instantiate(Resources.Load("popup", typeof(GameObject)) as GameObject);
        popup.transform.SetParent(GameObject.Find("Canvas").transform);
        popup.transform.position = new Vector3(150, 18, 0);
        popup.transform.localRotation = Quaternion.identity;
        popup.SetActive(true);

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
