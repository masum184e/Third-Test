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
        // ✅ Ensure  exists
        if (udpReceiver == null) {
            Debug.LogWarning("⚠ TextHandling: Missing UDPReceiver reference!");
            return;
        }

        string data = udpReceiver.modeData;
        if (string.IsNullOrEmpty(data)) return;

        try {
            ModeData modeObj = JsonUtility.FromJson<ModeData>(data);
            if (modeObj == null || string.IsNullOrEmpty(modeObj.value)) return;

            string mode = modeObj.value.Trim();

            if (texts == null || texts.Length <= 9 || texts[9] == null) {
                Debug.LogWarning("⚠ TextHandling: Text element at index 9 is missing or not assigned!");
                return;
            }

            texts[9].text = $"{mode}";
        }
        catch (Exception e) {
            Debug.LogWarning($"⚠ TextHandling JSON parse error: {e.Message}");
        }
    }
}
