using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This class represents the basic Wave Function Collapse Algorithm.
/// </summary>
public class ExtendedWFC : MonoBehaviour
{
	[SerializeField]
	private Tiles tileData;
	[SerializeField]
	private GameObject mazeEndPrefab;

	private Cell[] grid = new Cell[DIMENSION * DIMENSION];

	private const int DIMENSION = 20;
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

		StartOver();
	}

	// Update is called once per frame
	void Update()
	{
		// Create a new level when the spacebar is pressed
		if (Input.GetKeyDown(KeyCode.Space))
		{
			StartOver();
			return;
		}

		if(!allCellsCollapsed) GenerateLevel();
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
	public void StartOver()
	{
		allCellsCollapsed = false;
		uniqueTileList.Clear();

		// Destroy any cells that already exist
		foreach (GameObject tile in GameObject.FindGameObjectsWithTag("Tile"))
		{
			Destroy(tile);
		}

		if(GameObject.FindGameObjectWithTag("MazeEnd") != null) Destroy(GameObject.FindGameObjectWithTag("MazeEnd"));

		// Generate cells
		for (int i = 0; i < DIMENSION; i++)
		{
			for (int j = 0; j < DIMENSION; j++)
			{
				// All cells start empty
				grid[j + i * DIMENSION] = new Cell(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 }, j, i);
			}
		}

		#region Generate Critical Path
		int currentYIndex = Random.Range(0, DIMENSION);
		int currentXIndex = 0;
		grid[currentYIndex * DIMENSION] = new Cell(new int[] { 21 }, currentXIndex, currentYIndex);

		Cell roomCell = grid[currentYIndex * DIMENSION];
		roomCell.isCollapsed = true;
		GameObject instantiatedTile = Instantiate(tileData.tiles[roomCell.options[0]].tile, new Vector2(roomCell.x * CELL_DIMENSION, roomCell.y * CELL_DIMENSION), tileData.tiles[roomCell.options[0]].tile.transform.rotation);
		instantiatedTile.GetComponent<SpriteRenderer>().color = Color.blue;

		startTile = instantiatedTile;

		int critPathLength = 1;
		uniqueTileList.Add(roomCell.options[0]);

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
			bool isRoomAdjacent = false;

			List<int> allOptions = new List<int>(tileData.tiles.Count);
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
			if (allOptions.Contains(26)) allOptions.Remove(26);
			#endregion

			grid[currentXIndex + currentYIndex * DIMENSION] = new Cell(allOptions.ToArray(), currentXIndex, currentYIndex);
			
			roomCell = grid[currentXIndex + currentYIndex * DIMENSION];
			roomCell.isCollapsed = true;

			if (roomCell.options.Length > 0)
			{
				int selectedOption = roomCell.options[Random.Range(0, roomCell.options.Length)];
				roomCell.options = new int[] { selectedOption };

				instantiatedTile = Instantiate(tileData.tiles[roomCell.options[0]].tile, new Vector2(roomCell.x * CELL_DIMENSION, roomCell.y * CELL_DIMENSION), tileData.tiles[roomCell.options[0]].tile.transform.rotation);
				instantiatedTile.GetComponent<SpriteRenderer>().color = Color.blue;

				endTile = instantiatedTile;

				critPathLength++;
				if (!uniqueTileList.Contains(roomCell.options[0])) uniqueTileList.Add(roomCell.options[0]);

				if (selectedOption > 22)
				{
					switch (selectedOption)
					{
						case 23:
							if (currentYIndex > 0 && currentXIndex < DIMENSION - 2 &&
								grid[(currentXIndex + 1) + currentYIndex * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex + 2) + currentYIndex * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex + 2) + (currentYIndex - 1) * DIMENSION].isCollapsed == false)
							{
								grid[(currentXIndex + 1) + currentYIndex * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 1, currentYIndex);
								grid[(currentXIndex + 1) + currentYIndex * DIMENSION].isCollapsed = true;
								grid[(currentXIndex + 2) + currentYIndex * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 2, currentYIndex);
								grid[(currentXIndex + 2) + currentYIndex * DIMENSION].isCollapsed = true;
								grid[(currentXIndex + 2) + (currentYIndex - 1) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 2, currentYIndex - 1);
								grid[(currentXIndex + 2) + (currentYIndex - 1) * DIMENSION].isCollapsed = true;

								currentXIndex += 2;
								currentYIndex--;

								critPathLength += 3;
							}
							else if (roomCell.options.Length > 1)
							{
								do
								{
									selectedOption = roomCell.options[Random.Range(0, roomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;

						case 24:
							if (currentYIndex < DIMENSION - 2 && currentXIndex > 0 &&
								grid[(currentXIndex + 1) + currentYIndex * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex + 1) + (currentYIndex + 1) * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex + 1) + (currentYIndex + 2) * DIMENSION].isCollapsed == false)
							{
								grid[(currentXIndex + 1) + currentYIndex * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 1, currentYIndex);
								grid[(currentXIndex + 1) + currentYIndex * DIMENSION].isCollapsed = true;
								grid[(currentXIndex + 1) + (currentYIndex + 1) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 1, currentYIndex + 1);
								grid[(currentXIndex + 1) + (currentYIndex + 1) * DIMENSION].isCollapsed = true;
								grid[(currentXIndex + 1) + (currentYIndex + 2) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 1, currentYIndex + 2);
								grid[(currentXIndex + 1) + (currentYIndex + 2) * DIMENSION].isCollapsed = true;

								currentXIndex++;
								currentYIndex += 2;

								critPathLength += 3;
							}
							else if (roomCell.options.Length > 1)
							{
								do
								{
									selectedOption = roomCell.options[Random.Range(0, roomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;

						case 25:
							if (currentYIndex > 0 && currentXIndex < DIMENSION - 2 &&
								grid[currentXIndex + (currentYIndex - 1) * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex + 1) + (currentYIndex - 1) * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex + 2) + (currentYIndex - 1) * DIMENSION].isCollapsed == false)
							{
								grid[currentXIndex + (currentYIndex - 1) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex, currentYIndex - 1);
								grid[currentXIndex + (currentYIndex - 1) * DIMENSION].isCollapsed = true;
								grid[(currentXIndex + 1) + (currentYIndex - 1) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 1, currentYIndex - 1);
								grid[(currentXIndex + 1) + (currentYIndex - 1) * DIMENSION].isCollapsed = true;
								grid[(currentXIndex + 2) + (currentYIndex - 1) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex + 2, currentYIndex - 1);
								grid[(currentXIndex + 2) + (currentYIndex - 1) * DIMENSION].isCollapsed = true;

								currentXIndex += 2;
								currentYIndex--;

								critPathLength += 3;
							}
							else if (roomCell.options.Length > 1)
							{
								do
								{
									selectedOption = roomCell.options[Random.Range(0, roomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;

						case 26:
							if (currentYIndex > 1 &&
								grid[(currentXIndex - 1) + currentYIndex * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex - 1) + (currentYIndex - 1) * DIMENSION].isCollapsed == false &&
								grid[(currentXIndex - 1) + (currentYIndex - 2) * DIMENSION].isCollapsed == false)
							{
								grid[(currentXIndex - 1) + currentYIndex * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex - 1, currentYIndex);
								grid[(currentXIndex - 1) + currentYIndex * DIMENSION].isCollapsed = true;
								grid[(currentXIndex - 1) + (currentYIndex - 1) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex - 1, currentYIndex - 1);
								grid[(currentXIndex - 1) + (currentYIndex - 1) * DIMENSION].isCollapsed = true;
								grid[(currentXIndex - 1) + (currentYIndex - 2) * DIMENSION] = new Cell(new int[] { selectedOption }, currentXIndex - 1, currentYIndex - 2);
								grid[(currentXIndex - 1) + (currentYIndex - 2) * DIMENSION].isCollapsed = true;

								currentXIndex--;
								currentYIndex -= 2;

								critPathLength += 3;
							}
							else if (roomCell.options.Length > 1)
							{
								do
								{
									selectedOption = roomCell.options[Random.Range(0, roomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;
					}
				}
			}
		}

		if (currentXIndex == DIMENSION - 2)
		{
			grid[++currentXIndex + currentYIndex * DIMENSION] = new Cell(new int[] { Random.Range(8, 22) }, currentXIndex, currentYIndex);

			roomCell = grid[currentXIndex + currentYIndex * DIMENSION];
			roomCell.isCollapsed = true;
			instantiatedTile = Instantiate(tileData.tiles[roomCell.options[0]].tile, new Vector2(roomCell.x * CELL_DIMENSION, roomCell.y * CELL_DIMENSION), tileData.tiles[roomCell.options[0]].tile.transform.rotation);
			instantiatedTile.GetComponent<SpriteRenderer>().color = Color.blue;

			endTile = instantiatedTile;

			critPathLength++;
			if (!uniqueTileList.Contains(roomCell.options[0])) uniqueTileList.Add(roomCell.options[0]);
		}

		Instantiate(mazeEndPrefab, endTile.transform.position, Quaternion.identity);

		Debug.Log("Critical Path Length: " + critPathLength);
		#endregion
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
			Debug.Log("Number of Unique Tiles: " + uniqueTileList.Count);
			foreach(GameObject tile in GameObject.FindGameObjectsWithTag("Tile"))
			{
				tile.GetComponent<SpriteRenderer>().color = Color.white;
			}
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

				if (selectedOption > 22)
				{
					int indexOfRandomCell = Array.IndexOf(grid, randomCell);

					switch (selectedOption)
					{
						case 23:
							if (indexOfRandomCell >= DIMENSION && indexOfRandomCell % DIMENSION < DIMENSION - 2 &&
								grid[indexOfRandomCell + 1].isCollapsed == false &&
								grid[indexOfRandomCell + 2].isCollapsed == false &&
								grid[indexOfRandomCell + 2 - DIMENSION].isCollapsed == false)
							{	
								grid[indexOfRandomCell + 1].isCollapsed = true;
								grid[indexOfRandomCell + 1].options = new int[] { selectedOption };
								grid[indexOfRandomCell + 2].isCollapsed = true;
								grid[indexOfRandomCell + 2].options = new int[] { selectedOption };
								grid[indexOfRandomCell + 2 - DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell + 2 - DIMENSION].options = new int[] { selectedOption };
							}
							else if (randomCell.options.Length > 1)
							{
								do
								{
									selectedOption = randomCell.options[Random.Range(0, randomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;

						case 24:
							if (indexOfRandomCell / DIMENSION < DIMENSION - 2 && indexOfRandomCell % DIMENSION > 0 &&
								grid[indexOfRandomCell + 1].isCollapsed == false &&
								grid[indexOfRandomCell + 1 + DIMENSION].isCollapsed == false &&
								grid[indexOfRandomCell + 1 + 2 * DIMENSION].isCollapsed == false)
							{
								grid[indexOfRandomCell + 1].isCollapsed = true;
								grid[indexOfRandomCell + 1].options = new int[] { selectedOption };
								grid[indexOfRandomCell + 1 + DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell + 1 + DIMENSION].options = new int[] { selectedOption };
								grid[indexOfRandomCell + 1 + 2 * DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell + 1 + 2 * DIMENSION].options = new int[] { selectedOption };
							}
							else if (randomCell.options.Length > 1)
							{
								do
								{
									selectedOption = randomCell.options[Random.Range(0, randomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;

						case 25:
							if (indexOfRandomCell >= DIMENSION && indexOfRandomCell % DIMENSION < DIMENSION - 2 &&
								grid[indexOfRandomCell - DIMENSION].isCollapsed == false &&
								grid[indexOfRandomCell + 1 - DIMENSION].isCollapsed == false &&
								grid[indexOfRandomCell + 2 - DIMENSION].isCollapsed == false)
							{
								grid[indexOfRandomCell - DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell - DIMENSION].options = new int[] { selectedOption };
								grid[indexOfRandomCell + 1 - DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell + 1 - DIMENSION].options = new int[] { selectedOption };
								grid[indexOfRandomCell + 2 - DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell + 2 - DIMENSION].options = new int[] { selectedOption };
							}
							else if (randomCell.options.Length > 1)
							{
								do
								{
									selectedOption = randomCell.options[Random.Range(0, randomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;

						case 26:
							if (indexOfRandomCell / DIMENSION > 1 &&
								grid[indexOfRandomCell - 1].isCollapsed == false &&
								grid[indexOfRandomCell - 1 - DIMENSION].isCollapsed == false &&
								grid[indexOfRandomCell - 1 - 2 * DIMENSION].isCollapsed == false)
							{
								grid[indexOfRandomCell - 1].isCollapsed = true;
								grid[indexOfRandomCell - 1].options = new int[] { selectedOption };
								grid[indexOfRandomCell - 1 - DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell - 1 - DIMENSION].options = new int[] { selectedOption };
								grid[indexOfRandomCell - 1 - 2 * DIMENSION].isCollapsed = true;
								grid[indexOfRandomCell - 1 - 2 * DIMENSION].options = new int[] { selectedOption };
							}
							else if (randomCell.options.Length > 1)
							{
								do
								{
									selectedOption = randomCell.options[Random.Range(0, randomCell.options.Length)];
								}
								while (selectedOption > 22);
							}
							else
							{
								StartOver();
								return;
							}

							break;
					}
				}

				randomCell.options = new int[] { selectedOption };

				Instantiate(tileData.tiles[randomCell.options[0]].tile, new Vector2(randomCell.x * CELL_DIMENSION, randomCell.y * CELL_DIMENSION), tileData.tiles[randomCell.options[0]].tile.transform.rotation);
				if (!uniqueTileList.Contains(randomCell.options[0])) uniqueTileList.Add(randomCell.options[0]);
			}
			else
			{
				StartOver();
				return;
			}

			// Create a new array to represent the new state of the grid
			Cell[] nextGrid = new Cell[DIMENSION * DIMENSION];

			for (int i = 0; i < DIMENSION; i++)
			{
				for (int j = 0; j < DIMENSION; j++)
				{
					Cell cell = grid[j + i * DIMENSION];

					// If the current cell is collapsed, add it to the new grid
					if (cell.isCollapsed)
					{
						nextGrid[j + i * DIMENSION] = grid[j + i * DIMENSION];
					}
					else
					{
						bool isRoomAdjacent = false;

						List<int> allOptions = new List<int>(tileData.tiles.Count);
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

						nextGrid[j + i * DIMENSION] = new Cell(allOptions.ToArray(), j, i);
					}
				}
			}

			grid = nextGrid;
		}
	}
}