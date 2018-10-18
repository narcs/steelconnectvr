using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gvr.Internal;

public enum StateManagerMode {
    Normal,
    Delete,
    CreateSite,
}

public class StateManager : MonoBehaviour {

    public GameObject laser;

    public GameObject confirm;
    public GameObject createSiteWindow;

    public GameObject currentObjectHover;
    public GameObject earthSphere;

    public GameObject destroyerObject;

	public GvrKeyboard keyboardManager;

    public StateManagerMode currentMode = StateManagerMode.Normal;

    public GameObject informationText;

    private GameObject _tempObject;
    private TextMesh _informationTextMesh;


    // Use this for initialization
    void Start () {
        confirm.SetActive(false);
        createSiteWindow.SetActive(false);
        _informationTextMesh = informationText.GetComponent<TextMesh>();
        informationText.transform.parent.parent.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown("s")) {
        //    GetComponent<WanManager>().UpdateWans();
        //} else if (Input.GetKeyDown("a")) {
        //    earthSphere.GetComponent<GlobeSiteCreation>().UpdateSites();
        //}
	}

    public void SwitchToDeleteMode() {
        currentMode = StateManagerMode.Delete;
        SetLaserColorForMode(currentMode);
    }

    public void SwitchToCreateSiteMode() {
        currentMode = StateManagerMode.CreateSite;
        SetLaserColorForMode(currentMode);

        earthSphere.GetComponent<SphereInteraction>().globeDragEnabled = false;

        createSiteWindow.GetComponent<CreateSiteWindow>().OnEnterCreateSiteMode();
        createSiteWindow.SetActive(true);
    }

    public void SwitchToNormalMode() {
        currentMode = StateManagerMode.Normal;
        SetLaserColorForMode(currentMode);

        earthSphere.GetComponent<SphereInteraction>().globeDragEnabled = true;

        keyboardManager.Hide();
        createSiteWindow.SetActive(false);
        createSiteWindow.GetComponent<CreateSiteWindow>().OnLeaveCreateSiteMode();
    }

    void SetLaserColorForMode(StateManagerMode mode) {
        Color newColor;
  
        switch (mode) {
            case StateManagerMode.Normal:
                newColor = Color.white;
                break;

            case StateManagerMode.Delete:
                newColor = Color.red;
                break;

            case StateManagerMode.CreateSite:
                newColor = Color.green;
                break;

            default:
                newColor = Color.magenta;
                break;
        }

        laser.GetComponent<GvrLaserVisual>().laserColor = newColor;
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = newColor;
    }

    public void ShowConfirm() {
        confirm.SetActive(true);
    }

    public void CloseConfirm() {
        confirm.SetActive(false);
    }

    public void SetDeleteConfirmText(GameObject gameObjectToDelete, string element) {
        _tempObject = gameObjectToDelete;
        TextMesh message = confirm.GetComponentInChildren<TextMesh>();
        message.text = $"Delete {element}?";
    }

    private void DeleteSite(GameObject gameObjectSite) {
        SiteMarker siteMarker = gameObjectSite.GetComponent<SiteMarker>();
        Destroyer destroyer = destroyerObject.GetComponent<Destroyer>();
        siteMarker.DeleteSite(destroyer);
    }

    private void DeleteUplink(GameObject gameObjectUplink) {
        UplinkMarker uplinkMarker = gameObjectUplink.GetComponent<UplinkMarker>();
        uplinkMarker.DeleteUplink();
    }

    public void DeleteGameObject() {
        if (_tempObject) {
            confirm.SetActive(false);
            if (_tempObject.tag == "Site") {
                DeleteSite(_tempObject);
            } else if (_tempObject.tag == "Uplink") {
                DeleteUplink(_tempObject);
            } else {
                Destroy(_tempObject);
            }
            // Delete API here
            Debug.Log($"Deleted: {_tempObject}");
            _tempObject = null;
        } else {
            Debug.LogError("No object to delete");
        }
    }

    public void DisplayInformation(string entityInformationText) {
        _informationTextMesh.text = entityInformationText;
        informationText.transform.parent.parent.gameObject.SetActive(true);
    }

    public void HideInformation() {
        informationText.transform.parent.parent.gameObject.SetActive(false);
    }
}
