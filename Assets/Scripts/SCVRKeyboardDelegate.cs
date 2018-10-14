using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCVRKeyboardDelegate : GvrKeyboardDelegateBase {
    public override event EventHandler KeyboardHidden;
    public override event EventHandler KeyboardShown;

    public GvrKeyboard keyboardManager;
    public SphereInteraction sphereInteraction;

    GameObject _currentVrInputField = null;
    Text _currentVrInputFieldText = null;


    // ---

    public void SetCurrentTextField(GameObject gameObject) {
        Debug.Log($"Current VR text field changed to {gameObject.name}");
        
        _currentVrInputField = gameObject;
        _currentVrInputFieldText = _currentVrInputField.GetComponentInChildren<Text>();
        keyboardManager.EditorText = _currentVrInputFieldText.text;

        keyboardManager.Show();
    }

    // ---

    public override void OnKeyboardEnterPressed(string edit_text) {
        Debug.Log($"Keyboard enter pressed callback called with text: {edit_text}");
    }

    public override void OnKeyboardError(GvrKeyboardError errorCode) {
        Debug.Log($"Keyboard error: {errorCode}");
    }

    public override void OnKeyboardHide() {
        Debug.Log("Keyboard hide callback called");
    }

    public override void OnKeyboardShow() {
        Debug.Log("Keyboard show callback called");
    }

    public override void OnKeyboardUpdate(string edit_text) {
        Debug.Log($"Keyboard update callback called with text: {edit_text}");
        if (_currentVrInputFieldText != null) {
            _currentVrInputFieldText.text = edit_text;
        }
    }
}
