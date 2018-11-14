using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gvr.Internal;

public enum StateManagerMode {
    Normal,
    Delete,
    CreateSite,
}

// These are just to make it clearer what each function or type expects.
// These aliases are all just equivalent to `string`, and so you can pass strings with no problems.
using OrgId = System.String;
using SiteId = System.String;
using WanId = System.String;
using UplinkId = System.String;
using SitelinkId = System.String;

public class StateManager : MonoBehaviour {
    public GameObject laser;

    public GameObject confirm;
    public GameObject createSiteWindow;

    public GameObject currentObjectHover;
    public GameObject earthSphere;
    public GameObject destroyerObject;
    public GameObject flatMap;

    public GameObject destroyerObject;

	public GvrKeyboard keyboardManager;

    public StateManagerMode currentMode = StateManagerMode.Normal;

    public GameObject informationText;

    private GameObject _tempObject;
    private TextMesh _informationTextMesh;

    private SteelConnectDataManager _dataManager;

    // Site marker code
    public Dictionary<SiteId, GameObject> currentSiteMarkerObjects = new Dictionary<SiteId, GameObject>();
    public Dictionary<SiteId, SiteMarker> currentSiteMarkers;
    private List<LineMarker> currentLineMarkers;

    // Use this for initialization
    void Start () {
        currentSiteMarkers = new Dictionary<SiteId, SiteMarker>();
        currentLineMarkers = new List<LineMarker>();

        _dataManager = GameObject.Find("State Manager").GetComponent<SteelConnectDataManager>();

        confirm.SetActive(false);
        createSiteWindow.SetActive(false);
        _informationTextMesh = informationText.GetComponent<TextMesh>();
        informationText.transform.parent.parent.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
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

        // ---
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

    public void UpdateSites(bool forceRefresh)
    {
        foreach (var entry in currentSiteMarkers)
        {
            if (entry.Value) {
                Destroy(entry.Value.gameObject);
            }
        }
        currentSiteMarkers.Clear();
        currentSiteMarkerObjects.Clear();


        var siteMarkersPromise = _dataManager.GetSites(forceRefresh)
            .Then(sites => {
                foreach (Site site in sites) {
                    if (site.coordinates.isValid)
                    {
                        if (earthSphere.activeSelf)
                        {
                            currentSiteMarkers.Add(site.id, earthSphere.GetComponent<GlobeSiteCreation>().placeSiteMarker(site, site.coordinates));
                        }
                        else
                        {
                            currentSiteMarkers.Add(site.id, flatMap.GetComponent<FlatSiteCreation>().placeSiteMarker(site, site.coordinates));
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Coordinates for site {site.id} are not valid, not adding site marker");
                    }
                }


                return currentSiteMarkers;
            });

        var sitelinkMarkersPromise = PromiseHelpers.All(siteMarkersPromise, _dataManager.GetSitelinks(forceRefresh))
            .Then(tup => {
                Dictionary<SiteId, SiteMarker> siteMarkers = tup.Item1;
                Dictionary<SiteId, List<SitelinkReporting>> sitelinks = tup.Item2;

                Debug.Log($"Adding sitelink markers, there are {siteMarkers.Count} site markers");

                // NOTE(andrew): Iterating through the sitemarkers dictionary means that if a site didn't
                // have a sitemarker, that site will be skipped. The same check is done later for the
                // remote site (because we get sitelinks for all sites regardless of if they hasd a sitemarker
                // created).
                foreach (var siteMarkerEntry in siteMarkers)
                {
                    SiteId siteId = siteMarkerEntry.Key;
                    SiteMarker siteMarker = siteMarkerEntry.Value;

                    foreach (SitelinkReporting sitelink in sitelinks[siteId])
                    {
                        Debug.Log($"Sitelink {sitelink.id} between {siteId} and {sitelink.remote_site}: status({sitelink.status}) state({sitelink.state})");

                        if (siteMarkers.ContainsKey(sitelink.remote_site))
                        {
                            Color lineColor;
                            float blinkPeriodSeconds;

                            if (sitelink.state == "up")
                            {
                                lineColor = Color.green;
                                blinkPeriodSeconds = 0.0f;
                            }
                            else
                            {
                                lineColor = Color.red;
                                blinkPeriodSeconds = 2.0f;
                            }

                            if (earthSphere.activeSelf)
                            {
                                currentLineMarkers.Add(earthSphere.GetComponent<GlobeSiteCreation>().drawLineBetweenSites(siteMarker, siteMarkers[sitelink.remote_site], lineColor, blinkPeriodSeconds));
                            }
                            else
                            {
                                currentLineMarkers.Add(flatMap.GetComponent<FlatSiteCreation>().drawLineBetweenSites(siteMarker, siteMarkers[sitelink.remote_site], lineColor, blinkPeriodSeconds));
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Remote site {sitelink.remote_site} has no site marker, not drawing sitelink");
                        }
                    }
                }
            })
            .Catch(err => Debug.LogError($"Error updating sites/sitelinks: {err.Message}"));
    }

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
            } else {
                Destroy(_tempObject);
            }
            // Delete API here
            Debug.Log($"Deleted: {_tempObject}");
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
