using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TcpServerScript : MonoBehaviour
{
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private const int Port = 12345;

    private void Start()
    {
        // サーバーの設定
        server = new TcpListener(IPAddress.Any, Port);
        server.Start();
        Debug.Log("サーバーが起動しました...");

        // 非同期でクライアント接続を待つ
        AcceptClientAsync();
    }

    private async void AcceptClientAsync()
    {
        try
        {
            client = await server.AcceptTcpClientAsync();
            Debug.Log("クライアントが接続されました");
            stream = client.GetStream();

            while (client.Connected)
            {
                await HandleClientAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("エラーが発生しました: " + ex.Message);
        }
    }

    private async System.Threading.Tasks.Task HandleClientAsync()
    {
        byte[] data = new byte[1024];
        int bytesRead = await stream.ReadAsync(data, 0, data.Length);

        if (bytesRead > 0)
        {
            string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);
            Debug.Log("受け取ったデータ: " + receivedMessage);

            if (receivedMessage == "1")
            {
                string responseMessage = "10";
                byte[] responseData = Encoding.ASCII.GetBytes(responseMessage);
                await stream.WriteAsync(responseData, 0, responseData.Length);
                Debug.Log("送信したデータ: " + responseMessage);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // クライアントとサーバーの接続を閉じる
        stream?.Close();
        client?.Close();
        server?.Stop();
        Debug.Log("サーバーを終了しました");
    }
}
