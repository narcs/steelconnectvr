namespace GoogleVR.KeyboardDemo {
    using UnityEngine;
    using UnityEngine.UI;
    using System;

    public class KeyboardDelegate : GvrKeyboardDelegateBase {
        public GameObject globe;

        public override event EventHandler KeyboardHidden;
        public override event EventHandler KeyboardShown;

        private const string DD_KEYBOARD_NOT_INSTALLED_MSG = "Please update the Daydream Keyboard app from the Play Store.";

        void Awake() {

        }

        public override void OnKeyboardShow() {
            Debug.Log("Calling Keyboard Show Delegate!");
            EventHandler handler = KeyboardShown;
            if (handler != null) {
                handler(this, null);
            }
            globe.GetComponent<SphereInteraction>().globeDragEnabled = false;
        }

        public override void OnKeyboardHide() {
            Debug.Log("Calling Keyboard Hide Delegate!");
            EventHandler handler = KeyboardHidden;
            if (handler != null) {
                handler(this, null);
            }
            globe.GetComponent<SphereInteraction>().globeDragEnabled = true;
        }

        public override void OnKeyboardUpdate(string text) {
            Debug.Log($"Keyboard update: {text}");
        }

        public override void OnKeyboardEnterPressed(string text) {
            Debug.Log("Calling Keyboard Enter Pressed Delegate: " + text);
        }

        public override void OnKeyboardError(GvrKeyboardError errCode) {
            Debug.Log("Calling Keyboard Error Delegate: ");
            switch (errCode) {
                case GvrKeyboardError.UNKNOWN:
                    Debug.Log("Unknown Error");
                    break;
                case GvrKeyboardError.SERVICE_NOT_CONNECTED:
                    Debug.Log("Service not connected");
                    break;
                case GvrKeyboardError.NO_LOCALES_FOUND:
                    Debug.Log("No locales found");
                    break;
                case GvrKeyboardError.SDK_LOAD_FAILED:
                    Debug.LogWarning(DD_KEYBOARD_NOT_INSTALLED_MSG);
                    break;
            }
        }

        public void LaunchPlayStore() {
#if !UNITY_ANDROID
            Debug.LogError("GVR Keyboard available only on Android.");
#else
            GvrKeyboardIntent.Instance.LaunchPlayStore();
#endif  // !UNITY_ANDROID
        }
    }
}
