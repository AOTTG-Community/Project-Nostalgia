using Optimization;
using Optimization.Caching;
using System.Collections.Generic;
using UnityEngine;

public class InRoomChat : Photon.MonoBehaviour
{
    private bool AlignBottom = true;
    public static InRoomChat Chat;
    private static  string chatString = "";
    private string inputLine = string.Empty;
    private static int minIndex = 0;
    private Vector2 scrollPos = Vectors.v2zero;
    public static readonly string ChatRPC = "Chat";
    public static Rect GuiRect = new Rect(0f, 100f, 300f, 470f);

    public static Rect GuiRect2 = new Rect(30f, 575f, 300f, 25f);

    public static List<string> messages = new List<string>();
    public bool IsVisible = true;

    public void AddLine(string newLine)
    {
        messages.Add(newLine);
        if (messages.Count > 10) minIndex++;
        chatString = "";
        for(int i = minIndex; i < messages.Count; i++)
        {
            chatString += messages[i] + (i == messages.Count - 1 ? "" : "\n");
        }
    }

    private void Awake()
    {
        Chat = this;
    }

    public static void Clear()
    {
        messages.Clear();
        chatString = string.Empty;
    }

    public void OnGUI()
    {
        if (!this.IsVisible || PhotonNetwork.connectionStateDetailed != PeerState.Joined)
        {
            return;
        }
        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
        {
            if (!inputLine.IsNullOrWhiteSpace())
            {
                if (this.inputLine == "\t")
                {
                    inputLine = string.Empty;
                    GUI.FocusControl(string.Empty);
                    return;
                }
                if (RCManager.RCEvents.ContainsKey("OnChatInput"))
                {
                    string key = (string)RCManager.RCVariableNames["OnChatInput"];
                    if (RCManager.stringVariables.ContainsKey(key))
                    {
                        RCManager.stringVariables[key] = this.inputLine;
                    }
                    else
                    {
                        RCManager.stringVariables.Add(key, this.inputLine);
                    }
                    ((RCEvent)RCManager.RCEvents["OnChatInput"]).checkEvent();
                }
                if (!inputLine.StartsWith("/"))
                {
                    FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[] { this.inputLine, LoginFengKAI.player.name });
                }
                else
                {
                    string[] args = inputLine.Remove(0, 1).ToLower().Split(' ');
                    switch (args[0])
                    {
                        case "restart":
                            FengGameManagerMKII.FGM.RestartGame(false);
                            FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[] { "<color=#A8FF24>MasterClient has restarted the game!</color>", "" });
                            break;

                        case "kick":
                            {
                                int ID;
                                if (!PhotonNetwork.IsMasterClient)
                                {
                                    AddLine("<color=red><b>Error: </b></color>Not MC!");
                                    break;
                                }
                                else if (!int.TryParse(args[1], out ID))
                                {
                                    AddLine("<color=red><b>Error: </b></color>Invalid input.");
                                    break;
                                }
                                PhotonPlayer player = PhotonPlayer.Find(ID);
                                if (player == null)
                                {
                                    AddLine("<color=red><b>Error: </b></color>No such player.");
                                    break;
                                }
                                PhotonNetwork.CloseConnection(player);
                                FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[] { $"<color=#A8FF24>Player [{ID}] {player.UIName.ToRGBA()} has been kicked!</color>", "" });
                            }
                            break;
                    }
                }
                inputLine = string.Empty;
                GUI.FocusControl(string.Empty);
                return;
            }
            else
            {
                this.inputLine = "\t";
                GUI.FocusControl("ChatInput");
            }
        }
        GUI.SetNextControlName(string.Empty);
        GUILayout.BeginArea(GuiRect);
        GUILayout.FlexibleSpace();
        //string text = string.Empty;
        //if (InRoomChat.messages.Count < 10)
        //{
        //    for (int j = 0; j < InRoomChat.messages.Count; j++)
        //    {
        //        text = text + InRoomChat.messages[j] + "\n";
        //    }
        //}
        //else
        //{
        //    for (int k = InRoomChat.messages.Count - 10; k < InRoomChat.messages.Count; k++)
        //    {
        //        text = text + InRoomChat.messages[k] + "\n";
        //    }
        //}
        GUILayout.Label(chatString, new GUILayoutOption[0]);
        GUILayout.EndArea();
        GUILayout.BeginArea(InRoomChat.GuiRect2);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        GUI.SetNextControlName("ChatInput");
        this.inputLine = GUILayout.TextField(this.inputLine, new GUILayoutOption[0]);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public void setPosition()
    {
        if (this.AlignBottom)
        {
            InRoomChat.GuiRect = new Rect(0f, (float)(Screen.height - 500), 300f, 470f);
            InRoomChat.GuiRect2 = new Rect(30f, (float)(Screen.height - 300 + 275), 300f, 25f);
        }
    }

    public void Start()
    {
        this.setPosition();
    }
}