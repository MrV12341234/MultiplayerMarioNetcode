using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;          // Netcode for GameObjects
using UnityEngine.SceneManagement;

public class DisconnectButton : MonoBehaviour
{
    [SerializeField] private Button exitButton;   // drag your UI Button here
    // [SerializeField] private string menuScene = "MainMenu";   // optional: scene to load after disconnect

    private void Awake()
    {
        exitButton.onClick.AddListener(Disconnect);
    }

    private void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
             // 1. Shut down networking (works whether you are Host, Server-only, or Client)
                    NetworkManager.Singleton.Shutdown();
                    Destroy(NetworkManager.Singleton.gameObject); // destorys NetworkManager so it doesnt reload
                    
                    var netGameMgr = NetworkGameManager.Instance;
                    if (netGameMgr) Destroy(netGameMgr.gameObject);
        }
        SceneManager.LoadScene(0);
           
    }
}