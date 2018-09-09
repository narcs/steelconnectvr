using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.SteelConnect;
public class WanInformation : MonoBehaviour {
    private GameObject _wanObject;
    private Camera _camera;

	void Start () {
        _wanObject = transform.parent.gameObject;
        WanMarker wanMarker = _wanObject.GetComponent<WanMarker>();
        Wan wan = wanMarker.wan;
        string text = $"Id: {wan.id}\n" +
                      $"Name: {wan.name}\n" +
                      $"Longname: {wan.longname}\n" +
                      $"Org: {wan.org}\n";
        TextMesh textMesh = GetComponent<TextMesh>();
        textMesh.text = text;

        _camera = Camera.main;
	}
	
	void Update () {
        // Billboard the text
        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
	}
}
