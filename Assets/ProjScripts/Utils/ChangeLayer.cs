using Unity.Netcode;
using UnityEngine;

public class ChangeLayer : NetworkBehaviour
{
    [SerializeField] GameObject HMD;
    [SerializeField] private GameObject Head;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        int layer = LayerMask.NameToLayer("Player");
        HMD.layer = layer;
        Head.layer = layer;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

    }
}
