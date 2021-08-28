using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class cameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;
	private Vector2 move;
	private bool canLook;

	void OnLook(InputValue movementValue) {
		this.move = movementValue.Get<Vector2>();
	}
	void OnCanLook(InputValue value) {
		this.canLook = value.Get<float>() > 0;
	}

	// Start is called before the first frame update
	void Start()
    {
        this.offset = this.transform.position - this.player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = this.player.transform.position + offset;
		if (this.canLook) {
			this.transform.RotateAround(this.player.transform.position, Vector3.up, this.move.x);
			this.transform.RotateAround(this.player.transform.position, Vector3.left, this.move.y);
		}
	}
}
