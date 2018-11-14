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

    public SiteMarker placeSiteMarker(Site site, LatLong latLong) {
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

        //currentSiteMarkers.Add(site.id, newSiteMarker);
        //currentSiteMarkerObjects[site.id] = newSiteMarkerObject;

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