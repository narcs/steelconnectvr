using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using RSG;
using Proyecto26;
using Models.SteelConnect;

// Type alias for clarity.
using SiteID = System.String;

public class GlobeSiteCreation : MonoBehaviour {
    // The prefab that represents a site. Its up will be oriented facing 
    // away from the center of the globe.
    public GameObject siteMarkerPrefab;

    // The prefab that represents a sitelink.
    public GameObject sitelinkMarkerPrefab;
    public Dictionary<SiteID, GameObject> currentSiteMarkerObjects = new Dictionary<SiteID, GameObject>();

    private float globeRadius;

    private Dictionary<SiteID, SiteMarker> currentSiteMarkers;
    private List<SitelinkMarker> currentSitelinkMarkers;

    private SteelConnect steelConnect;

    void Start() {
        currentSiteMarkers = new Dictionary<string, SiteMarker>();
        currentSitelinkMarkers = new List<SitelinkMarker>();

        steelConnect = new SteelConnect();
        updateGlobeRadius();
    }

    void updateGlobeRadius() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        globeRadius = (mesh.bounds.size.x / 2.0f) * transform.localScale.x;
    }

    public void UpdateSites() {
        foreach (var entry in currentSiteMarkers) {
            if (entry.Value) Destroy(entry.Value.gameObject);
        }
        currentSiteMarkers.Clear();
        currentSiteMarkerObjects.Clear();

        foreach (SitelinkMarker sitelinkMarker in currentSitelinkMarkers) {
            Destroy(sitelinkMarker.gameObject);
        }
        currentSitelinkMarkers.Clear();

        // ---

        // What follows is a whole lot of promise-based async code.

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

                for (int i = 0; i < sites.Count(); ++i) {
                    Site site = sites[i];
                    if (latLongs.ElementAt(i).isValid) {
                        siteMarkers.Add(site.id, placeSiteMarker(site, latLongs.ElementAt(i)));
                    } else {
                        Debug.LogWarning($"LatLong for site {site.id} is not valid, not adding site marker");
                    }
                }

                return siteMarkers;
            });

        var sitelinksPairsPromise = sitesPromise
            .Then(sites => new List<Site>(sites))
            .Then(sites => steelConnect.GetSitelinkPairsForSites(sites));

        // Connect sites by sitelink pairs.
        PromiseHelpers.All(siteMarkersPromise, sitelinksPairsPromise)
            .Then(tup => {
                Dictionary<SiteID, SiteMarker> siteMarkers = tup.Item1;
                List<SitelinkPair> sitelinkPairs = tup.Item2;

                foreach (SitelinkPair sitelinkPair in sitelinkPairs) {
                    Debug.Log($"Sitelink pair has {sitelinkPair.pair.Count} sitelinks");
                    if (sitelinkPair.IsValid()) {
                        Sitelink sitelink0 = sitelinkPair.pair[0];
                        Sitelink sitelink1 = sitelinkPair.pair[1];

                        Debug.Log($"Sitelink pair {sitelink0.id}/{sitelink1.id} between {sitelink0.local_site} and {sitelink0.remote_site}: first element of pair has status({sitelink0.status}) state({sitelink0.state})");

                        // For now, just use sitelink0 as "the" sitelink. The problem with this is, the order of sitelinks
                        // in a pair is probably not deterministic, so they may swap between refreshes.
                        // TODO: Deal with this somehow, eg. sitelink markers have SitelinkPairs attached, not just a single sitelink.

                        if (siteMarkers.ContainsKey(sitelink0.local_site) && siteMarkers.ContainsKey(sitelink0.remote_site)) {
                            placeSitelinkMarker(sitelinkPair);
                        } else {
                            Debug.LogWarning($"Sitelink between {sitelink0.local_site} and {sitelink0.remote_site} can't be drawn because one or both sitemarkers are missing");
                        }
                    } else {
                        Debug.LogError("A sitelink pair is invalid!");
                    }
                }
            })
            .Catch(err => Debug.LogError($"Error updating sites/sitelinks: {err.Message}\n{err.StackTrace}"));
    }

    SiteMarker placeSiteMarker(Site site, LatLong latLong) {
        Vector3 sitePosition = LatLongUtility.LatLongToCartesian(latLong, globeRadius);

        // We want the site marker's up to face outwards. If we just use Quaternion.LookRotation
        // directly and just specify the forward vector, the marker's forward will face outwards.
        // So we need to rotate it so its up faces forward first.
        Quaternion upToForward = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        Quaternion siteOrientation = Quaternion.LookRotation(sitePosition.normalized) * upToForward;

        Debug.Log($"Site {site.id} is at world position {sitePosition}");

        GameObject newSiteMarkerObject = Instantiate(siteMarkerPrefab, sitePosition, siteOrientation, this.transform);
        SiteMarker newSiteMarker = newSiteMarkerObject.GetComponent<SiteMarker>();
        newSiteMarker.site = site;

        currentSiteMarkers.Add(site.id, newSiteMarker);
        currentSiteMarkerObjects[site.id] = newSiteMarkerObject;

        return newSiteMarker;
    }

    SitelinkMarker placeSitelinkMarker(SitelinkPair sitelinkPair) {
        Sitelink sitelink0 = sitelinkPair.pair[0];

        SiteMarker fromSite = currentSiteMarkers[sitelink0.local_site];
        SiteMarker toSite = currentSiteMarkers[sitelink0.remote_site];
        Debug.Log($"Drawing sitelink from {fromSite.site.name} to {toSite.site.name}");

        GameObject sitelinkMarkerObject = Instantiate(sitelinkMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
        SitelinkMarker sitelinkMarker = sitelinkMarkerObject.GetComponent<SitelinkMarker>();
        sitelinkMarker.Set(fromSite, toSite, sitelinkPair, transform.position, globeRadius * 1.03f);
        currentSitelinkMarkers.Add(sitelinkMarker);

        return sitelinkMarker;
    }
}