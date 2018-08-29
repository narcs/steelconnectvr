using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gvr.Internal;

public class SiteDeletion : MonoBehaviour {

    public GameObject laser;

    private bool deletionActive = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnableSiteDeletion() {
        // Change to red
        laser.GetComponent<GvrLaserVisual>().laserColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        deletionActive = true;
    }

    public void DisableSiteDeletion() {
        // Change back to white
        laser.GetComponent<GvrLaserVisual>().laserColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        deletionActive = false;
    }
}
