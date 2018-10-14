using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Mapbox.Unity.Map;
using Mapbox.Utils;

public class FlatMapInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public AbstractMap map;
    public StateManager statemanager;

    public bool dragEnabled = true;
    private bool isCurrentlyDragging = false;

    private bool _pointingAt = false;

    private GvrControllerInputDevice dominantController;
    private Vector3 previousOrientation;
    private Vector2 previousTouch;
    private Vector3 translate;

    public float panFactor = 0.5f;
    private float velocityDecayFactor = 0.92f;

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
                //Debug.Log(touchPos + " " + previousTouch);
                translate = new Vector3();
                Vector2 orientationDelta = touchPos - previousTouch;

                translate.x += orientationDelta.x * panFactor;
                translate.z += orientationDelta.y * panFactor;
                previousTouch = touchPos;

                this.transform.Translate(translate);
            }
            // When the pad is let go recenter the map
            if (dominantController.GetButtonUp(GvrControllerButton.TouchPadTouch))
            {
                //Debug.Log("Stop Touch");
                Recenter();
                previousTouch = dominantController.TouchPos;
            }
            // When the button is clicked zoom in
            if (dominantController.GetButtonDown(GvrControllerButton.TouchPadButton)) // TODO: Change to double click
            {
                ZoomIn();
            }
            // When the app button is clicked zoom out
            if (dominantController.GetButtonDown(GvrControllerButton.App)) // TODO: Change to double click
            {
                ZoomOut();
            }
        }

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

    void ZoomIn()
    {
        map.UpdateMap(map.CenterLatitudeLongitude, map.Zoom + 0.5f);

        RescaleSiteMarkers();
    }

    void ZoomOut()
    {
        if (map.Zoom > 3.0f)
            map.UpdateMap(map.CenterLatitudeLongitude, map.Zoom - 0.5f);

        RescaleSiteMarkers();
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
