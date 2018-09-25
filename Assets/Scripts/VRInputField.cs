using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputField : MonoBehaviour {
    public GameObject KeyboardDelegateObject;
    SCVRKeyboardDelegate _keyboardDelegate;

	void Start () {
        _keyboardDelegate = KeyboardDelegateObject.GetComponent<SCVRKeyboardDelegate>();
    }

    public void SetFieldAsCurrent() {
        Debug.Log($"Setting VR input field \"{gameObject.name}\" as current");
        _keyboardDelegate.SetCurrentTextField(gameObject);
    }
}
