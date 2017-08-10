using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class postmatchButton : MonoBehaviour {
    public void new_match()
    {
        SceneManager.LoadScene("Matchmaking");
    }
	public void quit()
    {
        SceneManager.LoadScene("PreMatch");
    }
}
