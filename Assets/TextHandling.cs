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
        string matrixText = udpReceiver.matrixData;

        if (texts == null || texts.Length < 10)
            return;

        try {
            if (texts[0] != null)
                texts[0].text = string.IsNullOrEmpty(expressionText) ? "" : expressionText;

            if (texts[1] != null)
                texts[1].text = string.IsNullOrEmpty(resultText) ? "" : $"={resultText}";

            if (texts[9] != null)
                texts[9].text = string.IsNullOrEmpty(modeText) ? "" : modeText;


            if (!string.IsNullOrEmpty(matrixText)){
                string[] lines = matrixText.Split('\n');
                for (int i = 0; i < Mathf.Min(lines.Length, texts.Length - 2); i++){
                    Debug.Log($"{i} => {lines[i]}");
                    texts[i].text = lines[i];
                }
            }else{
                for(int i = 0; i < 10; i++){
                    texts[i].text = "";
                }
            }

        } catch (Exception e) {
            Debug.LogWarning($"⚠ TextHandling JSON parse error: {e.Message}");
        }
    }
}
