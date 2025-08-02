using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;   // <-- Unity Transport

public class NetworkManagerUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("Join-by-IP UI")]
    [SerializeField] private TMP_InputField ipInputField;      // drag the InputField here
    [SerializeField] private TMP_InputField gamertagInput;
    [SerializeField] private TextMeshProUGUI feedbackText;     // optional – drag a Text label here
    [SerializeField] private ushort port = 7777;               // default UTP port

    private bool isAttemptingConnection;

    #region Unity lifecycle
    private void Awake()
    {
        serverButton.onClick.AddListener(StartServer);
        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(StartClient);

        if (feedbackText) feedbackText.text = string.Empty;
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback  -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
    #endregion

    #region Button handlers
    private void StartServer() => NetworkManager.Singleton.StartServer();

    private void StartHost()
    {
        if (!ValidateGamertag()) return;

        // Save locally – also handy when you return to menu after a match
        PlayerPrefs.SetString("Gamertag", gamertagInput.text.Trim());

        // Pass the name to the server through ConnectionData
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.UTF8.GetBytes(gamertagInput.text.Trim());
        
        NetworkManager.Singleton.StartHost(); 
    } 

    private void StartClient()
    {
        if (!ValidateGamertag()) return;
        
        PlayerPrefs.SetString("Gamertag", gamertagInput.text.Trim());
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.UTF8.GetBytes(gamertagInput.text.Trim());
                
        var ip = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            ShowFeedback("<color=#ff0000>Please enter a host IP address."); // <color=#ff0000> is red
            return;
        }

        // Configure Unity Transport before connecting
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            // For Netcode 1.8+, SetConnectionData(address, port) – otherwise set ConnectionData.Address/Port directly
            transport.SetConnectionData(ip, port);
        }

        ShowFeedback($"<color=#00ff00>Connecting to {ip}:{port} …"); // <color=#00ff00> is green
        isAttemptingConnection = true;
        
        
        
        NetworkManager.Singleton.StartClient();
    }
    #endregion

    #region Connection callbacks
    private void OnClientConnected(ulong clientId)
    {
        if (!isAttemptingConnection || clientId != NetworkManager.Singleton.LocalClientId) return;

        isAttemptingConnection = false;
        ShowFeedback(string.Empty);     // clear on success
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!isAttemptingConnection || clientId != NetworkManager.Singleton.LocalClientId) return;

        isAttemptingConnection = false;
        ShowFeedback("<color=#ff0000>Failed to connect – check the IP and be sure the host is running."); // <color=#ff0000> is red
    }
    #endregion

    private void ShowFeedback(string message)
    {
        if (feedbackText) feedbackText.text = message;
        else              Debug.Log(message);
    }

    private bool ValidateGamertag()
    {
        if (string.IsNullOrWhiteSpace(gamertagInput.text))
        {
            feedbackText.text = "<color=#ff0000>Please enter a name";
            return false;
        }

        feedbackText.text = string.Empty;
        return true;
    }
}
