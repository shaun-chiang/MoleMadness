using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using UnityEngine.SceneManagement;

public class GameSparksListener : MonoBehaviour {

    // Use this for initialization
    void Awake()
    {
        Debug.Log("GameSparkListener Awake!");
        if (!GameSparksManager.ListenersInitialized)
        {
            ChallengeStartedMessage.Listener += ChallengeStartedMessageHandler;
            ScriptMessage.Listener += GetMessages;
            ChallengeWonMessage.Listener += GetWinMessages;
            ChallengeLostMessage.Listener += GetLostMessages;
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
        if (SceneManager.GetActiveScene().name=="Matchmaking") {
            GameManager.initGame(jsonmessage);
        }
    }

    public void GetWinMessages(ChallengeWonMessage message)
    {
        SceneManager.LoadScene("PostMatchWin");
    }
    public void GetLostMessages(ChallengeLostMessage message)
    {
        SceneManager.LoadScene("PostMatchLose");
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

                if (GameManager.currentGameState == GameManager.GameState.RESPAWNBABY)
                {
                    // You need to respawn your baby, start respawn timer instead
                    Debug.Log("Your turn to respawn baby mole");
                    GameManager.startRespawnTimer();
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
                } else if (GameManager.timerState == GameManager.TimerState.OPPRESPAWNTIMER)
                {
                    Debug.Log("Your opponent had respawned the baby mole, it is your turn now.");
                    GameManager.timeLeft = GameManager.timeLeftCache;
                    GameManager.timeLeftCache = -1;
                    GameManager.timerState = GameManager.TimerState.YOURTIMER;
                    GameManager.startTimer(GameManager.timeLeft);
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
                } else if (GameManager.currentGameState == GameManager.GameState.SPAWNINGMOTHER)
                {
                    Debug.Log("Your turn to spawn Mother Mole and Baby Mole.");
                    // Do not start timer
                    GameManager.setTurn(GameManager.GameTurn.PLAYERTURN);
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
                }
                else
                {
                    GameManager.endTurn();
                }
                
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
                    // here 1
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
