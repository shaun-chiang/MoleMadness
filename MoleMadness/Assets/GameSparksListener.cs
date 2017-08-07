using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;

public class GameSparksListener : MonoBehaviour {
    // Use this for initialization
    void Awake()
    {
        ChallengeStartedMessage.Listener += ChallengeStartedMessageHandler;
        ScriptMessage.Listener += GetMessages;
    }
    void ChallengeStartedMessageHandler(ChallengeStartedMessage message)
    {
        JSONObject jsonmessage = new JSONObject(message.JSONString);
        GameManager.initGame(jsonmessage);
    }

    public void GetMessages(ScriptMessage message)
    {
        if (message.ExtCode == "TurnConsumed")
        {
            //Check whose turn ended, if it’s our turn, we play
            JSONObject jsonmessage = new JSONObject(message.JSONString);
            string playerEnded = jsonmessage["data"]["TURNENDED"].ToString();
            //print(playerEnded);
            string myId = PlayerPrefs.GetString("playerId").Replace("\"", "");
            if (playerEnded != myId && GameManager.currentGameTurn == GameManager.GameTurn.OPPONENTTURN)
            {
                // opponent ended
                if (GameManager.p2ready == true)
                {
                    MapManager mapManager = MapManager.getInstance();
                    GameObject baby = mapManager.baby;
                    GameObject mother = mapManager.mother;
                    GameManager.initPosition(mother.transform.position, baby.transform.position);
                }
                else
                {
                    GameManager.startTurn();
                }
            }
        }
    }

}
