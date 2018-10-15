﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler{
    public Site site;
    public GameObject model;
    public GameObject information;
    public GameObject explosionPrefab;

    private StateManager _stateManager;
    private Behaviour _halo;
    private MeshRenderer _informationMeshRenderer;
    private SteelConnect _steelConnect;

    void Start() {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _halo = (Behaviour)model.GetComponent("Halo");
        _halo.enabled = false;
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
        _steelConnect = new SteelConnect();
    }

    void Update() {

    }

    public void SiteDestruction() {
        GameObject explosion = Instantiate(explosionPrefab, transform);
        ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();
        particleSystem.Play();
        model.SetActive(false);
        Destroy(gameObject, particleSystem.main.duration);
    }

    public void DeleteSite(Destroyer destroyer) {
        _steelConnect.DeleteSite(site.id)
            .Then(response => {
                if (response.StatusCode == 200) {
                    Debug.Log($"Site deleted: {site.name}");
                    destroyer.StartDestruction(gameObject);
                } else {
                    Debug.LogError($"Unable to delete site: {site.name}.\n" +
                        $"Status code: {response.StatusCode}\n" +
                        $"Error: {response.Error}");
                }
            });
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
        if (_stateManager.currentMode == StateManagerMode.Delete) {
            // Confirmation panel
            _stateManager.ShowConfirm();
            _stateManager.SetDeleteConfirmText(gameObject, site.name);
        }
    }

    
}
