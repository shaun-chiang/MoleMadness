using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {

    public enum TileType { HOLE, FLAT, HILL };

    public GameObject tileObject;
    public TileType tileType;
	public int x;
	public int z;

	public List<Tile> links;
	public bool status = false;

	public Tile()
    {

    }
	public Tile(int x, int z)
	{
		this.x = x;
		this.z = z;
	}
}
