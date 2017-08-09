using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class achievementListener : MonoBehaviour {

    public Font font;
    
    // Use this for initialization
    public void Start()
    {
        Debug.Log("ACHIEVEMENT meow");
        AchievementEarnedMessage.Listener += GetMessages;
    }

    public void GetMessages(AchievementEarnedMessage message)
    {
        JSONObject jsonmessage = new JSONObject(message.JSONString);
        //do anything with the jsonmessage
        Debug.Log("ACHIEVEMENT " + jsonmessage["summary"].ToString());
        PlayerPrefs.SetString("Achievement", jsonmessage.ToString());
    }


    // Update is called once per frame
    void Update () {
		
	}
}
