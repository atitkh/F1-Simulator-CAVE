using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiddleVR;

public class ObjectManipulation : MonoBehaviour
{
    private Vector3 startingPos;
    private Quaternion startingRot;
    // Start is called before the first frame update
    //private IEnumerator coroutine;

    public enum viveButton
    {
        Trigger,
        TouchPad,
        Grip,
        Menu
    }
    void Start()
    {
        startingPos = transform.position;
        startingRot = transform.rotation;
    }
    public void OnButtonDown(int iButton, bool iPressed)
    {
        if (iButton == (int)(viveButton.Grip) && iPressed){
            // gameObject.transform.Translate(Vector3.up*((float)MVR.Kernel.GetDeltaTime()));
            // TODO: start process that makes object flow back to original position
            StartCoroutine(MoveFunction());
        }
    }

    IEnumerator MoveFunction()
    {
        float timeSinceStarted = 0f;
        while (true)
        {
            timeSinceStarted += (float)MVR.Kernel.GetDeltaTime();
            transform.position = Vector3.Lerp(transform.position, startingPos, timeSinceStarted);
            transform.rotation = Quaternion.Lerp(transform.rotation, startingRot, timeSinceStarted);
            // If the object has arrived, stop the coroutine
            if (transform.position == startingPos && transform.rotation == startingRot)
            {
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
