using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainUI : MonoBehaviour
{
    public static MainUI Instance;
    [SerializeField] private GameObject mainUI;
    public GameObject firstUI;
    public GameObject onlineUI;
    public GameObject roomUI;
    public GameObject loadingUI;
    public GameObject userAvatar;
    //public GameObject micOnBtn;
    //public GameObject micOffBtn;
    public Sprite defaultAvatar;
    public GameObject[] roomAvatar;
    [SerializeField] private GameObject userSettingsUI;
    [SerializeField] private GameObject avatarImages;
    [SerializeField] private GameObject avatarImageSettings;    
    public Text userAvatarName;
    [SerializeField] private InputField inputUsername;
    [SerializeField] private Dropdown selectGender;
    public Sprite[] maleAvatars;
    public Sprite[] femaleAvatars;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitiateParameters();
    }

    void Update()
    {
        mainUI.transform.localScale = new Vector3(Screen.width / 1440f, Screen.height / 720f, 1f);
    }

    public void InitiateParameters()
    {
        if(PlayerPrefs.GetString("USER_NAME") != "")
        {
            userAvatarName.text = PlayerPrefs.GetString("USER_NAME");
            inputUsername.text = PlayerPrefs.GetString("USER_NAME");
        }
        else
        {
            userAvatarName.text = "Guest";
            PlayerPrefs.SetString("USER_NAME", "Guest");
        }                
        
        if (PlayerPrefs.GetInt("GENDER") == 0)
        {
            selectGender.value = 0;
            userAvatar.GetComponent<Image>().sprite = maleAvatars[PlayerPrefs.GetInt("AVATAR_INDEX")];
            avatarImageSettings.GetComponent<Image>().sprite = maleAvatars[PlayerPrefs.GetInt("AVATAR_INDEX")];

            for (int i = 0; i < maleAvatars.Length; i++)
            {
                avatarImages.transform.GetChild(i).GetComponent<Image>().sprite = maleAvatars[i];
            }
        }
        else
        {
            selectGender.value = 1;
            userAvatar.GetComponent<Image>().sprite = femaleAvatars[PlayerPrefs.GetInt("AVATAR_INDEX")];
            avatarImageSettings.GetComponent<Image>().sprite = femaleAvatars[PlayerPrefs.GetInt("AVATAR_INDEX")];

            for (int i = 0; i < maleAvatars.Length; i++)
            {
                avatarImages.transform.GetChild(i).GetComponent<Image>().sprite = femaleAvatars[i];
            }
        }
    }

    // User settings
    public void GenderSelection()
    {
        if(selectGender.value == 0)
        {
            PlayerPrefs.SetInt("GENDER", 0);
        }
        else
        {
            PlayerPrefs.SetInt("GENDER", 1);
        }

        InitiateParameters();
    }
    
    public void SelecteAvatar()
    {
        PlayerPrefs.SetInt("AVATAR_INDEX", int.Parse(EventSystem.current.currentSelectedGameObject.name.Split(" ")[1]) - 1);
        avatarImageSettings.GetComponent<Image>().sprite = EventSystem.current.currentSelectedGameObject.GetComponent<Image>().sprite;
    }

    public void SaveUserSettings()
    {
        if(inputUsername.text != "")
        {
            userSettingsUI.SetActive(false);
            PlayerPrefs.SetString("USER_NAME", inputUsername.text);
            InitiateParameters();
        }                
    }

    public static int CreateRandomNumber()
    {
        int tempRandom;
        tempRandom = Random.Range(2000, 8000) + System.DateTime.Now.Millisecond % 1000;

        return tempRandom;
    }
}
