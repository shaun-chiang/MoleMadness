using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapManager : MonoBehaviour {

    bool generating;

    MapManager instance;

    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;
    public int x;
    public int z;
    public GameObject player;
	public GUIText TextDisplay;



    public GameObject hillPrefab;
    public GameObject flatPrefab;
    public GameObject holePrefab;
    public GameObject playerPrefab;
    public GameObject[] projections;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public LineRenderer lineRenderer;
	public Tile currentTile;
	public Tile playerTile;
	public int playerSteps;
	public int steps = 3;
	public List<Vector3> positions;
	public bool canMove = false;

    System.Random pseudoRandom;
    int randomX = 0;
    int randomZ = 0;

    const int initHoleNumber = 8;

    private Ray ray;
    private RaycastHit hit;

    private const float TILE_SIZE = 1;
    private const float TILE_OFFSET = 0.5f;
    // subject to changes
    private const float PLAYER_HEIGHT = 0.5f;

    Tile[,] map;
    private GameObject[,] selectProjectors;

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
        initPlayer();
        generating = false;
		UpdateText("Blank");

		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.widthMultiplier = 0.2f;


		// A simple 2 color gradient with a fixed alpha of 1.0f.
		float alpha = 1.0f;
		Gradient gradient = new Gradient();
		gradient.SetKeys(
			new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
			new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
		);
		lineRenderer.colorGradient = gradient;

		getPaths (x, z, steps, new List<Tile>());	
		positions = new List<Vector3> ();
		positions.Add (new Vector3 (x + TILE_OFFSET, 0.5f, z + TILE_OFFSET));
		currentTile = map [x, z];
		playerTile = map [x, z];
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
				if (hit.collider.tag == "Tile") {
					int x = (int)hit.point.x;
					int z = (int)hit.point.z;
					if (map [x, z] == playerTile) {
						canMove = true;
					}
				}
            }
        }
		if (Input.GetMouseButton (0)) {	
			if (Physics.Raycast (ray, out hit, 100f)) {	
				if (hit.collider.tag == "Tile" && canMove) {
					int x = (int)hit.point.x;
					int z = (int)hit.point.z;
					if (map [x, z] != currentTile) {
						if (steps != 0 && currentTile.links.Contains (map [x, z])) {
							currentTile = map [x, z];
							positions.Add (new Vector3 (x + TILE_OFFSET, 0.5f, z + TILE_OFFSET));
							if (currentTile.tileType != Tile.TileType.HOLE) {
								steps -= 1;
							}
							UpdateText (steps + "");
							clearProjection ();
							getPaths (x, z, steps, new List<Tile> ());
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
					if (hit.collider.tag == "Tile") {
						int x = (int)hit.point.x;
						int z = (int)hit.point.z;
						if (map [x, z] == currentTile) {
							movePlayer (x, z);
							playerSteps = 2;
							steps = playerSteps;
							playerTile = map [x, z];
							currentTile = playerTile;
						} else {
							currentTile = playerTile;
							steps = playerSteps;
						}
						clearProjection();
						getPaths (playerTile.x, playerTile.z, playerSteps, new List<Tile> ());
						positions.Add (new Vector3 (playerTile.x + TILE_OFFSET, 0.5f, playerTile.z + TILE_OFFSET));
					}
				}
			}
			UpdateText (steps + "");
			canMove = false;
			lineRenderer.positionCount = 0;
			lineRenderer.SetPositions (new Vector3[0]);
		}
		


        if (Input.GetMouseButtonDown(1))
        {
            if (!generating)
            {
                Debug.Log("Inverting with right click");
                generating = true;
                invertMap();
                generating = false;
            } else
            {
                Debug.Log("Pressed right click when generating.");
            }
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(2))
        {
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.tag == "Tile")
                {
                    int x = (int) hit.point.x;
                    int z = (int) hit.point.z;
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

    void castProjection(int x, int z)
    {
        if (selectProjectors[x, z] == null)
        {
            selectProjectors[x, z] = Instantiate(projections[0], new Vector3(x + TILE_OFFSET, 4, z + TILE_OFFSET), Quaternion.Euler(new Vector3(90, 0, 0)));
        }
    }
	void clearProjection()
	{
		for (int x = 0; x < width; x++)
		{
			for (int z = 0; z < height; z++)
			{
				Destroy (selectProjectors [x, z]);
				selectProjectors [x, z] = null;
			}
		}
	}

    void initPlayer()
    {
        x = 4;
        z = 4;
        player = Instantiate(playerPrefab, new Vector3(x + TILE_OFFSET,PLAYER_HEIGHT,z + TILE_OFFSET), Quaternion.identity);
    }

    void movePlayer(int x, int z)
    {
        player.transform.position = new Vector3(x + TILE_OFFSET, PLAYER_HEIGHT, z + TILE_OFFSET);
        //player = Instantiate(playerPrefab, new Vector3(x + TILE_OFFSET, PLAYER_HEIGHT, z + TILE_OFFSET), Quaternion.identity);
    }

	void UpdateText (string s)
	{
		TextDisplay.text = "Steps: " + s;
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

	List<Tile> getPaths(int x, int z, int steps, List<Tile> explored,bool showProjections = true)
	{
		List<Tile> output = new List<Tile> ();
		if (steps !=0){
		output.Add (map [x, z]);
		if (showProjections) {
			castProjection (x, z);
		}
			foreach (Tile link in map[x,z].links) {
				if (!explored.Contains (link)) {
					output.Add (link);
					if (showProjections) {
						castProjection (link.x, link.z);
					}
					if (link.tileType == Tile.TileType.HOLE) {
						output.AddRange (getPaths (link.x, link.z, steps, output));
					}
					for (int step = steps - 1; step > 0; step--) {
						output.AddRange (getPaths (link.x, link.z, step, output));
					}
				}
			}
		}
	
		return output;

	}
		
}
