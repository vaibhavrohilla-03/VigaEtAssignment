using UnityEngine;
using Unity.Netcode;

public class PlayerVideoSource : NetworkBehaviour
{
    [SerializeField] private Camera videoCaptureCamera;
    private RenderTexture m_VideoRenderTexture;
    public RenderTexture VideoRenderTexture => m_VideoRenderTexture;
    [SerializeField] private int textureWidth = 256;
    [SerializeField] private int textureHeight = 256;
    private int depthBufferBits = 32;
    private RenderTextureFormat colorFormat = RenderTextureFormat.ARGB32;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_VideoRenderTexture = new RenderTexture(textureWidth, textureHeight, depthBufferBits, colorFormat);
        m_VideoRenderTexture.name = $"Player_{OwnerClientId}_VideoFeed_RT_{textureWidth}x{textureHeight}_{colorFormat}";
        m_VideoRenderTexture.antiAliasing = 1;
        m_VideoRenderTexture.useMipMap = false;
        m_VideoRenderTexture.wrapMode = TextureWrapMode.Mirror;
        m_VideoRenderTexture.filterMode = FilterMode.Bilinear;

        m_VideoRenderTexture.Create();
        
        videoCaptureCamera.targetTexture = m_VideoRenderTexture;
        videoCaptureCamera.enabled = true;
        Debug.Log($" {m_VideoRenderTexture.name} created. IsCreated: {m_VideoRenderTexture.IsCreated()}", this);
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        videoCaptureCamera.targetTexture = null;
        videoCaptureCamera.enabled = false;
        m_VideoRenderTexture.Release();
        Destroy(m_VideoRenderTexture);
        m_VideoRenderTexture = null;
        Debug.Log($"Clean up RenderTexture for Player {OwnerClientId}");

    }
}