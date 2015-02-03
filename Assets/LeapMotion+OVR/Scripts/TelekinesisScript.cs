using UnityEngine;
using System.Collections;
using Leap;

public class TelekinesisScript : MonoBehaviour {

    public Controller controller;

    Ray targetRay;
    public GameObject rayStartObject;
    string selectedAbility = "TK";
    LineRenderer sight;
    public Material lineMaterial;
    bool firstHit = true;

    bool TKActive = false;

    float zLoc;

    GameObject hitObject;

    Transform hitParent;

    GameObject palm = null;

    Vector3 storedHit = new Vector3(0, 0, 0);

    public Object thisPrefab;


	// Use this for initialization
	void Start () 
    {
        controller = new Controller();
	}
	
	// Update is called once per frame
    void Update()
    {
        
        Frame frame = null; //The latest frame
        Frame previous = null;
        if (controller.IsConnected) //controller is a Controller object
        {
            frame = controller.Frame(); //The latest frame
            previous = controller.Frame(1); //The previous frame
        }

        Hand hand = frame.Hands.Rightmost;
        //Vector position = hand.PalmPosition;
        //Vector velocity = hand.PalmVelocity;
        //Vector direction = hand.Direction;

        //Vector normal = hand.PalmNormal;
        //targetRay = normal.ToUnity();

        if(palm == null && hand.GrabStrength < .5)
        {
            palm = GameObject.Find("CleanRobotRightHand(Clone)/palm");

            /* The first time this palm is found.*/
            if (palm != null)
            {
                GameObject newObject = (GameObject)Instantiate(thisPrefab);

                TelekinesisScript script = newObject.GetComponent<TelekinesisScript>();

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
                script.sight.SetWidth(0.1f,0.1f);
            }
        }

        /* rayStartObject is only not null in the cloned TKManager.*/
        else if (rayStartObject != null && hand.GrabStrength < .5)
        {
            RaycastHit hit;
            targetRay = new Ray(this.transform.position, this.transform.forward);

            Debug.Log(sight != null);
            sight.SetPosition(0, rayStartObject.transform.position);
            sight.SetPosition(1, targetRay.GetPoint(10));
            sight.SetWidth(0.1f, 0.1f);
            Debug.DrawRay(rayStartObject.transform.position, rayStartObject.transform.forward);

            if (Physics.Raycast(targetRay, out hit, 100))
            {

                Debug.Log(TKActive);

                hitObject = hit.collider.gameObject;

                Debug.Log("Hit");
                sight.SetPosition(1, hit.point);

                switch (selectedAbility)
                {
                    //Telek is activate
                    case "TK":
                        //user is holding t
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
                        }

                        else
                        {

                            Debug.Log("Step 1 Confirmed");
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
                }
            }

            else
            {
                sight.SetPosition(1, targetRay.GetPoint(100));
            }
        }
    }
}
