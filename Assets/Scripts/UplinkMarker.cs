using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class UplinkMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

    public Uplink uplink;
    public GameObject wan;
    public GameObject site;
    public GameObject line;
    public GameObject information;

    private StateManager _stateManager;
    private MeshRenderer _informationMeshRenderer;
    private LineRenderer _lineRenderer;
    private Color _startColour;
    private CapsuleCollider _capsule;


	void Start () {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
        _lineRenderer = line.GetComponent<LineRenderer>();
        _startColour = _lineRenderer.startColor;
        _capsule = gameObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
        _capsule.radius = _lineRenderer.startWidth / 2;
        _capsule.center = Vector3.zero;
        _capsule.direction = 2; // Z-axis for easier "LookAt" orientation

    }
	
	// Update is called once per frame
	void Update () {
        if (wan && site) {
            Vector3[] points = new Vector3[2] {
                wan.transform.position,
                site.transform.position
            };
            _lineRenderer.SetPositions(points);

            _capsule.transform.position = wan.transform.position + (site.transform.position - wan.transform.position) / 2;
            _capsule.transform.LookAt(wan.transform.position);
            _capsule.height = (site.transform.position - wan.transform.position).magnitude;
        }
	}

    public void OnPointerEnter(PointerEventData eventData) {
        Color hoverColour = new Color(1, 0.65f, 0, 1);
        Debug.Log("hello");
        _lineRenderer.startColor = hoverColour;
        _informationMeshRenderer.enabled = true;
        _stateManager.currentObjectHover = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _lineRenderer.startColor = _startColour;
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
