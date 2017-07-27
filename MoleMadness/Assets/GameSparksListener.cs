using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using UnityEngine.SceneManagement;

public class GameSparksListener : MonoBehaviour {
    // Use this for initialization
    private string seed;
    private string myID;
    void Awake()
    {
        ChallengeStartedMessage.Listener += ChallengeStartedMessageHandler;
    }
    void ChallengeStartedMessageHandler(ChallengeStartedMessage message)
    {
        JSONObject jsonmessage = new JSONObject(message.JSONString);
        seed = jsonmessage["challenge"]["challengeId"].ToString();
        Debug.Log(seed);
        PlayerPrefs.SetString("GameSeed",seed); //use this as seed for match
        myID = jsonmessage["challenge"]["playerId"].ToString();
        string player1ID = jsonmessage["challenge"]["player1"].ToString();
        if (myID == player1ID)
        {
            PlayerPrefs.SetInt("player1", 1);
        } else
        {
            PlayerPrefs.SetInt("player1", 0);
        }
        SceneManager.LoadScene(4);
    }
}
