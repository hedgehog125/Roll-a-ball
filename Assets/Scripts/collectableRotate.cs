using UnityEngine;
using System;

public class collectableRotate : MonoBehaviour {
	public Vector3 rotateSpeed;
	public float bobAmount;
	public float bobSpeed;


	private float yCentre;
	private float time;
	private void Start() {
		this.yCentre = transform.position.y;
	}
	// Before rendering each frame..
	void Update() {
		transform.Rotate(this.rotateSpeed * Time.deltaTime);
		this.time += Time.deltaTime;
		transform.position = new Vector3(transform.position.x, (Mathf.Sin(this.time * this.bobSpeed) * this.bobAmount) + yCentre, transform.position.z);
	}
}