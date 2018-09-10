using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class WanMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

    public Wan wan;
    public GameObject cloud;
    public GameObject information;

    private Behaviour _halo;
    private MeshRenderer _informationMeshRenderer;

	void Start () {
        _halo = (Behaviour)cloud.GetComponent("Halo");
        _halo.enabled = false;
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
        _informationMeshRenderer.enabled = false;
	}
	
	void Update () {
		
	}

    public void OnPointerEnter(PointerEventData eventData) {
        _halo.enabled = true;
        _informationMeshRenderer.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _halo.enabled = false;
        _informationMeshRenderer.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
        // Render uplink line from WAN to pointer
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Create uplink if finish on site
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Create uplink
    }
}
