﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;

public class GameSparksListener : MonoBehaviour {

    // Use this for initialization
    void Awake()
    {
        ChallengeStartedMessage.Listener += ChallengeStartedMessageHandler;
    }
    void ChallengeStartedMessageHandler(ChallengeStartedMessage message)
    {
        JSONObject jsonmessage = new JSONObject(message.JSONString);
        Debug.Log(jsonmessage["challenge"]["challengeId"]); //use this as seed for match
        GameManager gameManager = GameManager.getInstance();
    }
}
