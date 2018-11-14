using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gvr.Internal;

using RSG;
using Models.SteelConnect;

// These are just to make it clearer what each function or type expects.
// These aliases are all just equivalent to `string`, and so you can pass strings with no problems.
using OrgId = System.String;
using SiteId = System.String;
using WanId = System.String;
using UplinkId = System.String;
using SitelinkId = System.String;

public enum StateManagerMode {
    Normal,
    Delete,
    CreateSite,
}

public class StateManager : MonoBehaviour {
    public GameObject laser;

    public GameObject confirm;
    public GameObject createSiteWindow;

    public GameObject currentObjectHover;
    public GameObject earthSphere;
    public GameObject destroyerObject;
    public GameObject explosionPrefab;
    public GameObject flatMap;

	public GvrKeyboard keyboardManager;

    public StateManagerMode currentMode = StateManagerMode.Normal;

    public GameObject informationText;

    private GameObject _tempObject;
    private TextMesh _informationTextMesh;

    private SteelConnectDataManager _dataManager;

    // Site marker code
    public Dictionary<SiteId, SiteMarker> currentSiteMarkers;
    private List<SitelinkMarker> currentSitelinkMarkers;
    private WanManager _wanManager;

    void Start () {
        currentSiteMarkers = new Dictionary<SiteId, SiteMarker>();
        currentSitelinkMarkers = new List<SitelinkMarker>();
        _wanManager = gameObject.GetComponent<WanManager>();

        _dataManager = gameObject.GetComponent<SteelConnectDataManager>();

        confirm.SetActive(false);
        createSiteWindow.SetActive(false);
        _informationTextMesh = informationText.GetComponent<TextMesh>();
        informationText.transform.parent.parent.gameObject.SetActive(false);

        StartCoroutine("UpdateSitesOnStartUp");
    }
	
    IEnumerator UpdateSitesOnStartUp() {
        while (!_dataManager.IsInstantiated()) {
            yield return null;
        }
        UpdateSitesForceRefresh();
        yield return null;
    }

    // Update Sites
    public void UpdateSitesForceRefresh() {
        UpdateSites(true);
    }

    public void SwitchToDeleteMode() {
        currentMode = StateManagerMode.Delete;
        SetLaserColorForMode(currentMode);
    }

    public void SwitchToCreateSiteMode() {
        currentMode = StateManagerMode.CreateSite;
        SetLaserColorForMode(currentMode);
        earthSphere.GetComponent<SphereInteraction>().globeDragEnabled = false;

        createSiteWindow.GetComponent<CreateSiteWindow>().OnEnterCreateSiteMode();
        createSiteWindow.SetActive(true);
    }

    public void SwitchToNormalMode() {
        currentMode = StateManagerMode.Normal;
        SetLaserColorForMode(currentMode);

        earthSphere.GetComponent<SphereInteraction>().globeDragEnabled = true;

        keyboardManager.Hide();
        createSiteWindow.SetActive(false);
        createSiteWindow.GetComponent<CreateSiteWindow>().OnLeaveCreateSiteMode();
	}

    public void UpdateSites(bool forceRefresh) {
        foreach (var entry in currentSiteMarkers) {
            if (entry.Value) {
                Destroy(entry.Value.gameObject);
            }
        }
        currentSiteMarkers.Clear();
        _wanManager.DestroyWans();


        var siteMarkersPromise = _dataManager.GetSites(forceRefresh)
            .Then(sites => {
                foreach (Site site in sites) {
                    if (site.coordinates.isValid) {
                        if (earthSphere.activeSelf) {
                            currentSiteMarkers.Add(site.id, earthSphere.GetComponent<GlobeSiteCreation>().placeSiteMarker(site, site.coordinates));
                        } else {
                            currentSiteMarkers.Add(site.id, flatMap.GetComponent<FlatSiteCreation>().placeSiteMarker(site, site.coordinates));
                        }
                    } else {
                        Debug.LogWarning($"Coordinates for site {site.id} are not valid, not adding site marker");
                    }
                }


                return currentSiteMarkers;
            });

        var sitelinkMarkersPromise = PromiseHelpers.All(siteMarkersPromise, _dataManager.GetSitelinkPairs(forceRefresh))
            .Then(tup => {
                Dictionary<SiteId, SiteMarker> siteMarkers = tup.Item1;
                List<SitelinkPair> sitelinkPairs = tup.Item2;

                foreach (SitelinkPair sitelinkPair in sitelinkPairs) {
                    Debug.Log($"Sitelink pair has {sitelinkPair.pair.Count} sitelinks");
                    if (sitelinkPair.IsValid()) {
                        SitelinkReporting sitelink0 = sitelinkPair.pair[0];
                        SitelinkReporting sitelink1 = sitelinkPair.pair[1];

                        Debug.Log($"Sitelink pair {sitelink0.id}/{sitelink1.id} between {sitelink0.local_site} and {sitelink0.remote_site}: first element of pair has status({sitelink0.status}) state({sitelink0.state})");

                        // For now, just use sitelink0 as "the" sitelink. The problem with this is, the order of sitelinks
                        // in a pair is probably not deterministic, so they may swap between refreshes.
                        // TODO: Deal with this somehow, eg. sitelink markers have SitelinkPairs attached, not just a single sitelink.

                        if (siteMarkers.ContainsKey(sitelink0.local_site) && siteMarkers.ContainsKey(sitelink0.remote_site)) {
                            if (earthSphere.activeSelf) {
                                currentSitelinkMarkers.Add(earthSphere.GetComponent<GlobeSiteCreation>().placeSitelinkMarker(sitelinkPair, currentSiteMarkers));
                            } else {
                                currentSitelinkMarkers.Add(flatMap.GetComponent<FlatSiteCreation>().placeSitelinkMarker(sitelinkPair, currentSiteMarkers));
                            }
                        } else {
                            Debug.LogWarning($"Sitelink between {sitelink0.local_site} and {sitelink0.remote_site} can't be drawn because one or both sitemarkers are missing");
                        }
                    } else {
                        Debug.LogError("A sitelink pair is invalid!");
                    }
                }
            })
            .Then(() => {
                _wanManager.UpdateWans();
            })
            .Catch(err => Debug.LogError($"Error updating sites/sitelinks: {err.Message}\n{err.StackTrace}"));
    }

    void SetLaserColorForMode(StateManagerMode mode) {
        Color newColor;
  
        switch (mode) {
            case StateManagerMode.Normal:
                newColor = Color.white;
                break;

            case StateManagerMode.Delete:
                newColor = Color.red;
                break;

            case StateManagerMode.CreateSite:
                newColor = Color.green;
                break;

            default:
                newColor = Color.magenta;
                break;
        }

        laser.GetComponent<GvrLaserVisual>().laserColor = newColor;
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = newColor;
    }

    public void ShowConfirm() {
        confirm.SetActive(true);
    }

    public void CloseConfirm() {
        confirm.SetActive(false);
    }

    public void SetDeleteConfirmText(GameObject gameObjectToDelete, string element) {
        _tempObject = gameObjectToDelete;
        TextMesh message = confirm.GetComponentInChildren<TextMesh>();
        message.text = $"Delete {element}?";
    }

    private void DeleteSite(GameObject gameObjectSite) {
        SiteMarker siteMarker = gameObjectSite.GetComponent<SiteMarker>();
        Destroyer destroyer = destroyerObject.GetComponent<Destroyer>();
        siteMarker.DeleteSite(destroyer);
    }

    private void DeleteUplink(GameObject gameObjectUplink) {
        UplinkMarker uplinkMarker = gameObjectUplink.GetComponent<UplinkMarker>();
        uplinkMarker.DeleteUplink();
    }

    public void DeleteGameObject() {
        if (_tempObject) {
            confirm.SetActive(false);
            if (_tempObject.tag == "Site") {
                DeleteSite(_tempObject);
            } else if (_tempObject.tag == "Uplink") {
                DeleteUplink(_tempObject);
            } else if (_tempObject.tag == "WAN") {
                Debug.LogWarning("Delete WAN feature not implemented yet");
            } else {
                Debug.LogWarning($"Delete feature for {_tempObject} not implemented");
            }
            _tempObject = null;
        } else {
            Debug.LogError("No object to delete");
        }
    }

    public void ChangeMap()
    {
        //currentSiteMarkers = new Dictionary<string, SiteMarker>();
        //currentLineMarkers = new List<LineMarker>();

        earthSphere.SetActive(!earthSphere.activeSelf);
        flatMap.SetActive(!flatMap.activeSelf);

        UpdateSites(false);
    }
    public void DisplayInformation(string entityInformationText) {
        _informationTextMesh.text = entityInformationText;
        informationText.transform.parent.parent.gameObject.SetActive(true);
    }

    public void HideInformation() {
        informationText.transform.parent.parent.gameObject.SetActive(false);
    }
}
