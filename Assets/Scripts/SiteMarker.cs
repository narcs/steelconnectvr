using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler{
    public Site site;
    public GameObject pillar;
    public GameObject information;

    private StateManager _stateManager;
    private Behaviour _halo;
    private MeshRenderer _informationMeshRenderer;

    // Use this for initialization
    void Start() {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _halo = (Behaviour)pillar.GetComponent("Halo");
        _halo.enabled = false;
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPointerEnter(PointerEventData eventData) {
        _halo.enabled = true;
        _informationMeshRenderer.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _halo.enabled = false;
        _informationMeshRenderer.enabled = false;
    }

    // Need this event for PointerClick
    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnPointerUp(PointerEventData eventData) {
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Delete site
        if (_stateManager.currentMode == StateManager.Mode.DeleteSite) {
            _stateManager.SetGameObjectToDelete(gameObject, site.name);
        }
    }

    
}
