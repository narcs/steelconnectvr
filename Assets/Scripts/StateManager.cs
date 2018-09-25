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

    private GameObject _tempObject;

    public StateManagerMode currentMode = StateManagerMode.Normal;

    // Use this for initialization
    void Start () {
        confirm.SetActive(false);
        createSiteWindow.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("s")) {
            GetComponent<WanManager>().UpdateWans();
        } else if (Input.GetKeyDown("a")) {
            earthSphere.GetComponent<GlobeSiteCreation>().UpdateSites();
        }
	}

    public void SwitchToDeleteMode() {
        currentMode = StateManagerMode.Delete;
        SetLaserColorForMode(currentMode);
    }

    public void SwitchToCreateSiteMode() {
        currentMode = StateManagerMode.CreateSite;
        SetLaserColorForMode(currentMode);
        earthSphere.GetComponent<SphereInteraction>().globeDragEnabled = false;
        createSiteWindow.SetActive(true);
    }

    public void SwitchToNormalMode() {
        currentMode = StateManagerMode.Normal;
        SetLaserColorForMode(currentMode);
        earthSphere.GetComponent<SphereInteraction>().globeDragEnabled = true;
        createSiteWindow.SetActive(false);
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
        ParticleSystem particleSystem = siteMarker.explosion.GetComponent<ParticleSystem>();
        particleSystem.Play();
        siteMarker.model.SetActive(false);
        Destroy(_tempObject, particleSystem.main.duration);
        Debug.Log("Site deletion");
    }

    public void DeleteGameObject() {
        if (_tempObject) {
            confirm.SetActive(false);
            if (_tempObject.tag == "Site") {
                DeleteSite(_tempObject);
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
}
