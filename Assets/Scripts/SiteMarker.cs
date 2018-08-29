using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Proyecto26;
using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler{
    public Site Site;
    public float hoverIncreaseScale = 0.5f;

    private string _rendererObjectName = "Pillar";
    private Transform _rendererTransform;

    private string _informationObjectName = "Information";
    private Transform _information;
    private MeshRenderer _informationMeshRenderer;

    // Use this for initialization
    void Start() {
        _rendererTransform = transform.Find(_rendererObjectName);
        _information = transform.Find(_informationObjectName);
        _informationMeshRenderer = _information.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPointerEnter(PointerEventData eventData) {
        // Make size larger
        _rendererTransform.localScale += new Vector3(hoverIncreaseScale, hoverIncreaseScale, hoverIncreaseScale);
    }

    public void OnPointerExit(PointerEventData eventData) {
        // Revert to original size
        _rendererTransform.localScale -= new Vector3(hoverIncreaseScale, hoverIncreaseScale, hoverIncreaseScale);
    }

    // Need this event for PointerClick
    public void OnPointerDown(PointerEventData eventData) {
        // Show site information
        //_meshRenderer.enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Hide site information
        //_meshRenderer.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        _informationMeshRenderer.enabled = !_informationMeshRenderer.enabled;
    }

    
}
