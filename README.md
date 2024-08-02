```python3
import socket
import threading
import time

class TCPServer:
    def __init__(self, host='127.0.0.1', port=12345):
        self.host = host
        self.port = port
        self.server_socket = self._create_server_socket()
        self.Flag = False  # Flagをインスタンス変数として定義
        self.SceneMode = -1 # SceneModeをインスタンス変数として定義
    
    def _create_server_socket(self):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        server_socket.bind((self.host, self.port))
        server_socket.listen(1)
        print("サーバーが起動しました...")
        return server_socket
    
    def wait_for_connection(self):
        client_socket, addr = self.server_socket.accept()
        print(f"接続されました: {addr}")
        return client_socket
    
    def handle_client(self, client_socket):
        while True:
            data = client_socket.recv(1024)
            if not data:
                break
            
            received_message = data.decode()
            print(f"受け取ったデータ: {received_message}")

            # received_messageをint型に変換する
            scene_mode = int(received_message)
            if scene_mode > 0:
                self._set_SceneMode(scene_mode)
                response = received_message + '0'
                client_socket.send(response.encode())
                print(f"送信したデータ: {response}")

        client_socket.close()
        print("クライアント接続が切れました")
        self._set_SceneMode(-1)
    
    def start(self):
        try:
            while True:
                print("接続待ち...")
                client_socket = self.wait_for_connection()
                self.handle_client(client_socket)
        finally:
            self.close_connections()
    
    def close_connections(self):
        if self.server_socket:
            self.server_socket.close()
        print("サーバーを終了しました")

    def _get_Flag(self):
        return self.Flag
    
    def _set_Flag(self, value):
        self.Flag = value

    def _get_SceneMode(self) -> int:
        return self.SceneMode
    
    def _set_SceneMode(self, value: int):
        self.SceneMode = value

if __name__ == "__main__":
    server = TCPServer(host='127.0.0.1', port=12345)
    server_thread = threading.Thread(target=server.start)
    server_thread.start()

    while True:
        print("SceneMode: ", server._get_SceneMode())
        time.sleep(0.01)

```
