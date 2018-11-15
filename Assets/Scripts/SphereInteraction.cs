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
    private bool _isCurrentlyDragging = false;

    private GvrControllerInputDevice _dominantController;

    private Vector3 _previousOrientation;
    private Vector3 _localRotation;

    private Vector3 _localRotationVelocity = Vector3.zero;

    private float _panFactor = 120.0f;
    private float _velocityDecayFactor = 0.92f;

    private float _lastClick = 0.0f;

    public float sphereRadius = 1.0f;

    void Start() {
        _dominantController = GvrControllerInput.GetDevice(GvrControllerHand.Dominant);
        _previousOrientation = _dominantController.Orientation * Vector3.forward;
    }

    void Update() {
        if (_isCurrentlyDragging) {
            Vector3 orientationDelta = (_dominantController.Orientation * Vector3.forward) - _previousOrientation;

            _localRotation.x += orientationDelta.x * _panFactor;
            _localRotation.y -= orientationDelta.y * _panFactor;

            _localRotationVelocity.x = orientationDelta.x * _panFactor / Time.deltaTime;
            _localRotationVelocity.x = orientationDelta.x * _panFactor / Time.deltaTime;
        } else {
            // Momentum.
            _localRotationVelocity.x *= _velocityDecayFactor;
            _localRotationVelocity.y *= _velocityDecayFactor;

            _localRotation.x += _localRotationVelocity.x * Time.deltaTime;
            _localRotation.y -= _localRotationVelocity.y * Time.deltaTime;
        }

        // Prevents flipping upside down
        if (_localRotation.y < -90.0f) {
            _localRotation.y = -90.0f;
        } else if (_localRotation.y > 90.0f) {
            _localRotation.y = 90.0f;
        }

        Quaternion QT = Quaternion.Euler(_localRotation.y, _localRotation.x, 0);
        pivotObject.transform.rotation = Quaternion.Lerp(pivotObject.transform.rotation, QT, Time.deltaTime * 10);
        _previousOrientation = _dominantController.Orientation * Vector3.forward;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (globeDragEnabled && eventData.clickTime - _lastClick < 1)
        {
            Vector3 pos = eventData.pointerPressRaycast.worldPosition;
            pos = Quaternion.Inverse(globeMap.transform.rotation) * pos;

            Vector2d latlong = Conversions.GeoFromGlobePosition(pos, 1);

            _lastClick = eventData.clickTime;
            stateManager.ChangeMap();
            Debug.Log(latlong);
            flatMap.UpdateMap(latlong, flatMap.Zoom);
            return;
        }
        _lastClick = eventData.clickTime;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (globeDragEnabled) {
            _isCurrentlyDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        _isCurrentlyDragging = false;
    }
}
