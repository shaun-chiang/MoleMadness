using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapManager : MonoBehaviour {

    bool generating;

    public static MapManager mapManagerInstance;
    public static GameManager gameManagerInstance;

    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;
    public int x;
    public int z;
    public GameObject mother;
    public GameObject baby;
    public GUIText TextDisplay;

    public GameObject hillPrefab;
    public GameObject flatPrefab;
    public GameObject holePrefab;
    public GameObject motherPrefab;
    public GameObject babyPrefab;
    public GameObject[] projections;
    public GameObject[] powers;

    System.Random pseudoRandom;
    int randomX = 0;
    int randomZ = 0;

    const int initHoleNumber = 8;

    private Ray ray;
    private RaycastHit hit;

    public const float TILE_SIZE = 1;
    public const float TILE_OFFSET = 0.5f;
    // subject to changes
    private const float MOTHER_HEIGHT = 0.5f;

    public Tile[,] map;
    private GameObject[,] selectProjectors;

    private void Awake()
    {
        if (mapManagerInstance != null)
        {
            DestroyObject(gameObject);
        } else
        {
            mapManagerInstance = this;
        }

        GameManager gameManager = new GameManager(GameManager.GameState.SPAWNINGMOTHER);
        gameManagerInstance = GameManager.getInstance();
    }

    private void Start()
    {
        generating = true;
        selectProjectors = new GameObject[10,10];

        map = new Tile[width, height];
        if (useRandomSeed)
        {
            seed = DateTime.Now.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
        RandomFillMap();
        createTilesFromMap();
        showAvailableSpawnLocations();
        generating = false;
		UpdateText("Blank");
    }

    public static MapManager getInstance()
    {
        return mapManagerInstance;
    }

    void RandomFillMap()
    {

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
				map[x, z] = new Tile(x,z);
                map[x, z].tileType = Tile.TileType.FLAT;
            }
        }

        randomGenerateHolesAndHills();
        
    }

    void Update()
    {
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

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, 100f))
            {
                int x = (int)hit.point.x;
                int z = (int)hit.point.z;
                // logic for handling SPAWNINGMOTHER state
                if (gameManagerInstance.currentGameState == GameManager.GameState.SPAWNINGMOTHER)
                {
                    // check if tile selected is a hole, valid spawning location.
                    if (map[x, z].tileType == Tile.TileType.HOLE)
                    {
                        initMother(x, z);
                        UpdateText(string.Format("Spawn Mother at {0},{1}", x, z));
                        gameManagerInstance.currentGameState = GameManager.GameState.SPAWNINGBABY;
                        clearProjection(x, z);
                    }
                    else
                    {
                        UpdateText("Target tile is not valid spawning location, please select a hole");
                    }
                } else if (gameManagerInstance.currentGameState == GameManager.GameState.SPAWNINGBABY)
                {
                    // check if tile selected is a hole, valid spawning location.
                    if (map[x, z].tileType == Tile.TileType.HOLE)
                    {
                        if (map[x, z].tileType == Tile.TileType.HOLE)
                        initBaby(x, z);
                        UpdateText(string.Format("Spawn Baby at {0},{1}", x, z));
                        gameManagerInstance.currentGameState = GameManager.GameState.PLAYERTURN;
                        clearAllProjections();
                    }
                    else
                    {
                        UpdateText("Target tile is not valid spawning location, please select a hole");
                    }
                } else
                {
                    if (hit.collider.tag == "Tile")
                    {
                        movePlayer(x, z);
                        UpdateText(map[x, z].x + "_" + map[x, z].z);
                        getPaths(x, z, 2);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (!generating)
            {
                PowerManager.PowerType powerRandom = (PowerManager.PowerType) pseudoRandom.Next(4);

                Debug.Log("Spawn power with right click");
                generating = true;
                PowerManager.spawnPower(3, 6, powerRandom);
                generating = false;
            }
            else
            {
                Debug.Log("Pressed right click when generating.");
            }

            //if (!generating)
            //{
            //    Debug.Log("Inverting with right click");
            //    generating = true;
            //    invertMap();
            //    generating = false;
            //} else
            //{
            //    Debug.Log("Pressed right click when generating.");
            //}
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(2))
        {
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.tag == "Tile")
                {
                    int x = (int) hit.point.x;
                    int z = (int)hit.point.z;
                    Debug.Log("Found Tile.");
                    castProjection(x, z);
                    //Debug.Log(hit.point.ToString());
                    //Debug.Log(string.Format("x: {0}, z: {1}",x,z));
                }
            }
        }

    }

    void randomGenerateHolesAndHills()
    {
        // Init holes
        int holeCount = 0;
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

    void invertMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (map[x, z].tileType == Tile.TileType.HILL)
                {
                    Destroy(map[x, z].tileObject);
                    map[x, z].tileType = Tile.TileType.HOLE;
                    map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
                }
                else if (map[x, z].tileType == Tile.TileType.HOLE)
                {
                    Destroy(map[x, z].tileObject);
                    map[x, z].tileType = Tile.TileType.HILL;
                    map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + TILE_OFFSET + TILE_SIZE * x, 0, transform.position.y + TILE_OFFSET + TILE_SIZE * z), transform.rotation);
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
				map [x, z].links = GenerateLinks (x, z);
            }
        }
    }

    void destroyMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                map[x, z].tileType = Tile.TileType.FLAT;
                Destroy(map[x, z].tileObject);
            }
        }
    }

    void clearAllProjections()
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

    void clearProjection(int x, int z)
    {
        if (selectProjectors[x, z] != null)
        {
            DestroyObject(selectProjectors[x, z]);
            selectProjectors[x, z] = null;
        }
    }

    void castProjection(int x, int z)
    {
        if (selectProjectors[x, z] == null)
        {
            selectProjectors[x, z] = Instantiate(projections[0], new Vector3(x + TILE_OFFSET, 4, z + TILE_OFFSET), Quaternion.Euler(new Vector3(90, 0, 0)));
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
                    castProjection(x, z);
                }
            }
        }
    }

    void initMother(int x,int z)
    {
        mother = Instantiate(motherPrefab, new Vector3(x + TILE_OFFSET,MOTHER_HEIGHT,z + TILE_OFFSET), Quaternion.identity);
    }

    void initBaby(int x, int z)
    {
        baby = Instantiate(babyPrefab, new Vector3(x + TILE_OFFSET, MOTHER_HEIGHT, z + TILE_OFFSET), Quaternion.identity);
    }

    void movePlayer(int x, int z)
    {
        mother.transform.position = new Vector3(x + TILE_OFFSET, MOTHER_HEIGHT, z + TILE_OFFSET);
        //player = Instantiate(playerPrefab, new Vector3(x + TILE_OFFSET, PLAYER_HEIGHT, z + TILE_OFFSET), Quaternion.identity);
    }

	void UpdateText (string s)
	{
		TextDisplay.text = "Info: " + s;
	}

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

	List<Tile> getPaths(int x, int z, int steps)
	{
		List<Tile> output = new List<Tile> ();
		foreach (Tile link in map[x,z].links) {
			output.Add (link);
			castProjection (link.x, link.z);
			if (link.tileType == Tile.TileType.HOLE) {
				output.AddRange(getPaths (link.x, link.z, 2));
			}
		}
		return output;

	}
}
