using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SocketServer : MonoBehaviour
{
    public static SocketServer Instance { get; private set; }

    private TcpListener listener;
    private Thread listenerThread;
    private bool running = true;

    private KeyboardInputManager input;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Find ML input automatically
        input = KeyboardInputManager.Instance;

        if (input == null)
        {
            Debug.LogError("[SocketServer] No MLInputDevice found in scene!");
            return;
        }

        StartServer();
    }

    void StartServer()
    {
        if (listenerThread != null) return;

        running = true;
        listenerThread = new Thread(ServerThread);
        listenerThread.IsBackground = true;
        listenerThread.Start();

        Debug.Log("[SocketServer] Listening on port 5005");
    }

    void ServerThread()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, 5005);
            listener.Server.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true
            );
            listener.Start();

            while (running)
            {
                TcpClient client = null;

                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (SocketException)
                {
                    if (!running) break;
                    continue;
                }

                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int length = stream.Read(buffer, 0, buffer.Length);
                    if (length <= 0) continue;

                    string cmd = Encoding.UTF8.GetString(buffer, 0, length).Trim();
                    HandleCommand(cmd);
                }
            }
        }
        finally
        {
            listener?.Stop();
        }
    }


    private void HandleCommand(string cmd)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (input == null) return;

            switch (cmd)
            {
                case "MOVE_LEFT":
                    input.ML_MoveLeft();
                    break;
                case "MOVE_RIGHT":
                    input.ML_MoveRight();
                    break;
                case "STOP":
                    input.ML_StopMove();
                    break;
                case "NORMAL_SHOT":
                    input.ML_NormalShot();
                    break;
                case "CV_SHOT":
                    input.ML_CVShot();
                    break;
            }
        });
    }



    void StopServer()
    {
        running = false;

        if (listener != null)
        {
            listener.Stop();
            listener = null;
        }

        if (listenerThread != null)
        {
            listenerThread.Join();
            listenerThread = null;
        }
    }

    void OnDisable()
    {
        StopServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

}
