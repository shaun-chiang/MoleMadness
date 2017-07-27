using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;

public class GameManager{

    public enum GameState { SPAWNINGMOTHER, SPAWNINGBABY, PLAYERTURN, OPPONENTTURN, RESPAWNBABY };
    public enum Characters { MOTHER, BABY }

    public GameState currentGameState;
    public static GameManager instance;
    public bool player1;

    public GameManager(GameState gameState)
    {
        currentGameState = gameState;
        // for testing, to be init from gamespark
        player1 = true;
        if (instance == null)
        {
            instance = this;
        }
    }

    public static GameManager getInstance()
    {
        return instance;
    }

    public static void initPosition(int MotherX, int MotherZ, int BabyX, int BabyZ)
    {
        
        new LogChallengeEventRequest().SetEventKey("action_SETPOS")
           .SetEventAttribute("challengeInstanceId","597957ae57203b04d00739ea")
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
               }
           });


    }

}
