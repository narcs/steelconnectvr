using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputField : MonoBehaviour {
    public SCVRKeyboardDelegate keyboardDelegate;

	void Start () {

    }

    public void SetFieldAsCurrent() {
        Debug.Log($"Setting VR input field \"{gameObject.name}\" as current");
        keyboardDelegate.SetCurrentTextField(gameObject);
    }
}
