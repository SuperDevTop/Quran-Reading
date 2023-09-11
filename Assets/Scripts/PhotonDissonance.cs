using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Networking;
using Dissonance;
using Dissonance.Networking;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class PhotonDissonance : MonoBehaviourPunCallbacks, IOnEventCallback
{   
    [SerializeField] private GameObject kickDialog;
    [SerializeField] private GameObject micOnBtn;
    [SerializeField] private GameObject micOffBtn;
    [SerializeField] private GameObject dissonanceSettings;
    [SerializeField] private Text alertText;
    public GameObject[] roomList;
    List<RoomInfo> rooms = new List<RoomInfo>();
    private string kickUsername = "";
    string selectedRoom = "";
    string gameVersion = "1";
    private bool isConnectedFirst = false;
    private bool isConnecting = false;
    public bool isJoinedChannel;
    private DissonanceComms _comms;
    RoomMembership rMembership;

    //private int kickAgreeNum;
    //public string testvalue;

    private void Awake()
    {
        //Instance = this;
        //PhotonNetwork.AutomaticallySyncScene = true;
        if (Advertisements.Instance.UserConsentWasSet())
        {
            Advertisements.Instance.Initialize();
        }
        else
        {
            Advertisements.Instance.Initialize();
        }
    }

    void Start()
    {
        isJoinedChannel = false;
        _comms = FindObjectOfType<DissonanceComms>();
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
        string roomName = MainUI.Instance.userAvatarName.text + "#" + MainUI.CreateRandomNumber();

        if (EventSystem.current.currentSelectedGameObject.name == "AllBtn")
        {
            roomName += "???All";
        }
        else if(EventSystem.current.currentSelectedGameObject.name == "MaleBtn")
        {
            roomName += "???Male";
        }
        else
        {
            roomName += "???Female";
        }                
        
        MainUI.Instance.loadingUI.SetActive(true);
        MainUI.Instance.genderSelection.SetActive(false);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)4;

        // disconnect issue
        roomOptions.CleanupCacheOnLeave = false;
        //roomOptions.PlayerTtl = -1;
        //roomOptions.EmptyRoomTtl = 500;
        PhotonNetwork.KeepAliveInBackground = 60000;

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        PhotonNetwork.NickName = MainUI.Instance.userAvatarName.text + "#" + PlayerPrefs.GetInt("GENDER") + "#" + PlayerPrefs.GetInt("AVATAR_INDEX");

        MainUI.Instance.roomAvatar[0].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName.Split("#")[0];
        MainUI.Instance.roomAvatar[0].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            MainUI.Instance.userAvatar.GetComponent<Image>().sprite;
    }

    public void RoomlistClick()
    {
        selectedRoom = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
        string genderStr = EventSystem.current.currentSelectedGameObject.transform.GetChild(1).GetComponent<Text>().text;
        selectedRoom += "???" + genderStr;

        if(genderStr == "All" || genderStr == "Male" && PlayerPrefs.GetInt("GENDER") == 0
            || genderStr == "Female" && PlayerPrefs.GetInt("GENDER") == 1)
        {
            PhotonNetwork.JoinRoom(selectedRoom);
            PhotonNetwork.NickName = MainUI.Instance.userAvatarName.text + "#" + PlayerPrefs.GetInt("GENDER") + "#" + PlayerPrefs.GetInt("AVATAR_INDEX");
            MainUI.Instance.loadingUI.SetActive(true);
        }
        else
        {
            StartCoroutine(DelayToShowAlert("You can't join this room."));
        }        
    }

    public void LeaveRoomBtnClick()
    {
        MainUI.Instance.roomUI.SetActive(false);
        MainUI.Instance.onlineUI.SetActive(true);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby(TypedLobby.Default);

        _comms.Rooms.Leave(rMembership);

        StartCoroutine(ShowingBanner());
        ShowInterstitial();
    }

    public void ClickPlayerAvatar()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            kickUsername = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            Sprite tempSprite = EventSystem.current.currentSelectedGameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite;

            if (tempSprite != MainUI.Instance.defaultAvatar && kickUsername != "Open")
            {
                kickDialog.SetActive(true);
                kickDialog.transform.GetChild(1).GetComponent<Text>().text = "Do you agree to kick " + kickUsername + "?";
            }
        }
    }

    public void KickBtnClick()
    {
        kickDialog.SetActive(false);
        //kickAgreeNum = 0;
        SyncKickPlayer(new object[] { kickUsername });
    }

    public void MicOnClick()
    {
        micOffBtn.SetActive(false);
        micOnBtn.SetActive(true);
        dissonanceSettings.GetComponent<VoiceBroadcastTrigger>().IsMuted = false;
    }

    public void MicOffClick()
    {
        micOnBtn.SetActive(false);
        micOffBtn.SetActive(true);
        dissonanceSettings.GetComponent<VoiceBroadcastTrigger>().IsMuted = true;
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

        for (int i = 0; i < rooms.Count; i++)
        {
            if (!rooms[i].IsVisible || !rooms[i].IsOpen)
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
            roomList[i].transform.GetChild(0).GetComponent<Text>().text = rooms[i].Name.Split("???")[0];
            roomList[i].transform.GetChild(1).GetComponent<Text>().text = rooms[i].Name.Split("???")[1];

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
        for (int i = 0; i < 3; i++)
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
        print("Joined room");
        rooms = new List<RoomInfo>();

        MainUI.Instance.roomAvatar[0].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName.Split("#")[0];
        MainUI.Instance.roomAvatar[0].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            MainUI.Instance.userAvatar.GetComponent<Image>().sprite;

        RefreshPlayerNames();

        rMembership = _comms.Rooms.Join(PhotonNetwork.CurrentRoom.Name); 

        // test
        MainUI.Instance.loadingUI.SetActive(false);
        MainUI.Instance.onlineUI.SetActive(false);
        MainUI.Instance.roomUI.SetActive(true);

        micOffBtn.SetActive(false);
        micOnBtn.SetActive(true);
        dissonanceSettings.GetComponent<VoiceBroadcastTrigger>().IsMuted = false;
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
        if (PhotonNetwork.CurrentRoom.Players.Count == 4)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
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
        }
        else if (eventCode == 2)
        {
            object[] infos = (object[])photonEvent.CustomData;
            string kickName = (string)infos[0];

            if (PhotonNetwork.NickName.Split("#")[0] == kickName)
            {
                LeaveRoomBtnClick();
            }            
        }
    }

    IEnumerator DelayToShowAlert(string str)
    {
        alertText.text = str;
        alertText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        alertText.gameObject.SetActive(false);
    }

    IEnumerator ShowingBanner()
    {
        ShawBanner();

        yield return new WaitForSeconds(3f);

        HideBanner();
    }

    // integrate ads
    public void ShawBanner()
    {
        Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM);
    }

    public void HideBanner()
    {
        Advertisements.Instance.HideBanner();
    }

    public void ShowInterstitial()
    {
        Advertisements.Instance.ShowInterstitial();
    }

    void OnApplicationQuit()
    {
        _comms.Rooms.Leave(rMembership);
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
}
