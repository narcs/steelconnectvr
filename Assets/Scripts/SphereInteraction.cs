﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SphereInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    // The object that we rotate to orbit. The player should be a child of this.
    public GameObject pivotObject;

    public bool globeDragEnabled = true;
    private bool isCurrentlyDragging = false;

    private GvrControllerInputDevice dominantController;

    private Vector3 previousOrientation;
    private Vector3 localRotation;

    private Vector3 localRotationVelocity = Vector3.zero;

    private float panFactor = 120.0f;
    private float velocityDecayFactor = 0.92f;

    void Start() {
        dominantController = GvrControllerInput.GetDevice(GvrControllerHand.Dominant);
        previousOrientation = dominantController.Orientation * Vector3.forward;
    }

    void LateUpdate() {
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

    public void OnPointerDown(PointerEventData eventData) {
        if (globeDragEnabled) {
            isCurrentlyDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        isCurrentlyDragging = false;
    }
}