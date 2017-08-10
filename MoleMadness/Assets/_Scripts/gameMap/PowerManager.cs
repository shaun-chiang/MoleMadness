using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PowerManager {

    public enum PowerType { A,B,C,D,E,None }
    public static MapManager mapManager = MapManager.getInstance();
    public static bool[] spawned = new bool[4];
	public static Dictionary<string, GameManager.Powers> spawnCoor =
            new Dictionary<string, GameManager.Powers>(){
		{ "3,3", GameManager.Powers.NOTHING},
		{ "3,6", GameManager.Powers.NOTHING},
		{ "6,3", GameManager.Powers.NOTHING},
		{ "6,6", GameManager.Powers.NOTHING}
        };
    public const float POWERHEIGHT = 1;

	public static void spawnPower(int x, int z,  GameManager.Powers powerType)
    {
        Transform transform = mapManager.GetComponent<Transform>();
        string key = string.Format("{0},{1}", x, z);
        //Debug.Log(spawnCoor[key]);
        try
        {
			if (spawnCoor[key] == GameManager.Powers.NOTHING)
            {
                Debug.Log(string.Format("Instantiating Power {0} at {1}, {2}", powerType, x, z));
                UnityEngine.Object.Instantiate(mapManager.powers[(int)powerType], new Vector3(transform.position.x + MapManager.TILE_OFFSET + MapManager.TILE_SIZE * x, POWERHEIGHT, transform.position.y + MapManager.TILE_OFFSET + MapManager.TILE_SIZE * z), Quaternion.identity);
                spawnCoor[key] = powerType;
            }
            else
            {
                Debug.Log(string.Format("Power {0} exists at {1}, {2}", spawnCoor[key], x, z));
            }
        } catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("Invalid positions for instantiating power up. Only accept (3,3), (3,6), (6,3), (6,6)");
        }
    }

}
