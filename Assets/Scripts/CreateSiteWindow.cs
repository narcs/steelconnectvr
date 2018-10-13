using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateSiteWindow : MonoBehaviour {
    public Text SiteNameText;
    public Text SiteCountryText;
    public Text SiteCityText;

    public Text StatusText;

    // ---

	// Use this for initialization
	void Start () {
        StatusText.text = "";
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // ---

    public void ClearTextFields() {
        SiteNameText.text = string.Empty;
        SiteCountryText.text = string.Empty;
        SiteCityText.text = string.Empty;
    }

    public void OnCreateSitePressed() {
        string siteName = SiteNameText.text;
        string siteCountry = SiteCountryText.text;
        string siteCityText = SiteCityText.text;



        ClearTextFields();
    }
}
