using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Proyecto26;

public class CreateSiteWindow : MonoBehaviour {
    public Text SiteNameText;
    public Text SiteCountryText;
    public Text SiteCityText;

    public Text StatusText;

    private SteelConnect _steelConnect;

    private bool _createdSite = false;

    // ---

	// Use this for initialization
	void Start () {
        _steelConnect = new SteelConnect();

        ResetWindow();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // ---

    void ResetWindow() {
        SiteNameText.text = string.Empty;
        SiteCountryText.text = string.Empty;
        SiteCityText.text = string.Empty;

        ResetStatusText();
    }

    void ResetStatusText() {
        StatusText.text = "Enter site details then press \"Create site\"";
        StatusText.color = Color.white;
    }

    public void OnEnterCreateSiteMode() {
        
    }

    public void OnLeaveCreateSiteMode() {
        if (_createdSite) {
            ResetWindow();
            _createdSite = false;
        } else {
            ResetStatusText();
        }
    }

    public void OnCreateSitePressed() {
        Debug.Log("Create site button pressed!");

        string siteName = SiteNameText.text;
        string siteCountry = SiteCountryText.text;
        string siteCity = SiteCityText.text;

        _steelConnect.CreateSite(siteName, siteName, siteCity, siteCountry)
            .Then(response => {
                StatusText.text = "Site created! Don't forget to press \"Update Sites\"";
                StatusText.color = Color.green;
                _createdSite = true;
            })
            .Catch(err => {
                StatusText.text = "There was a problem creating the site, see the log window to your right";
                StatusText.color = Color.red;
            });
    }
}
