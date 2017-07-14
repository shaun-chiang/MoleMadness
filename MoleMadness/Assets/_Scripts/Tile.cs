using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {

    public enum TileType { HOLE, FLAT, HILL };

    public GameObject tileObject;
    public TileType tileType;

    public Tile()
    {

    }
}
