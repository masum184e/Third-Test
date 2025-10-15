using UnityEngine;
using System;

public class HandTracking : MonoBehaviour
{
    public UDPReceiver udpReceiver;

    [Header("Assign 21 point GameObjects for Left and Right hands")]
    public GameObject[] leftHandPoints;
    public GameObject[] rightHandPoints;

    void Update(){
        // Copy local variable to avoid race condition with UDP thread
        string data = udpReceiver.data;

        if (string.IsNullOrEmpty(data)) return;

        try{
            // Parse the same JSON structure as in UDPReceiver
            Packet packet = JsonUtility.FromJson<Packet>(data);
            if (packet == null || packet.data == null || packet.type != "landmarks") return;

            // --- LEFT HAND ---
           float scale = 0.01f;
        float offsetX = -3.5f;
        float offsetY = -2.0f;

        // --- LEFT HAND ---
        if (packet.data.left != null && packet.data.left.Length >= 63) {
            for (int i = 0; i < 21; i++) {
                int baseIdx = i * 3;
                float x = packet.data.left[baseIdx] * scale + offsetX;
                float y = packet.data.left[baseIdx + 1] * scale + offsetY;
                float z = packet.data.left[baseIdx + 2] * scale;

                if (i < leftHandPoints.Length)
                    leftHandPoints[i].transform.position = new Vector3(x, y, z);
            }
        }

        // --- RIGHT HAND ---
        if (packet.data.right != null && packet.data.right.Length >= 63) {
            for (int i = 0; i < 21; i++) {
                int baseIdx = i * 3;
                float x = -(packet.data.right[baseIdx]) * scale - offsetX; // mirror horizontally
                float y = packet.data.right[baseIdx + 1] * scale + offsetY;
                float z = packet.data.right[baseIdx + 2] * scale;

                if (i < rightHandPoints.Length)
                    rightHandPoints[i].transform.position = new Vector3(x, y, z);
            }
        }
        
        }
        catch (Exception e)
        {
            Debug.LogWarning($"⚠ HandTracking JSON parse error: {e.Message}");
        }
    }
}
