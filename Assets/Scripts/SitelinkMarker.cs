using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Models.SteelConnect;

public class SitelinkMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private SiteMarker fromSiteMarker;
    private SiteMarker toSiteMarker;
    private SitelinkPair sitelinkPair;

    private Vector3 globePosition;
    private float globeRadius;

    private int numPoints = 32; // TODO: Calculate based on sphere surface distance.
    private float lineWidth;
    private Color lineColor;

    private float blinkPeriodSeconds = 0.0f;
    private float blinkLevel = 0.0f;
    private float blinkDirection = 1.0f;

    private LineRenderer lineRenderer;
    private CapsuleCollider collider;

    private StateManager _stateManager;
    private string _information;

    // Use this for initialization
    void Start () {
        
    }

    public void Set(SiteMarker fromSiteMarker, SiteMarker toSiteMarker, SitelinkPair sitelinkPair, Vector3 globePosition, float globeRadius) {
        this.fromSiteMarker = fromSiteMarker;
        this.toSiteMarker = toSiteMarker;
        this.sitelinkPair = sitelinkPair;
        lineRenderer = GetComponent<LineRenderer>();
        collider = GetComponent<CapsuleCollider>();

        // TODO: Get these values in a better way, eg. link the globe object here with a public member variable.
        this.globePosition = globePosition;
        this.globeRadius = globeRadius;

        // ---

        Sitelink sitelink0 = sitelinkPair.pair[0];

        lineColor = Color.green;
        lineWidth = 0.1f;
        blinkPeriodSeconds = 0.0f;

        if (sitelink0.state == "up") {
            lineColor = Color.green;

            lineWidth = 0.05f + sitelink0.throughput_out * 0.01f;
        } else {
            lineColor = Color.red;
            blinkPeriodSeconds = 2.0f;
        }

        // ---

        Draw();
        UpdateCollider();
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

            // Update the line renderer.
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

    // Update the collider for being able to point at the sitelink marker with the controller.
    // https://answers.unity.com/questions/470943/collider-for-line-renderer.html
    // TODO: This only works for short sitelinks, as it's just a capsule between the two sites.
    public void UpdateCollider() {
        collider.radius = lineWidth / 2;
        collider.direction = 2;

        Vector3 fromPos = fromSiteMarker.transform.position;
        Vector3 toPos = toSiteMarker.transform.position;
        collider.transform.position = fromPos + (toPos - fromPos) / 2;
        collider.height = (toPos - fromPos).magnitude;
    }

    // ---

    public void UpdateInformation() {
        Sitelink sitelink0 = sitelinkPair.pair[0];
        Sitelink sitelink1 = sitelinkPair.pair[1];

        _information = "Sitelink pair:\n" +
                      $"Sites: {sitelink0.local_site} <-> {sitelink1.remote_site}\n" +
                      $"Sitelink IDs: {sitelink0.id}/{sitelink1.id}\n" +
                      $"States: {sitelink0.state}/{sitelink1.state}\n" +
                      $"Statuses: {sitelink0.status}/{sitelink1.status}\n" +
                      $"In use: {sitelink0.inuse}/{sitelink1.inuse}\n" +
                      $"Sitelink 0 throughput in/out: {sitelink0.throughput_in}/{sitelink0.throughput_out}\n" + 
                      $"Sitelink 1 throughput in/out: {sitelink1.throughput_in}/{sitelink1.throughput_out}\n";
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _stateManager.DisplayInformation(_information);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _stateManager.HideInformation();
    }
}
