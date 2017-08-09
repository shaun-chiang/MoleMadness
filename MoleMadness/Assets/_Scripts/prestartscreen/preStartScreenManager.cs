using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class preStartScreenManager : MonoBehaviour {

    public GameObject anim;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(anim.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            SceneManager.LoadScene("StartScreen");
            
        }
	}

    public void skip()
    {
        {
            SceneManager.LoadScene("StartScreen");
        }
    }
}
