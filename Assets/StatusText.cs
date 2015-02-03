using UnityEngine;
using System.Collections;

public class StatusText : MonoBehaviour {

	public int statusTextX = 0;
	public int statusTextY = 5;
	public int statusTextZ = 15;

	// Use this for initialization
	void Start () {


        this.transform.parent = GameObject.Find("CenterEyeAnchor").transform;
		transform.localPosition = new Vector3(statusTextX, statusTextY, statusTextZ);
		transform.localRotation = new Quaternion (0, 0, 0, 0);

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
