using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
	[SerializeField]
	private int seed;

	private void Awake()
	{
		if (seed == -1) seed = Random.Range(0, 100000); // Only set a random seed if we want a random one
		Random.InitState(seed);
	}

	// Start is called before the first frame update
	void Start()
    {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public static bool CheckClick(GameObject obj)
	{
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		return Physics.Raycast(mouseRay, out RaycastHit hit) && hit.collider.gameObject == obj && Input.GetMouseButtonDown(0);
	}
}
