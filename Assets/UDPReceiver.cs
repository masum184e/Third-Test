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
                        if (packet.landmarkData != null){
                            landmarkData = JsonUtility.ToJson(packet.landmarkData);
                            if (printToConsole)Debug.Log($"Landmarks received");
                        }
                        break;

                    case "symbol":
                        if (packet.symbolData != null){
                            symbolData = JsonUtility.ToJson(packet.symbolData);
                            if (printToConsole)Debug.Log($"Symbol received");
                        }
                        break;

                    case "mode":
                        if (packet.modeData != null){
                            modeData = JsonUtility.ToJson(packet.modeData);
                            if (printToConsole)Debug.Log($"Mode received");
                        }
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
    public SymbolData symbolData;
    public ModeData modeData;
}

[Serializable]
public class SymbolData { public string value; }
[Serializable]
public class ModeData { public string value; }
[Serializable]
public class LandmarkData { public float[] left; public float[] right; }
