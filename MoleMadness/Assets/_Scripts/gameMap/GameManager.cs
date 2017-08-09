using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;
using UnityEngine.SceneManagement;

public class GameManager
{

    public enum GameState { SPAWNINGMOTHER, SPAWNINGBABY, RESPAWNBABY, WAITING , ACTIVE};
    public enum GameTurn { PLAYERTURN, OPPONENTTURN};
    public enum Characters { MOTHER, BABY }
    public enum MoveResult { NOTHING, HITBABY }
	public enum Powers { NOTHING, EXCAVATOR, MOLEINSTINCT, EARTHSHAKE, DIAGONOL}

    public static GameState currentGameState;
    public static GameTurn currentGameTurn;
    public static GameManager gameManagerinstance;
    public static MapManager mapManagerInstance;
    public static bool player1;
    public static bool p2ready;
    public static int movesLeft;
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
            currentGameTurn = GameTurn.PLAYERTURN;
        }
        else
        {
            PlayerPrefs.SetInt("player1", 0);
            currentGameTurn = GameTurn.OPPONENTTURN;
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

    // called only when initialize=ing mother and baby position
    public static void initPosition(Vector3 motherPos, Vector3 babyPos)
    {
        string cid = getChallengeId();
        Debug.Log(string.Format("Init mother at {0},{1} and baby at {2},{3} using challengeId {4}", motherPos.x, motherPos.z, babyPos.x, babyPos.z, cid));
        new LogChallengeEventRequest().SetEventKey("action_SETPOS")
           .SetEventAttribute("challengeInstanceId", cid)
           .SetEventAttribute("babyX", (long) babyPos.x)
           .SetEventAttribute("babyY", (long) babyPos.z)
           .SetEventAttribute("motherX", (long) motherPos.x)
           .SetEventAttribute("motherY", (long) motherPos.z)
           .Send((response) =>
           {
               if (!response.HasErrors)
               { 
                   if (PlayerPrefs.GetInt("player1") == 1)
                   {
                       Debug.Log("Positions Set for player 1");
                       player1 = true;
                       endTurn();
                       mapManagerInstance.instructionText.text = "Waiting for opponent";
                       currentGameState = GameState.WAITING;

                       //mapManagerInstance.instructionText.text = "Move Mother";
                   }
                   else
                   {
                       Debug.Log("Positions Set for player 2");
                       player1 = false;
                       // player has already init mother and baby mole
                       if (currentGameTurn == GameTurn.PLAYERTURN)
                       {
                           mapManagerInstance.instructionText.text = "Opponent's Turn";
                           endTurn();
                       } else
                       {
                           // wait for first player to finish
                           // send positions in listener logic
                           mapManagerInstance.instructionText.text = "Waiting for opponent";
                           p2ready = true;
                       }
                   }
               }
               else
               {
                   Debug.Log("Error with Positions");
                   Debug.Log(response.JSONString);
               }
           });
    }

    public static void sendMoveCheck(int x, int z)
    {
        // send target destination to server to check for interaction with baby or other objects
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
                    if (movesLeft == 0)
                    {
                        endTurn();
                    }
                }
                else
                {
                    Debug.Log("Unsuccessful move check");
                }
            });

    }

    public static void sendMoveUpdate(Vector3 motherPos, Vector3 babyPos, string mapState)
    {
        Debug.Log("map: " + mapState);
        
        // send movement updates to server to update map, baby and mother positions
        new LogChallengeEventRequest().SetEventKey("action_SETPOSFIELD")
            .SetEventAttribute("challengeInstanceId", getChallengeId())
            .SetEventAttribute("babyX", (long)babyPos.x)
            .SetEventAttribute("babyY", (long)babyPos.z)
            .SetEventAttribute("motherX", (long)motherPos.x)
            .SetEventAttribute("motherY", (long)motherPos.z)
            .SetEventAttribute("field", mapState)
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

    public static void startTurn()
    {
        new LogChallengeEventRequest().SetEventKey("action_STARTTURN")
            .SetEventAttribute("challengeInstanceId",getChallengeId())
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Successful Start Turn");
                    // init moves
                    movesLeft = 3;
                    mapManagerInstance.moveText.text = "Move: " + movesLeft;

                    // set Instruction to place Mother Mole
                    mapManagerInstance.instructionText.text = "Move Mother";
                }
                else
                {
                    Debug.Log("Unsuccessful Start Turn");
                }
            });
    }

    public static void endTurn()
    {
        new LogChallengeEventRequest().SetEventKey("action_ENDTURN")
            .SetEventAttribute("challengeInstanceId",getChallengeId())
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Successful End Turn");
                }
                else
                {
                    Debug.Log("Unsuccessful End Turn");
                }
            });

    }

    public static void startRespawnTimer()
    {
        new LogChallengeEventRequest().SetEventKey("START_TIMER")
            .SetEventAttribute("challengeInstanceId",getChallengeId())
            .SetEventAttribute("SECONDS",15)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Successful Start timer");
                }
                else
                {
                    Debug.Log("Unsuccessful Start timer");
                }
            });
    }

    public static void stopRespawnTimer()
    {
        new LogChallengeEventRequest().SetEventKey("STOP_TIMER")
            .SetEventAttribute("challengeInstanceId",getChallengeId())
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Successful stop respawn timer");
                }
                else
                {
                    Debug.Log("Unsuccessful stop respawn timer");
                }
            });

    }
}
