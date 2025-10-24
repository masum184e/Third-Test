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
    public volatile string modeData;
    public volatile string expressionData;
    public volatile string resultData;
    public volatile string matrixData;

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

                    case "matrix":
                        if (packet.matrixData != null && packet.matrixData.Length > 0){
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < packet.matrixData.Length; i++){
                                if (!string.IsNullOrEmpty(packet.matrixData[i]))
                                    sb.AppendLine(packet.matrixData[i]);
                            }
                            matrixData = sb.ToString();
                            if (printToConsole) Debug.Log($"✅ Matrix data received:\n{matrixData}");
                        } else {
                            matrixData = "";
                            if (printToConsole) Debug.Log("🧹 Matrix cleared");
                        }
                        break;

                    case "symbol":
                        symbolData = !string.IsNullOrEmpty(packet.symbolData) ? packet.symbolData : "";
                        if (printToConsole)
                            Debug.Log(string.IsNullOrEmpty(symbolData) ? "🧹 Symbol cleared" : "✅ Symbol received");
                        break;

                    case "mode":
                        modeData = !string.IsNullOrEmpty(packet.modeData) ? packet.modeData : "";
                        if (printToConsole)
                            Debug.Log(string.IsNullOrEmpty(modeData) ? "🧹 Mode cleared" : $"✅ Mode received: {modeData}");
                        break;

                    case "expression":
                        expressionData = !string.IsNullOrEmpty(packet.expressionData) ? packet.expressionData : "";
                        if (printToConsole)
                            Debug.Log(string.IsNullOrEmpty(expressionData) ? "🧹 Expression cleared" : "✅ Expression received");
                        break;

                    case "result":
                        resultData = !string.IsNullOrEmpty(packet.resultData) ? packet.resultData : "";
                        if (printToConsole)
                            Debug.Log(string.IsNullOrEmpty(resultData) ? "🧹 Result cleared" : "✅ Result received");
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
    public string[] matrixData;
}

[Serializable]
public class LandmarkData { public float[] left; public float[] right; }
