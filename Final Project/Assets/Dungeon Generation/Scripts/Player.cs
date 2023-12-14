using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Rigidbody body;
    public float speed;
	private Vector3 forwardVelocity = Vector3.zero;
	private Vector3 rightVelocity = Vector3.zero;
	public BaseWFC wfc;
	public float horizontalSpeed = 2f;
	public float verticalSpeed = 2f;
	private Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
		cameraTransform = Camera.main.transform;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKey(KeyCode.W))
		{
			forwardVelocity = transform.forward * speed;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			forwardVelocity = transform.forward * -speed;
		}
		else
		{
			forwardVelocity = Vector3.zero;
		}

		if (Input.GetKey(KeyCode.A))
		{
			rightVelocity = transform.right * -speed;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			rightVelocity = transform.right * speed;
		}
		else
		{
			rightVelocity = Vector3.zero;
		}

		transform.Rotate(0, horizontalSpeed * Input.GetAxis("Mouse X"), 0);
		cameraTransform.Rotate(-verticalSpeed * Input.GetAxis("Mouse Y"), 0, 0);

		if(cameraTransform.rotation.eulerAngles.x < 180f) cameraTransform.rotation = Quaternion.Euler(Mathf.Clamp(cameraTransform.rotation.eulerAngles.x, 0f, 25f), cameraTransform.rotation.eulerAngles.y, 0);
		else cameraTransform.rotation = Quaternion.Euler(Mathf.Clamp(cameraTransform.rotation.eulerAngles.x, 335f, 360f), cameraTransform.rotation.eulerAngles.y, 0);
	}

    private void FixedUpdate()
    {
		body.velocity = forwardVelocity + rightVelocity;
    }

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("MazeEnd"))
		{
			SceneManager.LoadScene("Space");
		}
	}
}
