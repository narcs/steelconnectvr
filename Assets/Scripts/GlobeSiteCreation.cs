using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using RSG;
using Proyecto26;
using Models.SteelConnect;

// TODO: Figure out how to refresh sitelinks without regetting all sites.

// Type alias for clarity.
using SiteID = System.String;

public class GlobeSiteCreation : MonoBehaviour {
    // The prefab that represents a site. Its up will be oriented facing 
    // away from the center of the globe.
    public GameObject SiteMarkerPrefab;

    // The prefab that represents a sitelink connecting sites.
    public GameObject SitelinkMarkerPrefab;

    private SteelConnect steelConnect;
    private float globeRadius;

    // -- Current state. --
    private Dictionary<SiteID, SiteMarker> currentSiteMarkers;
    private Dictionary<SiteID, SitelinkMarker> currentSitelinkMarkers;


    void Start() {
        currentSiteMarkers = new Dictionary<SiteID, SiteMarker>();
        currentSitelinkMarkers = new Dictionary<SiteID, SitelinkMarker>();

        steelConnect = new SteelConnect();
        updateGlobeRadius();
    }

    void updateGlobeRadius() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        globeRadius = (mesh.bounds.size.x / 2.0f) * transform.localScale.x;
    }

    public void UpdateSites() {
        foreach (var entry in currentSiteMarkers) {
            Destroy(entry.Value.gameObject);
        }
        currentSiteMarkers.Clear();

        foreach (var entry in currentSitelinkMarkers) {
            Destroy(entry.Value.gameObject);
        }
        currentSitelinkMarkers.Clear();

        // ---

        // What follows is a whole lot of promise-based async code.
        // TODO: I promise I'll explain what each promise is for.

        // Gets the list of sites from the SteelConnect API and returns it as an array of Sites.
        var sitesPromise = steelConnect.GetSitesInOrg()
            .Then(sites => { Debug.Log($"Got {sites.items.Count()} sites"); return sites.items; });

        // Gets the LatLong for each site.
        var latLongPromise = sitesPromise
            .ThenAll(sites => sites.Select(site => LatLongUtility.GetLatLongForAddress(site.street_address, site.city, site.country)));

        // Info on PromiseHelpers.All: https://github.com/Real-Serious-Games/C-Sharp-Promise/issues/33
        // Combines sites and their LatLongs to place the sitemarkers.
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

        // Gets sitelinks for each site.
        var sitelinksPromise = sitesPromise
            .ThenAll(sites => sites.Select(site => steelConnect.GetSitelinks(site.id)))
            .Then(sitelinksList => sitelinksList.Select(sitelinks => sitelinks.items));

        // Combines sites and sitelinks to create a dict mapping site IDs to sitelink arrays.
        var sitelinksDictPromise = PromiseHelpers.All(sitesPromise, sitelinksPromise)
            .Then(tup => {
                Site[] sites = tup.Item1;
                IEnumerable<Sitelink[]> sitelinks = tup.Item2;
                Dictionary<SiteID, Sitelink[]> sitelinksDict = new Dictionary<SiteID, Sitelink[]>();

                for (int i = 0; i < sites.Count(); ++i) {
                    sitelinksDict.Add(sites[i].id, sitelinks.ElementAt(i));
                }

                return sitelinksDict;
            });

        // Connects sites by sitelinks.
        var sitelinkMarkersPromise = PromiseHelpers.All(siteMarkersPromise, sitelinksDictPromise)
            .Then(tup => {
                Dictionary<SiteID, SiteMarker> siteMarkers = tup.Item1;
                Dictionary<SiteID, Sitelink[]> sitelinks = tup.Item2;

                // NOTE: Iterating through the sitemarkers dictionary means that if a site didn't
                // have a sitemarker, that site will be skipped. The same check is done later for the
                // remote site (because we get sitelinks for all sites regardless of if they hasd a sitemarker
                // created).
                foreach (var siteMarkerEntry in siteMarkers) {
                    SiteID siteId = siteMarkerEntry.Key;
                    SiteMarker siteMarker = siteMarkerEntry.Value;

                    foreach (Sitelink sitelink in sitelinks[siteId]) {
                        Debug.Log($"Sitelink {sitelink.id} between {siteId} and {sitelink.remote_site}: status({sitelink.status}) state({sitelink.state}) throughput_in({sitelink.throughput_in}) throughput_out({sitelink.throughput_out})");

                        if (siteMarkers.ContainsKey(sitelink.remote_site)) {
                            placeSitelinkMarker(siteId, sitelink);
                        } else {
                            Debug.LogWarning($"Remote site {sitelink.remote_site} has no site marker, not drawing sitelink");
                        }
                    }
                }
            })
            .Catch(err => Debug.LogError($"Error updating sites/sitelinks: {err.Message}"));
    }

    public void UpdateReportingData() {

    }

    SiteMarker placeSiteMarker(Site site, LatLong latLong) {
        Vector3 sitePosition = LatLongUtility.LatLongToCartesian(latLong, globeRadius);

        // We want the site marker's up to face outwards. If we just use Quaternion.LookRotation
        // directly and just specify the forward vector, the marker's forward will face outwards.
        // So we need to rotate it so its up faces forward first.
        Quaternion upToForward = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        Quaternion siteOrientation = Quaternion.LookRotation(sitePosition.normalized) * upToForward;

        Debug.Log($"Site {site.id} is at world position {sitePosition}");

        GameObject newSiteMarkerObject = Instantiate(SiteMarkerPrefab, sitePosition, siteOrientation, this.transform);
        SiteMarker newSiteMarker = newSiteMarkerObject.GetComponent<SiteMarker>();
        newSiteMarker.Site = site;

        currentSiteMarkers.Add(site.id, newSiteMarker);

        return newSiteMarker;
    }

    SitelinkMarker placeSitelinkMarker(SiteID siteId, Sitelink sitelink) {
        SiteMarker fromSite = currentSiteMarkers[siteId];
        SiteMarker toSite = currentSiteMarkers[sitelink.remote_site];
        Debug.Log($"Drawing sitelink from {fromSite.Site.name} to {toSite.Site.name}");

        GameObject sitelinkMarkerObject = Instantiate(SitelinkMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
        SitelinkMarker sitelinkMarker = sitelinkMarkerObject.GetComponent<SitelinkMarker>();
        sitelinkMarker.Set(fromSite, toSite, sitelink, transform.position, globeRadius * 1.03f);
        currentSitelinkMarkers.Add(siteId, sitelinkMarker);

        return sitelinkMarker;
    }
}
