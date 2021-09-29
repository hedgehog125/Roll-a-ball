using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class ballMovement : MonoBehaviour
{
    public float speed;
	public float midAirSpeed;
	public float jumpPower;
	public float jumpSpeedBoost;
	public int maxCoyoteTime;
	public int maxJumpBufferTime;
	public int maxJumpHold;
	public float jumpHoldCurveSteepness;
	public int nextLevelDelay;
	public float initialClimbSpeed;
	public float climbTriggerSpeed;
	public float climbSpeedTotalGain;
	public float maxClimbTime;
	public float climbCancelTime;
	public float topClimbCancelMargin;
	public float neutralSlowdown;
	public float neutralDeadzone;

	public GameObject mainCamera;
	public GameObject collectiblesParent;
	public GameObject levelInfoSprite;
	private levelInfoScript levelInfo;
	public TextMeshProUGUI countObject;
	public GameObject winObject;
	public GameObject endObject;


	private Rigidbody rb;
	private Collider col;

    private Vector2 move;
	private bool isJumping;
	private int jumpBufferTick;
	private List<GameObject> onground = new List<GameObject>();
	private List<GameObject> climbObjects = new List<GameObject>();
	private int coyoteTime;
	private float holdJumpTime;
	private int nextLevelTick;
	private int collectibleCount;
	private float climbTick;
	private bool climbing;
	private float currentClimbingSpeed;
	private bool climbCancelling;
	private float climbCancelTick;


	private void UpdateCount() {
		countObject.text = "Count: " + collectibleCount.ToString() + "/" + collectiblesParent.transform.childCount;
		if (! levelInfo.showCount) { // Hide
			countObject.text = "";
		}
	}
	void Start()
    {
        rb = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();

		levelInfo = levelInfoSprite.GetComponent<levelInfoScript>();
		UpdateCount();
	}    

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        move = movementVector;
    }

	void OnJump(InputValue value) {
		isJumping = value.Get<float>() > 0;
	}
	void OnCollisionEnter(Collision collision) {
		float bottomY = transform.position.y - (col.bounds.size.y / 4);
		bool below = false;
		int count = collision.contactCount;
		for (int i = 0; i < count; i++) {
			if (collision.GetContact(i).point.y <= bottomY) { // Point of contact has to be in the bottom quarter.
				below = true;
				break;
			}
		}
		if (below) {
			onground.Add(collision.gameObject);
			climbing = false;
			rb.useGravity = true;
			climbTick = 0;
		}
		else if (onground.Count == 0 && Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) >= climbTriggerSpeed) {
			climbObjects.Add(collision.gameObject);
			if ((! climbing) && (climbTick <= 0 || climbCancelling)) {
				currentClimbingSpeed = initialClimbSpeed;
				climbing = true;
				rb.useGravity = false;
				climbCancelling = false;
			}
        }
	}
	void OnTriggerEnter(Collider collision) {
		GameObject collect = collision.gameObject;
		if (collect.CompareTag("Collectible")) {
			collect.SetActive(false);

			collectibleCount++;
			UpdateCount();
			if (collectibleCount == collectiblesParent.transform.childCount) {
				winObject.SetActive(true);

				if (levelInfo.lastLevel) {
					endObject.SetActive(true);
				}
				else {
					nextLevelTick = 1;
				}
			}
		}
	}
	void OnCollisionExit(Collision collision) {
		onground.Remove(collision.gameObject);
		climbObjects.Remove(collision.gameObject);
		if (climbObjects.Count == 0) {
			float bottomY = transform.position.y - (col.bounds.size.y / 4);
			if (bottomY >= collision.gameObject.transform.position.y + (collision.gameObject.GetComponent<Collider>().bounds.size.y / 2) + topClimbCancelMargin) {
				climbing = false;
				climbCancelling = false;
				rb.useGravity = true;
			}
			else {
				climbCancelling = true;
				climbCancelTick = 0;
			}
		}
	}

	void FixedUpdate() {
		if (Math.Sqrt(Math.Pow(move.x, 2) + Math.Pow(move.y, 2)) <= neutralDeadzone) { // Neutral
			if (onground.Count != 0) {
				rb.velocity = new Vector3(rb.velocity.x / neutralSlowdown, rb.velocity.y, rb.velocity.z / neutralSlowdown);
			}
		}

		if (nextLevelTick == nextLevelDelay) {
			SceneManager.LoadScene("Level " + (levelInfo.levelID + 1), LoadSceneMode.Single);
			return;
		}
		else if (nextLevelTick > 0) {
			nextLevelTick++;
		}

		bool onGround = onground.Count != 0;
		if (onGround) {
			coyoteTime = 0;
		}
		else {
			if (coyoteTime != maxCoyoteTime) {
				coyoteTime++;
				onGround = true;
			}
		}

		float y = 0.0f;
		if (isJumping) {
			if (onGround || (holdJumpTime > 0 && holdJumpTime < maxJumpHold)) {
				jumpBufferTick = 0;
				holdJumpTime++;
				y = (jumpPower / (
					Mathf.Sqrt(
						holdJumpTime
						* jumpHoldCurveSteepness
					)
					- (jumpHoldCurveSteepness - 1)
				)) * ((Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) * jumpSpeedBoost) + 1);
			}	
		}
		else {
			if (holdJumpTime > 0) {
				holdJumpTime = 0.0f;
				coyoteTime = 0;
			}
		}

		// Prevent jump from happening if it was pressed in air once it's been pressed for longer than maxJumpBufferTime
		if ((! (holdJumpTime > 0)) && (((! onGround) && isJumping) || jumpBufferTick != 0)) {
			if (jumpBufferTick == maxJumpBufferTime) {
				jumpBufferTick = 0;
				isJumping = false;
			}
			else {
				jumpBufferTick++;
			}
		}

		float rad = Mathf.Atan2(move.x, move.y) + (mainCamera.transform.eulerAngles.y * Mathf.Deg2Rad);
		float distance = Mathf.Sqrt(Mathf.Pow(move.x, 2) + Mathf.Pow(move.y, 2));
		float currentSpeed = speed;
		if (! onGround) {
			currentSpeed *= midAirSpeed;
		}

		if (climbing) {
			if (climbCancelling) {
				if (climbCancelTick >= climbCancelTime) {
					climbing = false;
					rb.useGravity = true;
					climbCancelling = false;
				}
				else {
					climbCancelTick += Time.deltaTime;
				}
			}
			if (climbTick > maxClimbTime) {
				climbing = false;
				rb.useGravity = true;
				climbCancelling = false;
			}
			else {
				climbTick += Time.deltaTime;
				currentClimbingSpeed += climbSpeedTotalGain * Time.deltaTime;
			}
		}
		Vector3 move3 = new Vector3(
            Mathf.Sin(rad) * currentSpeed * distance,
            y + (climbing? currentClimbingSpeed : 0),
			Mathf.Cos(rad) * currentSpeed * distance
        );
        rb.AddForce(move3);
    }
}
