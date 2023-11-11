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

	// Start is called before the first frame update
	void Start()
    {
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
}
