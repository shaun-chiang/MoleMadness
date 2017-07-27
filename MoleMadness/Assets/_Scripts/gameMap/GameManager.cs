using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;

public class GameManager{

    public enum GameState { SPAWNINGMOTHER, SPAWNINGBABY, PLAYERTURN, OPPONENTTURN, RESPAWNBABY };
    public enum Characters { MOTHER, BABY }

    public GameState currentGameState;
    public static GameManager gameManagerinstance;
    public static MapManager mapManagerinstance;
    public bool player1;

    public GameManager(GameState gameState)
    {
        currentGameState = gameState;
        // for testing, to be init from gamespark
        player1 = true;
        if (gameManagerinstance == null)
        {
            gameManagerinstance = this;
        }
    }

    public static GameManager getInstance()
    {
        return gameManagerinstance;
    }

    public static string getChallengeId()
    {
        return PlayerPrefs.GetString("GameSeed");
    }

    public static void getPlayerId()
    {
        new AccountDetailsRequest().Send((response) =>
        {
            Debug.Log("Requesting Account Details...");
            if (!response.HasErrors)
            {
                Debug.Log("Got Player Details");
                Debug.Log(response.JSONString);
            }
            else
            {
                Debug.Log("Error Receiving Account Details");
            }
        });
    }

    public static void initPosition(int MotherX, int MotherZ, int BabyX, int BabyZ)
    {
        string cid = getChallengeId();
        Debug.Log(string.Format("Init mother at {0},{1} and baby at {2},{3} using challengeId {4}",MotherX,MotherZ,BabyX,BabyZ,cid));
        new LogChallengeEventRequest().SetEventKey("action_SETPOS")
           .SetEventAttribute("challengeInstanceId", cid)
           .SetEventAttribute("babyX", BabyX)
           .SetEventAttribute("babyY", BabyZ)
           .SetEventAttribute("motherX", MotherX)
           .SetEventAttribute("motherY", MotherZ)
           .Send((response) =>
           {
               if (!response.HasErrors)
               {
                   Debug.Log("Positions Set");

               }
               else
               {
                   Debug.Log("Error with Positions");
                   Debug.Log(response.JSONString);
               }
           });


    }

}
