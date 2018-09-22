using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;

public class SitelinkMarker : MonoBehaviour {
    private SiteMarker fromSiteMarker;
    private SiteMarker toSiteMarker;
    private Sitelink sitelink;
    public string SitelinkId; // Viewable in the editor for debugging.

    private Vector3 globePosition;
    private float globeRadius;

    private int numPoints = 32; // TODO: Calculate based on sphere surface distance.
    private float lineWidth;
    private Color lineColor;

    private float blinkPeriodSeconds = 0.0f;
    private float blinkLevel = 0.0f;
    private float blinkDirection = 1.0f;

    private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
        
    }

    public void Set(SiteMarker fromSiteMarker, SiteMarker toSiteMarker, Sitelink sitelink, Vector3 globePosition, float globeRadius) {
        this.fromSiteMarker = fromSiteMarker;
        this.toSiteMarker = toSiteMarker;
        this.sitelink = sitelink;
        SitelinkId = sitelink.id;
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        // TODO: Get these values in a better way, eg. link the globe object here with a public member variable.
        this.globePosition = globePosition;
        this.globeRadius = globeRadius;

        // ---

        lineColor = Color.green;
        lineWidth = 0.1f;
        blinkPeriodSeconds = 0.0f;

        if (sitelink.state == "up") {
            lineColor = Color.green;

            lineWidth = 0.05f + sitelink.throughput_out * 0.01f;
        } else {
            lineColor = Color.red;
            blinkPeriodSeconds = 2.0f;
        }

        // ---

        Draw();
    }

    // Update is called once per frame
    void Update() {
        if (blinkPeriodSeconds > 0.0f) {
            blinkLevel += blinkDirection * (1/blinkPeriodSeconds) * 2 * Time.deltaTime;

            if (blinkLevel < 0.0f || blinkLevel > 1.0f) {
                blinkDirection *= -1.0f;
            }
        } else {
            blinkLevel = 1.0f;
        }

        lineColor.a = blinkLevel;

        // Redraw to update blink level.
        Draw();

        if (!(fromSiteMarker && toSiteMarker)) { //TODO: Optimise
            Destroy(gameObject);
        }
    }

    public void Draw() {
        // This check is so that if there is an sitelink marker with no from/to site (eg. because
        // it's a dummy marker for easy modification of the prefab), it won't try to draw itself
        // and spam the console with an error per frame.
        if (fromSiteMarker != null && toSiteMarker != null) {
            lineRenderer.positionCount = numPoints;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            for (int i = 0; i < numPoints; i++) {
                float progress = ((float)i) / (numPoints - 1);
                Vector3 result = Vector3.Slerp(
                    fromSiteMarker.gameObject.transform.position,
                    toSiteMarker.gameObject.transform.position,
                    progress);

                float radius = Vector3.Distance(globePosition, result);
                if (radius < globeRadius) {
                    result *= (globeRadius / radius);
                }

                lineRenderer.SetPosition(i, result);
            }
        }
    }
}
