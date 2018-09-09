using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gvr.Internal;

public class StateManager : MonoBehaviour {

    public GameObject laser;
    public GameObject confirm;

    private GameObject _tempObject;

    public bool deleteMode = false;


	// Use this for initialization
	void Start () {
        confirm.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("s")) {
            GetComponent<WanManager>().UpdateWans();
        }
		
	}

    public void EnableDeleteMode() {
        // Change to red
        laser.GetComponent<GvrLaserVisual>().laserColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        deleteMode = true;
    }

    public void DisableDeleteMode() {
        // Change back to white
        laser.GetComponent<GvrLaserVisual>().laserColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        laser.GetComponent<GvrLaserVisual>().laserColorEnd = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        deleteMode = false;
    }

    public void ShowConfirm() {
        confirm.SetActive(true);
    }

    public void CloseConfirm() {
        confirm.SetActive(false);
    }

    public void SetDeleteConfirmText(GameObject gameObject, string element) {
        _tempObject = gameObject;
        TextMesh message = confirm.GetComponentInChildren<TextMesh>();
        message.text = $"Delete {element}?";
    }

    public void DeleteGameObject() {
        if (_tempObject) {
            confirm.SetActive(false);
            Destroy(_tempObject);
            // Delete API here
            Debug.Log($"Deleted: {_tempObject}");
            _tempObject = null;
        } else {
            Debug.LogError("No object to delete");
        }
    }
}
