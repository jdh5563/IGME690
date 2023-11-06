using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ALL CODE FOUND HERE: https://medium.com/@mikeyoung_97230/creating-a-simple-camera-controller-in-unity3d-using-c-ec1a79584687
public class CameraController : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float sensitivity;
	// Start is called before the first frame update
	void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
		// Move the camera forward, backward, left, and right
		transform.position += Input.GetAxis("Vertical") * speed * Time.deltaTime * transform.forward;
		transform.position += Input.GetAxis("Horizontal") * speed * Time.deltaTime * transform.right;

		// Rotate the camera based on the mouse movement
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
		transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
	}
}
