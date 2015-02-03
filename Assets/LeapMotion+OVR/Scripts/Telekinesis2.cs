using UnityEngine;
using System.Collections;
using Leap;

public class Telekinesis2 : MonoBehaviour
{
    public Controller controller;

    //change based on # of abilities
    public int numberOfAbilities = 2;

    Ray targetRay;
    public GameObject rayStartObject;
    int selectedAbility = 0;

    LineRenderer sight;
    public Material lineMaterial;
    bool firstHit = true;

    //TK Variable
    bool TKActive = false;
    float zLoc;

    //For Cooldowns
    bool inCooldown = false;

    GameObject hitObject;
    Transform hitParent;

    GameObject palm = null;

    public Object thisPrefab;

    public float grabThreshold = .5f;
    // Use this for initialization
    void Start()
    {
        controller = new Controller();
    }

    // Update is called once per frame
    void Update()
    {

        //add switch - gesture here, and change for num of abilities
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectedAbility > (numberOfAbilities - 1))
            {
                selectedAbility = 0;
            }

            else
            {
                selectedAbility++;
            }
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectedAbility < 0)
            {
                selectedAbility = (numberOfAbilities) - 1;
            }

            else
            {
                selectedAbility--;
            }
        }

        Frame frame = null; //The latest frame
        Frame previous = null;
        if (controller.IsConnected) //controller is a Controller object
        {
            frame = controller.Frame(); //The latest frame
            previous = controller.Frame(1); //The previous frame
        }

        Hand rightHand = frame.Hands.Rightmost;
        Hand leftHand = frame.Hands.Leftmost;

        if (palm == null && rightHand.GrabStrength < grabThreshold)
        {
            palm = GameObject.Find("CleanRobotRightHand(Clone)/palm");

            /* The first time this palm is found.*/
            if (palm != null)
            {
                GameObject newObject = (GameObject)Instantiate(thisPrefab);

                Telekinesis2 script = newObject.GetComponent<Telekinesis2>();

                script.GetComponent<LineRenderer>().enabled = true;

                script.rayStartObject = palm;

                /* Seting this palm here so it won't try to create more copies of itself.*/
                script.palm = palm;

                /* Ray stuff*/
                newObject.transform.parent = script.rayStartObject.transform;
                newObject.transform.localPosition = new Vector3(0, 0, 0);

                /* Quaternions are magic*/
                //Quaternion direction = new Quaternion(0, 0, 0, 0);
                //direction *= Quaternion.Euler(90, 0, 0); // this add a 90 degrees X rotation

                newObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
                newObject.transform.Rotate(90, 0, 0);

                script.sight = newObject.GetComponent<LineRenderer>();
                script.sight.material = lineMaterial;
                script.sight.SetPosition(0, newObject.transform.position);
                script.sight.SetPosition(1, targetRay.GetPoint(10));
                script.sight.SetWidth(0.05f, 0.05f);
            }

            /* rayStartObject is only not null in the cloned TKManager.*/
            else if (rayStartObject != null && rightHand.GrabStrength < .5)
            {
                RaycastHit hit;
                targetRay = new Ray(this.transform.position, this.transform.forward);
                sight.SetPosition(0, rayStartObject.transform.position);
                sight.SetPosition(1, targetRay.GetPoint(10));
                sight.SetWidth(0.1f, 0.1f);
                Debug.DrawRay(rayStartObject.transform.position, rayStartObject.transform.forward);

                if (Physics.Raycast(targetRay, out hit, 100))
                {
                    hitObject = hit.collider.gameObject;

                    sight.SetPosition(1, hit.point);

                    switch (selectedAbility)
                    {
                        //Telek is activate
                        case 0: //TK
                            //user is holding t
                            //change this for gestures:
                            if (Input.GetKey("t"))
                            {
                                //this code should only be executed when the user presses t initially
                                if (firstHit)
                                {
                                    //stores parent of object before changing it
                                    hitParent = hitObject.transform.parent;

                                    //sets up bools for checks later
                                    firstHit = false;
                                    TKActive = true;
                                    hitObject.transform.parent = rayStartObject.transform;
                                }


                                //Closer
                                //change this for gestures
                                if (Input.GetKey(KeyCode.DownArrow))
                                {
                                    float changeZ = hitObject.transform.localPosition.z;


                                    if (changeZ - 0.1f > 5)
                                    {
                                        changeZ -= 0.1f;
                                    }

                                    hitObject.transform.localPosition = new Vector3(hitObject.transform.localPosition.x,
                                                                                    hitObject.transform.localPosition.y, changeZ);
                                }

                                //Further
                                //change this for gestures
                                else if (Input.GetKey(KeyCode.UpArrow))
                                {
                                    float changeZ = hitObject.transform.localPosition.z;
                                    changeZ += 0.1f;
                                    hitObject.transform.localPosition = new Vector3(hitObject.transform.localPosition.x,
                                                                                    hitObject.transform.localPosition.y, changeZ);
                                }
                            }

                            else
                            {
                                //if the user just let go of t
                                if (TKActive)
                                {
                                    //restores rightful parent
                                    hitObject.transform.parent = hitParent;
                                    TKActive = false;
                                    firstHit = true;
                                }
                            }
                            break;


                        case 1: //gravity

                            if (Input.GetKey("g") && (!inCooldown))
                            {
                                ReverseGravity(hitObject);
                                StartCoroutine(Cooldown(3f, "GravityAbilityCool", null));
                            }
                            break;
                    }
                }

                else
                {
                    sight.SetPosition(1, targetRay.GetPoint(100));
                }
            }
        }
    }

	void ReverseGravity(GameObject flyingObject)
	{
		flyingObject.rigidbody.useGravity = false;
		flyingObject.rigidbody.velocity = new Vector3(0, 0.8f, 0);
		StartCoroutine(Cooldown (5, "GravityReverse", flyingObject));
	}

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
