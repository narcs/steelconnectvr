using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;

public class SphereInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
    // The object that we rotate to orbit. The player should be a child of this.
    public GameObject pivotObject;
    public AbstractMap flatMap;
    public AbstractMap globeMap;
    public StateManager stateManager;

    public bool globeDragEnabled = true;
    private bool isCurrentlyDragging = false;

    private GvrControllerInputDevice dominantController;

    private Vector3 previousOrientation;
    private Vector3 localRotation;

    private Vector3 localRotationVelocity = Vector3.zero;

    private float panFactor = 120.0f;
    private float velocityDecayFactor = 0.92f;

    private float lastClick = 0.0f;
    public float doubleClickSpeed = 1.0f; // The delay between clicks allowed for double clicking

    public float sphereRadius = 1.0f; // This should match the radius of the globe, geocoding may not work if it does not

    void Start() {
        dominantController = GvrControllerInput.GetDevice(GvrControllerHand.Dominant);
        previousOrientation = dominantController.Orientation * Vector3.forward;
    }

    void Update() {
        if (isCurrentlyDragging) {
            Vector3 orientationDelta = (dominantController.Orientation * Vector3.forward) - previousOrientation;

            localRotation.x += orientationDelta.x * panFactor;
            localRotation.y -= orientationDelta.y * panFactor;

            localRotationVelocity.x = orientationDelta.x * panFactor / Time.deltaTime;
            localRotationVelocity.x = orientationDelta.x * panFactor / Time.deltaTime;
        } else {
            // Momentum.
            localRotationVelocity.x *= velocityDecayFactor;
            localRotationVelocity.y *= velocityDecayFactor;

            localRotation.x += localRotationVelocity.x * Time.deltaTime;
            localRotation.y -= localRotationVelocity.y * Time.deltaTime;
        }

        // Prevents flipping upside down
        if (localRotation.y < -90.0f) {
            localRotation.y = -90.0f;
        } else if (localRotation.y > 90.0f) {
            localRotation.y = 90.0f;
        }

        Quaternion QT = Quaternion.Euler(localRotation.y, localRotation.x, 0);
        pivotObject.transform.rotation = Quaternion.Lerp(pivotObject.transform.rotation, QT, Time.deltaTime * 10);
        previousOrientation = dominantController.Orientation * Vector3.forward;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (globeDragEnabled && eventData.clickTime - lastClick < doubleClickSpeed)
        {
            Vector3 pos = eventData.pointerPressRaycast.worldPosition;
            pos = Quaternion.Inverse(globeMap.transform.rotation) * pos;

            Vector2d latlong = Conversions.GeoFromGlobePosition(pos, sphereRadius);

            lastClick = eventData.clickTime;
            stateManager.ChangeMap();
            Debug.Log(latlong);
            flatMap.UpdateMap(latlong, flatMap.Zoom);
            return;
        }
        lastClick = eventData.clickTime;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (globeDragEnabled) {
            isCurrentlyDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        isCurrentlyDragging = false;
    }
}
