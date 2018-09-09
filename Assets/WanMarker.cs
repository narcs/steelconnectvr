using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class WanMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler{

    public GameObject cloud;
    private Behaviour _halo;

	void Start () {
        _halo = (Behaviour)cloud.GetComponent("Halo");
        _halo.enabled = false;
	}
	
	void Update () {
		
	}

    public void OnPointerEnter(PointerEventData eventData) {
        _halo.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _halo.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
    }
}
