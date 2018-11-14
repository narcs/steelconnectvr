using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using RSG;
using Proyecto26;
using Models.SteelConnect;


public class WanManager : MonoBehaviour {

    public GameObject panel;
    public GameObject wanPrefab;
    public GameObject uplinkPrefab;
    public GameObject earthSphere;
    public Dictionary<string, Uplink> uplinks = new Dictionary<string, Uplink>();

    private List<Wan> _wans = new List<Wan>();
    private StateManager _stateManager;
    private SteelConnectDataManager _dataManager;
    private bool _showUplinks = false;
    private GlobeSiteCreation _globeSiteCreation;
    private float _nameTextYPosition = 155f;

	void Start () {
        _stateManager = GameObject.Find("State Manager").GetComponent<StateManager>();
        _dataManager = GameObject.Find("State Manager").GetComponent<SteelConnectDataManager>();
        _globeSiteCreation = earthSphere.GetComponent<GlobeSiteCreation>();
	}

    private void ShowHideUplinks() {
        foreach (Transform wan in panel.transform) {
            WanMarker wanmarker = wan.GetComponent<WanMarker>();
            wanmarker.uplinks.SetActive(_showUplinks);
        }
    }

    public void ShowHideUplinksToggle() {
        _showUplinks = !_showUplinks;
        ShowHideUplinks();
    }

    public void DestroyWans() {
        foreach (Transform child in panel.transform) {
            Destroy(child.gameObject);
        }
        _wans.Clear();
        uplinks.Clear();
    }

    public void UpdateWans() {
        panel.SetActive(true);
        DestroyWans();
        // Get WANs from SteelConnect API
        _dataManager.GetWans(true)
            .Then(wans => wans.ForEach(wan => {
                _wans.Add(wan);
            }))
            .Then(() =>
                // Get uplinks from SteelConnect API
                _dataManager.GetUplinks(true)
                    .Then(uplinks => uplinks.ForEach(uplink => {
                        this.uplinks[uplink.id] = uplink;
                    })))
            .Then(() => {
                // Create WAN gameObjects
                bool nameTextOnTop = true;
                foreach (Wan wan in _wans) {
                    GameObject newWanMarkerObject = Instantiate(wanPrefab, panel.transform);
                    WanMarker newWanMarker = newWanMarkerObject.GetComponent<WanMarker>();
                    newWanMarker.wan = wan;
                    // Alternate WAN name Y position
                    if (nameTextOnTop) {
                        newWanMarker.text.transform.localPosition = new Vector3(0, _nameTextYPosition, 0);
                    }
                    nameTextOnTop = !nameTextOnTop;
                    Debug.Log($"Created {wan.id}");
                    // Create Uplink Marker Objects for each WAN
                    foreach (string uplinkID in wan.uplinks) {
                        GameObject newUplinkMarkerObject = Instantiate(uplinkPrefab, newWanMarker.uplinks.transform);
                        UplinkMarker newUplinkMarker = newUplinkMarkerObject.GetComponent<UplinkMarker>();
                        newUplinkMarker.uplink = uplinks[uplinkID];
                        newUplinkMarker.wan = newWanMarkerObject;
                        string uplinkSiteID = newUplinkMarker.uplink.site;
                        if (_stateManager.currentSiteMarkers.ContainsKey(uplinkSiteID)) {
                            newUplinkMarker.site = _stateManager.currentSiteMarkers[uplinkSiteID].gameObject;
                        } else {
                            Debug.LogError($"Site does not exist: {uplinkSiteID}");
                        }
                    }
                }
                ShowHideUplinks();
            });


    }
	
}
