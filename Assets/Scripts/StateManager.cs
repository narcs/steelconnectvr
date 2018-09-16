using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gvr.Internal;

public class StateManager : MonoBehaviour {

    public GameObject laser;
    public GameObject confirm;

    private GameObject _tempObject;

    public enum Mode {
        // Default mode where the user just can spin the globe.
        Normal,

        DeleteSite,
        CreateSite,
    }

    public Mode currentMode;

	// Use this for initialization
	void Start () {
        CloseConfirm();
        SwitchToNormalMode();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("s")) {
            GetComponent<WanManager>().UpdateWans();
        }
	}

    public void SwitchToNormalMode() {
        currentMode = Mode.Normal;
        SetLaserColorForMode(currentMode);
    }

    public void SwitchToDeleteSiteMode() {
        currentMode = Mode.DeleteSite;
        SetLaserColorForMode(currentMode);
    }

    public void SwitchToCreateSiteMode() {
        currentMode = Mode.CreateSite;
        SetLaserColorForMode(currentMode);
    }

    void SetLaserColorForMode(Mode mode) {
        Color color;

        switch (mode) {
            case Mode.Normal:
                color = Color.white;
                break;
            case Mode.DeleteSite:
                color = Color.red;
                break;
            case Mode.CreateSite:
                color = Color.green;
                break;
            default:
                color = Color.white;
                break;
        }

        laser.GetComponent<GvrLaserVisual>().laserColor = color;
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = color;

        Debug.Log($"Set laser color for {mode.ToString()}");
    }

    public void ShowConfirm() {
        confirm.SetActive(true);
    }

    public void CloseConfirm() {
        confirm.SetActive(false);
    }

    public void SetConfirmText(string text) {
        TextMesh message = confirm.GetComponentInChildren<TextMesh>();
        message.text = text;
    }

    public void SetGameObjectToDelete(GameObject gameObject, string element) {
        _tempObject = gameObject;
        SetConfirmText($"Delete {element}?");
        ShowConfirm();
    }

    public void DeleteGameObject() {
        if (_tempObject) {
            CloseConfirm();
            Destroy(_tempObject);

            // Delete API here

            Debug.Log($"Deleted: {_tempObject}");
            _tempObject = null;
        } else {
            Debug.LogError("No object to delete");
        }
    }
}
