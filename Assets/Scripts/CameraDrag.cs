using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDrag : MonoBehaviour {

    //public float dragSpeed = 2;
    public float rotateSpeed = 3.5f;
    //private Vector3 dragOrigin;
    private float _x, _y;

	
	// Update is called once per frame
	void Update () {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    dragOrigin = Input.mousePosition;
        //    return;
        //}

        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        //    Vector3 move = new Vector3(pos.x * -dragSpeed, 0, pos.y * -dragSpeed);

        //    transform.Translate(move, Space.World);
        //}

        // Rotation code if mouse is dragged
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotateSpeed, -Input.GetAxis("Mouse X") * rotateSpeed, 0));

            _x = transform.rotation.eulerAngles.x;
            _y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(_x, _y, 0);
        }
        // Transform code if keys are pressed
        if (Input.GetKey("w"))  // Forward
        {
            Vector3 translate = Camera.main.transform.forward * 1.0f;
            transform.Translate(translate);
        }
        if (Input.GetKey("s")) // Back
        {
            Vector3 translate = Camera.main.transform.forward * -1.0f;
            transform.Translate(translate);
        }
        if(Input.GetKey("q"))  // Up
        {
            Vector3 translate = Camera.main.transform.up * 1.0f;
            transform.Translate(translate);
        }
        if (Input.GetKey("e")) // Down
        {
            Vector3 translate = Camera.main.transform.up * -1.0f;
            transform.Translate(translate);
        }
        if (Input.GetKey("a")) // Left
        {
            Vector3 translate = Camera.main.transform.right * -1.0f;
            transform.Translate(translate);
        }
        if (Input.GetKey("d")) // Right
        {
            Vector3 translate = Camera.main.transform.right * 1.0f;
            transform.Translate(translate);
        }

    }
}
