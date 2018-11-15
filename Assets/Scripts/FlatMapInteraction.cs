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
    private bool isCurrentlyDragging = false;

    private bool _pointingAt = false;

    private GvrControllerInputDevice dominantController;
    private Vector3 previousOrientation;
    private Vector2 previousTouch;
    private Vector3 translate;
    private bool _previousTouchState = false;

    public float panFactor = 1f;
    private float velocityDecayFactor = 0.92f;
    private float zoomFactor = 1.0f;

    public int xRange = 5;
    public int zRange = 5;

    // Use this for initialization
    void Start () {
        dominantController = GvrControllerInput.GetDevice(GvrControllerHand.Dominant);
        //previousOrientation = dominantController.Orientation * Vector3.forward;
        previousTouch = dominantController.TouchPos;
        translate = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        if (_pointingAt)
        {
            // While touching the pad pan the map
            if (dominantController.GetButton(GvrControllerButton.TouchPadTouch))
            {
                Vector2 touchPos = dominantController.TouchPos;

                if (_previousTouchState)
                {
                    translate = new Vector3();
                    Vector2 orientationDelta = touchPos - previousTouch;

                    translate.x += orientationDelta.x * panFactor;
                    translate.z += orientationDelta.y * panFactor;

                    this.transform.Translate(translate);
                }
                _previousTouchState = true;
                previousTouch = touchPos;
            }
            // When the pad is let go recenter the map
            if (dominantController.GetButtonUp(GvrControllerButton.TouchPadTouch))
            {
                Recenter();
                previousTouch = dominantController.TouchPos;
                _previousTouchState = false;
            }
            // When the button is clicked zoom in
            if (dominantController.GetButtonDown(GvrControllerButton.TouchPadButton)) // Using event instead
            {
                
            }
            // When the app button is clicked zoom out
            if (dominantController.GetButtonDown(GvrControllerButton.App)) // TODO: Change to double click
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
        Vector3 translate = new Vector3(0, -0.5f, 0);
        translate -= this.transform.position;
        this.transform.Translate(translate);

        foreach (var entry in statemanager.currentSiteMarkers)
        {
            entry.Value.transform.Translate(-translate);
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
                ChangeLayerRecursive(entry.Value.gameObject, 12);
            }
            else
            {
                ChangeLayerRecursive(entry.Value.gameObject, 10);
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
        map.UpdateMap(map.CenterLatitudeLongitude, map.Zoom + zoomFactor);

        RescaleSiteMarkers();
    }

    void ZoomOut()
    {
        if (map.Zoom > 3.0f)
            map.UpdateMap(map.CenterLatitudeLongitude, map.Zoom - zoomFactor);

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
