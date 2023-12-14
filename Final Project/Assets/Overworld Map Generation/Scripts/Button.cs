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

    public List<GameObject> connections = new List<GameObject>();

    [SerializeField]
    private Gradient[] sunGradients;

    [SerializeField]
    private Transform planetParent;
    private GameObject[] system;

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
        transform.parent.gameObject.SetActive(false);
        int numPlanets = Random.Range(3, 9) + 1;
        system = new GameObject[numPlanets];

		Planet sun = Instantiate(planetPrefabs[planetPrefabs.Length - 1], planetParent).GetComponent<Planet>();
        sun.shapeSettings.planetRadius = Random.Range(8f, 12f);
		sun.shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.baseRoughness = Random.Range(0.5f, 3f);
		sun.shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.centre = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));

		sun.transform.Rotate(new Vector3(0, Random.Range(0f, 360f), 0));

        int sunGradientIndex = Random.Range(0, sunGradients.Length);

		sun.colourSettings.oceanColour = sunGradients[sunGradientIndex];
        sun.colourSettings.biomeColourSettings.biomes[0].gradient.SetKeys(new GradientColorKey[] { sunGradients[sunGradientIndex].colorKeys[sunGradients[sunGradientIndex].colorKeys.Length - 1] }, sunGradients[sunGradientIndex].alphaKeys);

		sun.GeneratePlanet();

		for (int i = 1; i < numPlanets; i++)
        {
            system[i] = Instantiate(planetPrefabs[Random.Range(0, planetPrefabs.Length - 1)], planetParent);
			system[i].GetComponent<Planet>().shapeSettings.planetRadius = Random.Range(1f, 5f);

			system[i].GetComponent<Planet>().shapeSettings.noiseLayers[0].noiseSettings.simpleNoiseSettings.baseRoughness = Random.Range(0.2f, 2.3f);
            system[i].GetComponent<Planet>().shapeSettings.noiseLayers[0].noiseSettings.simpleNoiseSettings.centre = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            system[i].GetComponent<Planet>().shapeSettings.noiseLayers[0].noiseSettings.simpleNoiseSettings.minValue = Random.Range(1f, 1.5f);// Simple

            system[i].GetComponent<Planet>().shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.strength = Random.Range(1f, 2f);
			system[i].GetComponent<Planet>().shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.baseRoughness = Random.Range(0.2f, 1f);
			system[i].GetComponent<Planet>().shapeSettings.noiseLayers[1].noiseSettings.simpleNoiseSettings.minValue = Random.Range(0.37f, 2f);// Rigid

            system[i].GetComponent<Planet>().GeneratePlanet();

            system[i].transform.position = new Vector3(sun.GetComponent<Planet>().shapeSettings.planetRadius + 10 * i, 0, 0);
            system[i].transform.Rotate(new Vector3(0, Random.Range(0f, 360f), 0));
        }
	}
}
