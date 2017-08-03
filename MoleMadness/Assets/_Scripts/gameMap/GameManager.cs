using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;

public class GameManager
{

    public enum GameState { SPAWNINGMOTHER, SPAWNINGBABY, PLAYERTURN, OPPONENTTURN, RESPAWNBABY };
    public enum Characters { MOTHER, BABY }
    public enum MoveResult { NOTHING, HITBABY }

    public GameState currentGameState;
    public static GameManager gameManagerinstance;
    public static MapManager mapManagerInstance;
    public static bool player1;
    public static int movesLeft;
    public static bool myTurn;
    public static int myBabyHealth;
    public static int oppBabyHealth;

    public GameManager(GameState gameState)
    {
        currentGameState = gameState;
        if (gameManagerinstance == null)
        {
            gameManagerinstance = this;
        }
        if (mapManagerInstance == null)
        {
            mapManagerInstance = MapManager.getInstance();
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

    public static void initGame(JSONObject jsonmessage)
    {
        // init new game
        Debug.Log(jsonmessage);
        string seed = jsonmessage["challenge"]["challengeId"].ToString().Replace("\"", "");
        Debug.Log(seed);
        PlayerPrefs.SetString("GameSeed", seed); //use this as seed for match
        string myId = PlayerPrefs.GetString("playerId").Replace("\"", "");
        Debug.Log(string.Format("myID: {0}", myId));
        string player1Id = jsonmessage["challenge"]["scriptData"]["player1"].ToString().Replace("\"", "");
        Debug.Log(string.Format("player1Id: {0}", player1Id));
        if (myId == player1Id)
        {
            PlayerPrefs.SetInt("player1", 1);
        }
        else
        {
            PlayerPrefs.SetInt("player1", 0);
        }

        // Load Game Map first to init mapManagerInstance
        Debug.Log("Loading Game Map");
        SceneManager.LoadScene("Game Map");
    }

    public static void initText()
    {
        // init Baby Health
        myBabyHealth = 3;
        oppBabyHealth = 3;
        mapManagerInstance.myBabyText.text = "My Baby: " + myBabyHealth;
        mapManagerInstance.oppBabyText.text = "Opp Baby: " + oppBabyHealth;

        // init moves
        movesLeft = 0;
        mapManagerInstance.moveText.text = "Move: " + movesLeft;

        // set Instruction to place Mother Mole
        mapManagerInstance.instructionText.text = "Place Mother";
    }

    public static void initPosition(int MotherX, int MotherZ, int BabyX, int BabyZ)
    {
        string cid = getChallengeId();
        Debug.Log(string.Format("Init mother at {0},{1} and baby at {2},{3} using challengeId {4}", MotherX, MotherZ, BabyX, BabyZ, cid));
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
                   if (PlayerPrefs.GetInt("player1") == 1)
                   {
                       player1 = true;
                       myTurn = true;
                       mapManagerInstance.instructionText.text = "Move Mother";
                   }
                   else
                   {
                       player1 = false;
                       myTurn = false;
                       mapManagerInstance.instructionText.text = "Opponent's turn";
                   }
               }
               else
               {
                   Debug.Log("Error with Positions");
                   Debug.Log(response.JSONString);
               }
           });
    }

    public static void sendMove(int x, int z)
    {
        // send movement to server to check for feedback
        new LogChallengeEventRequest().SetEventKey("action_MOVECHECK")
            .SetEventAttribute("challengeInstanceId", getChallengeId())
            .SetEventAttribute("posX", x)
            .SetEventAttribute("posY", z)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    //Debug.Log(response.JSONString);
                    JSONObject jsonmessage = new JSONObject(response.JSONString);
                    //Debug.Log(jsonmessage["scriptData"]);
                    if (jsonmessage["scriptData"]["Result"].ToString() == "Hit Baby")
                    {
                        Debug.Log("Opponent baby mole hit");
                        oppBabyHealth -= 1;
                        mapManagerInstance.oppBabyText.text = "Opp Baby: " + oppBabyHealth;
                    }
                    else
                    {
                        Debug.Log("Nothing particular happen");
                    }
                    mapManagerInstance.movePlayer(x, z);
                    movesLeft -= 1;
                    mapManagerInstance.moveText.text = "Move: " + movesLeft;
                }
                else
                {
                    Debug.Log("Unsuccessful move check");
                }
            });

    }

}
