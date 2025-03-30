using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    private void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started Host");
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Started Client");
        });
    }

    public void TryStartClient()
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
