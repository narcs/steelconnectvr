using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRInputField : MonoBehaviour {
    public SCVRKeyboardDelegate keyboardDelegate;

	void Start () {
        GetComponentInChildren<Text>().text = string.Empty;
    }

    public void SetFieldAsCurrent() {
        Debug.Log($"Setting VR input field \"{gameObject.name}\" as current");
        keyboardDelegate.SetCurrentTextField(gameObject);
    }
}
