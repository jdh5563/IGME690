using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
	[Header("Random Seed"), Space]
	[SerializeField] private int seed;

	[Space, Header("Cellular Automata Objects"), Space]
    [SerializeField] private GameObject cellPrefab;
	[SerializeField] private GameObject needlePrefab;
	[SerializeField] private Material aliveMat;
	[SerializeField] private Material deadMat;
    [SerializeField] private GameObject cellContainer;

	[Space, Header("Cellular Automata Parameters"), Space]
	[SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float aliveChance;
    [SerializeField] private Vector2 killRange;
    [SerializeField] private int reviveThreshold;

	[Space, Header("A* Objects"), Space]
	[SerializeField] private Material roadMat;

	[Space, Header("Helpful Tools"), Space]
    [SerializeField] private int maxCAIterations;
	[SerializeField] private int maxAStarIterations;
	[SerializeField] private bool debugFlag;
	[SerializeField] private float simulationDelay;

	private Cell[,] cells;
    private bool isPausing = false;
    private int numCAIterations = 0;

    // Start is called before the first frame update
    void Start()
    {
		// 21363
		// 15455
		Random.InitState(seed);
        cells = new Cell[height, width];

        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                cells[i, j] = Instantiate(cellPrefab, cellContainer.transform.position + new Vector3(j * cellPrefab.transform.localScale.x, 0, i * cellPrefab.transform.localScale.z), Quaternion.identity, cellContainer.transform).GetComponent<Cell>();
                cells[i, j].X = j;
                cells[i, j].Y = i;
				cells[i, j].IsAlive = Random.Range(0f, 1f) < aliveChance;
				cells[i, j].GetComponent<MeshRenderer>().material = cells[i, j].IsAlive ? aliveMat : deadMat;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Run multiple iterations of A* where there are multiple stops on the way to the true end for roads

        // Run cellular automata
        if (numCAIterations < maxCAIterations)
        {
            if (debugFlag && !isPausing) StartCoroutine(RunDelayedSimulation());
            else cells = Simulate();

			if (++numCAIterations == maxCAIterations)
			{
				ExtrudeCells();
				Cell cell;
				do
				{
					switch (Random.Range(0, 4))
					{
						// Q1
						case 0:
							cell = cells[Random.Range(height / 2, height), Random.Range(width / 2, width)];
							break;
						// Q2
						case 1:
							cell = cells[Random.Range(height / 2, height), Random.Range(0, width / 2)];
							break;
						// Q3
						case 2:
							cell = cells[Random.Range(0, height / 2), Random.Range(0, width / 2)];
							break;
						// Q4
						default:
							cell = cells[Random.Range(0, height / 2), Random.Range(width / 2, width)];
							break;
					}
				}
				while (cell.IsAlive);

				for (int i = 0; i < maxAStarIterations; i++)
				{
					int numTries = 0;
					GameObject newStart = null;
					do
					{
						if (++numTries > 10)
						{
							i--;
							break;
						}

						newStart = AStar(cell.gameObject);
					}
					while (newStart == null);

					if (newStart != null) cell = newStart.GetComponent<Cell>();
				}
			}
		}
	}

    private IEnumerator RunDelayedSimulation()
    {
        isPausing = true;
        cells = Simulate();

        yield return new WaitForSeconds(simulationDelay);

        isPausing = false;
    }

	#region Cellular Automata
	private Cell[,] Simulate()
    {
		Cell[,] newCells = new Cell[height, width];
        List<Cell> cellsToDestroy = new List<Cell>();

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{

				int numLivingNeighbors = CountLivingNeighbors(cells[i, j].X, cells[i, j].Y);
                newCells[i, j] = cells[i, j];
				if (cells[i, j].IsAlive)
				{
					if (numLivingNeighbors < killRange.x || numLivingNeighbors > killRange.y)
					{
                        cellsToDestroy.Add(cells[i, j]);
						newCells[i, j] = Instantiate(cellPrefab, cellContainer.transform.position + new Vector3(j * cellPrefab.transform.localScale.x, 0, i * cellPrefab.transform.localScale.z), Quaternion.identity, cellContainer.transform).GetComponent<Cell>();
						newCells[i, j].X = j;
						newCells[i, j].Y = i;
						newCells[i, j].IsAlive = false;
						newCells[i, j].GetComponent<MeshRenderer>().material = deadMat;
					}
				}
				else if (numLivingNeighbors == reviveThreshold)
				{
					cellsToDestroy.Add(cells[i, j]);
					newCells[i, j] = Instantiate(cellPrefab, cellContainer.transform.position + new Vector3(j * cellPrefab.transform.localScale.x, 0, i * cellPrefab.transform.localScale.z), Quaternion.identity, cellContainer.transform).GetComponent<Cell>();
					newCells[i, j].X = j;
					newCells[i, j].Y = i;
					newCells[i, j].IsAlive = true;
					newCells[i, j].GetComponent<MeshRenderer>().material = aliveMat;
				}
			}
		}

        cellsToDestroy.ForEach(cell => Destroy(cell.gameObject));

        return newCells;
	}

    private int CountLivingNeighbors(int x, int y)
    {
        int numLivingNeighbors = 0;

		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
                if (!(i == 0 && j == 0) && x + j >= 0 && y + i >= 0 && x + j < width && y + i < height && cells[y + i, x + j].IsAlive)
                {
                    numLivingNeighbors++;
                }
            }
		}

        return numLivingNeighbors;
	}

    private void ExtrudeCells()
    {
		foreach (Cell cell in cells)
		{
			if (cell.IsAlive)
			{
				int height = Random.Range(3, 21);
				for (int i = 1; i <= height; i++)
				{
					// This should be replaced with a set of prefabs that represent blocks with windows/lights
					GameObject building = Instantiate(cellPrefab, new Vector3(cell.transform.position.x, i, cell.transform.position.z), Quaternion.identity, cell.transform);
					building.GetComponent<MeshRenderer>().material = aliveMat;
				}

				if (Random.Range(0f, 1f) < 0.25f) Instantiate(needlePrefab, new Vector3(cell.transform.position.x, height + 1.5f, cell.transform.position.z), Quaternion.identity, cell.transform);
			}
		}
	}
	#endregion

	#region A* Roads
	private GameObject AStar(GameObject startCell)
	{
		GameObject endCell;

		do
		{
			if (startCell.transform.position.x < width / 2)
			{
				// Go to Q2
				if (startCell.transform.position.z < height / 2)
				{
					endCell = cells[Random.Range(height / 2, height), Random.Range(0, width / 2)].gameObject;
				}
				// Go to Q1
				else
				{
					endCell = cells[Random.Range(height / 2, height), Random.Range(width / 2, width)].gameObject;
				}
			}
			else
			{
				// Go to Q3
				if (startCell.transform.position.z < height / 2)
				{
					endCell = cells[Random.Range(0, height / 2), Random.Range(0, width / 2)].gameObject;
				}
				// Go to Q4
				else
				{
					endCell = cells[Random.Range(0, height / 2), Random.Range(width / 2, width)].gameObject;
				}
			}
		}
		while (endCell.GetComponent<Cell>().IsAlive);

		List < GameObject > openList = new List<GameObject>();
		List<GameObject> closedList = new List<GameObject>();

		openList.Add(startCell);

		GameObject currentCell;

		while(openList.Count > 0)
		{
			currentCell = FindClosestCell(openList, endCell);

			if (currentCell == endCell)
			{
				closedList.Add(currentCell);
				break;
			}

			for(int i = 0; i < 4; i++)
			{
				GameObject nextCell;

				switch (i)
				{
					// Look up
					case 0:
						if (currentCell.GetComponent<Cell>().Y == height - 1 || cells[currentCell.GetComponent<Cell>().Y + 1, currentCell.GetComponent<Cell>().X].IsAlive) continue;
						nextCell = cells[currentCell.GetComponent<Cell>().Y + 1, currentCell.GetComponent<Cell>().X].gameObject;
						break;
					// Look right
					case 1:
						if (currentCell.GetComponent<Cell>().X == width - 1 || cells[currentCell.GetComponent<Cell>().Y, currentCell.GetComponent<Cell>().X + 1].IsAlive) continue;
						nextCell = cells[currentCell.GetComponent<Cell>().Y, currentCell.GetComponent<Cell>().X + 1].gameObject;
						break;
					// Look down
					case 2:
						if (currentCell.GetComponent<Cell>().Y == 0 || cells[currentCell.GetComponent<Cell>().Y - 1, currentCell.GetComponent<Cell>().X].IsAlive) continue;
						nextCell = cells[currentCell.GetComponent<Cell>().Y - 1, currentCell.GetComponent<Cell>().X].gameObject;
						break;
					// Look left				
					default:
						if (currentCell.GetComponent<Cell>().X == 0 || cells[currentCell.GetComponent<Cell>().Y, currentCell.GetComponent<Cell>().X - 1].IsAlive) continue;
						nextCell = cells[currentCell.GetComponent<Cell>().Y, currentCell.GetComponent<Cell>().X - 1].gameObject;
						break;
				}

				if (closedList.Contains(nextCell) || openList.Contains(nextCell)) continue;

				openList.Add(nextCell);
			}

			openList.Remove(currentCell);
			closedList.Add(currentCell);
		}

		if (closedList[closedList.Count - 1] != endCell) return null;

		foreach(GameObject cell in closedList)
		{
			cell.GetComponent<MeshRenderer>().material = roadMat;
		}

		return endCell;
	}

	/// <summary>
	/// Find the cell closest to the end.
	/// </summary>
	private GameObject FindClosestCell(List<GameObject> cells, GameObject endCell)
	{
		GameObject closestCell = cells[0];
		for(int i = 1; i < cells.Count; i++)
		{
			if (ManhattanHeuristic(cells[i], endCell) < ManhattanHeuristic(closestCell, endCell)) closestCell = cells[i];
		}

		return closestCell;
	}

	/// <summary>
	/// Compute a Manhattan Heuristic.
	/// </summary>
	private float ManhattanHeuristic(GameObject start, GameObject end)
	{
		return
			Mathf.Abs(start.transform.position.x - end.transform.position.x) +
			Mathf.Abs(start.transform.position.z - end.transform.position.z);
	}
	#endregion
}