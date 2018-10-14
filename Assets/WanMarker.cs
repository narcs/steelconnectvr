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

    private Behaviour _halo;
    private MeshRenderer _informationMeshRenderer;
    private GameObject _currentUplinkCreation = null;
    private List<GameObject> _uplinks = new List<GameObject>();
    private StateManager _stateManager;
    private GameObject _reticle;

	void Start () {
        _halo = (Behaviour)cloud.GetComponent("Halo");
        _halo.enabled = false;
        _informationMeshRenderer = information.GetComponent<MeshRenderer>();
        _informationMeshRenderer.enabled = false;
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _reticle = GameObject.Find("Reticle");
	}
	
	void Update () {
        // Position uplink line from WAN to pointer
        if (_currentUplinkCreation) {
            SetLine();
        }
        // Position uplink line to follow WANs
        // TODO: Optimise
        //foreach (GameObject uplink in _uplinks) {
        //    LineRenderer lineRenderer = uplink.GetComponent<LineRenderer>();
        //    lineRenderer.SetPosition(0, transform.position);
        //}
		
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
        // Delete WAN
        if (_stateManager.currentMode == StateManagerMode.Delete) {
            // Confirmation panel
            _stateManager.ShowConfirm();
            _stateManager.SetDeleteConfirmText(gameObject, wan.name);
        } else {
            // Create uplink GameObject
            _currentUplinkCreation = Instantiate(uplinkPrefab, transform);
            UplinkMarker uplinkMarker = _currentUplinkCreation.GetComponent<UplinkMarker>();
            uplinkMarker.created = false;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Create uplink if selecting site
        if (_stateManager.currentMode != StateManagerMode.Delete) {
            if (_stateManager.currentObjectHover) {
                _uplinks.Add(_currentUplinkCreation);
                Debug.Log($"Created uplink {_currentUplinkCreation} from WAN: {wan.id} to site:{_stateManager.currentObjectHover.GetComponent<SiteMarker>().site.id}");
                UplinkMarker uplinkMarker = _currentUplinkCreation.GetComponent<UplinkMarker>();
                GameObject line = _currentUplinkCreation.transform.Find("Line").gameObject;
                line.GetComponent<BoxCollider>().enabled = true;
                uplinkMarker.created = true;
                uplinkMarker.wan = gameObject;
                uplinkMarker.site = _stateManager.currentObjectHover;
                // Create uplink API call
            } else {
                Destroy(_currentUplinkCreation);
            }
            _currentUplinkCreation = null;
        }
    }
    private void SetGlobalScale(Transform transform, Vector3 globalScale) {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y,
            globalScale.z / transform.lossyScale.z);
    }

    private void SetLine() {
        Vector3 heading = transform.position - _reticle.transform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        Vector3 midPoint = (transform.position + _reticle.transform.position) / 2;
        GameObject line = _currentUplinkCreation.transform.Find("Line").gameObject;
        // Disable so we don't get hover information
        line.GetComponent<BoxCollider>().enabled = false;
        line.SetActive(true);
        line.transform.position = midPoint;
        SetGlobalScale(line.transform, new Vector3(1, 1, distance));
        // Set X and Y localscale so cube appears to be a line
        line.transform.localScale = new Vector3(10, 10, line.transform.localScale.z);
        line.transform.rotation = Quaternion.LookRotation(direction);
    }
}
