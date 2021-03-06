﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

using RSG;
using Proyecto26;
using Models.SteelConnect;

// Type alias for clarity.
using SiteId = System.String;

public class FlatSiteCreation : MonoBehaviour {

    public GameObject siteMarkerPrefab;
    public GameObject sitelinkMarkerPrefab;

    public AbstractMap map;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public SiteMarker placeSiteMarker(Site site, LatLong latLong)
    {
        Debug.Log($"FLAT MAP: Site {site.name} is at {latLong}");
        Vector3 sitePosition = map.GeoToWorldPosition(new Vector2d(latLong.latitude, latLong.longitude), false);
        //Vector3 sitePosition = new Vector3((float)sPosition.x, (float)sPosition.y, 0);
        Quaternion siteOrientation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        Debug.Log($"FLAT MAP: Site {site.id} is at world position {sitePosition}");

        GameObject newSiteMarkerObject = Instantiate(siteMarkerPrefab, sitePosition, siteOrientation, this.transform);
        SiteMarker newSiteMarker = newSiteMarkerObject.GetComponent<SiteMarker>();
        Vector3 translate = new Vector3(0.0f, this.transform.position.y);
        //newSiteMarker.transform.Translate(translate);

        newSiteMarker.transform.Rotate(this.transform.eulerAngles);

        newSiteMarker.site = site;

        return newSiteMarker;
    }

    public SitelinkMarker placeSitelinkMarker(SitelinkPair sitelinkPair, Dictionary<SiteId, SiteMarker> siteMarkers) {
        SitelinkReporting sitelink0 = sitelinkPair.pair[0];

        SiteMarker fromSite = siteMarkers[sitelink0.local_site];
        SiteMarker toSite = siteMarkers[sitelink0.remote_site];
        Debug.Log($"Drawing sitelink from {fromSite.site.name} to {toSite.site.name}");

        GameObject sitelinkMarkerObject = Instantiate(sitelinkMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
        SitelinkMarker sitelinkMarker = sitelinkMarkerObject.GetComponent<SitelinkMarker>();
        sitelinkMarker.Set(fromSite, toSite, sitelinkPair, transform.position);

        return sitelinkMarker;
    }
}