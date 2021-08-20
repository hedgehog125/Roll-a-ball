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


	private Rigidbody rb;
	private Collider col;
    private Vector2 move;
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
		float maxY = this.transform.position.y - (this.col.bounds.size.y / 2);
		bool below = false;
		int count = collision.contactCount;
		for (int i = 0; i < count; i++) {
			if (collision.GetContact(i).point.y <= maxY) {
				below = true;
				break;
			}
		}
		//collision.gameObject.transform.position.y + (collision.gameObject.GetComponent<Collider>().bounds.size.y / 2) <= this.transform.position.y
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
			if (this.onground.Count != 0|| (this.holdJumpTime > 0 && this.holdJumpTime < this.maxJumpHold)) {
				this.holdJumpTime++;
				y = (this.jumpPower / (
					(float)Math.Sqrt(
						(double)this.holdJumpTime
						* this.jumpHoldCurveSteepness
					)
					- (this.jumpHoldCurveSteepness - 1)
				)) * (((float)Math.Sqrt(Math.Pow(rb.velocity.x, 2) + Math.Pow(rb.velocity.z, 2)) * this.jumpSpeedBoost) + 1);
			}
		}
		else {
			this.holdJumpTime = 0.0f;
		}

        Vector3 move3 = new Vector3(
            this.move.x,
            y,
            this.move.y
        );
        rb.AddForce(move3 * speed);
    }
}
