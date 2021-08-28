using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;


public class cmCanLook : MonoBehaviour
{
    // Start is called before the first frame update
	private CinemachineInputProvider cmInput;
	private InputActionReference XYAxis;
	public bool canLook;
    void Start()
    {
		this.cmInput = GetComponent<CinemachineInputProvider>();
		this.XYAxis = this.cmInput.XYAxis;
		this.CanLookChange(false);
	}

	void CanLookChange(bool can) {
		if (can) {
			this.cmInput.XYAxis = this.XYAxis;
			this.canLook = true;
		}
		else {
			this.cmInput.XYAxis = null;
			this.canLook = false;
		}
	}

	void OnCanLookClick(InputValue value) {
		this.CanLookChange(value.Get<float>() > 0);
	}
	void OnCanLookRStick(InputValue value) {
		Vector2 vec = value.Get<Vector2>();
		if (Mathf.Sqrt(Mathf.Pow(vec.x, 2) + Mathf.Pow(vec.y, 2)) > 0.05f) {
			this.CanLookChange(true);
		}
		this.CanLookChange(false);
	}
}
