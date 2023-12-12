using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    [SerializeField] private UnityEvent unityEvent;
    private Ray mouseRay;
    private RaycastHit hit;

	[SerializeField]
	private GameObject[] planetPrefabs;

    public bool hasConnections = false;

	// Start is called before the first frame update
	void Start()
    {
        unityEvent.AddListener(GenerateSystem);
    }

    // Update is called once per frame
    void Update()
    {
		mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(mouseRay, out hit) && hit.collider.gameObject == gameObject)
        {
            if(Input.GetMouseButtonDown(0))
            {
                unityEvent.Invoke();
            }
        }
    }

    public void GenerateSystem()
    {
        for (int i = 0; i < Random.Range(3, 9); i++)
        {
            GameObject planet = Instantiate(planetPrefabs[Random.Range(0, planetPrefabs.Length)]);
            planet.GetComponent<Planet>().shapeSettings.planetRadius = Random.Range(1f, 5f);

            planet.GetComponent<Planet>().shapeSettings.noiseLayers[0].noiseSettings.simpleNoiseSettings.baseRoughness = Random.Range(0.2f, 2.3f);
            planet.GetComponent<Planet>().shapeSettings.noiseLayers[0].noiseSettings.simpleNoiseSettings.centre = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            planet.GetComponent<Planet>().shapeSettings.noiseLayers[0].noiseSettings.simpleNoiseSettings.minValue = Random.Range(1f, 1.5f);// Simple

            planet.GetComponent<Planet>().shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.strength = Random.Range(1f, 2f);
			planet.GetComponent<Planet>().shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.baseRoughness = Random.Range(0.2f, 1f);
			planet.GetComponent<Planet>().shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.minValue = Random.Range(0.37f, 2f);// Rigid

            planet.GetComponent<Planet>().GeneratePlanet();

            planet.transform.position = new Vector3(10 * i, 0, 0);
            planet.transform.Rotate(new Vector3(0, Random.Range(0f, 360f), 0));
        }
	}
}
