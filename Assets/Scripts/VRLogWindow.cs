using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRLogWindow : MonoBehaviour {
    private GameObject textObject;
    private Text textComponent;

    private Queue<string> lines;
    private string text;

    private int num_lines = 0;
    private int MAX_LINES = 100;

    void Start() {
        textObject = gameObject.transform.Find("Panel/ScrollRect/LogText").gameObject;
        textComponent = textObject.GetComponent<Text>();
        textComponent.text = "";

        Application.logMessageReceived += handleLogMessage;
    }

    void Update() {

    }

    void handleLogMessage(string logString, string stackTrace, LogType type) {
        num_lines += 1;

        if (num_lines > MAX_LINES) {
            num_lines = MAX_LINES;

            int newline_position = textComponent.text.IndexOf("\n");
            textComponent.text = textComponent.text.Substring(newline_position + 1);
        }

        string line = $"<color={logTypeToColorString(type)}>{logString}</color>\n";
        textComponent.text += line;
    }

    string logTypeToColorString(LogType type) {
        switch (type) {
            case LogType.Assert: return "orange";
            case LogType.Error: return "red";
            case LogType.Exception: return "orange";
            case LogType.Warning: return "yellow";
            case LogType.Log: return "white";

            default: return "white";
        }
    }
}
