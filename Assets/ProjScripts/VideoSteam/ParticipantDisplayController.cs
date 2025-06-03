using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParticipantDisplayController : MonoBehaviour
{
    [SerializeField] private Image videoFeedImage; 
    [SerializeField] private TMP_Text playerNameText;    
    [SerializeField] private Image micStatusOnIcon;       
    [SerializeField] private Image micStatusOffIcon;      
    [SerializeField] private Image voiceAmplitudeFillImage; 

    private ulong m_AssociatedClientId;
    public ulong AssociatedClientId => m_AssociatedClientId;

    private Material m_InstancedVideoMaterial;
    public bool IsVideoTextureSet { get; private set; } = false;

    void Awake()
    {
        m_InstancedVideoMaterial = new Material(videoFeedImage.material);
        videoFeedImage.material = m_InstancedVideoMaterial; 
        voiceAmplitudeFillImage.fillAmount = 0f;
        UpdateMicStatus(false);
    }
    public void Setup(ulong clientId, string playerName, RenderTexture initialVideoTexture)
    {
        m_AssociatedClientId = clientId;
        IsVideoTextureSet = false;
        playerNameText.text = playerName;
        TrySetVideoFeedTexture(initialVideoTexture);
        UpdateMicStatus(false);
        UpdateVoiceAmplitude(0f);
    }
    public bool TrySetVideoFeedTexture(RenderTexture videoTexture)
    {
        IsVideoTextureSet = false;

        if (m_InstancedVideoMaterial == null)
        {
           
            if (videoFeedImage != null) videoFeedImage.enabled = false;
            return false;
        }

        if (videoTexture != null)
        {
             
            m_InstancedVideoMaterial.SetTexture("_BaseMap", videoTexture);
            videoFeedImage.material = m_InstancedVideoMaterial;
            videoFeedImage.enabled = true;
            IsVideoTextureSet = true;
            return true;
        }
        else
        {
            m_InstancedVideoMaterial.SetTexture("_BaseMap", null);
            videoFeedImage.material = m_InstancedVideoMaterial; 
            videoFeedImage.enabled = false;
            return false;
        }
    }
    public void UpdatePlayerName(string newName)
    {
        playerNameText.text = newName;
    }
    public void UpdateMicStatus(bool isSpeaking)
    {
        if (micStatusOnIcon != null) micStatusOnIcon.enabled = isSpeaking;
        if (micStatusOffIcon != null) micStatusOffIcon.enabled = !isSpeaking;
    }
    public void UpdateVoiceAmplitude(float amplitude)
    {
        if (voiceAmplitudeFillImage != null) voiceAmplitudeFillImage.fillAmount = Mathf.Clamp01(amplitude);
    }
    public void ClearDisplay()
    {
        playerNameText.text = "";
        TrySetVideoFeedTexture(null);
        UpdateMicStatus(false);
        UpdateVoiceAmplitude(0f);
        m_AssociatedClientId = 0;
    }
    void OnDestroy()
    {
        if (m_InstancedVideoMaterial != null)
        {
            Destroy(m_InstancedVideoMaterial);
            m_InstancedVideoMaterial = null;
        }
    }
}