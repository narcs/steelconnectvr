﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Proyecto26;
using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler{
    public Site Site;
    public float hoverIncreaseScale = 0.5f;

    private string rendererObjectName = "Pillar";
    private string informationObjectName = "Information";

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPointerEnter(PointerEventData eventData) {
        // Make size larger
        Transform rendererTransform = transform.Find(rendererObjectName);
        rendererTransform.localScale += new Vector3(hoverIncreaseScale, hoverIncreaseScale, hoverIncreaseScale);
    }

    public void OnPointerExit(PointerEventData eventData) {
        // Revert to original size
        Transform rendererTransform = transform.Find(rendererObjectName);
        rendererTransform.localScale -= new Vector3(hoverIncreaseScale, hoverIncreaseScale, hoverIncreaseScale);
    }

    // Need this event for PointerClick
    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Show site information
        Transform information = transform.Find(informationObjectName);
        MeshRenderer meshRenderer = information.GetComponent<MeshRenderer>();
        meshRenderer.enabled = !meshRenderer.enabled;
    }
}
