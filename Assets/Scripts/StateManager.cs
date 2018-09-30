using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gvr.Internal;

using RSG;
using Proyecto26;
using Models.SteelConnect;

// Type alias for clarity.
using SiteID = System.String;

public class StateManager : MonoBehaviour {

    public GameObject laser;
    public GameObject confirm;
    public bool deleteMode = false;
    public GameObject currentObjectHover;
    public GameObject earthSphere;
    public GameObject flatMap;

    private GameObject _tempObject;

    // Site marker code
    public Dictionary<SiteID, GameObject> currentSiteMarkerObjects = new Dictionary<SiteID, GameObject>();
    private Dictionary<SiteID, SiteMarker> currentSiteMarkers;
    private List<LineMarker> currentLineMarkers;

    private SteelConnect steelConnect;

    // Use this for initialization
    void Start () {
        currentSiteMarkers = new Dictionary<string, SiteMarker>();
        currentLineMarkers = new List<LineMarker>();

        steelConnect = new SteelConnect();

        confirm.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("s")) {
            GetComponent<WanManager>().UpdateWans();
        } else if (Input.GetKeyDown("a")) {
            UpdateSites();
        }
	}

    // Update Sites
    public void UpdateSites() {
        foreach (var entry in currentSiteMarkers)
        {
            if (entry.Value) Destroy(entry.Value.gameObject);
        }
        currentSiteMarkers.Clear();
        currentSiteMarkerObjects.Clear();

        foreach (LineMarker lineMarker in currentLineMarkers)
        {
            Destroy(lineMarker.gameObject);
        }
        currentLineMarkers.Clear();

        // ---

        // What follows is a whole lot of promise-based async code.
        // TODO: I promise I'll explain what each promise is for.

        // Gets the list of sites from the SteelConnect API and returns it as an array of Sites.
        var sitesPromise = steelConnect.GetSitesInOrg()
            .Then(sites => { Debug.Log($"Got {sites.items.Count()} sites"); return sites.items; });

        var latLongPromise = sitesPromise
            .ThenAll(sites => sites.Select(site => LatLongUtility.GetLatLongForAddress(site.street_address, site.city, site.country)));

        // Info on PromiseHelpers.All: https://github.com/Real-Serious-Games/C-Sharp-Promise/issues/33
        var siteMarkersPromise = PromiseHelpers.All(sitesPromise, latLongPromise)
            .Then(tup => {
                Site[] sites = tup.Item1;
                IEnumerable<LatLong> latLongs = tup.Item2;
                Dictionary<SiteID, SiteMarker> siteMarkers = new Dictionary<SiteID, SiteMarker>();

                for (int i = 0; i < sites.Count(); ++i)
                {
                    Site site = sites[i];
                    if (latLongs.ElementAt(i).isValid)
                    {
                        if (earthSphere.activeSelf)
                        {
                            siteMarkers.Add(site.id, earthSphere.GetComponent<GlobeSiteCreation>().placeSiteMarker(site, latLongs.ElementAt(i)));
                        }
                        else
                        {
                            siteMarkers.Add(site.id, flatMap.GetComponent<FlatSiteCreation>().placeSiteMarker(site, latLongs.ElementAt(i)));
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"LatLong for site {site.id} is not valid, not adding site marker");
                    }
                }

                return siteMarkers;
            });

        var sitelinksPromise = sitesPromise
            .ThenAll(sites => sites.Select(site => steelConnect.GetSitelinks(site.id)))
            .Then(sitelinksList => sitelinksList.Select(sitelinks => sitelinks.items));

        var sitelinksDictPromise = PromiseHelpers.All(sitesPromise, sitelinksPromise)
            .Then(tup => {
                Site[] sites = tup.Item1;
                IEnumerable<Sitelink[]> sitelinks = tup.Item2;
                Dictionary<SiteID, Sitelink[]> sitelinksDict = new Dictionary<SiteID, Sitelink[]>();

                for (int i = 0; i < sites.Count(); ++i)
                {
                    sitelinksDict.Add(sites[i].id, sitelinks.ElementAt(i));
                }

                return sitelinksDict;
            });

        // Connect random pairs of sites, just for something to show.
        var arbitraryLineMarkersPromise = PromiseHelpers.All(siteMarkersPromise, sitelinksDictPromise)
            .Then(tup => {
                Dictionary<SiteID, SiteMarker> siteMarkers = tup.Item1;
                Dictionary<SiteID, Sitelink[]> sitelinks = tup.Item2;

                // NOTE: Iterating through the sitemarkers dictionary means that if a site didn't
                // have a sitemarker, that site will be skipped. The same check is done later for the
                // remote site (because we get sitelinks for all sites regardless of if they hasd a sitemarker
                // created).
                foreach (var siteMarkerEntry in siteMarkers)
                {
                    SiteID siteId = siteMarkerEntry.Key;
                    SiteMarker siteMarker = siteMarkerEntry.Value;

                    foreach (Sitelink sitelink in sitelinks[siteId])
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
        SiteMarker siteMarker = gameObjectSite.GetComponent<SiteMarker>();
        ParticleSystem particleSystem = siteMarker.explosion.GetComponent<ParticleSystem>();
        particleSystem.Play();
        siteMarker.model.SetActive(false);
        Destroy(_tempObject, particleSystem.main.duration);
        Debug.Log("Site deletion");
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
        currentSiteMarkers = new Dictionary<string, SiteMarker>();
        currentLineMarkers = new List<LineMarker>();

        earthSphere.SetActive(!earthSphere.activeSelf);
        flatMap.SetActive(!flatMap.activeSelf);
    }
}
