using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;
public class SiteInformation : MonoBehaviour {
    private GameObject siteObject;

	// Use this for initialization
	void Start () {
        siteObject = transform.parent.gameObject;
        SiteMarker siteMarker = siteObject.GetComponent<SiteMarker>();
        Site site = siteMarker.Site;
        string text = $"Id: {site.id}\n" +
                      $"Name: {site.name}\n" +
                      $"Longname: {site.longname}\n" +
                      $"Org: {site.org}\n" +
                      $"Country: {site.country}\n" +
                      $"City: {site.city}\n" +
                      $"Street Address: {site.street_address}";
        TextMesh textMesh = GetComponent<TextMesh>();
        textMesh.text = text;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
