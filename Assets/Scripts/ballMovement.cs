using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ballMovement : MonoBehaviour
{
    public float speed;
	public float jumpPower;
	public float jumpSpeedBoost;
	public int maxCoyoteTime;
	public int maxJumpHold;
	public float jumpHoldCurveSteepness;
	public GameObject mainCamera;


	private Rigidbody rb;
	private Collider col;

    public Vector2 move;
	private bool isJumping;
	public List<GameObject> onground = new List<GameObject>();
	private int coyoteTime;
	private float holdJumpTime;


	void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
		this.col = this.GetComponent<Collider>();
    }    

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        this.move = movementVector;
    }

	void OnJump(InputValue value) {
		this.isJumping = value.Get<float>() > 0;
	}
	void OnCollisionEnter(Collision collision) {
		float bottomY = this.transform.position.y - (this.col.bounds.size.y / 4);
		bool below = false;
		int count = collision.contactCount;
		for (int i = 0; i < count; i++) {
			if (collision.GetContact(i).point.y <= bottomY) { // Point of contact has to be in the bottom quarter.
				below = true;
				break;
			}
		}
		// collision.gameObject.transform.position.y + (collision.gameObject.GetComponent<Collider>().bounds.size.y / 2) <= this.transform.position.y
		if (below) {
			this.onground.Add(collision.gameObject);
		}

	}
	void OnCollisionExit(Collision collision) {
		this.onground.Remove(collision.gameObject);
	}

	void FixedUpdate() {
		float y = 0.0f;
		if (this.isJumping) {
			if (this.onground.Count != 0 || (this.holdJumpTime > 0 && this.holdJumpTime < this.maxJumpHold)) {
				this.holdJumpTime++;
				y = (this.jumpPower / (
					Mathf.Sqrt(
						this.holdJumpTime
						* this.jumpHoldCurveSteepness
					)
					- (this.jumpHoldCurveSteepness - 1)
				)) * ((Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) * this.jumpSpeedBoost) + 1);
			}
		}
		else {
			this.holdJumpTime = 0.0f;
		}

		float rad = Mathf.Atan2(this.move.x, this.move.y) + (this.mainCamera.transform.eulerAngles.y * Mathf.Deg2Rad);
		float distance = Mathf.Sqrt(Mathf.Pow(this.move.x, 2) + Mathf.Pow(this.move.y, 2));
        Vector3 move3 = new Vector3(
            Mathf.Sin(rad) * speed * distance,
            y,
			Mathf.Cos(rad) * speed * distance
        );
        rb.AddForce(move3);
    }
}
