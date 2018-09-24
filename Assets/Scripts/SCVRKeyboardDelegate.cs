using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCVRKeyboardDelegate : GvrKeyboardDelegateBase {
    public override event EventHandler KeyboardHidden;
    public override event EventHandler KeyboardShown;

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
    }
}
