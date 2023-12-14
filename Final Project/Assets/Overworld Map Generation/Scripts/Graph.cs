using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private GameObject connectionPrefab;
    [SerializeField, Min(3)]
    private int gridLength;
    [SerializeField, Min(3)]
    private int gridHeight;
    [SerializeField]
    private int cellWidth;
    [SerializeField]
    private int cellHeight;

    private GameObject[,] graph;
    private GameObject startNode;

	// Start is called before the first frame update
	void Start()
    {
		GenerateGraph();
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
            foreach (Transform nodeTransform in transform)
            {
                Destroy(nodeTransform.gameObject);
            }

            GenerateGraph();
        }
    }

    private void Connect(Vector2 from, Vector2 to, float distance, Transform parent)
	{
        GameObject connection = Instantiate(connectionPrefab, Vector2.Lerp(from, to, 0.5f), Quaternion.identity, parent);
        SpriteRenderer connectionRenderer = connection.GetComponent<SpriteRenderer>();
        connectionRenderer.size = new Vector2(distance, connectionRenderer.size.y);
        connection.transform.Rotate(new Vector3(0, 0, Mathf.Atan2(from.y - to.y, from.x - to.x) * Mathf.Rad2Deg));
	}

    private void GenerateGraph()
	{
        graph = new GameObject[gridHeight, gridLength];

		int currentIndex = Random.Range(0, graph.GetLength(0));
		graph[currentIndex, 0] = Instantiate(nodePrefab, new Vector2(Random.Range(-cellWidth / 2f, cellWidth / 2f), (currentIndex * cellHeight) + Random.Range(-cellHeight / 2f, cellHeight / 2f)), Quaternion.identity, transform);
        startNode = graph[currentIndex, 0];

		for (int j = 1; j < graph.GetLength(1); j++)
		{
            int previousIndex = currentIndex;

			if (currentIndex == 0)
			{
                currentIndex = Random.Range(0, 1);
			}
			else if (currentIndex == graph.GetLength(0) - 1)
			{
                currentIndex = Random.Range(graph.GetLength(0) - 2, graph.GetLength(0) - 1);
			}
			else
			{
                currentIndex = Random.Range(currentIndex - 1, currentIndex + 1);
			}

            graph[currentIndex, j] = Instantiate(nodePrefab, new Vector2((j * cellWidth) + Random.Range(-cellWidth / 2.5f, cellWidth / 2.5f), (currentIndex * cellHeight) + Random.Range(-cellHeight / 2f, cellHeight / 2f)), Quaternion.identity, transform);
            Connect(graph[previousIndex, j - 1].transform.position, graph[currentIndex, j].transform.position, Vector2.Distance(graph[previousIndex, j - 1].transform.position, graph[currentIndex, j].transform.position), graph[previousIndex, j - 1].transform);
            graph[previousIndex, j - 1].GetComponent<Button>().connections.Add(graph[currentIndex, j]);
			graph[currentIndex, j].GetComponent<Button>().connections.Add(graph[previousIndex, j - 1]);
        }

		for (int i = 0; i < graph.GetLength(0); i++)
        {
            for (int j = 0; j < graph.GetLength(1); j++)
            {
                if (graph[i, j] == null && Random.Range(0f, 1f) < 0.75f)
                {
                    graph[i, j] = Instantiate(nodePrefab, new Vector2((j * cellWidth) + Random.Range(-cellWidth / 2f, cellWidth / 2f), (i * cellHeight) + Random.Range(-cellHeight / 2f, cellHeight / 2f)), Quaternion.identity, transform);

                    // This needs to change
                    foreach (GameObject node in graph)
                    {
                        if (node != null && node != graph[i, j])
                        {
                            float distance = Vector2.Distance(graph[i, j].transform.position, node.transform.position);

                            if (distance < 7f)
                            {
                                Connect(graph[i, j].transform.position, node.transform.position, distance, graph[i, j].transform);
								graph[i, j].GetComponent<Button>().connections.Add(node);
								node.GetComponent<Button>().connections.Add(graph[i, j]);
							}
                        }
                    }
                }
            }
        }

		List<GameObject> validNodes = FindValidNodes(startNode.GetComponent<Button>(), new List<GameObject>() { startNode });

        foreach(GameObject node in graph)
        {
            if (node && !validNodes.Contains(node))
            {
                Destroy(node);
            }
        }
    }

    private List<GameObject> FindValidNodes(Button node, List<GameObject> validNodes)
    {
        foreach (GameObject connection in node.connections)
        {
            if (!validNodes.Contains(connection))
            {
                validNodes.Add(connection);
                FindValidNodes(connection.GetComponent<Button>(), validNodes);
            }
        }

        return validNodes;
	}

	private void OnDrawGizmos()
	{
        if (graph != null)
        {
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    Gizmos.DrawWireCube(new Vector2(j * cellWidth, i * cellHeight), new Vector2(cellWidth, cellHeight));
                }
            }
        }
	}
}
