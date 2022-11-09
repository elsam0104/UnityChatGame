using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class GameManager : MonoSingleton<GameManager>
{
    const char CHAR_TEMINATOR = ';';
    const char CHAR_COMMA = ',';
    const int DAMAGE_ATTACK = 20;

    private UserControl userControl;

    [SerializeField]
    private TMP_InputField nickName;
    [SerializeField]
    private TMP_InputField chat;

    private string myid;

    public GameObject User;
    public GameObject prefabUser;

    private Dictionary<string, UserControl> remoteUsers;
    private Queue<string> commandQueue;


    [SerializeField]
    private Button LogInBtn;
    [SerializeField]
    private Button LogOutBtn;
    [SerializeField]
    private Button ReviveBtn;
    [SerializeField]
    private TextMeshProUGUI textArea;
    private void Start()
    {
        userControl = GetComponent<UserControl>();
        remoteUsers = new Dictionary<string, UserControl>();
        commandQueue = new Queue<string>();

        LogInBtn.onClick.AddListener(OnLogin);
        LogOutBtn.onClick.AddListener(OnLogOut);
        ReviveBtn.onClick.AddListener(OnRevive);
        chat.onSubmit.AddListener(OnMessage);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (userControl.GetHP() > 0)
                {
                    SendCommand("#Attack#");
                }
            }
        }
    }

    public void OnLogin()
    {
        myid = nickName.text;
        if (myid != null)
        {
            SocketModule.Instance.Login(myid);
            userControl.transform.position = Vector3.zero;
            OnRevive();
        }
    }
    public void OnLogOut()
    {
        SocketModule.Instance.LogOut();
        foreach (var item in remoteUsers)
        {
            Destroy(item.Value.gameObject);
        }
        remoteUsers.Clear();
    }

    public void OnRevive()
    {
        SendCommand("#Heal#");
        userControl.Revive();
    }

    public UserControl AddUser(string id)
    {
        UserControl userControl = null;
        if (!remoteUsers.ContainsKey(id))
        {
            GameObject newUser = Instantiate(prefabUser);
            userControl = newUser.GetComponent<UserControl>();
            remoteUsers.Add(id, userControl);
        }
        return userControl;
    }
    public void UserLeft(string id)
    {
        if (remoteUsers.ContainsKey(id))
        {
            UserControl userControl = remoteUsers[id];
            Destroy(userControl.gameObject);
            remoteUsers.Remove(id);
        }
    }
    public void UserHeal(string id)
    {
        if (remoteUsers.ContainsKey(id))
        {
            UserControl userControl = remoteUsers[id];
            userControl.Revive();
        }
    }
    public void TakeDamage(string remain)
    {
        var strs = remain.Split(CHAR_COMMA);
        for (int i = 0; i < strs.Length; i++)
        {
            if (remoteUsers.ContainsKey(strs[i]))
            {
                UserControl userControl = remoteUsers[strs[i]];
                if (userControl != null)
                {
                    userControl.DropHP(DAMAGE_ATTACK);
                }
            }
            else
            {
                if (myid.CompareTo(strs[i]) == 0)
                {
                    userControl.DropHP(DAMAGE_ATTACK);
                }
            }
        }
    }
    public void SetMove(string id, string cmdMove)
    {
        if(remoteUsers.ContainsKey(id))
        {
            UserControl userControl = remoteUsers[id];
            string[] xy = cmdMove.Split(CHAR_COMMA);
            Vector3 pos = new Vector3(float.Parse(xy[0]), float.Parse(xy[1]));
            userControl.SetTargetPos();
        }
    }

    public void OnMessage(string str)
    {
        chat.text = "";
        textArea.text += "\n" + myid + ": " + str;
        if (myid != null)
        {
            SocketModule.Instance.SendData(chat.text);
        }
    }

    public void SendCommand(string cmd)
    {
        SocketModule.Instance.SendData(cmd);
        Debug.Log("cmd send: " + cmd);
    }


    public void QueueCommand(string cmd)
    {
        commandQueue.Enqueue(cmd);
    }

    public void ProcessQueue()
    {
        while (commandQueue.Count > 0)
        {
            string nextCommand = commandQueue.Dequeue();
            ProcessCommand(nextCommand);
        }
    }

    public void ProcessCommand(string cmd)
    {
        bool isMore = true;
        while (isMore)
        {
            Debug.Log("Process cmd " + cmd);
            //id
            int nameIdx = cmd.IndexOf('$');
            string id = "";

            if (nameIdx > 0)
            {
                id = cmd.Substring(0, nameIdx);
            }
            //command
            int cmdIdx1 = cmd.IndexOf('#');
            if (cmdIdx1 > nameIdx)
            {
                int cmdIdx2 = cmd.IndexOf('#', cmdIdx1 + 1);
                if (cmdIdx2 > cmdIdx1)
                {
                    string command = cmd.Substring(cmdIdx1 + 1, cmdIdx2 - cmdIdx1 - 1);
                    //end
                    string remain = "";
                    string nextCommand;
                    int endIdx = cmd.IndexOf(CHAR_TEMINATOR, cmdIdx2 + 1);
                    if (endIdx > cmdIdx2)
                    {
                        remain = cmd.Substring(cmdIdx2 + 1, endIdx - cmdIdx2 - 1);
                        nextCommand = cmd.Substring(endIdx + 1);
                    }
                    else
                    {
                        nextCommand = cmd.Substring(cmdIdx2 + 1);
                    }
                    if (myid.CompareTo(id) != 0)
                    {
                        switch (command)
                        {
                            case "Enter":
                                AddUser(id);
                                break;
                            case "Left":
                                UserLeft(id);
                                break;
                            case "Move":
                                SetMove(id, remain);
                                break;
                            case "Attack":

                                break;
                            case "Damage":
                                TakeDamage(remain);
                                break;
                            case "Heal":
                                UserHeal(id);
                                break;
                            case "History":
                                break;
                        }
                    }
                    else
                    {
                        Debug.Log("Command has not matched. Skip");
                    }
                    cmd = nextCommand;
                    if (cmd.Length <= 0)
                    {
                        isMore = false;
                    }
                }
                else
                {
                    isMore = false;
                }
            }
            else
            {
                isMore = false;
            }
        }//while
    }

}