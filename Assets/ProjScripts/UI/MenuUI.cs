using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.Android;
using XRMultiplayer;

    [DefaultExecutionOrder(100)]
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] InputActionReference ToggleMenuAction;
        [SerializeField] AudioMixer Mixer;

        [Header("Panels")]
        [SerializeField] GameObject m_HostRoomPanel;
        [SerializeField] GameObject ClientRoomPanel;
        [SerializeField] GameObject OfflineWarningPanels;
        [SerializeField] GameObject OnlinePanels;
        [SerializeField] GameObject[] Panels;
        [SerializeField] Toggle[] m_PanelToggles;

        [Header("Text Components")]
        [SerializeField] TMP_Text RoomCodeText;
        [SerializeField] TMP_Text TimeText;
        [SerializeField] TMP_Text RoomNameText;
        [SerializeField] TMP_InputField RoomNameInputField;
        [SerializeField] TMP_Text[] PlayerCountText;
     
        [Header("Voice Chat")]
        [SerializeField] Image m_LocalPlayerAudioVolume;
        [SerializeField] Image m_MutedIcon;
        [SerializeField] Image m_MicOnIcon;

        VoiceChatManager m_VoiceChatManager;
        PermissionCallbacks permCallbacks;

        private void Awake()
        {
            m_VoiceChatManager = FindFirstObjectByType<VoiceChatManager>();

            XRINetworkGameManager.Connected.Subscribe(ConnectOnline);
            XRINetworkGameManager.ConnectedRoomName.Subscribe(UpdateRoomName);

            m_VoiceChatManager.selfMuted.Subscribe(MutedChanged);

            ConnectOnline(false);

            if (ToggleMenuAction != null)
                ToggleMenuAction.action.performed += ctx => ToggleMenu();
            else
                Utils.Log("No toggle menu action assigned to OptionsPanel", 1);

            permCallbacks = new PermissionCallbacks();
            permCallbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            permCallbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Utils.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Utils.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        }

        void OnEnable()
        {
            TogglePanel(0);
        }

        private void OnDestroy()
        {
            XRINetworkGameManager.Connected.Unsubscribe(ConnectOnline);
            XRINetworkGameManager.ConnectedRoomName.Unsubscribe(UpdateRoomName);
            m_VoiceChatManager.selfMuted.Unsubscribe(MutedChanged);
        }

        private void Update()
        {
            TimeText.text = $"{DateTime.Now:h:mm}<size=4><voffset=1em>{DateTime.Now:tt}</size></voffset>";
            if (XRINetworkGameManager.Connected.Value)
            {
                m_LocalPlayerAudioVolume.fillAmount = XRINetworkPlayer.LocalPlayer.playerVoiceAmp;
            }
            else
            {
                m_LocalPlayerAudioVolume.fillAmount = OfflinePlayerAvatar.voiceAmp.Value;
            }
        }

        void ConnectOnline(bool connected)
        {
            OfflineWarningPanels.SetActive(!connected);
            OnlinePanels.SetActive(connected);


            if (connected)
            {
                m_HostRoomPanel.SetActive(NetworkManager.Singleton.IsServer);
                ClientRoomPanel.SetActive(!NetworkManager.Singleton.IsServer);
                UpdateRoomName(XRINetworkGameManager.ConnectedRoomName.Value);
                m_MutedIcon.enabled = false;
                m_MicOnIcon.enabled = true;
                m_LocalPlayerAudioVolume.enabled = true;
                ToggleMenu(true, true);
            }
            
        }

        public void TogglePanel(int panelID)
        {
            for (int i = 0; i < Panels.Length; i++)
            {
                m_PanelToggles[i].SetIsOnWithoutNotify(panelID == i);
                Panels[i].SetActive(i == panelID);
            }
        }

        /// <summary>
        /// Toggles the menu on or off.
        /// </summary>
        /// <param name="overrideToggle"></param>
        /// <param name="overrideValue"></param>
        public void ToggleMenu(bool overrideToggle = false, bool overrideValue = false)
        {
            if (overrideToggle)
            {
                gameObject.SetActive(overrideValue);
            }
            else
            {
                ToggleMenu();
            }
            TogglePanel(0);
        }

        public void ToggleMenu()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void LogOut()
        {
            XRINetworkGameManager.Instance.Disconnect();
        }

        public void QuickJoin()
        {
            XRINetworkGameManager.Instance.QuickJoinLobby();
        }
        public void ToggleMute()
        {
            m_VoiceChatManager.ToggleSelfMute();
        }
        void MutedChanged(bool muted)
        {
            m_MutedIcon.enabled = muted;
            m_MicOnIcon.enabled = !muted;
            m_LocalPlayerAudioVolume.enabled = !muted;
            PlayerHudNotification.Instance.ShowText($"<b>Microphone: {(muted ? "OFF" : "ON")}</b>");
        }
        public void SubmitNewRoomName(string text)
        {
            XRINetworkGameManager.Instance.lobbyManager.UpdateLobbyName(text);
        }

        void UpdateRoomName(string newValue)
        {
            RoomCodeText.text = $"Room Code: {XRINetworkGameManager.ConnectedRoomCode}";
            RoomNameText.text =  XRINetworkGameManager.ConnectedRoomName.Value;
            RoomNameInputField.text = XRINetworkGameManager.ConnectedRoomName.Value;
        }
          
}

