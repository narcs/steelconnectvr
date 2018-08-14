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

    // The prefab that represents a line connecting sites.
    public GameObject lineMarkerPrefab;

    private float globeRadius;

    private Dictionary<SiteID, SiteMarker> currentSiteMarkers;
    private List<LineMarker> currentLineMarkers;

    private SteelConnect steelConnect;

    void Start() {
        currentSiteMarkers = new Dictionary<string, SiteMarker>();
        currentLineMarkers = new List<LineMarker>();

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

        foreach (LineMarker lineMarker in currentLineMarkers) {
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
                Dictionary<SiteID, SiteMarker> siteMarkers = new Dictionary<string, SiteMarker>();

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

        // Connect random pairs of sites, just for something to show.
        var arbitraryLineMarkersPromise = siteMarkersPromise
            .Then(siteMarkers => {
                List<SiteMarker> siteMarkersList = new List<SiteMarker>(siteMarkers.Values);
                List<SiteMarker> shuffledSiteMarkers = siteMarkersList.OrderBy(a => Random.value).ToList();

                for (int i = 0; i < shuffledSiteMarkers.Count; i += 2) {
                    SiteMarker site1 = shuffledSiteMarkers[i];
                    SiteMarker site2 = shuffledSiteMarkers[i + 1];

                    drawLineBetweenSites(site1, site2, Random.ColorHSV(0f,1f, 1f,1f, 1f,1f));
                }
            })
            .Catch(err => Debug.LogError($"Error updating sites/sitelinks: {err.Message}"));
    }

    SiteMarker placeSiteMarker(Site site, LatLong latLong) {
        Vector3 sitePosition = LatLongUtility.LatLongToCartesian(latLong, globeRadius);

        // We want the site marker's up to face outwards. If we just use Quaternion.LookRotation
        // directly and just specify the forward vector, the marker's forward will face outwards.
        // So we need to rotate it so its up faces forward first.
        Quaternion upToForward = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        Quaternion siteOrientation = Quaternion.LookRotation(sitePosition.normalized) * upToForward;

        Debug.Log($"Site {site.id} is at {sitePosition}");

        GameObject newSiteMarkerObject = Instantiate(siteMarkerPrefab, sitePosition, siteOrientation, this.transform);
        SiteMarker newSiteMarker = newSiteMarkerObject.GetComponent<SiteMarker>();
        newSiteMarker.Site = site;

        currentSiteMarkers.Add(site.id, newSiteMarker);

        return newSiteMarker;
    }

    LineMarker drawLineBetweenSites(SiteMarker site1, SiteMarker site2, Color color) {
        Debug.Log($"Drawing line between {site1.Site.name} and {site2.Site.name}");

        GameObject lineMarkerObject = Instantiate(lineMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
        LineMarker lineMarker = lineMarkerObject.GetComponent<LineMarker>();

        lineMarker.StartPosition = site1.transform.position;
        lineMarker.EndPosition = site2.transform.position;
        lineMarker.SpherePosition = transform.position;
        lineMarker.SphereRadius = globeRadius * 1.03f;
        lineMarker.Color = color;
        lineMarker.NumPoints = 32; // TODO: Calculate based on sphere surface distance.

        lineMarker.Redraw();

        currentLineMarkers.Add(lineMarker);

        return lineMarker;
    }
}