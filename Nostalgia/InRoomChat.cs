using Photon;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        if (!this.IsVisible && (PhotonNetwork.connectionStateDetailed != global::PeerState.Joined))
        {
            return;
        }
        if ((Event.current.type == EventType.KeyDown) && ((Event.current.keyCode == KeyCode.KeypadEnter) || (Event.current.keyCode == KeyCode.Return)))
        {
            if (!string.IsNullOrEmpty(this.inputLine))
            {
                /**
                 * This regex is used to parse input chat commands
                 * @command and @args are used in "/command arg" type input
                 * @single is used when "/command" type input
                 * @tab because tab
                 * @any when any normal chat input
                 */
                Match matches = Regex.Match(this.inputLine, @"\\|\/(?<command>\w*)\s(?<args>[^\s].*)|\\|\/(?<single>\w*)|(?<tab>\t)|(?<any>^[^\\\/].*?$)");

                if (matches.Groups["tab"].Value != "")
                {
                    this.inputLine = string.Empty;
                    GUI.FocusControl(string.Empty);
                    return;
                }

                // Add normal commands without arguments
                if (matches.Groups["single"].Value != "")
                {
                    // Could be done by using ifs but whatever
                    string command = matches.Groups["single"].Value.Trim();
                    switch (command)
                    {
                        case "restart": // Restarts the game
                            if (PhotonNetwork.isMasterClient)
                            {
                                this.inputLine = string.Empty;
                                GUI.FocusControl(string.Empty);
                                GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().restartGame(false);
                            }
                            break;
                        // Add more commands
                        default:
                            break;
                    }
                    return;
                }

                if (matches.Groups["command"].Value != "" && matches.Groups["args"].Value != "")
                {
                    string command = matches.Groups["command"].Value.Trim();
                    string arg = matches.Groups["args"].Value.Trim();
                    switch (command)
                    {
                        case "kick": // Kick player command
                            if (arg.StartsWith("#"))
                            {
                                string ID = arg.Substring(1);
                                if (ID == PhotonNetwork.masterClient.ID.ToString())
                                {
                                    GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("Error: you can't kick master client.");
                                    break;
                                }
                                if (ID == PhotonNetwork.player.ID.ToString())
                                {
                                    GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("Error: you can't kick yourself.");
                                    break;
                                }
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
                                    GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("Error: player with ID: " + ID.ToString() + " doesn't exist");
                                    break;
                                }

                                object[] parameters = new object[] { this.inputLine, LoginFengKAI.player.name };
                                GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().photonView.RPC("Chat", PhotonTargets.All, parameters);
                            } else
                            {
                                GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE("Invalid command, missing #");
                            }
                            break;
                        // Add more commands
                        default:
                            break;
                    }
                }

                // Any chat message
                if (matches.Groups["any"].Value != "")
                {
                    object[] objArray1 = new object[] { this.inputLine, LoginFengKAI.player.name };
                    GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().photonView.RPC("Chat", PhotonTargets.All, objArray1);
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

    public void setPosition()
    {
        if (this.AlignBottom)
        {
            GuiRect = new Rect(0f, (float)(Screen.height - 500), 300f, 470f);
            GuiRect2 = new Rect(30f, (float)((Screen.height - 300) + 0x113), 300f, 25f);
        }
    }

    public void Start()
    {
        this.setPosition();
    }
}

