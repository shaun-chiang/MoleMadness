using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wardrobeManager : MonoBehaviour {

    // preferably can use a getAccessories() method or something to get these lists? 
    // so like.. hats_list will be accessoriesList[0] and clothes_list will be accessoriesList[1] etc... 
    public List<GameObject> hats_list = new List<GameObject>();
    public List<GameObject> clothes_list = new List<GameObject>();
    public List<GameObject> baby_list = new List<GameObject>();

    //POSITIONS 
    // for buttons: 37.5, + 75 (x); 82.5, -75 (y)



    // Use this for initialization
    void Start () {
        initializeLists("hats", hats_list);
        initializeLists("clothes", clothes_list);
        initializeLists("baby", baby_list);
    }
	
	// Update is called once per frame
	void Update () {
		
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
}
