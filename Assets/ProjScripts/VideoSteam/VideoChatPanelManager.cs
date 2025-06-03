using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer;

public class VideoChatPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject participantDisplayPrefab;
    [SerializeField] private Transform participantSlotsContainer;
    [SerializeField] private int maxDisplaySlots = 4;

    private Dictionary<ulong, ParticipantDisplayController> m_ActiveParticipantDisplays = new Dictionary<ulong, ParticipantDisplayController>();
    private Dictionary<ulong, XRINetworkPlayer> m_TrackedPlayers = new Dictionary<ulong, XRINetworkPlayer>();
    private Dictionary<ulong, Action<string>> m_PlayerNameUpdateActions = new Dictionary<ulong, Action<string>>();

    void Start()
    {
        XRINetworkGameManager.Instance.playerStateChanged += HandlePlayerStateChanged;
        PopulatePanelWithCurrentPlayers();
    }

    void Update()
    {
        foreach (var displayEntry in m_ActiveParticipantDisplays)
        {
            ulong clientId = displayEntry.Key;
            ParticipantDisplayController displayController = displayEntry.Value;
            XRINetworkPlayer networkPlayer = null;

            if (!m_TrackedPlayers.TryGetValue(clientId, out networkPlayer) || networkPlayer == null)
            {
                if (XRINetworkGameManager.Instance.GetPlayerByID(clientId, out XRINetworkPlayer foundPlayer) && foundPlayer != null)
                {
                    m_TrackedPlayers[clientId] = foundPlayer;
                    networkPlayer = foundPlayer;
                }
            }

            if (networkPlayer != null)
            {
                ProcessParticipantUpdate(displayController, networkPlayer);
            }
            else
            {
                // Player noT found reset display
                displayController.UpdateVoiceAmplitude(0f);
                displayController.UpdateMicStatus(false);
                if (displayController.IsVideoTextureSet)
                {
                    displayController.TrySetVideoFeedTexture(null);
                }
            }
        }
    }

    private void ProcessParticipantUpdate(ParticipantDisplayController displayController, XRINetworkPlayer networkPlayer)
    {
        displayController.UpdateVoiceAmplitude(networkPlayer.playerVoiceAmp);
        bool isTransmittingAudio = !networkPlayer.selfMuted.Value && networkPlayer.playerVoiceAmp > 0.01f;
        displayController.UpdateMicStatus(isTransmittingAudio);

        if (!displayController.IsVideoTextureSet)
        {
            PlayerVideoSource videoSource = networkPlayer.GetComponent<PlayerVideoSource>();
            if (videoSource != null && videoSource.VideoRenderTexture != null)
            {
                displayController.TrySetVideoFeedTexture(videoSource.VideoRenderTexture);
            }
        }
    }

    void OnDestroy()
    {
        if (XRINetworkGameManager.Instance != null)
        {
            XRINetworkGameManager.Instance.playerStateChanged -= HandlePlayerStateChanged;
        }
        ClearAllParticipantDisplays();
    }

    void PopulatePanelWithCurrentPlayers()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.ConnectedClientsList == null) return;

        ClearAllParticipantDisplays();

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (!m_ActiveParticipantDisplays.ContainsKey(client.ClientId))
            {
                HandlePlayerStateChanged(client.ClientId, true);
            }
        }
    }

    private void HandlePlayerStateChanged(ulong clientId, bool isJoining)
    {
        if (isJoining)
        {
            if (m_ActiveParticipantDisplays.Count < maxDisplaySlots && !m_ActiveParticipantDisplays.ContainsKey(clientId))
            {
                AddParticipantDisplay(clientId);
            }
        }
        else
        {
            RemoveParticipantDisplay(clientId);
        }
    }

    private void AddParticipantDisplay(ulong clientId)
    {
        if (!XRINetworkGameManager.Instance.GetPlayerByID(clientId, out XRINetworkPlayer networkPlayer) || networkPlayer == null)
        {
            Debug.LogError($"Could not find XRINetworkPlayer for ClientId:{clientId}", this);
            return;
        }
        m_TrackedPlayers[clientId] = networkPlayer;

        PlayerVideoSource videoSource = networkPlayer.GetComponent<PlayerVideoSource>();

        GameObject displayGO = Instantiate(participantDisplayPrefab, participantSlotsContainer);
        ParticipantDisplayController displayController = displayGO.GetComponent<ParticipantDisplayController>();

        if (displayController != null)
        {
            string initialName = networkPlayer.playerName;
            if (string.IsNullOrEmpty(initialName) || initialName == "Player")
            {
                initialName = $"Player {clientId}";
            }

            displayController.Setup(clientId, initialName, null);
            m_ActiveParticipantDisplays[clientId] = displayController;
            displayGO.name = $"ParticipantDisplay_Client_{clientId}";

            Action<string> nameUpdateAction = (newName) => {
                if (displayController != null && displayController.AssociatedClientId == clientId)
                {
                    displayController.UpdatePlayerName(newName);
                }
            };
            networkPlayer.onNameUpdated += nameUpdateAction;
            m_PlayerNameUpdateActions[clientId] = nameUpdateAction;

            if (videoSource != null && videoSource.VideoRenderTexture != null)
            {
                displayController.TrySetVideoFeedTexture(videoSource.VideoRenderTexture);
            }
        }
        else
        {
            
            Destroy(displayGO);
            m_TrackedPlayers.Remove(clientId);
        }
    }

    private void RemoveParticipantDisplay(ulong clientId)
    {
        if (m_TrackedPlayers.TryGetValue(clientId, out XRINetworkPlayer networkPlayer) && networkPlayer != null)
        {
            if (m_PlayerNameUpdateActions.TryGetValue(clientId, out Action<string> nameUpdateAction))
            {
                networkPlayer.onNameUpdated -= nameUpdateAction;
                m_PlayerNameUpdateActions.Remove(clientId);
            }
        }

        if (m_ActiveParticipantDisplays.TryGetValue(clientId, out ParticipantDisplayController displayController))
        {
            Destroy(displayController.gameObject);
            m_ActiveParticipantDisplays.Remove(clientId);
        }
        m_TrackedPlayers.Remove(clientId);
    }

    private void ClearAllParticipantDisplays()
    {
        List<ulong> clientIdsToRemove = new List<ulong>(m_ActiveParticipantDisplays.Keys);
        foreach (ulong clientId in clientIdsToRemove)
        {
            RemoveParticipantDisplay(clientId);
        }
    }

    public void UpdateParticipantSimpleMicStatus(ulong speakingClientId, bool isCurrentlyTransmitting)
    {
        if (m_ActiveParticipantDisplays.TryGetValue(speakingClientId, out ParticipantDisplayController displayController))
        {
            displayController.UpdateMicStatus(isCurrentlyTransmitting);
        }
    }
}