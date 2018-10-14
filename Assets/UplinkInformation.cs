using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;

public class UplinkInformation : MonoBehaviour {
    private GameObject _uplinkObject;
    private Camera _camera;

    void Start () {
        _uplinkObject = transform.parent.gameObject;
        UpdateInformation();
        _camera = Camera.main;
    }
	
    void Update () {
        // Billboard the text
        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
    }

    public void UpdateInformation() {
        UplinkMarker uplinkMarker = _uplinkObject.GetComponent<UplinkMarker>();
        Uplink uplink = uplinkMarker.uplink;
        string text = $"Id: {uplink.id}\n" +
                      $"Name: {uplink.name}\n" +
                      $"Org: {uplink.org}\n" +
                      $"Site: {uplink.site}\n" +
                      $"WAN: {uplink.wan}\n" +
                      $"Appliance: {uplink.node}";
        TextMesh textMesh = GetComponent<TextMesh>();
        textMesh.text = text;
    }
}
