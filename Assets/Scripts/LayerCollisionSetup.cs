using UnityEngine;

/// Sets up the one-line layer-collision rule at startup. Used to set each client (remote player) to the layer called RemotePlayer. This was
/// done so enemys stopped killing remote clients
public class LayerCollisionSetup : MonoBehaviour
{
    [SerializeField] private string enemyLayer       = "Enemy";
    [SerializeField] private string remotePlayerLayer = "RemotePlayer";

    private void Awake()
    {
        int e  = LayerMask.NameToLayer(enemyLayer);
        int rp = LayerMask.NameToLayer(remotePlayerLayer);
        Physics2D.IgnoreLayerCollision(e, rp, true);
    }
}