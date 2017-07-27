using System.Collections;
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
        seed = jsonmessage["challenge"]["challengeId"].ToString();
        Debug.Log(seed);
        PlayerPrefs.SetString("GameSeed",seed); //use this as seed for match
        SceneManager.LoadScene(4);
    }
}
