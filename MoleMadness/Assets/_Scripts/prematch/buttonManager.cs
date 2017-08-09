using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class buttonManager : MonoBehaviour {


    public void loadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
