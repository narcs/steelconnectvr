using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRLogWindow : MonoBehaviour {
    private GameObject _textObject;
    private Text _textComponent;

    private Queue<string> _lines;
    private string _text;

    private int _num_lines = 0;
    private int _MAX_LINES = 100;

    void Start() {
        _textObject = gameObject.transform.Find("Panel/ScrollRect/LogText").gameObject;
        _textComponent = _textObject.GetComponent<Text>();
        _textComponent.text = "";

        Application.logMessageReceived += handleLogMessage;
    }

    void Update() {

    }

    void handleLogMessage(string logString, string stackTrace, LogType type) {
        _num_lines += 1;

        if (_num_lines > _MAX_LINES) {
            _num_lines = _MAX_LINES;

            int newline_position = _textComponent.text.IndexOf("\n");
            _textComponent.text = _textComponent.text.Substring(newline_position + 1);
        }

        string line = $"<color={logTypeToColorString(type)}>{logString}</color>\n";
        _textComponent.text += line;
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
