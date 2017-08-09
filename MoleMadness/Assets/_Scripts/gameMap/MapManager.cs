using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;

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

	//Line Renderer
	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public LineRenderer lineRenderer;
	public float pathHeight;
	public List<Vector3> positions;

	//Player Steps and Position information
	public Tile currentTile; // Path position - The current movement
	public Tile playerTile; // Player's position
	public int playerSteps = 2;
	public int steps;
	private int stepsreduction;
	public bool canMove = false;

	//PowerUps
	public bool power_diagonal; 

    System.Random pseudoRandom;

    const int initHoleNumber = 8;

    private Ray ray;
    private RaycastHit hit;

    public const float TILE_SIZE = 1;
    public const float TILE_OFFSET = 0.5f;

    private const float CHARACTER_HEIGHT = 0.01f;
    private const float PROJECTION_HEIGHT = 0.00f;
    private Quaternion CHARACTER_ROTATION = Quaternion.Euler(90, 0, 0);

    public Tile[,] map;
    private GameObject[,] selectProjectors;

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
//		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.widthMultiplier = 0.2f;
		// A simple 2 color gradient with a fixed alpha of 1.0f.
		float alpha = 1.0f;
		Gradient gradient = new Gradient();
		gradient.SetKeys(
			new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
			new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
		);
		lineRenderer.colorGradient = gradient;
		lineRenderer.positionCount = 0;
		pathHeight = 0.05f;

        // init text after scene load finish
        GameManager.initText();

		//Powerup initialization
		power_diagonal = false;
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
                timerText.text = "Times Up!";
            }
            else
            {
                timerText.text = GameManager.timeLeft.ToString();
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
		if (Input.GetKeyDown (KeyCode.D)) {
			power_diagonal = true;
		}
		if (Input.GetKeyDown (KeyCode.F)) {
			power_diagonal = false;
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
                        }
                        else
                        {
                            initBaby(x, z);
                            UpdateText(string.Format("Spawn Baby at {0},{1}", x, z));
                            clearAllSelections();
                            if (GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN)
                            {
                                GameManager.initPosition(mother.transform.position, baby.transform.position);
                            }
                        }
                    }
                    else
                    {
                        UpdateText("Target tile is not valid spawning location, please select a hole");
                    }
                }
                else if (GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN && GameManager.currentGameState == GameManager.GameState.ACTIVE)
                {
                    if (hit.collider.tag == "Tile")
                    {	
						UpdateText ("Tile");
						int motherX = (int) mother.transform.position.x;
						int motherZ = (int) mother.transform.position.z;

						if (motherX == x && motherZ == z)
						{
							// Set true when player is clicked to allow tracking when mouse held down
							canMove = true;
							positions = new List<Vector3> ();
							playerTile = map [x, z];
							currentTile = map [x, z];
							positions.Add (new Vector3 (playerTile.x + TILE_OFFSET, pathHeight, playerTile.z + TILE_OFFSET));
			
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
							getPaths (x, z, steps, new List<Tile> (), power_diagonal);
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
							getPaths (x, z, steps, new List<Tile> (), power_diagonal);
							lineRenderer.positionCount = positions.Count;
							lineRenderer.SetPositions (positions.ToArray ());
						}
					}
				}
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			positions = new List<Vector3> ();  
			if (canMove) {
				if (Physics.Raycast (ray, out hit, 100f)) {	
					if (hit.collider.tag == "Tile" && GameManager.currentGameTurn == GameManager.GameTurn.PLAYERTURN && GameManager.currentGameState == GameManager.GameState.ACTIVE) {
						float x = hit.point.x;
						float z = hit.point.z;
						Debug.Log (string.Format ("UPPP x: {0}, z: {1}", x, z));
						Debug.Log (string.Format ("current x: {0}, z: {1}", currentTile.x, currentTile.z));
						if (x>=currentTile.x && x<=currentTile.x +1 && z>=currentTile.z  && z<=currentTile.z+1 ) {
							Debug.Log ("in");
                            // check move will include delayed clearSelection and getPath in the callback to synchronize with moveUpdate
                            checkMove(currentTile.x, currentTile.z);
							//movePlayer(currentTile.x,currentTile.z);
							playerTile = currentTile;
							currentTile = playerTile;
						} else {
							currentTile = playerTile;
                            clearAllSelections();
                            getPaths(playerTile.x, playerTile.z, playerSteps, new List<Tile>(), power_diagonal);
                        }
					}
				}
				//clearAllSelections();
				//getPaths (playerTile.x, playerTile.z, playerSteps, new List<Tile> (), power_diagonal);
			}
			playerSteps = 2;
			steps = playerSteps;
			UpdateText (steps + "");
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


    void showAvailableSpawnLocations()
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
        getPaths(playerTile.x, playerTile.z, playerSteps, new List<Tile>(), power_diagonal);
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
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                map[x, z].tileType = tileMap[x, z];
            }
        }
        destroyMap();
        createTilesFromMap();
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

    //public Tile.TileType[,] convertIntToTileMap(int[,] intMap)
    //{
    //    Tile.TileType[,] tileMap = new Tile.TileType[width, height];
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int z = 0; z < height; z++)
    //        {
    //            tileMap[x, z] = (Tile.TileType)intMap[x, z];
    //        }
    //    }
    //    return tileMap;
    //}

    //Get the path of the player and cast selection on these paths
    List<Tile> getPaths(int x, int z, int steps, List<Tile> explored, bool diag = false, bool showProjections = true)
    {
        List<Tile> output = new List<Tile>();
        output.Add(map[x, z]);
        if (showProjections) {
            castSelection(x, z);
        }
        if (map[x, z].tileType == Tile.TileType.HILL) {
            steps = 0;
        }
        if (steps != 0) {
            List<Tile> linkage = map[x, z].links;
            if (diag) {
                linkage = map[x, z].linksDiag;
            }
            foreach (Tile link in linkage) {
                if (!explored.Contains(link)) {
                    output.Add(link);
                    if (showProjections) {
                        castSelection(link.x, link.z);
                    }
                    if (link.tileType == Tile.TileType.HOLE) {
                        output.AddRange(getPaths(link.x, link.z, steps, output, diag));
                    }
                    for (int step = steps - 1; step > 0; step--) {
                        output.AddRange(getPaths(link.x, link.z, step, output, diag));
                    }
                }
            }
        }

        return output;
    }

}
