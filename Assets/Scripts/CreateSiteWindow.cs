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

    private bool createdSite = false;

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

    public void ResetWindow() {
        SiteNameText.text = string.Empty;
        SiteCountryText.text = string.Empty;
        SiteCityText.text = string.Empty;

        StatusText.text = "Enter site details then press \"Create site\"";
        StatusText.color = Color.white;
    }

    public void OnEnterCreateSiteMode() {
        
    }

    public void OnLeaveCreateSiteMode() {
        if (createdSite) {
            ResetWindow();
            createdSite = false;
        }
    }

    public void OnCreateSitePressed() {
        string siteName = SiteNameText.text;
        string siteCountry = SiteCountryText.text;
        string siteCity = SiteCityText.text;

        _steelConnect.CreateSite(siteName, siteName, siteCity, siteCountry)
            .Then((resp) => {
                StatusText.text = "Site created! Don't forget to press \"Update Sites\"";
                StatusText.color = Color.green;
            })
            .Catch((err) => {
                StatusText.text = "There was a problem creating the site, see the log window to your right";
                StatusText.color = Color.red;

                if (err is RequestException) {
                    RequestException reqErr = err as RequestException;

                    if (reqErr.StatusCode == 400) {
                        Debug.Log($"Site creation parameters were invalid: {reqErr.ServerMessage}");
                    }  else if (reqErr.StatusCode == 500) {
                        Debug.Log($"Failed to create site: {reqErr.Message}");
                    } else {
                        Debug.Log($"Request exception: {reqErr.StatusCode} {reqErr.Message}\n    {reqErr.ServerMessage}\n{reqErr.StackTrace}");
                    }
                } else {
                    Debug.Log($"Other exception: {err.Message}\n{err.StackTrace}");
                }
            });

        createdSite = true;
    }
}
