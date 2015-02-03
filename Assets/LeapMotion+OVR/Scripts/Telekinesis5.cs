using UnityEngine;
using System.Collections;
using Leap;

public class Telekinesis5 : MonoBehaviour
{
    public GameObject player;

    public GameObject statusObject;
    TextMesh statusText;

    int numberOfAbilities = 3;

    public Controller controller;

    Ray targetRay;
    public GameObject rayStartObject;
    LineRenderer sight;

    int selectedAbility = 2;
    
    public Material lineMaterial;
    
    bool firstHit = true;

    bool TKActive = false;

    float zLoc;

    GameObject lastObject = null;

    GameObject hitObject = null;

    bool inCooldown = false;

    Transform hitParent;

    string physObject = "physObject";

    GameObject palm = null;
    public Object thisPrefab;

    // Use this for initialization
    void Start()
    {
        controller = new Controller();
        statusText = statusObject.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {



        Frame frame = null; //The latest frame
        if (controller.IsConnected) //controller is a Controller object
        {
            frame = controller.Frame(); //The latest frame
        }

        Hand rightHand = frame.Hands.Rightmost;
        Hand leftHand = frame.Hands.Leftmost;

        if (palm == null && rightHand.GrabStrength < .5)
        {
            palm = GameObject.Find("CleanRobotRightHand(Clone)/palm");

            /* The first time this palm is found.*/
            if (palm != null)
            {
                GameObject newTKM = null;
                if (GameObject.Find("TKManager(Clone)") == null)
                {
                    newTKM = (GameObject)Instantiate(thisPrefab);
                }

                statusText = statusObject.GetComponent<TextMesh>();

                Telekinesis5 rayScript = newTKM.GetComponent<Telekinesis5>();

                newTKM.GetComponent<LineRenderer>().enabled = true;

                Debug.Log(rayScript);
                rayScript.rayStartObject = palm;

                /* Seting this palm here so it won't try to create more copies of itself.*/
                rayScript.palm = palm;

                /* Ray stuff*/
                newTKM.transform.parent = rayScript.rayStartObject.transform;
                newTKM.transform.localPosition = new Vector3(0, 0, 0);

                newTKM.transform.localRotation = new Quaternion(0, 0, 0, 0);
                newTKM.transform.Rotate(90, 0, 0);

                rayScript.sight = newTKM.GetComponent<LineRenderer>();
                rayScript.sight.material = lineMaterial;
                rayScript.sight.SetPosition(0, newTKM.transform.position);
                rayScript.sight.SetWidth(0.1f, 0.1f);

                targetRay = new Ray(this.transform.position, this.transform.forward);



            }
        }

        /* rayStartObject is only not null in the cloned TKManager.*/
        else if (rayStartObject != null) //&& rightHand.GrabStrength < .5)
        {
            switch (selectedAbility)
            {
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
            if (Input.GetKeyDown("a"))
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

            else if (Input.GetKeyDown("d"))
            {
                if (selectedAbility <= 0)
                {
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
            if (!((Input.GetKey("s")) && (selectedAbility == 2)))
            {
                player.rigidbody.useGravity = true;
                targetRay = new Ray(this.transform.position, this.transform.forward);

            }

            //sets endpoints for visible ray
            sight.SetPosition(0, rayStartObject.transform.position);
            //sight.SetPosition (1, targetRay.GetPoint (100));
            sight.SetWidth(0.1f, 0.1f);

            //Debug.DrawRay (rayStartObject.transform.position, rayStartObject.transform.forward * 100);

            //creates raycast that goes distance of 100
            if (Physics.Raycast(targetRay, out hit, 500))
            {
                //sets object to whatever object was hit
                hitObject = hit.collider.gameObject;

                //checks if this object hit is the same as in last update
                if (TKActive && lastObject != null && hitObject != lastObject)
                {
                    //deactivates TK:
                    Debug.Log("Test?");
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
                        if (Input.GetKey("t"))
                        {
                            if (hit.collider.tag == physObject)
                            {
                                TKActive = true;
                                hitObject.rigidbody.useGravity = false;
                                //this code should only be executed when the user presses t initially
                                hitObject.rigidbody.velocity = new Vector3(0, 0, 0);
                                if (firstHit)
                                {
                                    //stores parent of object before changing it
                                    hitParent = hitObject.transform.parent;
                                    Debug.Log(hitObject.transform.parent);
                                    hitObject.transform.parent = rayStartObject.transform;
                                    //sets up bools for checks later

                                    firstHit = false;
                                }


                                //change this for gestures - brings closer
                                if (Input.GetKey("x"))
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
                                else if (Input.GetKey("w"))
                                {
                                    float changeZ = hitObject.transform.localPosition.z;
                                    changeZ += 0.1f;
                                    hitObject.transform.localPosition = new Vector3(hitObject.transform.localPosition.x,
                                                                                hitObject.transform.localPosition.y, changeZ);
                                }

                                //change this for gestures - throws
                                else if (Input.GetKey("y"))
                                {
                                    hitObject.transform.parent = hitParent;
                                    hitObject.rigidbody.useGravity = true;
                                    hitObject.rigidbody.velocity = transform.forward * 20;
                                    TKActive = false;
                                    firstHit = true;
                                }
                            }
                            else
                            {
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
                        if (hit.collider.tag == physObject)
                        {

                            if (Input.GetKey("s") && (!inCooldown))
                            {
                                ReverseGravity(hitObject);
                                StartCoroutine(Cooldown(3f, "GravityAbilityCool", null));
                            }
                        }
                        break;

                    case 2: //grapple
                        if (Input.GetKey("s"))
                        {
                            Debug.Log("GOGOGO");

                            //SetRay (hit.point);
                            //targetRay = new Ray(transform.position, hit.point);
                            player.rigidbody.velocity = new Vector3(0, 0, 0);
                            Debug.DrawRay(transform.position, hit.point);
                            Vector3 grapplePoint = hit.point;
                            sight.SetPosition(1, grapplePoint);
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
               /*  (TKActive)
                {
                    lastObject.transform.parent = hitParent;
                    lastObject.rigidbody.useGravity = true; //restores gravity
                    TKActive = false;
                    firstHit = true;
                } */

                //updates position of visible ray again
                if (!((Input.GetKey("a")) && (selectedAbility == 2)))
                {
                    sight.SetPosition(1, targetRay.GetPoint(500));
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