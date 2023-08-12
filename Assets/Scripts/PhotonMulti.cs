using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Networking;
using Agora.Rtc;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class PhotonMulti : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [System.Serializable]
    public class ChannelInfo
    {
        public string token;
    }

    public static PhotonMulti Instance;
    [SerializeField] private GameObject kickDialog;
    public GameObject[] roomList;
    List<RoomInfo> rooms = new List<RoomInfo>(); 
    public string getTokenUrl = "";
    private string _appID = "afee3e6b07a94b28b4736ff2c5937313";
    private string _token = "";
    private string kickUsername = "";
    private int kickAgreeNum;
    string selectedRoom = "";
    string gameVersion = "1";
    private bool isConnectedFirst = false;
    private bool isConnecting = false;
    public bool isJoinedChannel;
    internal IRtcEngine RtcEngine;

    private void Awake()
    {
        Instance = this;
        //PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        isJoinedChannel = false;
        SetupVoiceSDKEngine();
        InitEventHandler();     
    }

    void Update()
    {
        CheckPermissions();       
    }

    void ConnectToRegion()
    {
        AppSettings regionSettings = new()
        {
            UseNameServer = true,
            //FixedRegion = "usw",
            FixedRegion = "eu",
            AppIdRealtime = "934be238-d4a9-407f-8144-0ae0c4350d33",
            AppVersion = gameVersion,
        };
        PhotonNetwork.ConnectUsingSettings(regionSettings);

        //PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.GameVersion = gameVersion;
    }

    public void Connect()
    {
        isConnectedFirst = true;

        if (PhotonNetwork.IsConnected)
        {
            isConnecting = true;
            MainUI.Instance.onlineUI.SetActive(true);                  
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            isConnecting = true;    
            ConnectToRegion();            
            MainUI.Instance.loadingUI.SetActive(true);
        }
    }

    public void CreateBtnClick()
    {
        MainUI.Instance.loadingUI.SetActive(true);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)4;

        // disconnect issue
        roomOptions.CleanupCacheOnLeave = false;
        //roomOptions.PlayerTtl = -1;
        //roomOptions.EmptyRoomTtl = 500;
        PhotonNetwork.KeepAliveInBackground = 60000;

        PhotonNetwork.CreateRoom(MainUI.Instance.userAvatarName.text + MainUI.CreateRandomNumber(), roomOptions, TypedLobby.Default);
        PhotonNetwork.NickName = MainUI.Instance.userAvatarName.text + "#" + PlayerPrefs.GetInt("GENDER") + "#" + PlayerPrefs.GetInt("AVATAR_INDEX");

        MainUI.Instance.roomAvatar[0].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName.Split("#")[0];
        MainUI.Instance.roomAvatar[0].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            MainUI.Instance.userAvatar.GetComponent<Image>().sprite;        
    }

    public void RoomlistClick()
    {
        selectedRoom = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;        
        PhotonNetwork.JoinRoom(selectedRoom);
        PhotonNetwork.NickName = MainUI.Instance.userAvatarName.text + "#" + PlayerPrefs.GetInt("GENDER") + "#" + PlayerPrefs.GetInt("AVATAR_INDEX");
        MainUI.Instance.loadingUI.SetActive(true);
    }

    public void LeaveRoomBtnClick()
    {
        MainUI.Instance.roomUI.SetActive(false);
        MainUI.Instance.onlineUI.SetActive(true);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby(TypedLobby.Default);        
        LeaveVoiceChannel();    
    }

    public void ClickPlayerAvatar()
    {
        //kickUsername = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
        //kickDialog.SetActive(true);
        //kickDialog.transform.GetChild(1).GetComponent<Text>().text = "Do you agree to kick " + kickUsername + "?";
    }

    public void KickBtnClick()
    {
        kickDialog.SetActive(false);
        kickAgreeNum = 0;
        SyncKickPlayer(new object[] { kickUsername });
    }

    public void RefreshRoomList()
    {
        print("Refresh room");

        List<int> removedRooms = new List<int>();

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if (rooms[i].Name == rooms[j].Name)
                {
                    removedRooms.Add(j);
                }
            }
        } 

        for(int i = 0; i < rooms.Count; i++)
        {
            if(!rooms[i].IsVisible || !rooms[i].IsOpen)
            {
                removedRooms.Add(i);
            }
        }

        for (int i = 0; i < removedRooms.Count; i++)
        {
            rooms.RemoveAt(removedRooms[i]);
        }

        for (int i = 0; i < roomList.Length; i++)
        {
            roomList[i].SetActive(false);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            roomList[i].SetActive(true);
            roomList[i].transform.GetChild(0).GetComponent<Text>().text = rooms[i].Name;  

            if (string.Equals(selectedRoom, roomList[i].transform.GetChild(0).GetComponent<Text>().text))
            {
                roomList[i].transform.GetChild(0).GetComponent<Text>().color = Color.red;
            }
            else
            {
                roomList[i].transform.GetChild(0).GetComponent<Text>().color = Color.white;
            }
        }
    }

    public void RefreshPlayerNames()
    {
        for(int i = 0; i < 3; i++)
        {
            MainUI.Instance.roomAvatar[i + 1].transform.GetChild(0).GetComponent<Text>().text = "Open";
            MainUI.Instance.roomAvatar[i + 1].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = MainUI.Instance.defaultAvatar;
        }
        
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            string tempName = PhotonNetwork.PlayerListOthers[i].NickName;            
            MainUI.Instance.roomAvatar[i + 1].transform.GetChild(0).GetComponent<Text>().text = tempName.Split("#")[0];

            if (int.Parse(tempName.Split("#")[1]) == 0)
            {
                MainUI.Instance.roomAvatar[i + 1].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = MainUI.Instance.maleAvatars[int.Parse(tempName.Split("#")[2])];
            }
            else
            {
                MainUI.Instance.roomAvatar[i + 1].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = MainUI.Instance.femaleAvatars[int.Parse(tempName.Split("#")[2])];
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        MainUI.Instance.loadingUI.SetActive(false);
        MainUI.Instance.onlineUI.SetActive(true);

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        print("Joined Lobby");

        if (!isConnectedFirst && isConnecting)
        {
            RefreshRoomList();
        }
    }

    public override void OnJoinedRoom()
    {
        rooms = new List<RoomInfo>();           

        MainUI.Instance.roomAvatar[0].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName.Split("#")[0];
        MainUI.Instance.roomAvatar[0].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            MainUI.Instance.userAvatar.GetComponent<Image>().sprite;

        RefreshPlayerNames();   

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GetTempToken(getTokenUrl, PhotonNetwork.CurrentRoom.Name));
        }

        // test
        //MainUI.Instance.loadingUI.SetActive(false);
        //MainUI.Instance.onlineUI.SetActive(false);
        //MainUI.Instance.roomUI.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomLists)
    {
        if (isConnecting)
        {
            if (isConnectedFirst)
            {
                rooms = roomLists;
                isConnectedFirst = false;
                RefreshRoomList();
            }
            else
            {
                foreach (RoomInfo info in roomLists)
                {
                    if (info.RemovedFromList)
                    {
                        rooms.Remove(info);
                        print("Removed");
                        RefreshRoomList();
                    }
                    else
                    {
                        rooms.Add(info);
                        print("Added");
                        RefreshRoomList();
                    }
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        RefreshPlayerNames();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {   
        if(PhotonNetwork.CurrentRoom.Players.Count == 4)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SyncTempToken(new object[] { _token });
        }

        RefreshPlayerNames();
    }

    public void SyncTempToken(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncKickPlayer(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(2, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == 1)
        {
            object[] infos = (object[])photonEvent.CustomData;

            _token = (string)infos[0];        

            if (!isJoinedChannel)
            {
                JoinVoiceChannel(_token, PhotonNetwork.CurrentRoom.Name);
            }
        }  
        else if(eventCode == 2)
        {
            object[] infos = (object[])photonEvent.CustomData;
            kickAgreeNum = 0;
          
            kickDialog.SetActive(true);            
            kickUsername = (string)infos[0];
            kickDialog.transform.GetChild(1).GetComponent<Text>().text = "Do you agree to kick " + kickUsername + "?";
        }
    }

    IEnumerator GetTempToken(string url, string channel)
    {
        WWWForm json = new WWWForm();
        json.AddField("channel", channel);

        UnityWebRequest uwr = UnityWebRequest.Post(url, json);
        //uwr.SetRequestHeader("Content-Type", "application/json");
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            StartCoroutine(GetTempToken(getTokenUrl, PhotonNetwork.CurrentRoom.Name));
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            ChannelInfo loadData = JsonUtility.FromJson<ChannelInfo>(uwr.downloadHandler.text);
            _token = loadData.token;       
            JoinVoiceChannel(_token, PhotonNetwork.CurrentRoom.Name);
        }
    }

    // Agora
    private void SetupVoiceSDKEngine()
    {
        // Create an RtcEngine instance.
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        RtcEngineContext context = new RtcEngineContext(_appID, 0,
        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_MEETING);
        // Initialize RtcEngine.
        RtcEngine.Initialize(context);
    }

    public void JoinVoiceChannel(string tempToken, string channelName)
    {
        // Enables the audio module.
        RtcEngine.EnableAudio();
        // Sets the user role ad broadcaster.
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        // Joins a channel.
        RtcEngine.JoinChannel(tempToken, channelName);
    }

    public void LeaveVoiceChannel()
    {        
        // Leaves the channel.
        RtcEngine.LeaveChannel();
        // Disable the audio modules.
        RtcEngine.DisableAudio();
        isJoinedChannel = false;        
    }

    public void StartPublishAudio()
    {        
        //MainUI.Instance.micOnBtn.SetActive(false);
        //MainUI.Instance.micOffBtn.SetActive(true);
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(true);
        var nRet = RtcEngine.UpdateChannelMediaOptions(options);    
    }

    public void StopPublishAudio()
    {
        //MainUI.Instance.micOnBtn.SetActive(true);
        //MainUI.Instance.micOffBtn.SetActive(false);
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        var nRet = RtcEngine.UpdateChannelMediaOptions(options);    
    }

    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            LeaveVoiceChannel();
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }

    // Permissions
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private ArrayList permissionList = new ArrayList() { Permission.Microphone };
#endif

    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    foreach (string permission in permissionList)
    {
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Permission.RequestUserPermission(permission);
        }
    }
#endif
    }

    private void InitEventHandler()
    {
        // Creates a UserEventHandler instance.
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly PhotonMulti _audioSample;

        internal UserEventHandler(PhotonMulti audioSample)
        {
            _audioSample = audioSample;
        }

        // This callback is triggered when the local user joins the channel.
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            print("Joined Channel");
            PhotonMulti.Instance.isJoinedChannel = true;
            //MainUI.Instance.micOnBtn.SetActive(false);
            //MainUI.Instance.micOffBtn.SetActive(true);

            // UI show
            MainUI.Instance.loadingUI.SetActive(false);
            MainUI.Instance.onlineUI.SetActive(false);
            MainUI.Instance.roomUI.SetActive(true);            
        }
    }
}
