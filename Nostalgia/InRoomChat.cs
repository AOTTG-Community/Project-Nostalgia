using Photon;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InRoomChat : Photon.MonoBehaviour
{
    private bool AlignBottom = true;
    public static readonly string ChatRPC = "Chat";
    public static Rect GuiRect = new Rect(0f, 100f, 300f, 470f);
    public static Rect GuiRect2 = new Rect(30f, 575f, 300f, 25f);
    private string inputLine = string.Empty;
    public bool IsVisible = true;
    public static List<string> messages = new List<string>();
    private Vector2 scrollPos = Vector2.zero;

    public void addLINE(string newLine)
    {
        messages.Add(newLine);
    }

    public void AddLine(string newLine)
    {
        messages.Add(newLine);
    }

    public void OnGUI()
    {
        if (this.IsVisible && (PhotonNetwork.connectionStateDetailed == global::PeerState.Joined))
        {
            if ((Event.current.type == EventType.KeyDown) && ((Event.current.keyCode == KeyCode.KeypadEnter) || (Event.current.keyCode == KeyCode.Return)))
            {
                if (!string.IsNullOrEmpty(this.inputLine))
                {
                    if (this.inputLine == "\t")
                    {
                        this.inputLine = string.Empty;
                        GUI.FocusControl(string.Empty);
                        return;
                    }
                    if ((this.inputLine == "/restart") && PhotonNetwork.isMasterClient)
                    {
                        this.inputLine = string.Empty;
                        GUI.FocusControl(string.Empty);
                        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().restartGame(false);
                        return;
                    }
                    if ((this.inputLine.Length <= 7) || !(this.inputLine.Substring(0, 7) == "/kick #"))
                    {
                        object[] parameters = new object[] { this.inputLine, LoginFengKAI.player.name };
                        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().photonView.RPC("Chat", PhotonTargets.All, parameters);
                    }
                    else if (this.inputLine.Remove(0, 7) == PhotonNetwork.masterClient.ID.ToString())
                    {
                        GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("error:can't kick master client.");
                    }
                    else if (this.inputLine.Remove(0, 7) == PhotonNetwork.player.ID.ToString())
                    {
                        GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("error:can't kick yourself.");
                    }
                    else
                    {
                        bool flag = false;
                        foreach (PhotonPlayer player in PhotonNetwork.playerList)
                        {
                            if (player.ID.ToString() == this.inputLine.Remove(0, 7))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("error:no such player.");
                        }
                        else
                        {
                            object[] objArray1 = new object[] { this.inputLine, LoginFengKAI.player.name };
                            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().photonView.RPC("Chat", PhotonTargets.All, objArray1);
                        }
                    }
                    this.inputLine = string.Empty;
                    GUI.FocusControl(string.Empty);
                    return;
                }
                this.inputLine = "\t";
                GUI.FocusControl("ChatInput");
            }
            GUI.SetNextControlName(string.Empty);
            GUILayout.BeginArea(GuiRect);
            GUILayout.FlexibleSpace();
            string text = string.Empty;
            if (messages.Count < 10)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    text = text + messages[i] + "\n";
                }
            }
            else
            {
                for (int j = messages.Count - 10; j < messages.Count; j++)
                {
                    text = text + messages[j] + "\n";
                }
            }
            GUILayout.Label(text, new GUILayoutOption[0]);
            GUILayout.EndArea();
            GUILayout.BeginArea(GuiRect2);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUI.SetNextControlName("ChatInput");
            this.inputLine = GUILayout.TextField(this.inputLine, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    public void setPosition()
    {
        if (this.AlignBottom)
        {
            GuiRect = new Rect(0f, (float) (Screen.height - 500), 300f, 470f);
            GuiRect2 = new Rect(30f, (float) ((Screen.height - 300) + 0x113), 300f, 25f);
        }
    }

    public void Start()
    {
        this.setPosition();
    }
}

