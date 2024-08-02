using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TCP : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private const string Host = "127.0.0.1";
    private const int Port = 12345;
    private const int RetryDelay = 2; // リトライの間隔（秒）
    private const int Timeout = 5000; // タイムアウトの時間（ミリ秒）
    private bool isConnecting = false;

    private void Start()
    {
        StartCoroutine(ConnectToServer());
    }

    private IEnumerator ConnectToServer()
    {
        isConnecting = true;
        while (isConnecting)
        {
            try
            {
                client = new TcpClient(Host, Port);
                stream = client.GetStream();
                isConnecting = false;
                Debug.Log("サーバーに接続しました");
            }
            catch (SocketException e)
            {
                Debug.LogWarning($"接続に失敗しました。リトライします... {e.Message}");
            }

            if (isConnecting) // catch ブロック外でリトライのための待機を行う
            {
                yield return new WaitForSeconds(RetryDelay);
            }
        }
    }

    private async void Update()
    {
        if (!isConnecting && (client == null || !client.Connected))
        {
            StartCoroutine(ConnectToServer());
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha1))
        {
            await SendMessageToServerAsync("1");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha2))
        {
            await SendMessageToServerAsync("2");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha3))
        {
            await SendMessageToServerAsync("3");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha4))
        {
            await SendMessageToServerAsync("4");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha5))
        {
            await SendMessageToServerAsync("5");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha6))
        {
            await SendMessageToServerAsync("6");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha7))
        {
            await SendMessageToServerAsync("7");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha8))
        {
            await SendMessageToServerAsync("8");
        }

        if (client != null && client.Connected && Input.GetKeyDown(KeyCode.Alpha9))
        {
            await SendMessageToServerAsync("9");
        }
    }

    private async Task SendMessageToServerAsync(string message)
    {
        if (stream != null)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
            Debug.Log("メッセージ送信: " + message);

            // 応答を非同期で待つ
            byte[] responseData = new byte[1024];
            int bytes = 0;
            bool receivedResponse = false;

            try
            {
                // タイムアウトを設定するための Task.Delay を使用
                Task<int> readTask = Task.Run(() => stream.Read(responseData, 0, responseData.Length));
                Task timeoutTask = Task.Delay(Timeout);
                Task completedTask = await Task.WhenAny(readTask, timeoutTask);

                if (completedTask == readTask)
                {
                    bytes = readTask.Result;
                    receivedResponse = bytes > 0;
                }
                else
                {
                    // タイムアウト
                    Debug.LogWarning("応答のタイムアウト");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"応答の受信中にエラーが発生しました: {e.Message}");
            }

            if (receivedResponse)
            {
                string responseMessage = Encoding.ASCII.GetString(responseData, 0, bytes);
                if (responseMessage.Equals(message + "0"))
                {
                    Debug.Log("サーバーからの応答: " + responseMessage);
                }
                else
                {
                    Debug.LogWarning("サーバーからの不正な応答: " + responseMessage);
                }

            }
            else
            {
                Debug.LogWarning("サーバーからの応答がありませんでした。再送信します...");
                // 応答がなかった場合に再送信
                await SendMessageToServerAsync(message);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // クライアントの接続を閉じる
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
