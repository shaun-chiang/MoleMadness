using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
