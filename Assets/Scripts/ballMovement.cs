using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ballMovement : MonoBehaviour
{
    public float speed;
	public float jumpPower;
	public float jumpSpeedBoost;
	public int maxCoyoteTime;
	public int maxJumpBufferTime;
	public int maxJumpHold;
	public float jumpHoldCurveSteepness;
	public GameObject mainCamera;
	public GameObject collectiblesParent;
	public GameObject levelInfoSprite;
	private levelInfoScript levelInfo;


	private Rigidbody rb;
	private Collider col;

    public Vector2 move;
	private bool isJumping;
	private int jumpBufferTick;
	private List<GameObject> onground = new List<GameObject>();
	private int coyoteTime;
	private float holdJumpTime;

	private int collectibleCount;


	void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
		this.col = this.GetComponent<Collider>();

		this.levelInfo = this.levelInfoSprite.GetComponent<levelInfoScript>();
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
		if (below) {
			this.onground.Add(collision.gameObject);
		}

	}
	void OnTriggerEnter(Collider collision) {
		GameObject collect = collision.gameObject;
		if (collect.CompareTag("Collectible")) {
			collect.SetActive(false);
			this.collectibleCount++;
			if (this.collectibleCount == this.collectiblesParent.transform.childCount) {
				int level = levelInfo.levelID + 1;
				if (levelInfo.lastLevel) level = 1;
				SceneManager.LoadScene("Level " + level, LoadSceneMode.Single);
			}
		}
	}
	void OnCollisionExit(Collision collision) {
		this.onground.Remove(collision.gameObject);
	}

	void FixedUpdate() {
		bool onGround = this.onground.Count != 0;
		if (onGround) {
			this.coyoteTime = 0;
		}
		else {
			if (this.coyoteTime != this.maxCoyoteTime) {
				this.coyoteTime++;
				onGround = true;
			}
		}

		float y = 0.0f;
		if (this.isJumping) {
			if (onGround || (this.holdJumpTime > 0 && this.holdJumpTime < this.maxJumpHold)) {
				this.jumpBufferTick = 0;
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
			if (this.holdJumpTime > 0) {
				this.holdJumpTime = 0.0f;
				this.coyoteTime = 0;
			}
		}

		// Prevent jump from happening if it was pressed in air once it's been pressed for longer than maxJumpBufferTime
		if ((! (this.holdJumpTime > 0)) && (((! onGround) && this.isJumping) || this.jumpBufferTick != 0)) {
			if (this.jumpBufferTick == this.maxJumpBufferTime) {
				this.jumpBufferTick = 0;
				this.isJumping = false;
			}
			else {
				this.jumpBufferTick++;
			}
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
