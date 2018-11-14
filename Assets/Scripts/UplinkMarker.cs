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
    public bool created = true;
    public Color uplinkColour = Color.yellow;
    public Color hoverColour = new Color(1, 0.65f, 0, 1); // Orange
    public float uplinkLineThickness = 10f;

    private StateManager _stateManager;
    private int _lineLayerMask;
    private SteelConnect _steelConnect;
    private string _information;

	void Start () {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _lineLayerMask = ~(1 << LayerMask.NameToLayer("Line"));
        _steelConnect = new SteelConnect();
        UpdateInformation();
        if (wan && site) {
            SetLine();
            line.GetComponent<Renderer>().material.color = uplinkColour;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (wan && site && created) {
            SetLine();
        }
	}

    public void OnPointerEnter(PointerEventData eventData) {
        line.GetComponent<MeshRenderer>().material.color = hoverColour;
        _stateManager.DisplayInformation(_information);
        // Don't show WAN information
        wan.GetComponent<WanMarker>().showInformation = false;
    }

    public void OnPointerExit(PointerEventData eventData) {
        line.GetComponent<MeshRenderer>().material.color = uplinkColour;
        _stateManager.HideInformation();
        // Re-enable WAN information
        wan.GetComponent<WanMarker>().showInformation = true;
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
            _stateManager.SetDeleteConfirmText(gameObject, uplink.id);
        }
    }

    private void SetLine() {
        Vector3 heading = site.transform.position - wan.transform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        Vector3 midPoint = (wan.transform.position + site.transform.position) / 2;
        // Linecast and see if Earth is in between WAN and site. If so, don't enable
        RaycastHit hit;
        if (Physics.Linecast(wan.transform.position, site.transform.position, out hit, _lineLayerMask)) {
            if (hit.collider != null) {
                if (hit.collider.tag == "Site") {
                    SiteMarker siteMarker = site.GetComponent<SiteMarker>();
                    SiteMarker hitSiteMarker = hit.transform.parent.parent.GetComponent<SiteMarker>();
                    if (hitSiteMarker.site.id == siteMarker.site.id) {
                        if (hitSiteMarker.gameObject.layer != 12 && siteMarker.gameObject.layer != 12)
                        {
                            line.SetActive(true);
                            line.transform.parent = transform;
                            line.transform.position = midPoint;
                            Utilities.SetGlobalScale(line.transform, new Vector3(1, 1, distance)); // Any value for x and y. Will change soon 
                                                                                                   // Set X and Y localscale so cube appears to be a line
                            line.transform.localScale = new Vector3(uplinkLineThickness, uplinkLineThickness, line.transform.localScale.z);
                            line.transform.rotation = Quaternion.LookRotation(direction);

                            return;
                        }
                    }
                }
            }
        }
        line.SetActive(false);
    }
    public void DeleteUplink() {
        line.SetActive(false);
        _steelConnect.DeleteUplink(uplink.id)
            .Then(response => {
                if (response.StatusCode == 200) {
                    Debug.Log($"Uplink deleted: {uplink.name}");
                    Destroy(gameObject);
                } else {
                    Debug.LogError($"Unable to delete uplink: {uplink.name}.\n" +
                        $"Status code: {response.StatusCode}\n" +
                        $"Error: {response.Error}");
                }
            });
    }

    public void UpdateInformation() {
        _information = $"Id: {uplink.id}\n" +
                      $"Name: {uplink.name}\n" +
                      $"Org: {uplink.org}\n" +
                      $"Site: {uplink.site}\n" +
                      $"WAN: {uplink.wan}\n" +
                      $"Appliance: {uplink.node}";
    }
}
