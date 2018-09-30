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
    public Dictionary<SiteID, GameObject> currentSiteMarkerObjects = new Dictionary<SiteID, GameObject>();

    private float globeRadius;

    private SteelConnect steelConnect;

    void Start() {
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

    public LineMarker drawLineBetweenSites(SiteMarker site1, SiteMarker site2, Color color, float blinkPeriodSeconds) {
        Debug.Log($"Drawing line between {site1.site.name} and {site2.site.name}");

        GameObject lineMarkerObject = Instantiate(lineMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
        LineMarker lineMarker = lineMarkerObject.GetComponent<LineMarker>();

        lineMarker.StartSiteMarker = site1;
        lineMarker.EndSiteMarker = site2;
        lineMarker.SpherePosition = transform.position;
        lineMarker.SphereRadius = globeRadius * 1.03f;
        lineMarker.Color = color;
        lineMarker.BlinkPeriodSeconds = blinkPeriodSeconds;
        lineMarker.NumPoints = 32; // TODO: Calculate based on sphere surface distance.

        //currentLineMarkers.Add(lineMarker);

        return lineMarker;
    }
}