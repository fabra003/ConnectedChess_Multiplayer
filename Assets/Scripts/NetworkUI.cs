using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button leaveButton;
    public Button rejoinButton;

    private void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started Host");
        });

        clientButton.onClick.AddListener(() =>
        {
            TryStartClient();
        });

        leaveButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.Shutdown();
                Debug.Log("Host/Server stopped.");
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
                Debug.Log("Client disconnected.");
            }
        });

        rejoinButton.onClick.AddListener(() =>
        {
            TryStartClient(); // Just retry connection
        });
    }

    private void TryStartClient()
    {
        bool success = NetworkManager.Singleton.StartClient();
        if (!success)
        {
            Debug.Log("Client failed to start. Host might not be running.");
        }
        else
        {
            Debug.Log("Client trying to connect...");
        }
    }
}
