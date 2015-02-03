using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	// Use this for initialization
	void Start () {
	 
	}
    public float force = 5;
    public float boomradius = 5;
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                Collider[] colliders = Physics.OverlapSphere(hit.point,boomradius);

                foreach(Collider c in colliders)
                {
                    if (c.rigidbody == null) continue;
                    c.rigidbody.AddExplosionForce(force,hit.point,boomradius,1,ForceMode.Impulse);
                //rigidbody.AddExplosionForce(force, hit.point, boomradius, 1, ForceMode.Impulse);
                }
            }
        }
	}
}
