using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Models.SteelConnect;

public class SitelinkMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private SiteMarker _fromSiteMarker;
    private SiteMarker _toSiteMarker;
    private SitelinkPair _sitelinkPair;

    private Vector3 _globePosition;
    private float _globeRadius;

    private int _numPoints = 8; // TODO: Calculate based on sphere surface distance.
    private float _lineWidth;
    private Color _lineColor;

    private float _blinkPeriodSeconds = 0.0f;
    private float _blinkLevel = 0.0f;
    private float _blinkDirection = 1.0f;

    private LineRenderer _lineRenderer;

    private StateManager _stateManager;
    private string _information;

    // Use this for initialization
    void Start () {
        
    }

    public void Set(SiteMarker fromSiteMarker, SiteMarker toSiteMarker, SitelinkPair sitelinkPair, Vector3 globePosition, float globeRadius = 0.0f) {
        this._fromSiteMarker = fromSiteMarker;
        this._toSiteMarker = toSiteMarker;
        this._sitelinkPair = sitelinkPair;

        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _lineRenderer = GetComponent<LineRenderer>();

        // TODO: Get these values in a better way, eg. link the globe object here with a public member variable.
        this._globePosition = globePosition;
        this._globeRadius = globeRadius;

        // ---

        SitelinkReporting sitelink0 = sitelinkPair.pair[0];

        _lineColor = Color.green;
        _lineWidth = 0.1f;
        _blinkPeriodSeconds = 0.0f;

        if (sitelink0.state == "up") {
            _lineColor = Color.green;

            _lineWidth = 0.05f + sitelink0.throughput_out * 0.01f;
        } else {
            _lineColor = Color.red;
            _blinkPeriodSeconds = 2.0f;
        }

        // ---

        Draw();
        UpdateCollider();
        UpdateInformation();
    }

    // Update is called once per frame
    void Update() {
        if (_blinkPeriodSeconds > 0.0f) {
            _blinkLevel += _blinkDirection * (1/_blinkPeriodSeconds) * 2 * Time.deltaTime;

            if (_blinkLevel < 0.0f || _blinkLevel > 1.0f) {
                _blinkDirection *= -1.0f;
            }
        } else {
            _blinkLevel = 1.0f;
        }

        _lineColor.a = _blinkLevel;

        // Redraw to update blink level.
        Draw();

        if (!(_fromSiteMarker && _toSiteMarker)) { //TODO: Optimise
            Destroy(gameObject);
        }
    }

    public void Draw() {
        // This check is so that if there is an sitelink marker with no from/to site (eg. because
        // it's a dummy marker for easy modification of the prefab), it won't try to draw itself
        // and spam the console with an error per frame.
        if (_fromSiteMarker != null && _toSiteMarker != null) {
            _lineRenderer.positionCount = _numPoints;
            _lineRenderer.startColor = _lineColor;
            _lineRenderer.endColor = _lineColor;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;

            // Update the line renderer.
            for (int i = 0; i < _numPoints; i++) {
                float progress = ((float)i) / (_numPoints - 1);
                Vector3 result = Vector3.Slerp(
                    _fromSiteMarker.gameObject.transform.position,
                    _toSiteMarker.gameObject.transform.position,
                    progress);

                float radius = Vector3.Distance(_globePosition, result);
                if (radius < _globeRadius) {
                    result *= (_globeRadius / radius);
                }

                _lineRenderer.SetPosition(i, result);
            }
        }
    }

    // Build a chain of colliders along the line.
    public void UpdateCollider() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        if (_fromSiteMarker != null && _toSiteMarker != null) {
            Vector3 lastPoint = Vector3.zero;
            
            for (int i = 0; i < _numPoints; i++) {
                float progress = ((float)i) / (_numPoints - 1);
                Vector3 result = Vector3.Slerp(
                    _fromSiteMarker.gameObject.transform.position,
                    _toSiteMarker.gameObject.transform.position,
                    progress);

                float radius = Vector3.Distance(_globePosition, result);
                if (radius < _globeRadius) {
                    result *= (_globeRadius / radius);
                }

                if (i > 0) {
                    GameObject colliderObject = new GameObject($"Collider{i}");
                    colliderObject.transform.SetParent(transform);

                    CapsuleCollider col = colliderObject.AddComponent<CapsuleCollider>();

                    Vector3 start = lastPoint;
                    Vector3 end = result;

                    col.radius = _lineWidth / 2;
                    col.height = (end - start).magnitude / 2;
                    col.center = Vector3.zero;
                    col.direction = 2; // Aligned on Z.
                    colliderObject.transform.position = start + (end - start) / 2;
                    colliderObject.transform.LookAt(start);
                }

                lastPoint = result;
            }
        }
    }

    // ---

    public void UpdateInformation() {
        SitelinkReporting sitelink0 = _sitelinkPair.pair[0];
        SitelinkReporting sitelink1 = _sitelinkPair.pair[1];

        _information = "Sitelink pair:\n" +
                      $"Sites: {sitelink0.local_site}\n<-> {sitelink0.remote_site}\n" +
                      //$"Sitelink IDs: {sitelink0.id}/{sitelink1.id}\n" +
                      $"States: {sitelink0.state}/{sitelink1.state}\n" +
                      //$"Statuses: {sitelink0.status}/{sitelink1.status}\n" +
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
