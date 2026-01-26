using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

public class EyeTrackingReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    public int port = 5052;

    [Header("UI Settings")]
    public RectTransform eyeCursor; 
    public float rangeX = 900.0f; 
    public float rangeY = 500.0f; 
    public float smoothing = 10.0f;

    [Header("Debug Info")]
    public string receivedRawData = ""; 

    private Thread receiveThread;
    private UdpClient client;
    private Vector2 targetPos = Vector2.zero;
    private bool isRunning = true;

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        if (eyeCursor == null) return;
        eyeCursor.anchoredPosition = Vector2.Lerp(eyeCursor.anchoredPosition, targetPos, Time.deltaTime * smoothing);
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (isRunning)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                receivedRawData = text; // 디버깅용

                // 괄호, 공백 제거
                text = text.Replace("(", "").Replace(")", "").Replace(" ", "");
                string[] coordinates = text.Split(',');

                // 🔥 [수정됨] 데이터 개수에 따라 처리 방식 나눔
                if (coordinates.Length >= 2)
                {
                    // X, Y 둘 다 왔을 때
                    float xRatio = float.Parse(coordinates[0], CultureInfo.InvariantCulture);
                    float yRatio = float.Parse(coordinates[1], CultureInfo.InvariantCulture);

                    float finalX = (xRatio - 0.5f) * 2 * rangeX;
                    float finalY = -(yRatio - 0.5f) * 2 * rangeY; 
                    targetPos = new Vector2(finalX, finalY);
                }
                else if (coordinates.Length == 1 && text.Length > 0)
                {
                    // 🔥 숫자 하나만 왔을 때 (현재 상황)
                    if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float xRatio))
                    {
                        float finalX = (xRatio - 0.5f) * 2 * rangeX;
                        // Y는 0(중앙)으로 고정하거나, 현재 Y위치 유지
                        targetPos = new Vector2(finalX, 0); 
                    }
                }
            }
            catch (System.Exception e)
            {
                // 스레드 종료 에러는 무시
                if (e is ThreadAbortException) return; 
                Debug.LogWarning($"파싱 에러: {receivedRawData}");
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (client != null) client.Close();
        if (receiveThread != null) receiveThread.Abort();
    }
}