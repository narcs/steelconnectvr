using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class WanMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    public Wan wan;
    public GameObject cloud;
    public GameObject information;
    public GameObject uplinkPrefab;
    public GameObject reticle;

    private Behaviour _halo;
    private MeshRenderer _informationMeshRenderer;
    private GameObject _currentUplinkCreation = null;
    private List<GameObject> _uplinks = new List<GameObject>();

	void Start () {
        _halo = (Behaviour)cloud.GetComponent("Halo");
        _halo.enabled = false;
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
        _informationMeshRenderer.enabled = false;
	}
	
	void Update () {
        // Position uplink line from WAN to pointer
        if (_currentUplinkCreation) {
            LineRenderer lineRenderer = _currentUplinkCreation.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            Vector3[] points = new Vector3[2] {
                transform.position,
                reticle.transform.position
            };
            lineRenderer.SetPositions(points);
        }
        // Position uplink line to follow WANs
        // TODO: Optimise
        foreach (GameObject uplink in _uplinks) {
            LineRenderer lineRenderer = uplink.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, transform.position);
        }
		
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
        // Create uplink GameObject
        _currentUplinkCreation = Instantiate(uplinkPrefab, transform);
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Create uplink if selecting site
        _uplinks.Add(_currentUplinkCreation);
        _currentUplinkCreation = null;
        // Create uplink API call
    }
}
