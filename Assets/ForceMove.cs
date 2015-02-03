using UnityEngine;
using System.Collections;

public class ForceMove : MonoBehaviour {
    public float force = 50;
    void Update()
    {

    }
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100))
            rigidbody.AddForceAtPosition((transform.position - hit.point) * force,
                                                    hit.point,ForceMode.Impulse);
    
    }
}
