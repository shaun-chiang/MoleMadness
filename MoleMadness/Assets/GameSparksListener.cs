using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;

public class GameSparksListener : MonoBehaviour {

    // Use this for initialization
    void Awake()
    {
        Debug.Log("GameSparkListener Awake!");
        if (!GameSparksManager.ListenersInitialized)
        {
            ChallengeStartedMessage.Listener += ChallengeStartedMessageHandler;
            ScriptMessage.Listener += GetMessages;
            GameSparksManager.ListenersInitialized = true;
            Debug.Log("Listeners Added!");
        } else
        {
            Debug.Log("Listeners had already been initialized");
        }
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
            string playerEnded = jsonmessage["data"]["TURNENDED"].ToString().Replace("\"", "");
            string myId = PlayerPrefs.GetString("playerId").Replace("\"", "");
            print("playerEnded: " + playerEnded);
            print("myId :" + myId);
            Debug.Log(string.Format("Going in to getMessage Checks! Turn: {0},State: {1}, You ended: {2}", GameManager.currentGameTurn, GameManager.currentGameState, playerEnded == myId));
            if (playerEnded != myId && GameManager.currentGameTurn == GameManager.GameTurn.OPPONENTTURN)
            {
                // opponent ended
                Debug.Log("Your Opponent ended his turn, it is now your turn.");
                GameManager.startTurn();
                GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);

                //if (GameManager.currentGameState == GameManager.GameState.SPAWNINGMOTHER)
                //{
                //    // you have yet to spawn mother and baby
                //    Debug.Log("Your turn to spawn Mother and Baby");
                //    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
                //}
                //else if (GameManager.currentGameState == GameManager.GameState.ACTIVE && GameManager.currentGameTurn == GameManager.GameTurn.OPPONENTTURN)
                //{
                //    // During Active Turns
                //    Debug.Log("Your turn started.");
                //    GameManager.startTurn();
                //}
            }
            else if (playerEnded != myId && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
            {
                // opponent ended while it is on your turn
                Debug.Log("Your opponent ended while it is on your turn. HOW IS THAT POSSIBLE?");
            } else if (playerEnded == myId && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
            {
                // Times up for your turn
                Debug.Log("Times up, your turn ended.");
                GameManager.endTurn();
                GameManager.setTurn(GameManager.GameTurn.OPPONENTTURN);
            } else if (playerEnded == myId && GameManager.currentGameTurn == GameManager.GameTurn.OPPONENTTURN)
            {
                // you ended on your own, this is just a callback to both players
                Debug.Log("You ended your turn.");
            }
            else
            {
                Debug.Log(string.Format("Out of condition checks! Turn: {0},State: {1}, You ended: {2}", GameManager.currentGameTurn, GameManager.currentGameState, playerEnded == myId));
            }
        } else if (message.ExtCode == "FieldChanged")
        {
            JSONObject jsonmessage = new JSONObject(message.JSONString);
            print("Field Change Detected");
            print(jsonmessage);
        }

    }

}
