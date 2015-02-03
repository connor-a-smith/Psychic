using UnityEngine;
using System.Collections;

public class RayScript : MonoBehaviour {

	public GameObject player;

	public GameObject statusObject;
	TextMesh statusText;
	
	//change based on # of abilities
	int numberOfAbilities = 3;
	
	//actual ray
	Ray targetRay;
	
	//the hand
	public GameObject rayStartObject;
	
	//toggle for abilities
	int selectedAbility = 0;
	
	//blue line
	LineRenderer sight;
	public Material lineMaterial;
	
	//first time the ray hits
	bool firstHit = true;
	
	//tag for physics objects
	string physObject = "physObject";
	
	//TK Variable
	bool TKActive = false;
	float zLoc;
	
	//For Cooldowns
	bool inCooldown = false;
	
	GameObject hitObject;
	GameObject lastObject = null;
	Transform hitParent;
	
	// Use this for initialization
	void Start () {

		statusText = statusObject.GetComponent<TextMesh>();
		
		//sets the ray to be a child of our hand
		this.transform.parent = rayStartObject.transform;
		this.transform.localPosition = new Vector3 (0, 0, 0);
		this.transform.localRotation = new Quaternion (0, 0, 0, 0);
		
		//Handles the start / endpoints of the visible line
		sight = this.GetComponent<LineRenderer> ();
		sight.material = lineMaterial;
		sight.SetPosition (0, this.transform.position);

		targetRay = new Ray (this.transform.position, this.transform.forward);
	}
	
	// Update is called once per frame
	void Update () 
	{

		switch (selectedAbility) {
		case 0:
			statusText.text = "Telekinesis";
			break;
		case 1:
			statusText.text = "Gravity";
			break;
		case 2:
			statusText.text = "Grapple";
			break;
		}

		//add switch - gesture here, and change for num of abilities
		if (Input.GetKeyDown (KeyCode.RightArrow)) 
		{
			//goes through different abilities
			if (selectedAbility >= (numberOfAbilities - 1)) 
			{
				selectedAbility = 0;
			}
			
			else
			{
				selectedAbility++;
			}
		}
		
		else if (Input.GetKeyDown ( KeyCode.LeftArrow)) 
		{
			if (selectedAbility <= 0) {
				selectedAbility = (numberOfAbilities) - 1;
			}
			
			else
			{
				selectedAbility--;
			}
		}
		
		//actual Raycast
		RaycastHit hit;
		
		//creates ray at hand, facing forward
		if (!((Input.GetKey("x")) && (selectedAbility == 2))) 
		{
			Debug.Log ("Original");
			player.rigidbody.useGravity = true;
			targetRay = new Ray (this.transform.position, this.transform.forward);

		}
		
		//sets endpoints for visible ray
		sight.SetPosition (0, rayStartObject.transform.position);
		//sight.SetPosition (1, targetRay.GetPoint (100));
		sight.SetWidth (0.1f, 0.1f);
		
		//Debug.DrawRay (rayStartObject.transform.position, rayStartObject.transform.forward * 100);
	
		//creates raycast that goes distance of 100
		if (Physics.Raycast (targetRay, out hit, 100))
		{
				//sets object to whatever object was hit
				hitObject = hit.collider.gameObject;
				
				//checks if this object hit is the same as in last update
				if (TKActive && lastObject != null && hitObject != lastObject) 
				{
					//deactivates TK:
					Debug.Log ("Test?");
					lastObject.transform.parent = hitParent;
					lastObject.rigidbody.useGravity = true;
					TKActive = false;
					firstHit = true;
				}
				
				//updates the visible ray
				sight.SetPosition(1, hit.point);
				
				//switches between abilities
				switch (selectedAbility)
				{
					//Telek is activate
				case 0: //TK
					//user is holding t
					//change this for gestures:
					if (Input.GetKey ("t")) 
					{
						if (hit.collider.tag == physObject) {
							TKActive = true;
							hitObject.rigidbody.useGravity = false;
							//this code should only be executed when the user presses t initially
							hitObject.rigidbody.velocity = new Vector3(0,0,0);
							if (firstHit) 
							{
								//stores parent of object before changing it
								hitParent = hitObject.transform.parent;
								Debug.Log (hitObject.transform.parent);
								hitObject.transform.parent = rayStartObject.transform;
								//sets up bools for checks later

								firstHit = false;
							}
						
						
							//change this for gestures - brings closer
							if (Input.GetKey (KeyCode.DownArrow)) 
							{
								float changeZ = hitObject.transform.localPosition.z;
							
								if (changeZ - 0.1f > 5) 
								{ 
									changeZ -= 0.1f;
								}
							
								hitObject.transform.localPosition = new Vector3(hitObject.transform.localPosition.x, 
							                                                hitObject.transform.localPosition.y, changeZ);
							}
						
							//change this for gestures - moves away
							else if (Input.GetKey (KeyCode.UpArrow)) 
							{
								float changeZ = hitObject.transform.localPosition.z;
								changeZ += 0.1f;
								hitObject.transform.localPosition = new Vector3(hitObject.transform.localPosition.x, 
							                                                hitObject.transform.localPosition.y, changeZ);
							}
						
							//change this for gestures - throws
							else if (Input.GetKey ("r")) 
							{
								hitObject.transform.parent = hitParent;
								hitObject.rigidbody.useGravity = true;
								hitObject.rigidbody.velocity = transform.forward * 20;
								TKActive = false;
								firstHit = true;
							}
						}
						else {
							if (TKActive) 
							{
								//restores rightful parent
								hitObject.transform.parent = hitParent;
								hitObject.rigidbody.useGravity = true;
								TKActive = false;
								firstHit = true;
							}
						}
					}
					break;
					
					
				case 1: //gravity
					if (hit.collider.tag == physObject) {
					
						if (Input.GetKey ("g") && (!inCooldown))
						{
							ReverseGravity(hitObject);
							StartCoroutine(Cooldown (3f, "GravityAbilityCool", null));
						}
					}
					break;

				case 2: //grapple
					if (Input.GetKey ("a"))
				    {
						Debug.Log ("GOGOGO");
						
						//SetRay (hit.point);
						//targetRay = new Ray(transform.position, hit.point);
						player.rigidbody.velocity = new Vector3(0,0,0);
						Debug.DrawRay(transform.position, hit.point);
						Vector3 grapplePoint = hit.point;
						sight.SetPosition (1, grapplePoint);
						player.rigidbody.useGravity = false;
						player.transform.position = Vector3.MoveTowards(player.transform.position, grapplePoint, 8 * Time.deltaTime);
					}
				break;
				}

				lastObject = hitObject;
			}

			else 
			{

				//if the TKActive variable was still activate and the raycast stopped
				if (TKActive) {
					lastObject.transform.parent = hitParent;
					lastObject.rigidbody.useGravity = true; //restores gravity
					TKActive = false;
					firstHit = true;
				}
				
				//updates position of visible ray again
				if (!((Input.GetKey("x")) && (selectedAbility == 2))) {
					sight.SetPosition (1, targetRay.GetPoint (100));
				}
			}
			//Debug.Log (hitParent);
		if (TKActive && !(Input.GetKey("t")))
		{
				hitObject.transform.parent = hitParent;
				hitObject.rigidbody.useGravity = true;
				TKActive = false;
				firstHit = true;
		}
}

	void SetRay(Vector3 vectorLocation) {
		this.targetRay = new Ray (transform.position, vectorLocation);
	}
	
	//method that handles the reversal of gravity
	void ReverseGravity(GameObject flyingObject)
	{
		flyingObject.rigidbody.useGravity = false;
		
		//sets upward velocity
		flyingObject.rigidbody.velocity = new Vector3(0, 0.8f, 0);
		
		//waits for 5 seconds
		StartCoroutine(Cooldown (5, "GravityReverse", flyingObject));
	}
	
	//handles cooldowns
	IEnumerator Cooldown(float seconds, string coolType, GameObject storedObject)
	{
		if (coolType == "GravityAbilityCool") {
			inCooldown = true;
			yield return new WaitForSeconds (seconds);
			inCooldown = false;
		}
		
		if(coolType == "GravityReverse") {
			yield return new WaitForSeconds(seconds);
			storedObject.rigidbody.useGravity = true;
		}
	}
}