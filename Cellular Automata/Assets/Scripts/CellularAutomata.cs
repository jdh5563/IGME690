using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
	[SerializeField] private Material aliveMat;
	[SerializeField] private Material deadMat;

    [SerializeField] private GameObject cellContainer;
    [SerializeField] private float simulationDelay;

	[SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float aliveChance;
    [SerializeField] private Vector2 killRange;
    [SerializeField] private int reviveThreshold;

    private Cell[,] cells;
    private bool isPausing = false;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[height, width];

        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                cells[i, j] = Instantiate(cellPrefab, new Vector3(j * cellPrefab.transform.localScale.x, 0, i * cellPrefab.transform.localScale.z), Quaternion.identity, cellContainer.transform).GetComponent<Cell>();
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
        if (!isPausing) StartCoroutine(RunSimulation());
    }

    private IEnumerator RunSimulation()
    {
        isPausing = true;
        cells = Simulate();

        yield return new WaitForSeconds(simulationDelay);

        isPausing = false;
    }

    private Cell[,] Simulate()
    {
		Cell[,] newCells = new Cell[height, width];
        List<Cell> cellsToDestroy = new List<Cell>();

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int numLivingNeighbors = CountLivingNeighbors(j, i);
                newCells[i, j] = cells[i, j];
				if (cells[i, j].IsAlive)
				{
                    Debug.Log(cells[i, j].X + " " + cells[i, j].Y);
					if (numLivingNeighbors < killRange.x || numLivingNeighbors > killRange.y)
					{
                        cellsToDestroy.Add(cells[i, j]);
						newCells[i, j] = Instantiate(cellPrefab, new Vector3(j * cellPrefab.transform.localScale.x, 0, i * cellPrefab.transform.localScale.z), Quaternion.identity, cellContainer.transform).GetComponent<Cell>();
						newCells[i, j].IsAlive = false;
						newCells[i, j].GetComponent<MeshRenderer>().material = deadMat;
					}
				}
				else if (numLivingNeighbors == reviveThreshold)
				{
					cellsToDestroy.Add(cells[i, j]);
					newCells[i, j] = Instantiate(cellPrefab, new Vector3(j * cellPrefab.transform.localScale.x, 0, i * cellPrefab.transform.localScale.z), Quaternion.identity, cellContainer.transform).GetComponent<Cell>();
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
                //if(!(i == 0 && j == 0) && x + j >= 0 && y + i >= 0 && x + j < width && y + i < height && cells[x + j, y + i].IsAlive)
                //{
                //    numLivingNeighbors++;
                //}

                if(i == 0 && j == 0)
                {

                }
                else if(x + j < 0 || x + j > width - 1 ||  y + i < 0 || y + i > height - 1)
                {

                }
                else if (cells[x + j, y + i].IsAlive)
                {
                    numLivingNeighbors++;
                }
			}
		}

        return numLivingNeighbors;
	}
}
