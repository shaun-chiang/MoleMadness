﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using UnityEngine.SceneManagement;

public class GameSparksListener : MonoBehaviour {
    // Use this for initialization
    private string seed;
    void Awake()
    {
        ChallengeStartedMessage.Listener += ChallengeStartedMessageHandler;
    }
    void ChallengeStartedMessageHandler(ChallengeStartedMessage message)
    {
        JSONObject jsonmessage = new JSONObject(message.JSONString);
        Debug.Log(jsonmessage);
        seed = jsonmessage["challenge"]["challengeId"].ToString().Replace("\"", "");
        Debug.Log(seed);
        PlayerPrefs.SetString("GameSeed",seed); //use this as seed for match
        string myId = PlayerPrefs.GetString("playerId").Replace("\"","");
        Debug.Log(string.Format("myID: {0}", myId));
        string player1Id = jsonmessage["challenge"]["scriptData"]["player1"].ToString().Replace("\"", "");
        Debug.Log(string.Format("player1Id: {0}", player1Id));
        if (myId == player1Id)
        {
            PlayerPrefs.SetInt("player1", 1);
        } else
        {
            PlayerPrefs.SetInt("player1", 0);
        }
        Debug.Log("Loading Game Map");
        SceneManager.LoadScene("Game Map");
    }
}
