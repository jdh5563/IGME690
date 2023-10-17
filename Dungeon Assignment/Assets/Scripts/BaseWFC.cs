using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the basic Wave Function Collapse Algorithm.
/// </summary>
public class BaseWFC : MonoBehaviour
{
	[SerializeField] private Tiles tileData;
	[SerializeField] private GameObject mazeEndPrefab;
	[SerializeField] private GameObject cratesPrefab;
	[SerializeField] private GameObject treasurePrefab;

	private Cell[] grid = new Cell[DIMENSION * DIMENSION];

	private const int DIMENSION = 10;
	private const int GRID_DIMENSION = DIMENSION * 10;
	private const int CELL_DIMENSION = GRID_DIMENSION / DIMENSION;

	public static bool allCellsCollapsed = false;
	public static GameObject startTile;
	public static GameObject endTile;

	private List<int> uniqueTileList = new List<int>();

	// Start is called before the first frame update
	void Start()
	{
		foreach (Tile tile in tileData.tiles)
		{
			tile.Analyze(tileData.tiles);
		}

		while (!StartOver()) { }
	}

	// Update is called once per frame
	void Update()
	{
		// Create a new level when the spacebar is pressed
		if (Input.GetKeyDown(KeyCode.Space))
		{
			while (!StartOver()) { }
			return;
		}

		if (!allCellsCollapsed) GenerateLevel();
	}

	/// <summary>
	/// Removes any tiles from the list of total available options that are not in the given list of valid options
	/// </summary>
	/// <param name="allOptions">All available tile options</param>
	/// <param name="validOptions">All valid tile options</param>
	private void CheckValid(List<int> allOptions, List<int> validOptions)
	{
		for (int i = allOptions.Count - 1; i >= 0; i--)
		{
			if (!validOptions.Contains(allOptions[i]))
			{
				allOptions.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Generates a new list of empty cells then creates a critical path for the level
	/// </summary>
	public bool StartOver()
	{
		allCellsCollapsed = false;
		uniqueTileList.Clear();

		// Destroy any cells that already exist
		foreach (GameObject tile in GameObject.FindGameObjectsWithTag("Tile"))
		{
			Destroy(tile);
		}

		if (GameObject.FindGameObjectWithTag("MazeEnd") != null) Destroy(GameObject.FindGameObjectWithTag("MazeEnd"));

		// Generate cells
		for (int i = 0; i < DIMENSION; i++)
		{
			for (int j = 0; j < DIMENSION; j++)
			{
				// All cells start empty
				grid[j + i * DIMENSION] = new Cell(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 }, j, i);
			}
		}

		#region Generate Critical Path
		int currentYIndex = Random.Range(1, DIMENSION - 1);
		int currentXIndex = 0;
		grid[currentYIndex * DIMENSION] = new Cell(new int[] { 21 }, currentXIndex, currentYIndex);

		Cell roomCell = grid[currentYIndex * DIMENSION];
		roomCell.isCollapsed = true;
		GameObject instantiatedTile = Instantiate(tileData.tiles[roomCell.options[0]].tile, new Vector3(roomCell.x * CELL_DIMENSION, 0, roomCell.y * CELL_DIMENSION), tileData.tiles[roomCell.options[0]].tile.transform.rotation);
		startTile = instantiatedTile;

		while (currentXIndex < DIMENSION - 2)
		{
			int previousYIndex = currentYIndex;
			int previousXIndex = currentXIndex;

			#region Select Next Cell Index
			if (currentYIndex == 0)
			{
				if (!grid[currentXIndex + DIMENSION].isCollapsed && tileData.tiles[grid[currentXIndex + DIMENSION].options[0]].edges[0] == "ABA")
				{
					currentYIndex = 1;
				}
				else
				{
					currentXIndex++;
				}
			}
			else if (currentYIndex == DIMENSION - 1)
			{
				if (!grid[currentXIndex + (DIMENSION - 2) * DIMENSION].isCollapsed && tileData.tiles[grid[currentXIndex + (DIMENSION - 2) * DIMENSION].options[0]].edges[2] == "ABA")
				{
					currentYIndex = DIMENSION - 2;
				}
				else
				{
					currentXIndex++;
				}
			}
			else
			{
				if (!grid[currentXIndex + (currentYIndex - 1) * DIMENSION].isCollapsed && tileData.tiles[grid[currentXIndex + currentYIndex * DIMENSION].options[0]].edges[0] == "ABA" && !grid[currentXIndex + (currentYIndex + 1) * DIMENSION].isCollapsed && tileData.tiles[grid[currentXIndex + currentYIndex * DIMENSION].options[0]].edges[2] == "ABA")
				{
					currentYIndex = Random.Range(0f, 1f) < 0.5f ? currentYIndex - 1 : currentYIndex + 1;
				}
				else if (grid[currentXIndex + (currentYIndex - 1) * DIMENSION].isCollapsed && tileData.tiles[grid[currentXIndex + currentYIndex * DIMENSION].options[0]].edges[0] == "ABA")
				{
					currentYIndex += 1;
				}
				else if (grid[currentXIndex + (currentYIndex + 1) * DIMENSION].isCollapsed && tileData.tiles[grid[currentXIndex + currentYIndex * DIMENSION].options[0]].edges[2] == "ABA")
				{
					currentYIndex -= 1;
				}
				else
				{
					currentXIndex++;
				}
			}
			#endregion

			#region Update Options
			List<int> invalidEdges = new List<int>();
			bool isRoomAdjacent = false;

			List<int> allOptions = new List<int>(22);
			for (int num = 0; num < allOptions.Capacity; num++)
			{
				allOptions.Add(num);
			}

			// Look at all adjacent cells to filter out invalid tile options for this cell

			// LOOK DOWN
			if (currentYIndex > 0)
			{
				Cell down = grid[currentXIndex + (currentYIndex - 1) * DIMENSION];
				List<int> validOptions = new List<int>();
				foreach (int option in down.options)
				{
					validOptions.AddRange(tileData.tiles[option].up);
				}

				CheckValid(allOptions, validOptions);

				if (down.isCollapsed && tileData.tiles[down.options[0]].isRoom)
				{
					isRoomAdjacent = true;
				}
			}
			else
			{
				// I0, Plus, T123. 1 3, 2Ac1, 2Ad23, 3 123, 4
				invalidEdges.AddRange(new int[] { 1, 2, 3, 4, 6, 11, 13, 16, 17, 19, 20, 21, 22 });
			}

			// LOOK LEFT
			if (currentXIndex > 0)
			{
				Cell left = grid[(currentXIndex - 1) + currentYIndex * DIMENSION];
				List<int> validOptions = new List<int>();

				foreach (int option in left.options)
				{
					validOptions.AddRange(tileData.tiles[option].right);
				}

				CheckValid(allOptions, validOptions);

				if (left.isCollapsed && tileData.tiles[left.options[0]].isRoom)
				{
					isRoomAdjacent = true;
				}
			}
			else
			{
				// I1, Plus, T023. 1 2, 2Ac0, 2Ad12, 3 012, 4
				invalidEdges.AddRange(new int[] { 0, 2, 3, 5, 6, 10, 12, 15, 16, 18, 19, 20, 22 });
			}

			// LOOK UP
			if (currentYIndex < DIMENSION - 1)
			{
				Cell up = grid[currentXIndex + (currentYIndex + 1) * DIMENSION];
				List<int> validOptions = new List<int>();

				foreach (int option in up.options)
				{
					validOptions.AddRange(tileData.tiles[option].down);
				}

				CheckValid(allOptions, validOptions);

				if (up.isCollapsed && tileData.tiles[up.options[0]].isRoom)
				{
					isRoomAdjacent = true;
				}
			}
			else
			{
				// I0, Plus, T013. 1 1, 2Ac1, 2Ad01, 3 013, 4
				invalidEdges.AddRange(new int[] { 0, 1, 3, 4, 6, 9, 13, 14, 15, 18, 19, 21, 22 });
			}

			// LOOK RIGHT
			if (currentXIndex < DIMENSION - 1)
			{
				Cell right = grid[(currentXIndex + 1) + currentYIndex * DIMENSION];
				List<int> validOptions = new List<int>();
				foreach (int option in right.options)
				{
					validOptions.AddRange(tileData.tiles[option].left);
				}

				CheckValid(allOptions, validOptions);

				if (right.isCollapsed && tileData.tiles[right.options[0]].isRoom)
				{
					isRoomAdjacent = true;
				}
			}
			else
			{
				// I1, Plus, T012. 1 0, 2Ac0, 2Ad03, 3 023, 4
				invalidEdges.AddRange(new int[] { 0, 1, 2, 5, 6, 8, 12, 14, 17, 18, 20, 21, 22 });
			}

			// This if statement will prevent rooms from being adjacent to each other
			// It requires the adjacent tile checks above to check if any adjacent tiles are room tiles
			if (isRoomAdjacent)
			{
				for (int index = 0; index < allOptions.Count; index++)
				{
					if (allOptions[index] > 7)
					{
						allOptions.RemoveRange(index, allOptions.Count - index);
						break;
					}
				}
			}

			foreach (int invalidOption in invalidEdges)
			{
				allOptions.Remove(invalidOption);
			}

			// Remove any options that do not have a right-side door
			if (allOptions.Contains(7)) allOptions.Remove(7);
			if (allOptions.Contains(3)) allOptions.Remove(3);
			if (allOptions.Contains(4)) allOptions.Remove(4);
			if (allOptions.Contains(9)) allOptions.Remove(9);
			if (allOptions.Contains(10)) allOptions.Remove(10);
			if (allOptions.Contains(11)) allOptions.Remove(11);
			if (allOptions.Contains(13)) allOptions.Remove(13);
			if (allOptions.Contains(15)) allOptions.Remove(15);
			if (allOptions.Contains(16)) allOptions.Remove(16);
			if (allOptions.Contains(19)) allOptions.Remove(19);

			if (allOptions.Count == 0) return false;
			#endregion

			grid[currentXIndex + currentYIndex * DIMENSION] = new Cell(allOptions.ToArray(), currentXIndex, currentYIndex);
			roomCell = grid[currentXIndex + currentYIndex * DIMENSION];
			roomCell.isCollapsed = true;

			if (roomCell.options.Length > 0)
			{
				int selectedOption = roomCell.options[Random.Range(0, roomCell.options.Length)];
				roomCell.options = new int[] { selectedOption };
				if (roomCell.options[0] > 7) roomCell.isRoom = true;

				instantiatedTile = Instantiate(tileData.tiles[roomCell.options[0]].tile, new Vector3(roomCell.x * CELL_DIMENSION, 0, roomCell.y * CELL_DIMENSION), tileData.tiles[roomCell.options[0]].tile.transform.rotation);
			}
		}

		if (currentYIndex == 0) currentYIndex++;
		else if(currentYIndex == DIMENSION - 1) currentYIndex--;
		grid[++currentXIndex + currentYIndex * DIMENSION] = new Cell(new int[] { 19 }, currentXIndex, currentYIndex);

		roomCell = grid[currentXIndex + currentYIndex * DIMENSION];
		roomCell.isCollapsed = true;
		instantiatedTile = Instantiate(tileData.tiles[roomCell.options[0]].tile, new Vector3(roomCell.x * CELL_DIMENSION, 0, roomCell.y * CELL_DIMENSION), tileData.tiles[roomCell.options[0]].tile.transform.rotation);
		endTile = instantiatedTile;
		Instantiate(mazeEndPrefab, new Vector3(endTile.transform.position.x, 0.5f, endTile.transform.position.z), Quaternion.identity);
		#endregion

		return true;
	}

	/// <summary>
	/// Run the Wave Function Collapse algorithm to generate a level
	/// </summary>
	private void GenerateLevel()
	{
		// Create a copy of the current grid to modify
		List<Cell> gridCopy = new List<Cell>();
		gridCopy.AddRange(grid);

		// Remove any collapsed cells
		for (int i = 0; i < gridCopy.Count; i++)
		{
			if (gridCopy[i].isCollapsed)
			{
				gridCopy.Remove(gridCopy[i]);
				i--;
			}
		}

		if(gridCopy.Count == 0)
		{
			allCellsCollapsed = true;
			return;
		}

		// Sort the grid by the number of options they have in ascending order
		gridCopy.Sort((cell1, cell2) => cell1.options.Length.CompareTo(cell2.options.Length));

		if (gridCopy.Count > 0)
		{
			int leastEntropy = gridCopy[0].options.Length;
			int stopIndex = 0;

			// Determine the last index where cells in the grid have the same entropy
			for (int i = 0; i < gridCopy.Count; i++)
			{
				if (gridCopy[i].options.Length > leastEntropy)
				{
					stopIndex = i;
					break;
				}
			}

			// Select a random cell out of those with the least entropy and collapse it
			int randomIndex = stopIndex == 0 ? Random.Range(0, gridCopy.Count) : Random.Range(0, stopIndex);
			Cell randomCell = gridCopy[randomIndex];
			randomCell.isCollapsed = true;

			// Instantiate a random tile at that cell if it has options available
			// Otherwise, the algorithm has failed and it must start over
			if (randomCell.options.Length > 0)
			{
				int selectedOption = randomCell.options[Random.Range(0, randomCell.options.Length)];

				randomCell.options = new int[] { selectedOption };
				if (randomCell.options[0] > 7) randomCell.isRoom = true;
				GameObject spawnedCell = Instantiate(tileData.tiles[randomCell.options[0]].tile, new Vector3(randomCell.x * CELL_DIMENSION, 0, randomCell.y * CELL_DIMENSION), tileData.tiles[randomCell.options[0]].tile.transform.rotation);
				if(randomCell.isRoom) GenerateLoot(spawnedCell, randomCell);
			}
			else
			{
				while (!StartOver()) { }
				return;
			}

			// Create a new array to represent the new state of the grid
			Cell[] nextGrid = new Cell[DIMENSION * DIMENSION];

			for (int i = 0; i < DIMENSION; i++)
			{
				for (int j = 0; j < DIMENSION; j++)
				{
					List<int> invalidEdges = new List<int>();
					Cell cell = grid[j + i * DIMENSION];

					// If the current cell is collapsed, add it to the new grid
					if (cell.isCollapsed)
					{
						nextGrid[j + i * DIMENSION] = grid[j + i * DIMENSION];
					}
					else
					{
						bool isRoomAdjacent = false;

						List<int> allOptions = new List<int>(22);
						for (int num = 0; num < allOptions.Capacity; num++)
						{
							allOptions.Add(num);
						}

						// Look at all adjacent cells to filter out invalid tile options for this cell

						// LOOK DOWN
						if (i > 0)
						{
							Cell down = grid[j + (i - 1) * DIMENSION];
							List<int> validOptions = new List<int>();
							foreach (int option in down.options)
							{
								validOptions.AddRange(tileData.tiles[option].up);
							}

							CheckValid(allOptions, validOptions);

							if (down.isCollapsed && tileData.tiles[down.options[0]].isRoom)
							{
								isRoomAdjacent = true;
							}
						}
						else
						{
							// I0, Plus, T123. 1 3, 2Ac1, 2Ad23, 3 123, 4
							invalidEdges.AddRange(new int[]{ 1, 2, 3, 4, 6, 11, 13, 16, 17, 19, 20, 21, 22 });
						}

						// LOOK LEFT
						if (j > 0)
						{
							Cell left = grid[(j - 1) + i * DIMENSION];
							List<int> validOptions = new List<int>();

							foreach (int option in left.options)
							{
								validOptions.AddRange(tileData.tiles[option].right);
							}

							CheckValid(allOptions, validOptions);

							if (left.isCollapsed && tileData.tiles[left.options[0]].isRoom)
							{
								isRoomAdjacent = true;
							}
						}
						else
						{
							// I1, Plus, T023. 1 2, 2Ac0, 2Ad12, 3 012, 4
							invalidEdges.AddRange(new int[] { 0, 2, 3, 5, 6, 10, 12, 15, 16, 18, 19, 20, 22 });
						}

						// LOOK UP
						if (i < DIMENSION - 1)
						{
							Cell up = grid[j + (i + 1) * DIMENSION];
							List<int> validOptions = new List<int>();

							foreach (int option in up.options)
							{
								validOptions.AddRange(tileData.tiles[option].down);
							}

							CheckValid(allOptions, validOptions);

							if (up.isCollapsed && tileData.tiles[up.options[0]].isRoom)
							{
								isRoomAdjacent = true;
							}
						}
						else
						{
							// I0, Plus, T013. 1 1, 2Ac1, 2Ad01, 3 013, 4
							invalidEdges.AddRange(new int[] { 0, 1, 3, 4, 6, 9, 13, 14, 15, 18, 19, 21, 22 });
						}

						// LOOK RIGHT
						if (j < DIMENSION - 1)
						{
							Cell right = grid[(j + 1) + i * DIMENSION];
							List<int> validOptions = new List<int>();
							foreach (int option in right.options)
							{
								validOptions.AddRange(tileData.tiles[option].left);
							}

							CheckValid(allOptions, validOptions);

							if (right.isCollapsed && tileData.tiles[right.options[0]].isRoom)
							{
								isRoomAdjacent = true;
							}
						}
						else
						{
							// I1, Plus, T012. 1 0, 2Ac0, 2Ad03, 3 023, 4
							invalidEdges.AddRange(new int[] { 0, 1, 2, 5, 6, 8, 12, 14, 17, 18, 20, 21, 22 });
						}

						// This if statement will prevent rooms from being adjacent to each other
						// It requires the adjacent tile checks above to check if any adjacent tiles are room tiles
						if (isRoomAdjacent)
						{
							for (int index = 0; index < allOptions.Count; index++)
							{
								if (allOptions[index] > 7)
								{
									allOptions.RemoveRange(index, allOptions.Count - index);
									break;
								}
							}
						}

						foreach(int invalidOption in invalidEdges)
						{
							allOptions.Remove(invalidOption);
						}

						nextGrid[j + i * DIMENSION] = new Cell(allOptions.ToArray(), j, i);
					}
				}
			}

			grid = nextGrid;
		}
	}

	private void GenerateLoot(GameObject spawnedCell, Cell cell)
	{
		bool hasTreasure = false;

		// Try to generate loot in each corner of the room
		for(int i = 0; i < 4; i++)
		{
			float x = spawnedCell.transform.position.x;
			float xOffset = 2.5f;
			float z = spawnedCell.transform.position.z;
			float zOffset = 2.5f;
			GameObject loot = null;

			// 25% chance to have treasure in the room
			if (!hasTreasure && Random.Range(0f, 1f) < 0.1f)
			{
				hasTreasure = true;
				loot = Instantiate(treasurePrefab, new Vector3(x, 0, z), Quaternion.identity);
				loot.transform.LookAt(spawnedCell.transform);
				loot.transform.SetParent(spawnedCell.transform, true);
			}
			else if(Random.Range(0f, 1f) < 0.3f)
			{
				loot = Instantiate(cratesPrefab, new Vector3(x, 0, z), Quaternion.identity);
				loot.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
				loot.transform.SetParent(spawnedCell.transform, true);
			}

			if (loot != null)
			{
				switch (i)
				{
					case 0:
						loot.transform.position = new Vector3(x + xOffset, 0, z + zOffset);
						break;
					case 1:
						loot.transform.position = new Vector3(x + xOffset, 0, z - zOffset);
						break;
					case 2:
						loot.transform.position = new Vector3(x - xOffset, 0, z + zOffset);
						break;
					default:
						loot.transform.position = new Vector3(x - xOffset, 0, z - zOffset);
						break;
				}
			}
		}
	}
}