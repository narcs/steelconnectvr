using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gvr.Internal;

using RSG;
using Proyecto26;
using Models.SteelConnect;

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
    public bool deleteMode = false;
    public GameObject currentObjectHover;
    public GameObject earthSphere;
    public GameObject destroyerObject;
    public GameObject flatMap;

    private GameObject _tempObject;

    private SteelConnectDataManager _dataManager;

    // Site marker code
    public Dictionary<SiteId, GameObject> currentSiteMarkerObjects = new Dictionary<SiteId, GameObject>();
    private Dictionary<SiteId, SiteMarker> currentSiteMarkers;
    private List<LineMarker> currentLineMarkers;

    // Use this for initialization
    void Start () {
        currentSiteMarkers = new Dictionary<SiteId, SiteMarker>();
        currentLineMarkers = new List<LineMarker>();

        _dataManager = GameObject.Find("State Manager").GetComponent<SteelConnectDataManager>();

        confirm.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("s")) {
            GetComponent<WanManager>().UpdateWans();
        } else if (Input.GetKeyDown("a")) {
            UpdateSites(true);
        }
	}

    // Update Sites
    public void UpdateSitesForceRefresh() {
        UpdateSites(true);
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

        foreach (LineMarker lineMarker in currentLineMarkers)
        {
            Destroy(lineMarker.gameObject);
        }
        currentLineMarkers.Clear();

        // ---

        var siteMarkersPromise = _dataManager.GetSites(forceRefresh)
            .Then(sites => {
                Dictionary<SiteId, SiteMarker> siteMarkers = new Dictionary<SiteId, SiteMarker>();

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

                return siteMarkers;
            });

        var sitelinkMarkersPromise = PromiseHelpers.All(siteMarkersPromise, _dataManager.GetSitelinks(forceRefresh))
            .Then(tup => {
                Dictionary<SiteId, SiteMarker> siteMarkers = tup.Item1;
                Dictionary<SiteId, List<SitelinkReporting>> sitelinks = tup.Item2;

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

    public void EnableDeleteMode() {
        // Change to red
        laser.GetComponent<GvrLaserVisual>().laserColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        deleteMode = true;
    }

    public void DisableDeleteMode() {
        // Change back to white
        laser.GetComponent<GvrLaserVisual>().laserColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        deleteMode = false;
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
        destroyer.StartDestruction(gameObjectSite);
        //SiteMarker siteMarker = gameObjectSite.GetComponent<SiteMarker>();
        //ParticleSystem particleSystem = siteMarker.explosion.GetComponent<ParticleSystem>();
        //particleSystem.Play();
        //siteMarker.model.SetActive(false);
        //Destroy(_tempObject, particleSystem.main.duration);
        //Debug.Log("Site deletion");
    }

    public void DeleteGameObject() {
        if (_tempObject) {
            confirm.SetActive(false);
            if (_tempObject.tag == "Site") {
                DeleteSite(_tempObject);
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
}
