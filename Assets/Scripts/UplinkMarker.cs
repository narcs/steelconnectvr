using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;

public class UplinkMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

    public Uplink uplink;
    public GameObject wan;
    public GameObject site;
    public GameObject information;
    public GameObject line;

    private StateManager _stateManager;
    private MeshRenderer _informationMeshRenderer;
    private Color _startColour;

    private void SetGlobalScale(Transform transform, Vector3 globalScale) {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y,
            globalScale.z / transform.lossyScale.z);
    }

    private void SetLine() {
        Vector3 heading = wan.transform.position - site.transform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        Vector3 midPoint = (wan.transform.position + site.transform.position) / 2;
        line.transform.parent = transform;
        line.transform.position = midPoint;
        SetGlobalScale(line.transform, new Vector3(1, 1, distance));
        // Set X and Y localscale so cube appears to be a line
        line.transform.localScale = new Vector3(10, 10, line.transform.localScale.z);
        line.transform.rotation = Quaternion.LookRotation(direction);

        _informationMeshRenderer.transform.position = midPoint;
    }

	void Start () {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
        if (wan && site) {
            SetLine();
            line.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (wan && site) {
            SetLine();
        }
	}

    public void OnPointerEnter(PointerEventData eventData) {
        line.GetComponent<MeshRenderer>().material.color = new Color(1, 0.65f, 0, 1); // Orange
        _informationMeshRenderer.enabled = true;
        _stateManager.currentObjectHover = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData) {
        line.GetComponent<MeshRenderer>().material.color = Color.yellow;
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
