using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;

public class LineMarker : MonoBehaviour {
    public SiteMarker StartSiteMarker;
    public SiteMarker EndSiteMarker;
    public int NumPoints;
    public float LineWidth;
    public Color Color;
    public Vector3 SpherePosition;
    public float SphereRadius;

    public float BlinkPeriodSeconds = 0.0f;
    private float blinkLevel = 0.0f;
    private float blinkDirection = 1.0f;

    private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
        lineRenderer = GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
		if (BlinkPeriodSeconds > 0.0f) {
            blinkLevel += blinkDirection * BlinkPeriodSeconds * 2 * Time.deltaTime;

            if (blinkLevel < 0.0f || blinkLevel > 1.0f) {
                blinkDirection *= -1.0f;
            }
        } else {
            blinkLevel = 1.0f;
        }

        Color.a = blinkLevel;

        Redraw();
    }

    public void Redraw() {
        lineRenderer.positionCount = NumPoints;
        lineRenderer.startColor = Color;
        lineRenderer.endColor = Color;
        lineRenderer.startWidth = LineWidth;
        lineRenderer.endWidth = LineWidth;

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
