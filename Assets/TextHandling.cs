using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TextHandling : MonoBehaviour {
    [Header("References")]
    public UDPReceiver udpReceiver;

    [Header("Text Elements (index 9 used for mode)")]
    public TextMeshProUGUI[] texts;

    void Update() {
        if (udpReceiver == null) {
            Debug.LogWarning("⚠ TextHandling: Missing UDPReceiver reference!");
            return;
        }

        string modeText = udpReceiver.modeData;
        string expressionText = udpReceiver.expressionData;
        string resultText = udpReceiver.resultData;

        if (texts == null || texts.Length < 10)
            return;

        try {
            if (!string.IsNullOrEmpty(expressionText) && texts[0] != null)
                texts[0].text = expressionText;

            if (!string.IsNullOrEmpty(resultText) && texts[1] != null)
                texts[1].text = $"={resultText}";

            if (!string.IsNullOrEmpty(modeText) && texts[9] != null)
                texts[9].text = modeText;
        }
        catch (Exception e) {
            Debug.LogWarning($"⚠ TextHandling JSON parse error: {e.Message}");
        }
    }
}
