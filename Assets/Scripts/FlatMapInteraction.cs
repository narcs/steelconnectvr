using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Mapbox.Unity.Map;
using Mapbox.Utils;

public class FlatMapInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public AbstractMap map;
    public StateManager statemanager;

    public bool dragEnabled = true;
    private bool _isCurrentlyDragging = false;

    private bool _pointingAt = false;

    private GvrControllerInputDevice _dominantController;
    private Vector3 _previousOrientation;
    private Vector2 _previousTouch;
    private Vector3 _translate;
    private bool _previousTouchState = false;
    private Vector3 _initTransform;

    public float panFactor = 1f;
    private float _velocityDecayFactor = 0.92f;
    private float _zoomFactor = 1.0f;
    public float minZoom = 3.0f; // The minimum zoom factor allowed
  
    public int xRange = 5;
    public int zRange = 5;

    public int hiddenLayer = 12; // This should match the layer number of a non-visible layer
    public int siteLayer = 10; // This should match the layer number sites are in

    // Use this for initialization
    void Start () {
        _dominantController = GvrControllerInput.GetDevice(GvrControllerHand.Dominant);
        //_previousOrientation = _dominantController.Orientation * Vector3.forward;
        _previousTouch = _dominantController.TouchPos;
        _translate = new Vector3();
        _initTransform = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (_pointingAt)
        {
            // While touching the pad pan the map
            if (_dominantController.GetButton(GvrControllerButton.TouchPadTouch))
            {
                Vector2 touchPos = _dominantController.TouchPos;

                if (_previousTouchState)
                {
                    _translate = new Vector3();
                    Vector2 orientationDelta = touchPos - _previousTouch;

                    _translate.x += orientationDelta.x * panFactor;
                    _translate.z += orientationDelta.y * panFactor;

                    this.transform.Translate(_translate);
                }
                _previousTouchState = true;
                _previousTouch = touchPos;
            }
            // When the pad is let go recenter the map
            if (_dominantController.GetButtonUp(GvrControllerButton.TouchPadTouch))
            {
                Recenter();
                _previousTouch = _dominantController.TouchPos;
                _previousTouchState = false;
            }
            // When the button is clicked zoom in
            if (_dominantController.GetButtonDown(GvrControllerButton.TouchPadButton)) // Using event instead
            {
                
            }
            // When the app button is clicked zoom out
            if (_dominantController.GetButtonDown(GvrControllerButton.App)) // TODO: Change to double click
            {
                ZoomOut();
            }
        }

        CullSiteMarkers();
    }

    void Recenter()
    {
        Vector2d latLong = map.WorldToGeoPosition(new Vector3());
        map.UpdateMap(latLong,map.AbsoluteZoom);
        Vector3 translate = new Vector3(0, _initTransform.y, 0);
        translate -= this.transform.position;
        this.transform.Translate(translate);

        foreach (var entry in statemanager.currentSiteMarkers)
        {
            entry.Value.transform.Translate(-translate);
        }

        foreach (var entry in statemanager._currentSitelinkMarkers) {
            entry.transform.Translate(-translate);
        }
    }

    void RescaleSiteMarkers()
    {
        foreach (var entry in statemanager.currentSiteMarkers)
        {
            Vector3 scale = entry.Value.initScale;

            scale.x = scale.x / map.transform.lossyScale.x;
            scale.y = scale.y / map.transform.lossyScale.y;
            scale.z = scale.z / map.transform.lossyScale.z;

            entry.Value.transform.localScale = scale;
        }
    }

    void CullSiteMarkers()
    {
        foreach (var entry in statemanager.currentSiteMarkers)
        {
            Vector3 pos = entry.Value.transform.position;

            if (Mathf.Abs(pos.x) > xRange || Mathf.Abs(pos.z) > zRange)
            {
                ChangeLayerRecursive(entry.Value.gameObject, hiddenLayer);
            }
            else
            {
                ChangeLayerRecursive(entry.Value.gameObject, siteLayer);
            }
        }
    }

    void ChangeLayerRecursive(GameObject obj, int layer)
    {
       obj.layer = layer;
        foreach (Transform child in obj.GetComponentInChildren<Transform>(true))
        {
            ChangeLayerRecursive(child.gameObject, layer);
        }
    }

    void ZoomIn()
    {
        map.UpdateMap(map.CenterLatitudeLongitude, map.Zoom + _zoomFactor);

        RescaleSiteMarkers();
    }

    void ZoomOut()
    {
        if (map.Zoom > minZoom)
            map.UpdateMap(map.CenterLatitudeLongitude, map.Zoom - _zoomFactor);

        RescaleSiteMarkers();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2d geo = map.WorldToGeoPosition(eventData.pointerPressRaycast.worldPosition);

        map.UpdateMap(geo, map.Zoom);
        ZoomIn();
        statemanager.UpdateEntities(false);
        // TODO: Fix RescaleSiteMarkers to work after UpdateSites
        // Currently broken because of async issues.
        //RescaleSiteMarkers();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointingAt = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointingAt = false;
    }
}
