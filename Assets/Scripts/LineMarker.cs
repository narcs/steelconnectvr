using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;

public class LineMarker : MonoBehaviour {
    public SiteMarker StartSiteMarker;
    public SiteMarker EndSiteMarker;
    public int NumPoints;
    public Color Color;
    public Vector3 SpherePosition;
    public float SphereRadius;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Redraw() {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = NumPoints;
        lineRenderer.startColor = Color;
        lineRenderer.endColor = Color;

        for (int i = 0; i < NumPoints; i++) {
            float progress = ((float)i) / (NumPoints - 1);
            Vector3 result = Vector3.Slerp(StartSiteMarker.gameObject.transform.position, EndSiteMarker.gameObject.transform.position, progress);
            float radius = Vector3.Distance(SpherePosition, result);

            if (radius < SphereRadius) {
                result *= (SphereRadius / radius);
            }

            lineRenderer.SetPosition(i, result);
        }
    }
}
