using GameSparks.Api.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager
{

    public enum GameState { SPAWNINGMOTHER, SPAWNINGBABY, RESPAWNBABY, WAITING , ACTIVE};
    public enum GameTurn { PLAYERTURN, OPPONENTTURN};
    public enum Characters { MOTHER, BABY }
    public enum MoveResult { NOTHING, HITBABY }

	public enum Powers { NOTHING, EXCAVATOR, MOLEINSTINCT, EARTHSHAKE, DIAGONAL}
    public enum TimerState { OFF, YOURTIMER, OPPTIMER, YOURRESPAWNTIMER, OPPRESPAWNTIMER }

    public static GameState currentGameState;
    public static GameTurn currentGameTurn;
    public static GameManager gameManagerinstance;
    public static MapManager mapManagerInstance;
    public static bool player1;
    public static bool initPositionComplete;
    public static bool p2JustInit = false;
    public static int movesLeft;
    public static int myBabyHealth;
    public static int oppBabyHealth;
    public static float timeLeft;
    public static float timeLeftCache = -1;
    public static TimerState timerState = TimerState.OFF;

    public const float TURNDURATION = 30;
    public const float RESPAWNDURATION = 10;

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

    public static void resetGame()
    {
        initPositionComplete = false;
        p2JustInit = false;
        timerState = TimerState.OFF;
    }

    public static string getChallengeId()
    {
        return PlayerPrefs.GetString("GameSeed");
    }

    public static void initGame(JSONObject jsonmessage)
    {
        // init new game
        initPositionComplete = false;
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
            player1 = true;
        }
        else
        {
            player1 = false;
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

        Debug.Log("Setting Turn in InitText");
        if (player1)
        {
            // set Instruction to place Mother Mole
            setTurn(GameTurn.PLAYERTURN);
            mapManagerInstance.instructionText.text = "Place Mother";
        } else
        {
            // set Instruction to wait for opponent init mother and baby
            setTurn(GameTurn.OPPONENTTURN);
            mapManagerInstance.instructionText.text = "Waiting for Opponent";
        }
    }

    // called only when initialize=ing mother and baby position
    public static void initPosition(Vector3 motherPos, Vector3 babyPos)
    {
        string cid = getChallengeId();
        Debug.Log(string.Format("Init mother at {0},{1} and baby at {2},{3} using challengeId {4}", (int) motherPos.x, (int) motherPos.z, (int) babyPos.x, (int) babyPos.z, cid));
        new LogChallengeEventRequest().SetEventKey("action_SETPOSFIELD")
           .SetEventAttribute("challengeInstanceId", cid)
           .SetEventAttribute("babyX", (int) babyPos.x)
           .SetEventAttribute("babyY", (int) babyPos.z)
           .SetEventAttribute("motherX", (int) motherPos.x)
           .SetEventAttribute("motherY", (int) motherPos.z)
           .SetEventAttribute("field", "init")
           .Send((response) =>
           {
               if (!response.HasErrors)
               {
                   if (!initPositionComplete)
                   {
                       initPositionComplete = true;
                   }
                   if (player1)
                   {
                       Debug.Log("Positions Set for player 1");
                       stopTimer();
                       currentGameState = GameState.WAITING;
                   }
                   else
                   {
                       Debug.Log("Positions Set for player 2");
                       p2JustInit = true;
                       stopTimer();
                       currentGameState = GameState.ACTIVE;
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
                    Debug.Log(string.Format("MoveCheck at {0},{1}: {2}",x,z, jsonmessage["scriptData"]["Result"].ToString().Replace("\"", "") ));
                    Debug.Log("MoveCheck: " + jsonmessage["scriptData"]["Result"].ToString().Replace("\"", ""));
                    mapManagerInstance.movePlayer(x, z);
                    movesLeft -= 1;
                    mapManagerInstance.moveText.text = "Move: " + movesLeft;
                    if (jsonmessage["scriptData"]["Result"].ToString().Replace("\"", "") == "Hit Baby")
                    {
                        Debug.Log("Opponent baby mole hit");
                        oppBabyHealth -= 1;
                        mapManagerInstance.oppBabyText.text = "Opp Baby: " + oppBabyHealth;
                        timeLeftCache = timeLeft;
                        timerState = TimerState.OPPRESPAWNTIMER;
                        timeLeft = RESPAWNDURATION;
                        stopTimer();
                        mapManagerInstance.instructionText.text = "Waiting for opponent to respawn baby";
                        endTurn();
                    }
                    else
                    {
                        Debug.Log("Nothing particular happen");
                        if (movesLeft == 0)
                        {
                            endTurn();
                        }
                    }
                }
                else
                {
                    Debug.Log("Unsuccessful move check");
                    Debug.Log(response.JSONString);
                }
            });

    }

    public static void sendMoveUpdate(Vector3 motherPos, Vector3 babyPos, string mapState)
    {
        Debug.Log("map: " + mapState);
        Debug.Log(string.Format("Move mother to {0},{1} and baby to {2},{3}", (int)motherPos.x, (int)motherPos.z, (int)babyPos.x, (int)babyPos.z));

        // send movement updates to server to update map, baby and mother positions
        new LogChallengeEventRequest().SetEventKey("action_SETPOSFIELD")
            .SetEventAttribute("challengeInstanceId", getChallengeId())
            .SetEventAttribute("babyX", (int) babyPos.x)
            .SetEventAttribute("babyY", (int) babyPos.z)
            .SetEventAttribute("motherX", (int) motherPos.x)
            .SetEventAttribute("motherY", (int) motherPos.z)
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

                    if (timeLeftCache != -1)
                    {
                        //moves left remains the same
                        timerState = TimerState.YOURTIMER;
                        timeLeft = timeLeftCache;
                        timeLeftCache = -1;

                        mapManagerInstance.timerText.text = timeLeft.ToString();
                    } else
                    {
                        // init moves
                        movesLeft = 3;
                        mapManagerInstance.moveText.text = "Move: " + movesLeft;

                        timerState = TimerState.YOURTIMER;
                        timeLeft = TURNDURATION;
                        mapManagerInstance.timerText.text = timeLeft.ToString();
                    }
                }
                else
                {
                    Debug.Log("Unsuccessful Start Turn");
                    Debug.Log(response.JSONString);
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

                    if (timerState == TimerState.OPPRESPAWNTIMER)
                    {
                        timeLeft = RESPAWNDURATION;
                    }
                    //else if (currentGameState == GameState.RESPAWNBABY)
                    //{
                    //    Debug.Log("Baby Spawned, switching back to opp timer");
                    //    timerState = TimerState.OPPTIMER;
                    //    timeLeft = timeLeftCache;
                    //}
                    else
                    {
                        timerState = TimerState.OPPTIMER;
                        timeLeft = TURNDURATION;
                    }
                    mapManagerInstance.timerText.text = timeLeft.ToString();
                    mapManagerInstance.clearAllSelections();
                    //if (!initPositionComplete)
                    //{
                    //    initPositionComplete = true;
                    //}
                    //Debug.Log("Setting Turn in endTurn");
                    //setTurn(GameTurn.OPPONENTTURN);
                }
                else
                {
                    if (currentGameState == GameState.RESPAWNBABY)
                    {
                        Debug.Log("Timer expired, switching back to opp timer");
                        timerState = TimerState.OPPTIMER;
                        timeLeft = timeLeftCache;
                    } else if (timerState == TimerState.YOURTIMER)
                    {
                        timerState = TimerState.OPPTIMER;
                        timeLeft = TURNDURATION;
                    }
                    mapManagerInstance.timerText.text = timeLeft.ToString();
                    mapManagerInstance.clearAllSelections();

                    // This often happens timer expired, since it is no longer your turn, you cannot end.
                    Debug.Log("Unsuccessful End Turn");
                    Debug.Log(response.JSONString);
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
                    Debug.Log("Successful Start Respawn timer");
                }
                else
                {
                    Debug.Log("Unsuccessful Start Respawn timer");
                }
            });
    }

    public static void stopTimer()
    {
        new LogChallengeEventRequest().SetEventKey("STOP_TIMER")
            .SetEventAttribute("challengeInstanceId",getChallengeId())
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("Successful stop timer");
                }
                else
                {
                    Debug.Log("Unsuccessful stop timer");
                }
            });

    }

    public static void getChallengeInfo()
    {
        new GetChallengeRequest()
        .SetChallengeInstanceId(getChallengeId())
        .Send((response) => {
            if (!response.HasErrors)
            {
                JSONObject jsonmessage = new JSONObject(response.JSONString);
                Debug.Log(jsonmessage);
                Debug.Log("Got challenge details"); // get details from response json
                }
            else
            {
                Debug.Log("unsuccessful get challenge details");
            }
			
        });

    }

    // Set Turn, logic handles gamestate handling as well
    public static void setTurn(GameTurn gameTurn)
    {
        if (gameTurn == GameTurn.PLAYERTURN)
        {
            Debug.Log("Set to Player Turn");
            mapManagerInstance.turnText.text = "My Turn";
            currentGameTurn = GameTurn.PLAYERTURN;
            if (!initPositionComplete)
            {
                Debug.Log("Set text to Place Mother");
                currentGameState = GameState.SPAWNINGMOTHER;
                mapManagerInstance.instructionText.text = "Place Mother";
            } else if (currentGameState == GameState.RESPAWNBABY)
            {
                mapManagerInstance.showAvailableSpawnLocations();
                mapManagerInstance.instructionText.text = "Your baby mole was shakened awake, please choose a new spawning location.";
            } else
            {
                Debug.Log("Set state to ACTIVE");
                currentGameState = GameState.ACTIVE;
                mapManagerInstance.instructionText.text = "Move Mother";
            }
        } else
        {
            mapManagerInstance.turnText.text = "Opp Turn";
            Debug.Log("Set to Opp Turn");
            currentGameTurn = GameTurn.OPPONENTTURN;
            if (currentGameState == GameState.RESPAWNBABY)
            {
                currentGameState = GameState.ACTIVE;
                timerState = TimerState.OPPTIMER;
                timeLeft = timeLeftCache;
                timeLeftCache = -1;
            }
            if (timerState == TimerState.OPPRESPAWNTIMER)
            {
                mapManagerInstance.instructionText.text = "Waiting for opponent to respawn baby";
            } else
            {
                mapManagerInstance.instructionText.text = "Waiting for opponent";
            }
        }
        
    }

	public static void pickupPower(string pos)
	{
		string cid = getChallengeId();
		new LogChallengeEventRequest().SetEventKey("TAKE_POWERUP")
			.SetEventAttribute("challengeInstanceId",cid)
			.SetEventAttribute("LOCATION", pos)
					.Send((response) =>
						{
							if (!response.HasErrors)
							{
								Debug.Log("Successful Take Powerup");
									}
									else
									{
										Debug.Log("Unsuccessful Take Powerup");
									}
									});
								
	}
	public static void startTimer(float timeLeft)
	{
		new LogChallengeEventRequest().SetEventKey("START_TIMER_COUNT")
			.SetEventAttribute("challengeInstanceId",getChallengeId())
			.SetEventAttribute("SECONDS",(long) timeLeft)
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
}
