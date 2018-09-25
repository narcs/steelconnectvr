using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler{
    public Site site;
    public GameObject model;
    public GameObject information;
    public GameObject explosion;

    private StateManager _stateManager;
    private Behaviour _halo;
    private MeshRenderer _informationMeshRenderer;

    void Start() {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _halo = (Behaviour)model.GetComponent("Halo");
        _halo.enabled = false;
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
    }

    void Update() {

    }

    public void DeleteSite() {
        ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();
        particleSystem.Play();
        model.SetActive(false);
        Destroy(gameObject, particleSystem.main.duration);
        Debug.Log($"Site deleted: {site.name}");
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _halo.enabled = true;
        _informationMeshRenderer.enabled = true;
        _stateManager.currentObjectHover = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _halo.enabled = false;
        _informationMeshRenderer.enabled = false;
        _stateManager.currentObjectHover = null;
    }

    // Need this event for PointerClick
    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnPointerUp(PointerEventData eventData) {
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Delete site
        if (_stateManager.deleteMode) {
            // Confirmation panel
            _stateManager.ShowConfirm();
            _stateManager.SetDeleteConfirmText(gameObject, site.name);
        }
    }

    
}
