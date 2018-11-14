using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class SiteMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler{
    public Site site;
    public GameObject model;
    public Vector3 initScale;
    public GameObject explosion;
    public AudioClip explosionSound;

    private StateManager _stateManager;
    private Behaviour _halo;
    private SteelConnect _steelConnect;
    private AudioSource _audioSource;
    private string _information;

    void Start() {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _halo = (Behaviour)model.GetComponent("Halo");
        _halo.enabled = false;
        _steelConnect = new SteelConnect();

        initScale = this.transform.localScale;
        _audioSource = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        UpdateInformation();
    }

    void Update() {

    }

    public void SiteDestruction() {
        ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();
        particleSystem.Play();
        model.SetActive(false);
        _audioSource.clip = explosionSound;
        _audioSource.Play();
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
        _stateManager.DisplayInformation(_information);
        _stateManager.currentObjectHover = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _halo.enabled = false;
        _stateManager.HideInformation();
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

    public void UpdateInformation() {
        _information = $"Id: {site.id}\n" +
                      $"Name: {site.name}\n" +
                      $"Longname: {site.longname}\n" +
                      $"Org: {site.org}\n" +
                      $"Country: {site.country}\n" +
                      $"City: {site.city}\n" +
                      $"Street Address: {site.street_address}";
    }

    
}
