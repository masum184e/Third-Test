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

        string boardText = udpReceiver.boardData;

        if (texts == null || texts.Length < 10)
            return;

        try {
             if (!string.IsNullOrEmpty(boardText)) {
                string[] lines = boardText.Split(new[] { '\n' }, StringSplitOptions.None);

                // ✅ Always fill all 10 slots
                for (int i = 0; i < texts.Length; i++) {
                    if (i < lines.Length)
                        texts[i].text = lines[i];
                    else
                        texts[i].text = "";
                }

            } else {
                // 🧹 Clear all text elements
                for (int i = 0; i < texts.Length; i++) {
                    texts[i].text = "";
                }
            }
        } catch (Exception e) {
            Debug.LogWarning($"⚠ TextHandling JSON parse error: {e.Message}");
        }
    }
}
