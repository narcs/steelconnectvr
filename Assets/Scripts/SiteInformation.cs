using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;
public class SiteInformation : MonoBehaviour {
    private GameObject _siteObject;
    private Camera _camera;

	// Use this for initialization
	void Start () {
        _siteObject = transform.parent.gameObject;
        SiteMarker siteMarker = _siteObject.GetComponent<SiteMarker>();
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

        _camera = Camera.main;
	}
	
	void Update () {
        // Billboard the text
        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
	}
}
