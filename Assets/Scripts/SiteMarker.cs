using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler{
    public Site site;
    public float hoverIncreaseScale = 0.5f;

    private StateManager _stateManager;

    private string _rendererObjectName = "Pillar";
    private Transform _rendererTransform;

    private GameObject _siteObject;
    private string _informationObjectName = "Information";
    private Transform _information;
    private MeshRenderer _informationMeshRenderer;

    // Use this for initialization
    void Start() {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _rendererTransform = transform.Find(_rendererObjectName);
        _siteObject = transform.parent.gameObject;
        _information = transform.Find(_informationObjectName);
        _informationMeshRenderer = _information.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPointerEnter(PointerEventData eventData) {
        // Make size larger
        _rendererTransform.localScale += new Vector3(hoverIncreaseScale, hoverIncreaseScale, hoverIncreaseScale);
        _informationMeshRenderer.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        // Revert to original size
        _rendererTransform.localScale -= new Vector3(hoverIncreaseScale, hoverIncreaseScale, hoverIncreaseScale);
        _informationMeshRenderer.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
        // Show site information
        //_meshRenderer.enabled = true;
        // Delete site
        if (_stateManager.deleteMode) {
            // Confirmation panel
            _stateManager.ShowConfirm();
            _stateManager.SetDeleteConfirmText(gameObject, site.name);
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Hide site information
        //_meshRenderer.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        //_informationMeshRenderer.enabled = !_informationMeshRenderer.enabled;
    }

    
}
