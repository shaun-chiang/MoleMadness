using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;

[RequireComponent(typeof(Button))]
public class MapManager : MonoBehaviour
{
	bool generating; 
    public static MapManager mapManagerInstance;
    public static GameManager gameManagerInstance;
    public static GameObject mapInstance;

    public int width;
    public int height;
    public string seed;
    public bool player1;
    public GameObject mother;
    public GameObject baby;
    public GUIText TextDisplay;
    public GUIText turnText;
    public GUIText moveText;
    public GUIText instructionText;
    public GUIText myBabyText;
    public GUIText oppBabyText;
    public GUIText timerText;

    public GameObject hillPrefab;
    public GameObject flatPrefab;
    public GameObject holePrefab;
    public GameObject motherPrefab;
    public GameObject babyPrefab;
    public GameObject[] selections;
    public GameObject[] powers;
	public GameObject direction;
	private GameObject arrow;
	public GameObject machine;
	private GameObject excavator;

	//Line Renderer
	public LineRenderer lineRenderer;
	public float pathHeight;
	public List<Vector3> positions;


	//Animation
	public GameObject animation;

	//Buttons
	public Button button1;
	public Button button2;
	public Sprite earthshakeSprite;
	public Sprite moleInstrinctSprite;
	public Sprite diagonalSprite;
	public Sprite excavatorSprite;
	public Sprite buttonSprite;


	//Player Steps and Position information
	public Tile currentTile; // Path position - The current movement
	public Tile playerTile; // Player's position
	public Tile excavatorTile; // Excavator's position
	public int playerSteps = 2;
	public int steps;
	private int stepsreduction;
	public bool canMove;
	public bool setPath;

	//PowerUps
	public bool power_diagonal; 
	public bool power_earthshake;
	public bool power_excavator;
	public List<Tile> excavatorPath;
	private bool excavator_active = false;
	public bool power_moleInstinct;
	public Tile babyMolePosition;
	public GameManager.Powers power33;
	public GameManager.Powers power36;
	public GameManager.Powers power66;
	public GameManager.Powers power63;
	public GameObject power33Obj;
	public GameObject power36Obj;
	public GameObject power66Obj;
	public GameObject power63Obj;
	public GameObject moleInjstinctObj;
	public GameObject earthshakeObj;
	public GameObject diagonalObj;
	public GameObject excavatorObj;
	public static Dictionary<string, GameManager.Powers> spawnCoor =
		new Dictionary<string, GameManager.Powers>(){
		{ "3,3", GameManager.Powers.NOTHING},
		{ "3,6", GameManager.Powers.NOTHING},
		{ "6,3", GameManager.Powers.NOTHING},
		{ "6,6", GameManager.Powers.NOTHING}
	};
	public GameManager.Powers[] BagPack;


    System.Random pseudoRandom;

    const int initHoleNumber = 8;

    private Ray ray;
    private RaycastHit hit;

    public const float TILE_SIZE = 1;
    public const float TILE_OFFSET = 0.5f;

    private const float CHARACTER_HEIGHT = 0.2f;
    private const float PROJECTION_HEIGHT = 0.00f;
    private Quaternion CHARACTER_ROTATION = Quaternion.Euler(90, 0, 0);

    public Tile[,] map;
    private GameObject[,] selectProjectors;
	private GameObject[,] selectRedProjectors;

    private void Awake()
    {
        if (mapManagerInstance != null)
        {
            DestroyObject(gameObject); 
        }
        else
        {
            mapManagerInstance = this;
        }
        mapInstance = gameObject;
        GameManager gameManager = new GameManager(GameManager.GameState.SPAWNINGMOTHER);
        gameManagerInstance = GameManager.getInstance();
    }

    private void Start()
    {
        generating = true;
        selectProjectors = new GameObject[10, 10];
		selectRedProjectors = new GameObject[10, 10];

        map = new Tile[width, height];
        seed = GameManager.getChallengeId();
        pseudoRandom = new System.Random(seed.GetHashCode());
        RandomFillMap();
        if (!GameManager.player1)
        {
            // invert the map to simulate the other side for player 2
            invertMap();
        }
        createTilesFromMap();
        showAvailableSpawnLocations();
        generating = false;
        UpdateText("Blank");

		// Initialising the Line Renderer for path direction
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.widthMultiplier = 0.2f;
		lineRenderer.positionCount = 0;
		pathHeight = 0.05f;

        // init text after scene load finish
        GameManager.initText();

		//Powerup initialization
		power_diagonal = false;
		power_earthshake = false;
		power_excavator = false;
		power_moleInstinct = false;
		power33 = GameManager.Powers.NOTHING;
		power63 = GameManager.Powers.NOTHING;
		power66 = GameManager.Powers.NOTHING;
		power36 = GameManager.Powers.NOTHING;
		BagPack = new GameManager.Powers[]{ GameManager.Powers.NOTHING, GameManager.Powers.NOTHING };
    }

    public static MapManager getInstance()
    {
        if (mapManagerInstance == null)
        {
            Debug.Log("Finding Map Manager");
            GameObject go = GameObject.FindGameObjectWithTag("Map");
            MapManager instance = go.GetComponent<MapManager>();
            return instance;
        }
        else
        {
            Debug.Log("Returning Map Manager");
            return mapManagerInstance;
        }
    }

    void RandomFillMap()
    {

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                map[x, z] = new Tile(x, z);
                map[x, z].tileType = Tile.TileType.FLAT;
            }
        }

        randomGenerateHolesAndHills();

    }

    void Update()
    {
        if (GameManager.timerState != GameManager.TimerState.OFF)
        {
            GameManager.timeLeft -= Time.deltaTime;
            if (GameManager.timeLeft <= 0)
            {
                //timerText.text = "Times Up!";
            }
            else
            {
                timerText.text = GameManager.timeLeft.ToString();
            }

            if (GameManager.timerState == GameManager.TimerState.YOURRESPAWNTIMER)
            {
                timerText.color = Color.white;
            } else if (GameManager.timerState == GameManager.TimerState.OPPRESPAWNTIMER)
            {
                timerText.color = Color.black;
            } else if (GameManager.timerState == GameManager.TimerState.YOURTIMER)
            {
                timerText.color = Color.blue;
            } else if (GameManager.timerState == GameManager.TimerState.OPPTIMER)
            {
                timerText.color = Color.red;
            }
            
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // For testing only
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (!generating)
            {
                Debug.Log("Generating with G");
                generating = true;
                destroyMap();
                //RandomFillMap();
                randomGenerateHolesAndHills();
                createTilesFromMap();
                generating = false;
            }
            else
            {
                Debug.Log("Pressed G when generating.");
            }
        }
		if (Input.GetKeyDown (KeyCode.Q)) {
			power_diagonal = true;
		}
		if (Input.GetKeyDown (KeyCode.A)) {
			power_diagonal = false;
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			if (power_earthshake) {
				power_earthshake = false;
				clearAllRedSelections();
			} else {
				power_earthshake = true;
				int x = (int)playerTile.x;
				int z = (int)playerTile.z;
				for (int w = 0; w < width; w++) {
					for (int h = 0; h < height; h++) {
						if ((w == x && h > z) || (w == x && h < z) || (w > x && h == z) || (w < x && h == z)) {
							castRedSelection (w, h);
						}
					}
				}
			}
	
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			power_excavator = true;
			excavatorTile = map [playerTile.x, playerTile.z];
			excavator = Instantiate (machine, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z), transform.rotation);
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			power_moleInstinct = true;
			DestroyObject (arrow);
			if (babyMolePosition.z > playerTile.z) {
				if (babyMolePosition.x == playerTile.x) {
					arrow = Instantiate (direction, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z + 0.5f), transform.rotation);
				} else if (babyMolePosition.x > playerTile.x) {
					arrow = Instantiate (direction, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x + 0.4f, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z + 0.4f), Quaternion.Euler (0, 45, 0));
				} else {
					arrow = Instantiate (direction, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x - 0.4f, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z + 0.4f), Quaternion.Euler (0, 315, 0));
				}

			}
			else if (babyMolePosition.z < playerTile.z) {
				if (babyMolePosition.x == playerTile.x) {
					arrow = Instantiate (direction, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z - 0.5f), Quaternion.Euler (0, 180, 0));
				} else if (babyMolePosition.x > playerTile.x) {
					arrow = Instantiate (direction, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x + 0.4f, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z - 0.4f), Quaternion.Euler (0, 135, 0));
				} else {
					arrow = Instantiate (direction, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x - 0.4f, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z - 0.4f), Quaternion.Euler (0, 225, 0));
				}
			}
			else {
				if (babyMolePosition.x > playerTile.x) {
					arrow = Instantiate(direction, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x + 0.5f, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z), Quaternion.Euler(0, 90, 0));
				}
				else if (babyMolePosition.x < playerTile.x) {
					arrow = Instantiate(direction, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * playerTile.x - 0.5f, 0.1f, transform.position.y + TILE_OFFSET + TILE_SIZE * playerTile.z), Quaternion.Euler(0, 270, 0));
				}
			}
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			GameManager.currentGameTurn = GameManager.GameTurn.PLAYERTURN;
			GameManager.currentGameState = GameManager.GameState.ACTIVE;
		}
		if (power33 != spawnCoor ["3,3"]) {
			power33 = spawnCoor ["3,3"];
			spawnPower(3, 3, power33, power33Obj);
		}
		if (power36 != spawnCoor ["3,6"]) {
			power36 = spawnCoor ["3,6"];
			spawnPower(3, 6, power36, power36Obj);
		}
		if (power66 != spawnCoor ["6,6"]) {
			power66 = spawnCoor ["6,6"];
			spawnPower(6, 6, power66, power66Obj);
		}
		if (power63 != spawnCoor ["6,3"]) {
			power63 = spawnCoor ["6,3"];
			spawnPower(6, 3, power63, power63Obj);
		}

        if (Input.GetMouseButtonDown(0))
        {

            Debug.Log(string.Format("GameState: {0},GameTurn: {1}", GameManager.currentGameState, GameManager.currentGameTurn));
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            { 
                int x = (int)hit.point.x;
                int z = (int)hit.point.z;
                // logic for handling spawning mother state
                if (GameManager.currentGameState == GameManager.GameState.SPAWNINGMOTHER && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
                {
                    // check if tile selected is a hole, valid spawning location.
                    if (map[x, z].tileType == Tile.TileType.HOLE)
                    {
                        initMother(x, z);
                        UpdateText(string.Format("Spawn Mother at {0},{1}", x, z));
                        instructionText.text = "Place Baby";
                        GameManager.currentGameState = GameManager.GameState.SPAWNINGBABY;

                        playerTile = map[x, z];
                        clearSelection(x, z);
                    }
                    else
                    {
                        UpdateText("Target tile is not valid spawning location, please select a hole");
                    }
                }
                // logic to handle touch when spawning baby
                else if ((GameManager.currentGameState == GameManager.GameState.SPAWNINGBABY || GameManager.currentGameState == GameManager.GameState.RESPAWNBABY) && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
                {
                    // check if tile selected is a hole, valid spawning location.
                    if (map[x, z].tileType == Tile.TileType.HOLE)
                    {
                        int motherX = (int)mother.transform.position.x;
                        int motherZ = (int)mother.transform.position.z;
                        Debug.Log(motherX + "," + motherZ);
                        if (motherX == x && motherZ == z && GameManager.currentGameState == GameManager.GameState.SPAWNINGBABY)
                        {
                            UpdateText("You must spawn baby away from mother for the start of the game");
                        } else if (GameManager.currentGameState == GameManager.GameState.RESPAWNBABY)
                        {
                            Debug.Log(string.Format("Respawning Baby at {0},{1}", x, z));
                            baby.transform.position = new Vector3(x + TILE_OFFSET, CHARACTER_HEIGHT, z + TILE_OFFSET);
                            GameManager.sendMoveUpdate(mother.transform.position, baby.transform.position, "respawn");
                            UpdateText(string.Format("Spawn Baby at {0},{1}", x, z));

                            Debug.Log("Baby Spawned, switching back to opp timer");
                            GameManager.currentGameState = GameManager.GameState.ACTIVE;
                            GameManager.timerState = GameManager.TimerState.OPPTIMER;
                            GameManager.timeLeft = GameManager.timeLeftCache;
                            GameManager.timeLeftCache = -1;

                            GameManager.endTurn();
                        } else
                        {
                            initBaby(x, z);
                            UpdateText(string.Format("Spawn Baby at {0},{1}", x, z));
                            clearAllSelections();
                            if (GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
                            {
                                GameManager.initPosition(mother.transform.position, baby.transform.position);
                            }
                        }
						babyMolePosition = map [x, z];
                    }
                    else
                    {
                        UpdateText("Target tile is not valid spawning location, please select a hole");
                    }
                }
                else if (GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN && GameManager.currentGameState == GameManager.GameState.ACTIVE)
                {
					if (power_earthshake)
					{
						int pX = (int)playerTile.x;
						int pZ = (int)playerTile.z;
						for (int w = 0; w < width; w++)
						{
							for (int h = 0; h < height; h++)
							{
								if (x == pX && z > pZ) {
									if (w == pX && h > pZ) {
										map [w, h].tileType = Tile.TileType.HOLE;
									}
								}
								if (x == pX && z < pZ) {
									if (w == pX && h < pZ) {
										map [w, h].tileType = Tile.TileType.HOLE;
									}
								}
								if (x > pX && z == pZ) {
									if (w > pX && h == pZ) {
										map [w, h].tileType = Tile.TileType.HOLE;
									}
								}
								if (x < pX && z == pZ) {
									if (w < pX && h == pZ) {
										map [w, h].tileType = Tile.TileType.HOLE;
									}
								}
							}
						}
						reloadMap ();
						clearAllRedSelections ();
						power_earthshake = false;

					}
					else if (excavator_active)
					{
						if (excavatorPath.Contains(map[x,z]))
						{
							map [x, z].tileType = Tile.TileType.HOLE;
							reloadMap ();
							clearAllRedSelections ();
							excavator_active = false;
						}
					}
                    else if (hit.collider.tag == "Tile")
                    {	
						UpdateText ("Tile");
						int motherX = (int) mother.transform.position.x;
						int motherZ = (int) mother.transform.position.z;


						if (motherX == x && motherZ == z)
						{
							if (power_moleInstinct) {
								DestroyObject (arrow);
								power_moleInstinct = false;
							}
							// Set true when player is clicked to allow tracking when mouse held down
							canMove = true;
							positions = new List<Vector3> ();
							playerTile = map [x, z];
							currentTile = map [x, z];
							getPaths (playerTile.x, playerTile.z, playerSteps, power_diagonal);
							positions.Add (new Vector3 (playerTile.x + TILE_OFFSET, pathHeight, playerTile.z + TILE_OFFSET));
			
						}
						else if (power_excavator) {
							if (excavatorTile.x == x && excavatorTile.z == z) {
								excavator_active = true;
								excavatorPath = oneMove (x, z, new List<Tile> (), power_diagonal, power_excavator);
							}
						}
					} 
                }
            }
        }
		if (Input.GetMouseButton (0)) {	
			if (Physics.Raycast (ray, out hit, 100f)) {
                //print(string.Format("Turn:{0} ,State:{1}",GameManager.currentGameTurn,GameManager.currentGameState));
				if (hit.collider.tag == "Tile" && canMove && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN && GameManager.currentGameState == GameManager.GameState.ACTIVE) {
					int x = (int)hit.point.x;
					int z = (int)hit.point.z;
					//Debug.Log (string.Format ("x: {0}, z: {1}", x, z));
					bool goBack = false;
					if (positions.Count > 1) {
						if (map [x, z] == map [(int)positions [positions.Count-2].x, (int)positions [positions.Count-2].z]) {
							goBack = true;
							if (currentTile.tileType == Tile.TileType.HILL) {
								steps += stepsreduction;
							}
							else if (currentTile.tileType == Tile.TileType.FLAT) {
								steps += 1;
							}
							if (map [x, z] == playerTile) {
								steps = 2;
							}
							currentTile = map [x, z];
							positions.RemoveAt(positions.Count-1);
							UpdateText (steps + "");
							clearAllSelections();
							getPaths (x, z, steps, power_diagonal);
							lineRenderer.positionCount = positions.Count;
							lineRenderer.SetPositions (positions.ToArray ());
						}
					}
					if (!goBack && map [x, z] != currentTile) {
						List<Tile> linkage = currentTile.links;
						if (power_diagonal) {
							linkage = currentTile.linksDiag;
						}
						if (steps != 0 && linkage.Contains (map [x, z])) {
							currentTile = map [x, z];
							positions.Add (new Vector3 (x + TILE_OFFSET, pathHeight, z + TILE_OFFSET));
							if (currentTile.tileType == Tile.TileType.FLAT) {
								steps -= 1;
								stepsreduction = 1;
							} 
							if (currentTile.tileType == Tile.TileType.HILL) {
								stepsreduction = steps;
								steps = 0;
							}
							UpdateText (steps + "");
							clearAllSelections();
							getPaths (x, z, steps, power_diagonal);
							lineRenderer.positionCount = positions.Count;
							lineRenderer.SetPositions (positions.ToArray ());
						}
					}
				}
			}
		}
		if (Input.GetMouseButtonUp (0)) {  
			if (canMove && !setPath) {
				if (Physics.Raycast (ray, out hit, 100f)) {	
					if (hit.collider.tag == "Tile" && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN && GameManager.currentGameState == GameManager.GameState.ACTIVE) {
						float x = hit.point.x;
						float z = hit.point.z;
						Debug.Log (string.Format ("UPPP x: {0}, z: {1}", x, z));
						Debug.Log (string.Format ("current x: {0}, z: {1}", currentTile.x, currentTile.z));
						if (x>=currentTile.x && x<=currentTile.x +1 && z>=currentTile.z  && (z<=currentTile.z+1) && currentTile!=playerTile) {
							Debug.Log ("in");
                            // check move will include delayed clearSelection and getPath in the callback to synchronize with moveUpdate
                            checkMove(currentTile.x, currentTile.z);
							//movePlayer(currentTile.x,currentTile.z);
							playerTile = currentTile;
							currentTile = playerTile;

							if (currentTile.tileType == Tile.TileType.HILL) {
								map [(int)currentTile.x, (int)currentTile.z].tileType = Tile.TileType.HOLE;
//								if (positions.Count > 2) {
//									for (int i = 1; i < positions.Count - 1; i++) {
//										movePlayer((int)positions[i].x,(int)positions[i].z);
//									}
//								}
								animation.GetComponent<Smashing_Animation> ().action ();
							} else {						
								for (int i = 1; i < positions.Count; i++) {
									int pX = (int)positions[i].x;
									int pZ = (int)positions[i].z;
									map [pX, pZ].tileType = Tile.TileType.HOLE;
								}
								animation.GetComponent<Digging_Animation> ().action ();
							}
							reloadMap ();
								
						} else {
							currentTile = playerTile;
                            clearAllSelections();
                            getPaths(playerTile.x, playerTile.z, playerSteps, power_diagonal);
                        }
					}
				}
				clearAllSelections();
			}
			positions = new List<Vector3> ();
			playerSteps = 2;
			steps = playerSteps;
//			UpdateText (steps + "");
			canMove = false;
			lineRenderer.positionCount = 0;
			lineRenderer.SetPositions (new Vector3[0]);
		}


        //if (Input.GetMouseButtonDown(1))
        //{
        //    if (!generating)
        //    {
        //        PowerManager.PowerType powerRandom = (PowerManager.PowerType)pseudoRandom.Next(4);

        //        Debug.Log("Spawn power with right click");
        //        generating = true;
        //        PowerManager.spawnPower(3, 6, powerRandom);
        //        generating = false;
        //    }
        //    else
        //    {
        //        Debug.Log("Pressed right click when generating.");
        //    }

        //    //if (!generating)
        //    //{
        //    //    Debug.Log("Inverting with right click");
        //    //    generating = true;
        //    //    invertMap();
        //    //    generating = false;
        //    //} else
        //    //{
        //    //    Debug.Log("Pressed right click when generating.");
        //    //}
        //}

        //ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Input.GetMouseButtonDown(2))
        //{
        //    if (Physics.Raycast(ray, out hit, 100f))
        //    {
        //        if (hit.collider.tag == "Tile")
        //        {
        //            int x = (int)hit.point.x;
        //            int z = (int)hit.point.z;
        //            Debug.Log("Found Tile.");
        //            castSelection(x, z);
        //            //Debug.Log(hit.point.ToString());
        //            //Debug.Log(string.Format("x: {0}, z: {1}",x,z));
        //        }
        //    }
        //}

    }

    void randomGenerateHolesAndHills()
    {
        // Init holes
        int holeCount = 0;
        int randomX = 0;
        int randomZ = 0;
        while (holeCount < initHoleNumber)
        {
            randomX = pseudoRandom.Next(0, width);
            randomZ = pseudoRandom.Next(0, width);
            if (map[randomX, randomZ].tileType != Tile.TileType.HOLE)
            {
                map[randomX, randomZ].tileType = Tile.TileType.HOLE;
                holeCount++;
            }
        }
        // Init Hills
        int hillCount = 0;
        while (hillCount < initHoleNumber)
        {
            randomX = pseudoRandom.Next(0, width);
            randomZ = pseudoRandom.Next(0, width);
            if (map[randomX, randomZ].tileType != Tile.TileType.HILL && map[randomX, randomZ].tileType != Tile.TileType.HOLE)
            {
                map[randomX, randomZ].tileType = Tile.TileType.HILL;
                hillCount++;
            }
        }
    }

    public void randomBabyRespawn()
    {
        List<int> Xs = new List<int>();
        List<int> Zs = new List<int>();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (map[x, z].tileType == Tile.TileType.HOLE)
                {
                    Xs.Add(x);
                    Zs.Add(z);
                }
            }
        }

        int numOfHoles = Xs.Count;
        int holeNum = pseudoRandom.Next(0, numOfHoles);

        baby.transform.position = new Vector3(Xs[holeNum] + TILE_OFFSET, CHARACTER_HEIGHT, Zs[holeNum] + TILE_OFFSET);
    }

    // invert the map before they are generated, change hill to hole and vice versa
    // method is called for player 2
    void invertMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (map[x, z].tileType == Tile.TileType.HILL)
                {
                    //Destroy(map[x, z].tileObject);
                    map[x, z].tileType = Tile.TileType.HOLE;
                    //map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                }
                else if (map[x, z].tileType == Tile.TileType.HOLE)
                {
                    //Destroy(map[x, z].tileObject);
                    map[x, z].tileType = Tile.TileType.HILL;
                    //map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                }
            }
        }
    }

    // invert the tile map received from 
    Tile.TileType[,] invertTileMap(Tile.TileType[,] tileMap)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (tileMap[x, z] == Tile.TileType.HILL)
                {
                    //Destroy(map[x, z].tileObject);
                    tileMap[x, z] = Tile.TileType.HOLE;
                    //map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                }
                else if (tileMap[x, z] == Tile.TileType.HOLE)
                {
                    //Destroy(map[x, z].tileObject);
                    tileMap[x, z] = Tile.TileType.HILL;
                    //map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                }
            }
        }
        return tileMap;
    }

    void reloadMap()
	{
		generating = true;
		destroyMap();
		createTilesFromMap ();
		generating = false;
	}

    void createTilesFromMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                switch (map[x, z].tileType)
                {
                    case Tile.TileType.FLAT:
                        map[x, z].tileObject = Instantiate(flatPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                        break;
                    case Tile.TileType.HILL:
                        map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                        break;
                    case Tile.TileType.HOLE:
                        map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                        break;
                    default:
                        break;
                }
                map[x, z].tileObject.transform.SetParent(transform);
                map[x, z].links = GenerateLinks(x, z);
				map[x, z].linksDiag = GenerateLinksDiag(x, z);
            }
        }
    }

    void destroyMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Destroy(map[x, z].tileObject);
            }
        }
    }

    public void clearAllSelections()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (selectProjectors[x, z] != null)
                {
                    DestroyObject(selectProjectors[x, z]);
                    selectProjectors[x, z] = null;
                }
            }
        }
    }

    void clearSelection(int x, int z)
    {
        if (selectProjectors[x, z] != null)
        {
            DestroyObject(selectProjectors[x, z]);
            selectProjectors[x, z] = null;
        }
    }

    void castSelection(int x, int z)
    {
        if (selectProjectors[x, z] == null)
        {
            selectProjectors[x, z] = Instantiate(selections[0], new Vector3(x + TILE_OFFSET, PROJECTION_HEIGHT, z + TILE_OFFSET), Quaternion.identity);
            selectProjectors[x, z].transform.SetParent(transform);
        }
    }

	void castRedSelection(int x, int z)
	{
		if (selectRedProjectors[x, z] == null)
		{
			selectRedProjectors[x, z] = Instantiate(selections[2], new Vector3(x + TILE_OFFSET, PROJECTION_HEIGHT, z + TILE_OFFSET), Quaternion.identity);
			selectRedProjectors[x, z].transform.SetParent(transform);
		}
	}

	void clearAllRedSelections()
	{
		for (int x = 0; x < width; x++)
		{
			for (int z = 0; z < height; z++)
			{
				if (selectRedProjectors[x, z] != null)
				{
					DestroyObject(selectRedProjectors[x, z]);
					selectRedProjectors[x, z] = null;
				}
			}
		}
	}

    public void showAvailableSpawnLocations()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (map[x, z].tileType == Tile.TileType.HOLE)
                {
                    castSelection(x, z);
                }
            }
        }
    }

    void initMother(int x, int z)
    {
        mother = Instantiate(motherPrefab, new Vector3(x + TILE_OFFSET, CHARACTER_HEIGHT, z + TILE_OFFSET), CHARACTER_ROTATION);
    }

    void initBaby(int x, int z)
    {
        baby = Instantiate(babyPrefab, new Vector3(x + TILE_OFFSET, CHARACTER_HEIGHT, z + TILE_OFFSET), CHARACTER_ROTATION);
    }


    void checkMove(int x, int z)
    {
        // check with server if move causes any feedback
        GameManager.sendMoveCheck(x, z);
    }

    public void movePlayer(int x, int z)
    {
        // never call directly, only called by GameManager after checking
        Debug.Log(string.Format("Moving to {0},{1}",x,z));
        mother.transform.position = new Vector3(x + TILE_OFFSET, CHARACTER_HEIGHT, z + TILE_OFFSET);
        clearAllSelections();
        getPaths(playerTile.x, playerTile.z, playerSteps, power_diagonal);
        int[,] intMap = convertTileToIntMap();
        GameManager.sendMoveUpdate(mother.transform.position, baby.transform.position, convertIntMapToString(convertTileToIntMap()));
    }
		
	// Update the UI text Display
	void UpdateText (string s)
	{
		TextDisplay.text = "Steps: " + s;
	}

	// Create paths that are 1 step away
	List<Tile> GenerateLinks(int x, int z)
	{
		List<Tile> output = new List<Tile>();
		if (x + 1 <= 9) 
		{
			output.Add (map[x + 1, z]);
		}
		if (x - 1 >= 0) 
		{
			output.Add (map[x - 1, z]);
		}
		if (z + 1 <= 9) 
		{
			output.Add (map[x, z + 1]);
		}
		if (z - 1 >= 0) 
		{
			output.Add (map[x, z - 1]);
		}
		return output;
	}
	//For path with Diagonal
	List<Tile> GenerateLinksDiag(int x, int z)
	{
		List<Tile> output = new List<Tile>();
		if (x + 1 <= 9) 
		{
			output.Add (map[x + 1, z]);
		}
		if (x - 1 >= 0) 
		{
			output.Add (map[x - 1, z]);
		}
		if (z + 1 <= 9) 
		{
			output.Add (map[x, z + 1]);
		}
		if (z - 1 >= 0) 
		{
			output.Add (map[x, z - 1]);
		}
		if (x + 1 <= 9 && z + 1 <=9) 
		{
			output.Add (map[x + 1, z + 1]);
		}
		if (x + 1 <= 9 && z - 1 >= 0) 
		{
			output.Add (map[x + 1, z - 1]);
		}
		if (x - 1 >= 0 && z - 1 >= 0) 
		{
			output.Add (map[x - 1, z - 1]);
		}
		if (x - 1 >= 0 && z + 1 <= 9) 
		{
			output.Add (map[x - 1, z + 1]);
		}

		return output;
	}

    public Tile.TileType[,] constructTileMapFromString(string stringMap)
    {
        //Tile.TileType[,] tileMap = new Tile.TileType[width, height];
        //string[] rows = stringMap.Split(';');
        //for (int rowNum = 0; rowNum < rows.Length; rowNum++)
        //{
        //    string[] elements = rows[rowNum].Split(',');
        //    for (int colNum = 0; colNum < elements.Length; colNum++)
        //    {
        //        int tileTypeConst = int.Parse(elements[colNum]);
        //        Tile.TileType tileType = (Tile.TileType)tileTypeConst;
        //        tileMap[colNum, rowNum] = tileType;
        //    }
        //}

        Tile.TileType[,] tileMap = new Tile.TileType[width, height];
        string[] cols = stringMap.Split(';');
        for (int colNum = 0;colNum < cols.Length; colNum++)
        {
            string[] elements = cols[colNum].Split(',');
            for (int rowNum = 0; rowNum < elements.Length; rowNum++)
            {
                int tileTypeConst = int.Parse(elements[rowNum]);
                Tile.TileType tileType = (Tile.TileType)tileTypeConst;
                tileMap[colNum, rowNum] = tileType;
            }
        }
        return tileMap;
    }

    public void loadTileMap(Tile.TileType[,] tileMap)
    {
        tileMap = invertTileMap(tileMap);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (map[x, z].tileType != tileMap[x, z])
                {
                    map[x, z].tileType = tileMap[x, z];
                    Destroy(map[x, z].tileObject);
                    switch (map[x, z].tileType)
                    {
                        case Tile.TileType.FLAT:
                            map[x, z].tileObject = Instantiate(flatPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                            break;
                        case Tile.TileType.HILL:
                            map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                            // Animation for Hill Creation
                            break;
                        case Tile.TileType.HOLE:
                            map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                            // Animation for Hole Creation
                            break;
                        default:
                            break;
                    }
                    map[x, z].tileObject.transform.SetParent(transform);
                    map[x, z].links = GenerateLinks(x, z);
                    map[x, z].linksDiag = GenerateLinksDiag(x, z);
                }
            }
        }
    }

    //public void loadTileMap(Tile.TileType[,] tileMap)
    //{
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int z = 0; z < height; z++)
    //        {
    //            map[x, z].tileType = tileMap[x, z];
    //        }
    //    }
    //    destroyMap();
    //    // map is inverted because it is retrieve from opponent POV
    //    invertMap();
    //    createTilesFromMap();
    //}

    public bool isMyBabyHit()
    {
        Vector3 babyPosition = baby.transform.position;
        bool babyHit = map[(int)babyPosition.x, (int)babyPosition.z].tileType == Tile.TileType.HILL;
        if (babyHit)
        {
            Debug.Log("My baby is hit");
        }
        return babyHit;
    }

	List<Tile> getPaths(int x, int z, int steps, bool diag = false)
	{
		List<Tile> output = new List<Tile> ();
		if (steps != 0) {
			oneMove (x, z, new List<Tile> (), diag);
			output.AddRange (oneMove (x, z, new List<Tile> (), diag));
			List<Tile> explored = new List<Tile> (output);
			explored.RemoveAt(0);
			if (steps == 2) {
				foreach (Tile tile in explored) {
					if (tile.tileType != Tile.TileType.HILL) {
						output.AddRange (oneMove (tile.x, tile.z, output, diag));
					}
				}
			}
		}
		
		return output;
	}

	List<Tile> oneMove(int x, int z, List<Tile> explored, bool diag = false, bool red = false)
	{
		List<Tile> output = new List<Tile> ();
//		output.AddRange (explored);
		output.Add (map [x, z]);
		if (red) {
			if (map [x, z].tileType != Tile.TileType.HOLE) {
				castRedSelection (x, z);
			}
		} else {
			castSelection (x, z);
		}
		List<Tile> linkage = map [x, z].links;
		if (diag) {
			linkage = map [x, z].linksDiag;
		}
		foreach (Tile link in linkage) {
			if (!explored.Contains (link) && !output.Contains (link)) {
				output.Add (link);
				if (red) {
					if (link.tileType != Tile.TileType.HOLE) {
						castRedSelection (link.x, link.z);
					}
				} else {
					castSelection (link.x, link.z);
				}
				if (link.tileType == Tile.TileType.HOLE) {
					explored.AddRange (output);
					output.AddRange (oneMove (link.x, link.z, explored, diag, red));
				}
			}
		}
		return output;
	}

    public int[,] convertTileToIntMap()
    {
        int[,] intMap = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                intMap[x, z] = (int) map[x, z].tileType;                 
            }
        }
        return intMap;
    }

    public string convertIntMapToString(int[,] intMap)
    {
        string stringMap = "";
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                stringMap += intMap[x, z];
                if (z < height - 1)
                {
                    stringMap += ",";
                }
            }
            if (x < width - 1)
            {
                stringMap += ";";
            }
        }
        return stringMap;
    }
	private void spawnPower(int x, int z, GameManager.Powers power, GameObject obj){
		DestroyObject (obj);
		if (power == GameManager.Powers.EARTHSHAKE) {
			obj = Instantiate (earthshakeObj, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0.13f, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
		} else if (power == GameManager.Powers.MOLEINSTINCT) {
			obj = Instantiate (moleInjstinctObj, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0.13f, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
		} else if (power == GameManager.Powers.EXCAVATOR) {
			obj = Instantiate (excavatorObj, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0.13f, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
		} else if (power == GameManager.Powers.DIAGONAL) {
			obj = Instantiate (diagonalObj, new Vector3 (transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0.13f, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
		}
	}

	private void getPower(int x, int z){
		if (x == 3 && z == 3) {
			if (BagPack [0] != GameManager.Powers.NOTHING) {
				BagPack [0] = power33;
				button1.image.overrideSprite = spriteSelector (power33);
			} else if (BagPack [1] != GameManager.Powers.NOTHING) {
				BagPack [1] = power33;
				button2.image.overrideSprite = spriteSelector (power33);
			}
		}
		if (x == 3 && z == 6) {
			if (BagPack [0] != GameManager.Powers.NOTHING) {
				BagPack [0] = power36;
				button1.image.overrideSprite = spriteSelector (power36);
			} else if (BagPack [1] != GameManager.Powers.NOTHING) {
				BagPack [1] = power36;
				button2.image.overrideSprite = spriteSelector (power36);
			}
		}
		if (x == 6 && z == 3) {
			if (BagPack [0] != GameManager.Powers.NOTHING) {
				BagPack [0] = power63;
				button1.image.overrideSprite = spriteSelector (power63);
			} else if (BagPack [1] != GameManager.Powers.NOTHING) {
				BagPack [1] = power63;
				button2.image.overrideSprite = spriteSelector (power63);
			}
		}
		if (x == 6 && z == 6) {
			if (BagPack [0] != GameManager.Powers.NOTHING) {
				BagPack [0] = power66;
				button1.image.overrideSprite = spriteSelector (power66);
			} else if (BagPack [1] != GameManager.Powers.NOTHING) {
				BagPack [1] = power66;
				button2.image.overrideSprite = spriteSelector (power66);
			}
		}
	}
	private Sprite spriteSelector(GameManager.Powers power){
		if (power == GameManager.Powers.EARTHSHAKE) {
			return earthshakeSprite;
		} else if (power == GameManager.Powers.MOLEINSTINCT) {
			return moleInstrinctSprite;
		} else if (power == GameManager.Powers.EXCAVATOR) {
			return excavatorSprite;
		} else if (power == GameManager.Powers.DIAGONAL) {
			return diagonalSprite;
		} else {
			return buttonSprite;
		}
	}
}