using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAt : MonoBehaviour
{
    public GameObject objectToLookAt;
    public Vector3 originalRotation = new Vector3(0, 0, 0);
    public bool active = true;

    // Start is called before the first frame update
    void Start()
    {
        if (objectToLookAt == null) {
            // active = false;
            objectToLookAt = GameObject.FindGameObjectWithTag("TestCam");
        }
        originalRotation = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            transform.LookAt(objectToLookAt.transform.position);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
