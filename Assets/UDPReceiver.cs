using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceiver : MonoBehaviour{
    Thread receiveThread;
    UdpClient client;
    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole = false;

    public volatile string landmarkData;
    public volatile string symbolData;
    public volatile string boardData;

    void Start(){
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData(){
        client = new UdpClient(port);
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

        while (startReceiving){
            try{
                byte[] dataBytes = client.Receive(ref anyIP);
                string json = Encoding.UTF8.GetString(dataBytes);

                Packet packet = JsonUtility.FromJson<Packet>(json);
                if (packet == null || string.IsNullOrEmpty(packet.type)){
                    if (printToConsole)Debug.LogWarning("⚠ Invalid or empty packet received.");
                    continue;
                }

               switch (packet.type){
                    case "landmarks":
                        if (packet.landmarkData != null &&
                            ((packet.landmarkData.left != null && packet.landmarkData.left.Length > 0) ||
                             (packet.landmarkData.right != null && packet.landmarkData.right.Length > 0))){
                            landmarkData = JsonUtility.ToJson(packet.landmarkData);
                            if (printToConsole)
                                Debug.Log("✅ Landmarks received");
                        } else {
                            // 🧹 Clear if empty or null
                            landmarkData = "";
                            if (printToConsole)
                                Debug.Log("🧹 Landmarks cleared");
                        }
                        break;

                    case "board":
                        if (packet.boardData != null && packet.boardData.Length > 0){
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < packet.boardData.Length; i++){
                                if (!string.IsNullOrEmpty(packet.boardData[i]))
                                    if (printToConsole) Debug.Log($"✅ board data received:\n{i} {packet.boardData[i]}");
                                    sb.AppendLine(packet.boardData[i]);
                            }
                            boardData = sb.ToString();
                        } else {
                            boardData = "";
                            if (printToConsole) Debug.Log("🧹 board cleared");
                        }
                        break;

                    case "symbol":
                        symbolData = !string.IsNullOrEmpty(packet.symbolData) ? packet.symbolData : "";
                        if (printToConsole)
                            Debug.Log(string.IsNullOrEmpty(symbolData) ? "🧹 Symbol cleared" : "✅ Symbol received");
                        break;

                }

            }
            catch (Exception err){
                Debug.LogWarning($"UDP Receiver error: {err.Message}");
            }
        }
    }

    void OnApplicationQuit(){
        startReceiving = false;
        if (client != null){
            client.Close();
            client = null;
        }

        if (receiveThread != null && receiveThread.IsAlive)receiveThread.Join();
    }
}

[Serializable]
public class Packet
{
    public string type;
    public LandmarkData landmarkData;
    public string symbolData;
    public string modeData;
    public string expressionData;
    public string resultData;
    public string subModeData;
    public string[] boardData;
}

[Serializable]
public class LandmarkData { public float[] left; public float[] right; }
