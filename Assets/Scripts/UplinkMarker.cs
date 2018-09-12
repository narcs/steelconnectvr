using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UplinkMarker : MonoBehaviour {
    public GameObject wan;
    public GameObject site;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (wan && site) {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            Vector3[] points = new Vector3[2] {
                wan.transform.position,
                site.transform.position
            };
            lineRenderer.SetPositions(points);

        }
	}
}
