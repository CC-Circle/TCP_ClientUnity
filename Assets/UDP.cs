using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TcpClientScript : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private const int MaxRetries = 3;
    private const int TimeoutMilliseconds = 5000; // 5秒のタイムアウト
    private const int RetryDelayMilliseconds = 1000; // 再送信間隔を1秒に設定

    private void Start()
    {
        // クライアントの設定
        client = new TcpClient("127.0.0.1", 12345);
        stream = client.GetStream();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToServer("1");
        }
    }

    private async void SendMessageToServer(string message)
    {
        if (stream != null)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            int retries = 0;
            bool responseReceived = false;

            while (retries < MaxRetries && !responseReceived)
            {
                try
                {
                    stream.Write(data, 0, data.Length);
                    Debug.Log("メッセージ送信: " + message);

                    // 応答を受け取るためのタスクを開始
                    var responseTask = ReadResponseAsync();
                    if (await Task.WhenAny(responseTask, Task.Delay(TimeoutMilliseconds)) == responseTask)
                    {
                        // 応答が受信できた
                        Debug.Log("サーバーからの応答: " + responseTask.Result);
                        responseReceived = true;
                    }
                    else
                    {
                        // タイムアウトが発生した
                        Debug.LogWarning("応答がタイムアウトしました。再送信します...");
                        retries++;
                        await Task.Delay(RetryDelayMilliseconds); // 再送信の間隔を設ける
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("エラーが発生しました: " + ex.Message);
                    retries++;
                    await Task.Delay(RetryDelayMilliseconds); // エラー発生時も再送信の間隔を設ける
                }
            }

            if (!responseReceived)
            {
                Debug.LogError("最大リトライ回数に達しました。サーバーからの応答がありません。");
            }
        }
    }

    private async Task<string> ReadResponseAsync()
    {
        byte[] responseData = new byte[1024];
        int bytes = await stream.ReadAsync(responseData, 0, responseData.Length);
        return Encoding.ASCII.GetString(responseData, 0, bytes);
    }

    private void OnApplicationQuit()
    {
        // クライアントの接続を閉じる
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
