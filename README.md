# AR/VR Environment - Internship Assignment

## Project Overview

**Goal:** This project is a proof-of-concept AR/VR application designed to enable at least two users to connect, share a virtual space, and communicate verbally in real-time. The primary focus is on demonstrating multi-user connectivity including video and voice connectivity .

**Description:** This application allows multiple users to join a shared virtual room where they are represented by simple avatars. Once connected, users can engage in real-time voice chat. The right-floating UI panel acts as the display for the video functionality , including a representation of their "video feed" (using Unity RenderTextures to capture their avatar's perspective), their name, and visual cues for speaking status (microphone on/off icons and  voice indicators)

**Key Functionality Implemented:**
* Multi-user session connectivity supporting at 4 concurrent users (can be changed through code)
* Users can join a session, for example, via a Room ID using Unity Lobby
* Clear real-time voice communication using Unity Vivox
* A dynamic UI panel showing participant video feeds, names, and speaking status
* Basic handling of network disconnections and connection status feedback.
## Chosen Tech Stack
* **Development Platform:**  `Unity 6`
* **AR/VR SDK/Platform:** Unity XR Interaction Toolkit (XRIT) `Open XR and Unity XR interaction Toolkit`
* **Networking Solution:**
    * Unity Netcode for GameObjects (NGO)
    * Unity Relay (for P2P/P2R connectivity)
    * Unity Lobby (for room management and discovery)
* **Voice Communication Solution:** `Unity Vivox`
* **Editor Multiplayer Testing:** `Unity Multiplayer Play Mode (MPPM)`

## Setup Instructions

1.  **Prerequisites:**
    * Unity Hub installed.
    * Unity Editor version `6.0.25` installed.
    * Git installed on your system.
    * A Unity Account with a Unity Project ID.

2.  **Cloning the Repository**

3.  **Setting up Unity Gaming Services (UGS):**
    * In the Unity Editor, go to `Edit > Project Settings > Services`.
    * If not already prompted, link the project to your own Unity Project ID.
    * In the `Project Settings > Services` window (or the Unity Dashboard for your project ID), enable and configure:
        * **Authentication:** Set up Anonymous login.
        * **Lobby:** Default configuration
        * **Relay:** Default configuration 
        * **Vivox:** Ensure the service is enabled.
    * *(Note: The project uses the Unity VR Multiplayer Template as a base, which includes managers for these services.)*

5.  **Running the Application (Multi-User Test):**
    * **Using Unity Multiplayer Play Mode (MPPM):**
        * Ensure MPPM is set up (usually available by default in recent Unity versions for multiplayer projects).
        * In the Unity Editor toolbar, look for the MPPM settings (often a play button with a dropdown for number of clients, or under `Multiplayer > Play Mode`). Set it to create 1 clone (for a total of 2 players: the main editor instance + 1 clone).
        * Open the main scene for joining/creating rooms (e.g., `MainMenuScene` - *please specify your scene name*).
        * Press Play in the Unity Editor. Two instances of the game will launch.
        * **Host:** In one instance, create a room (e.g., enter a room name and click "Create").
        * **Client:** In the other instance, find the created room in the list (you might need to refresh) or join via Room ID if that feature is prominent, and click "Join."
          
6.  **Finding Assets**
      *The main Scene is the JoinNCreate Scene in the Scenes folder
      *Scripts are in ProjScripts
      *Visuals used are in ProjMaterials and a Unity assets store package including models for floor and wall
   
## Design Choices & Implementation Details
* **Multi-User Connectivity & Session Management:**
    * The application utilizes **Unity Lobby** for session management. Users can create a new game session (room) by providing a room name, or they can view and join existing public rooms.
    * Once a lobby is created or joined, **Unity Netcode for GameObjects (NGO)** handles the network synchronization.
    * **Unity Relay** is used as the transport layer for NGO to facilitate connections between players, helping to bypass common NAT/firewall issues.
    * The `XRINetworkGameManager` from the VR Multiplayer Template manages the overall connection flow, authentication, and lobby interactions, with custom approval callbacks implemented for connection handling.
      
* **Voice Communication:**
    * **Unity Vivox** is integrated for real-time voice communication.
    * It provides clear, bidirectional audio between all connected participants in a room.
    * The setup aims for minimal latency for a natural conversational experience.
* **UI/UX for videofeedback Panel (I tried to align it with the screenshot givin in the assignment document) :**
    * A key UI element is a **right-floating panel for video feedback** implemented using a Unity World Space Canvas with wrist menu functionality
    * This panel dynamically displays a list of all connected participants (up to a maximum of 4). (It is toggled when the user looks at the back of their right controller) as shown in the demo video
    * For each participant, the panel shows:
        * Their **display name**.
        * A **"video feed"** area: This is achieved by dedicating a camera to each player's avatar within their networked prefab. This camera outputs to a runtime-generated `RenderTexture`. This `RenderTexture` is then applied to a `Material` instance used by a `RawImage` component in the participant's UI slot, effectively creating a live "picture-in-picture" view of that participant's avatar.
        * **Speaking Indication:**
            * Microphone On/Off icons that toggle based on whether the user is transmitting audio (determined by their `selfMuted` status and voice amplitude).
            * A voice amplitude fill image that visually responds to the loudness of the participant's voice.
    * This panel provides immediate visual feedback on who is in the room and who is speaking, as per the assignment's UI/UX considerations
* **Network Robustness:**
    * The application includes basic handling for network events such as players joining and leaving sessions.
    * UI elements and the participant list are updated accordingly.
    * Connection status feedback is provided through console logs and the application attempts to manage disconnections gracefully.

## Major Bugs Encountered (and Solutions/Workarounds)

* **Client Connection Timeout:**
    * **Symptom:** Clients would often time out waiting for the server to approve their connection request, even after successfully connecting at the transport layer.
    * **Root Cause & Solution:** This was traced to the host's `ConnectionApprovalCallback` for Netcode for GameObjects. Initially, either the callback was not being triggered because `NetworkManager.NetworkConfig.ConnectionApproval` was `false`, or if triggered, the `response.Pending = false;` call was missing, preventing the approval from being finalized and sent to the client. The solution involved:
        1.  Ensuring `NetworkManager.NetworkConfig.ConnectionApproval = true;` is set when a custom callback is used.
        2.  Also Ensured that the ConnectionApproval gets executed for both host and client made it such that the NetworkGameManager will always give a connection callback either on hosting or joining
        3.  Explicitly calling `response.Pending = false;` within the `ConnectionApprovalCallback` after setting `response.Approved = true;`.

* **Intermittent Client Disconnects in Multiplayer Play Mode (MPPM):**
    * **Symptom:** Occasionally, but not consistently reproducible, a client instance in MPPM would disconnect from the host shortly after joining, without a clear timeout or specific error message indicating the cause.
    * **Investigation:** This seemed to be related to potential state inconsistencies that could occur within the editor when running multiple instances via MPPM.
    * **Workaround:** Restarting the Multiplayer Play Mode session (stopping and restarting the play mode with clones) typically resolved the issue for subsequent attempts.
    * **OnemoreImpNote** Plz Make sure the MPPM version is 1.4.0 I am not sure if cloning the repo will revert the packages version so double check for that (MPPM 1.4.0 can be get by updating the Manifest.json `Rootdir\Packages\Manifest.json`)

## Demo Video:
  `https://drive.google.com/file/d/1LIizMLR5mTDIo-SS_Cnv_a_SrcPYmIFB/view?usp=sharing` 
  * though There is no audio in the video the voicechat functionality works


