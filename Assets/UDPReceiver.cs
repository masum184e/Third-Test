using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole = false;
    public volatile string landmarkData;
    public volatile string symbolData;

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

        while (startReceiving)
        {
            try
            {
                byte[] dataBytes = client.Receive(ref anyIP);
                string json = Encoding.UTF8.GetString(dataBytes);

                Packet packet = JsonUtility.FromJson<Packet>(json);

                if (packet.type == "landmarks" && packet.landmarkData != null)
                {
                    int leftCount = packet.landmarkData.left?.Length ?? 0;
                    int rightCount = packet.landmarkData.right?.Length ?? 0;

                    landmarkData = JsonUtility.ToJson(packet.landmarkData);

                    if (printToConsole)
                        Debug.Log($"📡 Landmarks received | Left: {leftCount} pts, Right: {rightCount} pts");
                }
                else if (packet.type == "symbol" && packet.symbolData != null)
                {
                    symbolData = JsonUtility.ToJson(packet.symbolData);
                    if (printToConsole)
                        Debug.Log($"🧮 Arithmetic result");
                }
            }
            catch (Exception err)
            {
                Debug.LogWarning($"UDP Receiver error: {err.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        startReceiving = false;
        if (client != null) client.Close();
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Interrupt();
    }
}

[Serializable]
public class Packet
{
    public string type;
    public LandmarkData landmarkData;
    public SymbolData symbolData;
}

[Serializable]
public class SymbolData
{
    public string value;
}

[Serializable]
public class LandmarkData
{
    public float[] left;
    public float[] right;
}
