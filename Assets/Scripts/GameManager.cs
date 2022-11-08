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

    private UserControl userControl;

    [SerializeField]
    private TMP_InputField nickName;
    [SerializeField]
    private TMP_InputField chat;

    private string id;

    public GameObject User;

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

    public void OnLogin()
    {
        id = nickName.text;
        if (id != null)
        {
            SocketModule.Instance.Login(id);
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

    public void OnMessage(string str)
    {
        chat.text = "";
        textArea.text += "\n" + id + ": " + str;
        if (id != null)
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
                    if (id.CompareTo(id) != 0)
                    {
                        switch (command)
                        {
                            case "Enter":
                                break;
                            case "Left":
                                break;
                            case "Move":
                                break;
                            case "Attack":
                                break;
                            case "Damage":
                                break;
                            case "Heal":
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