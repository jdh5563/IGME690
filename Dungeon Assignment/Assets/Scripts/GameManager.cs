using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject player;
	private bool hasPlayerSpawned = false;

	private void Update()
	{
		if (!BaseWFC.allCellsCollapsed)
		{
			player.transform.position = new Vector2(-5000, 0);
			hasPlayerSpawned = false;
		}
		else if (!hasPlayerSpawned)
		{
			player.transform.position = new Vector3(BaseWFC.startTile.transform.position.x, 1f, BaseWFC.startTile.transform.position.z);
			player.transform.rotation = Quaternion.Euler(0, 90, 0);
			hasPlayerSpawned = true;
		}
	}
}