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
			int power33 = int.Parse(jsonmessage["data"]["result"]["(3,3)"].ToString().Replace("\"", ""));
			int power36 = int.Parse(jsonmessage["data"]["result"]["(3,6)"].ToString().Replace("\"", ""));
			int power66 = int.Parse(jsonmessage["data"]["result"]["(6,6)"].ToString().Replace("\"", ""));
			int power63 = int.Parse(jsonmessage["data"]["result"]["(6,3)"].ToString().Replace("\"", ""));
			MapManager.spawnCoor["3,3"] = (GameManager.Powers)power33;
			MapManager.spawnCoor["3,6"] = (GameManager.Powers)power36;
			MapManager.spawnCoor["6,6"] = (GameManager.Powers)power66;
			MapManager.spawnCoor["6,3"] = (GameManager.Powers)power63;
			Debug.Log("powerup33: " + power33 + "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
			Debug.Log("powerup63: " + power63 + "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
			Debug.Log("powerup36: " + power36 + "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
			Debug.Log("powerup66: " + power66 + "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

            print("playerEnded: " + playerEnded);
            print("myId :" + myId);
            Debug.Log(string.Format("Going in to getMessage Checks! Turn: {0},State: {1}, You ended: {2}", GameManager.currentGameTurn, GameManager.currentGameState, playerEnded == myId));
            if (playerEnded != myId && GameManager.currentGameTurn == GameManager.GameTurn.OPPONENTTURN)
            {
                // opponent ended
                Debug.Log("Your Opponent ended his turn, it is now your turn.");

                if (GameManager.currentGameState == GameManager.GameState.RESPAWNBABY)
                {
                    // You need to respawn your baby, start respawn timer instead
                    Debug.Log("Your turn to respawn baby mole");
                    GameManager.startRespawnTimer();
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
<<<<<<< HEAD
=======
                } else if (GameManager.timerState == GameManager.TimerState.OPPRESPAWNTIMER)
                {
                    Debug.Log("Your opponent had respawned the baby mole, it is your turn now.");
                    GameManager.timeLeft = GameManager.timeLeftCache;
                    GameManager.timeLeftCache = -1;
                    GameManager.timerState = GameManager.TimerState.YOURTIMER;
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
                    if (GameManager.movesLeft <= 0)
                    {
                        GameManager.endTurn();
                    } else
                    {
                        GameManager.startTimer(GameManager.timeLeft);
                    }
                } else if (GameManager.currentGameState == GameManager.GameState.SPAWNINGMOTHER)
                {
                    Debug.Log("Your turn to spawn Mother Mole and Baby Mole.");
                    // Do not start timer
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
>>>>>>> ab29fc25c13e4ecbc94f333f3b166cef7706e931
                }
                else
                {
                    Debug.Log("Your Opponent ended his turn, it is now your turn.");
                    GameManager.startTurn();
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
                }
            }
            else if (playerEnded != myId && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
            {
                // opponent ended while it is on your turn
                Debug.Log("Your opponent ended while it is on your turn. HOW IS THAT POSSIBLE?");
            } else if (playerEnded == myId && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
            {
                // Your turn ended
                Debug.Log("Your turn endedd.");

                if (GameManager.currentGameState == GameManager.GameState.RESPAWNBABY)
                {
                    Debug.Log("randomly respawning baby.");
                    MapManager.getInstance().randomBabyRespawn();

                    // this stop timer fails normally if player respawn baby within time.
                    // it acts as a fail safe
                    // actually no need end cause startrespawntimer would have ended that
                    //GameManager.stopTimer();

                } else if (GameManager.currentGameState == GameManager.GameState.WAITING || GameManager.p2JustInit)
                {
                    // Do not call end turn
                    // stop timer is called to trigger this when waiting for opponent to init position
                    Debug.Log("stop timer is called to trigger this during start of game placing");
                    if (GameManager.p2JustInit)
                    {
                        GameManager.p2JustInit = false;
                        GameManager.timerState = GameManager.TimerState.OPPTIMER;
                        GameManager.timeLeft = GameManager.TURNDURATION;
                    }
                } else if (GameManager.timerState == GameManager.TimerState.OPPRESPAWNTIMER)
                {
                    // end turn triggered from stop timer to pass turn to opponent for respawn
                    Debug.Log("end turn triggered from stop timer to pass turn to opponent for respawn");
                }
                else
                {
                    // check if it is called by stop timer
                    GameManager.endTurn();

                }
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
            string playerChanged = jsonmessage["data"]["playerChanged"].ToString().Replace("\"", "");
            string newField = jsonmessage["data"]["newField"].ToString().Replace("\"", "");
            string myId = PlayerPrefs.GetString("playerId").Replace("\"", "");
            if (playerChanged != myId)
            {
                if (newField == "init")
                {
                    Debug.Log("Opponent initialized his mother mole and baby mole");
                } else if (newField == "respawn")
                {
                    Debug.Log("Opponent respawn his baby mole");
                } else
                {
                    Debug.Log("Opponent made a move");
                    MapManager mapManager = MapManager.getInstance();
                    Tile.TileType[,] tileMap = mapManager.constructTileMapFromString(newField);
                    mapManager.loadTileMap(tileMap);
                    if (mapManager.isMyBabyHit())
                    {
                        GameManager.timeLeftCache = GameManager.timeLeft;
                        GameManager.timerState = GameManager.TimerState.YOURRESPAWNTIMER;
                        GameManager.timeLeft = GameManager.RESPAWNDURATION;
                        GameManager.myBabyHealth -= 1;
                        mapManager.myBabyText.text = "My Baby: " + GameManager.myBabyHealth.ToString();
                        GameManager.currentGameState = GameManager.GameState.RESPAWNBABY;
                    }
                }
            }
        }

    }

}
