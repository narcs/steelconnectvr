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
    public Vector3 initScale;

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

        initScale = this.transform.localScale;
    }

    void Update() {

    }

    public void DeleteSite() {
        ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();
        particleSystem.Play();
        model.SetActive(false);
        _steelConnect.DeleteSite(site.id)
            .Then(response => {
                if (response.StatusCode == 200) {
                    Debug.Log($"Site deleted: {site.name}");
                } else {
                    Debug.LogError($"Unable to delete site: {site.name}.\n" +
                        $"Status code: {response.StatusCode}\n" +
                        $"Error: {response.Error}");
                }
            });
        Destroy(gameObject, particleSystem.main.duration);
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
