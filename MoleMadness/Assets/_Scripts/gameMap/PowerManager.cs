using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PowerManager {

    public enum PowerType { A,B,C,D,E,None }
    public static MapManager mapManager = MapManager.getInstance();
    public static bool[] spawned = new bool[4];
    private static Dictionary<string, PowerType> spawnCoor =
            new Dictionary<string, PowerType>(){
            { "3,3", PowerType.None},
            { "3,6", PowerType.None},
            { "6,3", PowerType.None},
            { "6,6", PowerType.None}
        };
    public const float POWERHEIGHT = 1;

	public static void spawnPower(int x, int z, PowerType powerType)
    {
        Transform transform = mapManager.GetComponent<Transform>();
        string key = string.Format("{0},{1}", x, z);
        //Debug.Log(spawnCoor[key]);
        try
        {
            if (spawnCoor[key] == PowerType.None)
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
