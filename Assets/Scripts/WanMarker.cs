using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Models.SteelConnect;
using RSG;
using Proyecto26;

public class WanMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    public Wan wan;
    public GameObject cloud;
    public GameObject uplinks;
    public GameObject uplinkPrefab;
    public bool showInformation = true;
    public float uplinkLineThickness = 10f;
    public TextMesh text;

    private Behaviour _halo;
    private GameObject _currentUplinkCreation = null;
    private GameObject _uplinkCreationInProgress = null;
    private List<GameObject> _uplinks = new List<GameObject>();
    private StateManager _stateManager;
    private GameObject _reticle;
    private SteelConnect _steelConnect;
    private string _information;

    void Start () {
        _halo = (Behaviour)cloud.GetComponent("Halo");
        _halo.enabled = false;
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _steelConnect = new SteelConnect();
        UpdateInformation();
        StartCoroutine("FindReticle");
    }
	
    void Update () {
        // Position uplink line from WAN to pointer
        if (_currentUplinkCreation) {
            SetLine();
        }
		
    }

    IEnumerator FindReticle() {
        while (_reticle == null) {
            _reticle = GameObject.Find("Reticle");
            yield return null;
        }
        yield return null;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _halo.enabled = true;
        if (showInformation) {
            _stateManager.DisplayInformation(_information);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        _halo.enabled = false;
        if (showInformation) {
            _stateManager.HideInformation();
        }
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
            if (_stateManager.currentObjectHover && _stateManager.currentObjectHover.tag == "Site") {
                GameObject currentSite = _stateManager.currentObjectHover;
                _uplinks.Add(_currentUplinkCreation);
                string siteId = currentSite.GetComponent<SiteMarker>().site.id;
                Debug.Log($"Creating uplink {_currentUplinkCreation} from WAN: {wan.id} to site:{siteId}");
                _uplinkCreationInProgress = _currentUplinkCreation;
                // Create uplink API call
                _steelConnect.CreateUplink(wan.id, siteId)
                    .Then(response => {
                        Uplink uplinkResponse = JsonUtility.FromJson<Uplink>(response.Text);
                        Debug.Log($"Uplink created {uplinkResponse.id} from WAN: {wan.id} to site:{siteId}");
                        UplinkMarker uplinkMarker = _uplinkCreationInProgress.GetComponent<UplinkMarker>();
                        GameObject line = _uplinkCreationInProgress.transform.Find("Line").gameObject;
                        line.GetComponent<BoxCollider>().enabled = true;
                        uplinkMarker.created = true;
                        uplinkMarker.uplink.id = uplinkResponse.id;
                        uplinkMarker.uplink.name = uplinkResponse.name;
                        uplinkMarker.uplink.node = uplinkResponse.node;
                        uplinkMarker.uplink.org = uplinkResponse.org;
                        uplinkMarker.uplink.site = uplinkResponse.site;
                        uplinkMarker.uplink.wan = uplinkResponse.wan;
                        uplinkMarker.UpdateInformation();
                        uplinkMarker.wan = gameObject;
                        uplinkMarker.site = currentSite;
                        _uplinkCreationInProgress.transform.parent = uplinks.transform;
                        _uplinkCreationInProgress = null;
                });
            } else {
                Destroy(_currentUplinkCreation);
            }
            _currentUplinkCreation = null;
        }
    }

    private void SetLine() {
        Vector3 heading = _reticle.transform.position - transform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        Vector3 midPoint = (transform.position + _reticle.transform.position) / 2;
        GameObject line = _currentUplinkCreation.transform.Find("Line").gameObject;
        // Disable so we don't get hover information
        line.GetComponent<BoxCollider>().enabled = false;
        line.SetActive(true);
        line.transform.position = midPoint;
        Utilities.SetGlobalScale(line.transform, new Vector3(1, 1, distance)); // Any value for x and y. Will change soon
        // Set X and Y localscale so cube appears to be a line
        line.transform.localScale = new Vector3(uplinkLineThickness, uplinkLineThickness, line.transform.localScale.z);
        line.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void UpdateInformation() {
        _information = $"Id: {wan.id}\n" +
                      $"Name: {wan.name}\n" +
                      $"Longname: {wan.longname}\n" +
                      $"Org: {wan.org}\n";
        text.text = wan.name;
    }
}
