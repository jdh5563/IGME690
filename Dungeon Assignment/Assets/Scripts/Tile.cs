using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a tile in the level. A tile is placed into a cell and has rules applied to it so only valid tiles are placed adjacently.
/// </summary>
public class Tile
{
	public string[] edges;
	public List<int> up = new List<int>();
	public List<int> right = new List<int>();
	public List<int> down = new List<int>();
	public List<int> left = new List<int>();
	public bool isRoom;

	public GameObject tile;

	public Tile(GameObject tile, string[] edges, bool isRoom)
	{
		this.tile = tile;
		this.edges = edges;
		this.isRoom = isRoom;
	}

	/// <summary>
	/// Builds lists of valid tiles that can be placed adjacently to this tile
	/// </summary>
	/// <param name="tiles">The list of all tiles</param>
	public void Analyze(List<Tile> tiles)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			Tile tile = tiles[i];

			// UP
			if (CompareEdge(tile.edges[2], this.edges[0]))
			{
				this.up.Add(i);
			}

			// RIGHT
			if (CompareEdge(tile.edges[3], this.edges[1]))
			{
				this.right.Add(i);
			}

			// DOWN
			if (CompareEdge(tile.edges[0], this.edges[2]))
			{
				this.down.Add(i);
			}

			// LEFT
			if (CompareEdge(tile.edges[1], this.edges[3]))
			{
				this.left.Add(i);
			}
		}
	}

	/// <summary>
	/// Compares the spelling of 2 edges. If one edge's spelling is equal to the spelling of a different edge reversed, the edge is valid.
	/// </summary>
	/// <param name="edge1">The first edge</param>
	/// <param name="edge2">The second edge</param>
	/// <returns>True if the edges are successfully validated</returns>
	private bool CompareEdge(string edge1, string edge2)
	{
		for (int i = 0; i < edge1.Length; i++)
		{
			if (edge1[i] != edge2[edge2.Length - 1 - i])
			{
				return false;
			}
		}

		return true;
	}
}