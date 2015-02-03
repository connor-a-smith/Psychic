using UnityEngine;
using System.Collections;
using Leap;

public class Telekinesis3 : MonoBehaviour
{
    int numberOfAbilities = 2;

    public Controller controller;

    Ray targetRay;
    public GameObject rayStartObject;
    int selectedAbility = 0;
    LineRenderer sight;
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

                Telekinesis3 rayScript = newTKM.GetComponent<Telekinesis3>();

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
            }
        }

        /* rayStartObject is only not null in the cloned TKManager.*/
        else if (rayStartObject != null && rightHand.GrabStrength < .5)
        {
            //add switch - gesture here, and change for num of abilities
            if (Input.GetKeyDown("k"))
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

            else if (Input.GetKeyDown("j"))
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

            RaycastHit hit;
            targetRay = new Ray(this.transform.position, this.transform.forward);

            Debug.Log(sight != null);
            sight.SetPosition(0, rayStartObject.transform.position);
            sight.SetPosition(1, targetRay.GetPoint(100));
            sight.SetWidth(0.1f, 0.1f);
            Debug.DrawRay(rayStartObject.transform.position, rayStartObject.transform.forward);

            if (Physics.Raycast(targetRay, out hit, 100))
            {

                hitObject = hit.collider.gameObject;

                //checks if a physical object
                if (hit.collider.tag == physObject)
                {

                    //sets object to whatever object was hit
                    hitObject = hit.collider.gameObject;

                    //checks if this object hit is the same as in last update
                    if (TKActive && lastObject != null && hitObject != lastObject)
                    {
                        //deactivates TK:
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
                                TKActive = true;
                                hitObject.rigidbody.useGravity = false;
                                //this code should only be executed when the user presses t initially
                                if (firstHit)
                                {
                                    //stores parent of object before changing it
                                    hitParent = hitObject.transform.parent;

                                    //sets up bools for checks later
                                    firstHit = false;
                                    hitObject.transform.parent = rayStartObject.transform;
                                }


                                //change this for gestures - brings closer
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

                                //change this for gestures - moves away
                                else if (Input.GetKey(KeyCode.UpArrow))
                                {
                                    float changeZ = hitObject.transform.localPosition.z;
                                    changeZ += 0.1f;
                                    hitObject.transform.localPosition = new Vector3(hitObject.transform.localPosition.x,
                                                                            hitObject.transform.localPosition.y, changeZ);
                                }

                                //change this for gestures - throws
                                else if (Input.GetKey("r"))
                                {
                                    hitObject.rigidbody.velocity = transform.forward * 20;
                                }
                            }

                            else
                            {
                                //if the user just let go of t
                                if (TKActive)
                                {
                                    //restores rightful parent
                                    hitObject.transform.parent = hitParent;
                                    hitObject.rigidbody.useGravity = true;
                                    TKActive = false;
                                    firstHit = true;
                                }
                            }
                            break;


                        case 1: //gravity

                            if (Input.GetKey("z") && (!inCooldown))
                            {
                                ReverseGravity(hitObject);
                                StartCoroutine(Cooldown(3f, "GravityAbilityCool", null));
                            }
                            break;
                    }

                    lastObject = hitObject;
                    Debug.Log(hitObject.rigidbody.useGravity);
                }

                else
                {
                    //if the TKActive variable was still activate and the raycast stopped
                    if (TKActive)
                    {
                        lastObject.transform.parent = hitParent;
                        lastObject.rigidbody.useGravity = true; //restores gravity
                        TKActive = false;
                        firstHit = true;
                    }

                    //updates position of visible ray again
                    sight.SetPosition(1, targetRay.GetPoint(100));
                }
            }
        }
    }

    //method that handles the reversal of gravity
    void ReverseGravity(GameObject flyingObject)
    {
        flyingObject.rigidbody.useGravity = false;

        //sets upward velocity
        flyingObject.rigidbody.velocity = new Vector3(0, 0.8f, 0);

        //waits for 5 seconds
        StartCoroutine(Cooldown(5, "GravityReverse", flyingObject));
    }

    //handles cooldowns
    IEnumerator Cooldown(float seconds, string coolType, GameObject storedObject)
    {
        if (coolType == "GravityAbilityCool")
        {
            inCooldown = true;
            yield return new WaitForSeconds(seconds);
            inCooldown = false;
        }

        if (coolType == "GravityReverse")
        {
            yield return new WaitForSeconds(seconds);
            storedObject.rigidbody.useGravity = true;
        }
    }
}
