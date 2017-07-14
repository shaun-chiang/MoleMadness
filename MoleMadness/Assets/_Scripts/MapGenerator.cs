using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour {

    bool generating;

    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;

    public GameObject hillPrefab;
    public GameObject flatPrefab;
    public GameObject holePrefab;

    System.Random pseudoRandom;
    int randomX = 0;
    int randomZ = 0;

    const int initHoleNumber = 8;

    const float xInterval = 1;
    const float zInterval = 1;

    Tile[,] map;

    private void Start()
    {
        generating = true;
        map = new Tile[width, height];
        if (useRandomSeed)
        {
            seed = DateTime.Now.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
        RandomFillMap();
        createTilesFromMap();
        generating = false;
    }

    void RandomFillMap()
    {

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                map[x, z] = new Tile();
                map[x, z].tileType = Tile.TileType.FLAT;
            }
        }

        randomGenerateHolesAndHills();
        
    }

    void Update()
    {
        // For testing only
        if (Input.GetMouseButtonDown(0))
            if (!generating)
            {
                Debug.Log("Generating with left click");
                generating = true;
                destroyMap();
                //RandomFillMap();
                randomGenerateHolesAndHills();
                createTilesFromMap();
                generating = false;
            }
            else
            {
                Debug.Log("Pressed Left click when generating.");
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

        //if (Input.GetMouseButtonDown(2))
        //    Debug.Log("Pressed middle click.");

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
                    map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + xInterval * x, 0, transform.position.y + zInterval * z), transform.rotation);
                }
                else if (map[x, z].tileType == Tile.TileType.HOLE)
                {
                    Destroy(map[x, z].tileObject);
                    map[x, z].tileType = Tile.TileType.HILL;
                    map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + xInterval * x, 0, transform.position.y + zInterval * z), transform.rotation);
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
                        map[x, z].tileObject = Instantiate(flatPrefab, new Vector3(transform.position.x + xInterval * x, 0, transform.position.y + zInterval * z), transform.rotation);
                        break;
                    case Tile.TileType.HILL:
                        map[x, z].tileObject = Instantiate(hillPrefab, new Vector3(transform.position.x + xInterval * x, 0, transform.position.y + zInterval * z), transform.rotation);
                        break;
                    case Tile.TileType.HOLE:
                        map[x, z].tileObject = Instantiate(holePrefab, new Vector3(transform.position.x + xInterval * x, 0, transform.position.y + zInterval * z), transform.rotation);
                        break;
                    default:
                        break;
                }
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

    //void OnDrawGizmos()
    //{
    //    if (map != null)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            for (int y = 0; y < height; y++)
    //            {
    //                Vector3 pos;
    //                switch (map[x, y])
    //                {
    //                    case BORDER:
    //                        Gizmos.color = Color.black;
    //                        pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
    //                        break;
    //                    case FLAT:
    //                        Gizmos.color = Color.green;
    //                        pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
    //                        break;
    //                    case HOLE:
    //                        Gizmos.color = Color.red;
    //                        pos = new Vector3(-width / 2 + x + .5f, -0.5f, -height / 2 + y + .5f);
    //                        break;
    //                    case HILL:
    //                        Gizmos.color = Color.blue;
    //                        pos = new Vector3(-width / 2 + x + .5f, 0.5f, -height / 2 + y + .5f);
    //                        break;
    //                    default:
    //                        pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
    //                        break;
    //                }

    //                Gizmos.DrawCube(pos, Vector3.one);
    //            }
    //        }
    //    }
    //}

}
