using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Tiles", order = 1)]
public class Tiles : ScriptableObject
{
	[SerializeField]
	private GameObject[] tilePrefabs;

	public List<Tile> tiles = new List<Tile>();

	private void OnEnable()
	{
		// Hallways
		tiles.Add(new Tile(tilePrefabs[0],  new string[] { "ABA", "ABA", "AAA", "ABA" }, false));
		tiles.Add(new Tile(tilePrefabs[1],  new string[] { "ABA", "ABA", "ABA", "AAA" }, false));
		tiles.Add(new Tile(tilePrefabs[2],  new string[] { "AAA", "ABA", "ABA", "ABA" }, false));
		tiles.Add(new Tile(tilePrefabs[3],  new string[] { "ABA", "AAA", "ABA", "ABA" }, false));
		tiles.Add(new Tile(tilePrefabs[4],  new string[] { "ABA", "AAA", "ABA", "AAA" }, false));
		tiles.Add(new Tile(tilePrefabs[5],  new string[] { "AAA", "ABA", "AAA", "ABA" }, false));
		tiles.Add(new Tile(tilePrefabs[6],  new string[] { "ABA", "ABA", "ABA", "ABA" }, false));

		// Blank Tile
		tiles.Add(new Tile(tilePrefabs[7], new string[] { "AAA", "AAA", "AAA", "AAA" }, false));

		// Basic Rooms						    
		tiles.Add(new Tile(tilePrefabs[8],  new string[] { "AAA", "ABA", "AAA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[9],  new string[] { "ABA", "AAA", "AAA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[10], new string[] { "AAA", "AAA", "AAA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[11], new string[] { "AAA", "AAA", "ABA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[12], new string[] { "AAA", "ABA", "AAA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[13], new string[] { "ABA", "AAA", "ABA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[14], new string[] { "ABA", "ABA", "AAA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[15], new string[] { "ABA", "AAA", "AAA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[16], new string[] { "AAA", "AAA", "ABA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[17], new string[] { "AAA", "ABA", "ABA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[18], new string[] { "ABA", "ABA", "AAA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[19], new string[] { "ABA", "AAA", "ABA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[20], new string[] { "AAA", "ABA", "ABA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[21], new string[] { "ABA", "ABA", "ABA", "AAA" }, true));
		tiles.Add(new Tile(tilePrefabs[22], new string[] { "ABA", "ABA", "ABA", "ABA" }, true));

		// Extended Algorithm Rooms
		tiles.Add(new Tile(tilePrefabs[23], new string[] { "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[24], new string[] { "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[25], new string[] { "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA" }, true));
		tiles.Add(new Tile(tilePrefabs[26], new string[] { "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA", "ABA" }, true));
	}
}
