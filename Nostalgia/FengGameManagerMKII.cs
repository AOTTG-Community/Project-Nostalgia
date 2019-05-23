using ExitGames.Client.Photon;
using Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FengGameManagerMKII : Photon.MonoBehaviour
{
    public static readonly string applicationId = "f1f6195c-df4a-40f9-bae5-4744c32901ef";
    private ArrayList chatContent;
    public GameObject checkpoint;
    private ArrayList cT;
    private float currentSpeed;
    public int difficulty;
    private bool endRacing;
    private ArrayList eT;
    private ArrayList fT;
    private float gameEndCD;
    private float gameEndTotalCDtime = 9f;
    public bool gameStart;
    private bool gameTimesUp;
    private ArrayList heroes;
    private int highestwave = 1;
    private ArrayList hooks;
    private int humanScore;
    public FengCustomInputs inputManager;
    private bool isLosing;
    private bool isPlayer1Winning;
    private bool isPlayer2Winning;
    private bool isWinning;
    public bool justSuicide;
    private ArrayList kicklist;
    private ArrayList killInfoGO = new ArrayList();
    public static bool LAN;
    public static string level = string.Empty;
    private string localRacingResult;
    private IN_GAME_MAIN_CAMERA mainCamera;
    private float maxSpeed;
    private string myLastHero;
    private string myLastRespawnTag = "playerRespawn";
    public float myRespawnTime;
    public bool needChooseSide;
    public int PVPhumanScore;
    private int PVPhumanScoreMax = 200;
    public int PVPtitanScore;
    private int PVPtitanScoreMax = 200;
    private ArrayList racingResult;
    public float roundTime;
    private int single_kills;
    private int single_maxDamage;
    private int single_totalDamage;
    private bool startRacing;
    private int[] teamScores;
    private int teamWinner;
    public int time = 600;
    private float timeElapse;
    private float timeTotalServer;
    private ArrayList titans;
    private int titanScore;
    private GameObject ui;
    public int wave = 1;

    public void addCamera(IN_GAME_MAIN_CAMERA c)
    {
        this.mainCamera = c;
    }

    public void addCT(COLOSSAL_TITAN titan)
    {
        this.cT.Add(titan);
    }

    public void addET(TITAN_EREN hero)
    {
        this.eT.Add(hero);
    }

    public void addFT(FEMALE_TITAN titan)
    {
        this.fT.Add(titan);
    }

    public void addHero(HERO hero)
    {
        this.heroes.Add(hero);
    }

    public void addHook(Bullet h)
    {
        this.hooks.Add(h);
    }

    public void addTitan(TITAN titan)
    {
        this.titans.Add(titan);
    }

    [RPC]
    private void Chat(string content, string sender)
    {
        if ((content.Length > 7) && (content.Substring(0, 7) == "/kick #"))
        {
            if (PhotonNetwork.isMasterClient)
            {
                this.kickPlayer(content.Remove(0, 7), sender);
            }
        }
        else
        {
            if (sender != string.Empty)
            {
                content = sender + ":" + content;
            }
            GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE(content);
        }
    }

    private bool checkIsTitanAllDie()
    {
        foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("titan"))
        {
            if ((obj2.GetComponent<TITAN>() != null) && !obj2.GetComponent<TITAN>().hasDie)
            {
                return false;
            }
            if (obj2.GetComponent<FEMALE_TITAN>() != null)
            {
                return false;
            }
        }
        return true;
    }

    public void checkPVPpts()
    {
        if (this.PVPtitanScore >= this.PVPtitanScoreMax)
        {
            this.PVPtitanScore = this.PVPtitanScoreMax;
            this.gameLose();
        }
        else if (this.PVPhumanScore >= this.PVPhumanScoreMax)
        {
            this.PVPhumanScore = this.PVPhumanScoreMax;
            this.gameWin();
        }
    }

    private void core()
    {
        if ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && this.needChooseSide)
        {
            if (GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().isInputDown[InputCode.flare1])
            {
                if (NGUITools.GetActive(this.ui.GetComponent<UIReferArray>().panels[3]))
                {
                    Screen.lockCursor = true;
                    Screen.showCursor = true;
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[0], true);
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[1], false);
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[2], false);
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[3], false);
                    GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = false;
                    GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = false;
                }
                else
                {
                    Screen.lockCursor = false;
                    Screen.showCursor = true;
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[0], false);
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[1], false);
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[2], false);
                    NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[3], true);
                    GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = true;
                    GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = true;
                }
            }
            if (GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().isInputDown[15] && !NGUITools.GetActive(this.ui.GetComponent<UIReferArray>().panels[3]))
            {
                NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[0], false);
                NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[1], true);
                NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[2], false);
                NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[3], false);
                Screen.showCursor = true;
                Screen.lockCursor = false;
                GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = true;
                GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = true;
                GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().showKeyMap();
                GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().justUPDATEME();
                GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().menuOn = true;
            }
        }
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE) || (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER))
        {
            string str11;
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
            {
                string content = string.Empty;
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    if (player.customProperties[PhotonPlayerProperty.dead] != null)
                    {
                        str11 = content;
                        object[] objArray1 = new object[] { str11, "[ffffff]#", player.ID, " " };
                        content = string.Concat(objArray1);
                        if (player.isLocal)
                        {
                            content = content + "> ";
                        }
                        if (player.isMasterClient)
                        {
                            content = content + "M ";
                        }
                        if ((bool) player.customProperties[PhotonPlayerProperty.dead])
                        {
                            content = content + "[" + ColorSet.color_red + "] *dead* ";
                            if (((int) player.customProperties[PhotonPlayerProperty.isTitan]) == 2)
                            {
                                content = content + "[" + ColorSet.color_titan_player + "] T ";
                            }
                            if (((int) player.customProperties[PhotonPlayerProperty.isTitan]) == 1)
                            {
                                if (((int) player.customProperties[PhotonPlayerProperty.team]) == 2)
                                {
                                    content = content + "[" + ColorSet.color_human_1 + "] H ";
                                }
                                else
                                {
                                    content = content + "[" + ColorSet.color_human + "] H ";
                                }
                            }
                        }
                        else
                        {
                            if (((int) player.customProperties[PhotonPlayerProperty.isTitan]) == 2)
                            {
                                content = content + "[" + ColorSet.color_titan_player + "] T ";
                            }
                            if (((int) player.customProperties[PhotonPlayerProperty.isTitan]) == 1)
                            {
                                if (((int) player.customProperties[PhotonPlayerProperty.team]) == 2)
                                {
                                    content = content + "[" + ColorSet.color_human_1 + "] H ";
                                }
                                else
                                {
                                    content = content + "[" + ColorSet.color_human + "] H ";
                                }
                            }
                        }
                        str11 = content;
                        object[] objArray2 = new object[] { str11, string.Empty, player.customProperties[PhotonPlayerProperty.name], "[ffffff]:", player.customProperties[PhotonPlayerProperty.kills], "/", player.customProperties[PhotonPlayerProperty.deaths], "/", player.customProperties[PhotonPlayerProperty.max_dmg], "/", player.customProperties[PhotonPlayerProperty.total_dmg] };
                        content = string.Concat(objArray2);
                        if ((bool) player.customProperties[PhotonPlayerProperty.dead])
                        {
                            content = content + "[-]";
                        }
                        content = content + "\n";
                    }
                }
                this.ShowHUDInfoTopLeft(content);
                if (((GameObject.Find("MainCamera") != null) && (IN_GAME_MAIN_CAMERA.gamemode != GAMEMODE.RACING)) && (GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver && !this.needChooseSide))
                {
                    this.ShowHUDInfoCenter("Press [F7D358]" + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.flare1] + "[-] to spectate the next player. \nPress [F7D358]" + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.flare2] + "[-] to spectate the previous player.\nPress [F7D358]" + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.attack1] + "[-] to enter the spectator mode.\n\n\n\n");
                    if (LevelInfo.getInfo(level).respawnMode == RespawnMode.DEATHMATCH)
                    {
                        this.myRespawnTime += Time.deltaTime;
                        int num2 = 10;
                        if (((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.isTitan]) == 2)
                        {
                            num2 = 15;
                        }
                        this.ShowHUDInfoCenterADD("Respawn in " + ((num2 - ((int) this.myRespawnTime))).ToString() + "s.");
                        if (this.myRespawnTime > num2)
                        {
                            this.myRespawnTime = 0f;
                            GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = false;
                            if (((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.isTitan]) == 2)
                            {
                                this.SpawnNonAITitan(this.myLastHero, "titanRespawn");
                            }
                            else
                            {
                                this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
                            }
                            GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = false;
                            this.ShowHUDInfoCenter(string.Empty);
                        }
                    }
                }
            }
            else if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
                {
                    if (!this.isLosing)
                    {
                        this.currentSpeed = GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().main_object.rigidbody.velocity.magnitude;
                        this.maxSpeed = Mathf.Max(this.maxSpeed, this.currentSpeed);
                        this.ShowHUDInfoTopLeft(string.Concat(new object[] { "Current Speed : ", (int) this.currentSpeed, "\nMax Speed:", this.maxSpeed }));
                    }
                }
                else
                {
                    this.ShowHUDInfoTopLeft(string.Concat(new object[] { "Kills:", this.single_kills, "\nMax Damage:", this.single_maxDamage, "\nTotal Damage:", this.single_totalDamage }));
                }
            }
            if (this.isLosing && (IN_GAME_MAIN_CAMERA.gamemode != GAMEMODE.RACING))
            {
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                {
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                    {
                        this.ShowHUDInfoCenter(string.Concat(new object[] { "Survive ", this.wave, " Waves!\n Press ", GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.restart], " to Restart.\n\n\n" }));
                    }
                    else
                    {
                        this.ShowHUDInfoCenter("Humanity Fail!\n Press " + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.restart] + " to Restart.\n\n\n");
                    }
                }
                else
                {
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                    {
                        this.ShowHUDInfoCenter(string.Concat(new object[] { "Survive ", this.wave, " Waves!\nGame Restart in ", (int) this.gameEndCD, "s\n\n" }));
                    }
                    else
                    {
                        this.ShowHUDInfoCenter("Humanity Fail!\nAgain!\nGame Restart in " + ((int) this.gameEndCD) + "s\n\n");
                    }
                    if (this.gameEndCD <= 0f)
                    {
                        this.gameEndCD = 0f;
                        if (PhotonNetwork.isMasterClient)
                        {
                            this.restartGame(false);
                        }
                        this.ShowHUDInfoCenter(string.Empty);
                    }
                    else
                    {
                        this.gameEndCD -= Time.deltaTime;
                    }
                }
            }
            if (this.isWinning)
            {
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                {
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
                    {
                        this.ShowHUDInfoCenter((((((int) (this.timeTotalServer * 10f)) * 0.1f) - 5f)).ToString() + "s !\n Press " + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.restart] + " to Restart.\n\n\n");
                    }
                    else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                    {
                        this.ShowHUDInfoCenter("Survive All Waves!\n Press " + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.restart] + " to Restart.\n\n\n");
                    }
                    else
                    {
                        this.ShowHUDInfoCenter("Humanity Win!\n Press " + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.restart] + " to Restart.\n\n\n");
                    }
                }
                else
                {
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
                    {
                        this.ShowHUDInfoCenter(string.Concat(new object[] { this.localRacingResult, "\n\nGame Restart in ", (int) this.gameEndCD, "s" }));
                    }
                    else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                    {
                        this.ShowHUDInfoCenter("Survive All Waves!\nGame Restart in " + ((int) this.gameEndCD) + "s\n\n");
                    }
                    else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
                    {
                        this.ShowHUDInfoCenter(string.Concat(new object[] { "Team ", this.teamWinner, " Win!\nGame Restart in ", (int) this.gameEndCD, "s\n\n" }));
                    }
                    else
                    {
                        this.ShowHUDInfoCenter("Humanity Win!\nGame Restart in " + ((int) this.gameEndCD) + "s\n\n");
                    }
                    if (this.gameEndCD <= 0f)
                    {
                        this.gameEndCD = 0f;
                        if (PhotonNetwork.isMasterClient)
                        {
                            this.restartGame(false);
                        }
                        this.ShowHUDInfoCenter(string.Empty);
                    }
                    else
                    {
                        this.gameEndCD -= Time.deltaTime;
                    }
                }
            }
            this.timeElapse += Time.deltaTime;
            this.roundTime += Time.deltaTime;
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
                {
                    if (!this.isWinning)
                    {
                        this.timeTotalServer += Time.deltaTime;
                    }
                }
                else if (!this.isLosing && !this.isWinning)
                {
                    this.timeTotalServer += Time.deltaTime;
                }
            }
            else
            {
                this.timeTotalServer += Time.deltaTime;
            }
            if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
            {
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                {
                    if (!this.isWinning)
                    {
                        this.ShowHUDInfoTopCenter("Time : " + ((((int) (this.timeTotalServer * 10f)) * 0.1f) - 5f));
                    }
                    if (this.timeTotalServer < 5f)
                    {
                        this.ShowHUDInfoCenter("RACE START IN " + ((int) (5f - this.timeTotalServer)));
                    }
                    else if (!this.startRacing)
                    {
                        this.ShowHUDInfoCenter(string.Empty);
                        this.startRacing = true;
                        this.endRacing = false;
                        GameObject.Find("door").SetActive(false);
                    }
                }
                else
                {
                    this.ShowHUDInfoTopCenter("Time : " + ((this.roundTime >= 20f) ? (((((int) (this.roundTime * 10f)) * 0.1f) - 20f)).ToString() : "WAITING"));
                    if (this.roundTime < 20f)
                    {
                        this.ShowHUDInfoCenter("RACE START IN " + ((int) (20f - this.roundTime)) + (!(this.localRacingResult == string.Empty) ? ("\nLast Round\n" + this.localRacingResult) : "\n\n"));
                    }
                    else if (!this.startRacing)
                    {
                        this.ShowHUDInfoCenter(string.Empty);
                        this.startRacing = true;
                        this.endRacing = false;
                        GameObject.Find("door").SetActive(false);
                    }
                }
                if (GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver && !this.needChooseSide)
                {
                    this.myRespawnTime += Time.deltaTime;
                    if (this.myRespawnTime > 1.5f)
                    {
                        this.myRespawnTime = 0f;
                        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = false;
                        if (this.checkpoint != null)
                        {
                            this.SpawnPlayerAt(this.myLastHero, this.checkpoint);
                        }
                        else
                        {
                            this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
                        }
                        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = false;
                        this.ShowHUDInfoCenter(string.Empty);
                    }
                }
            }
            if (this.timeElapse > 1f)
            {
                this.timeElapse--;
                string str2 = string.Empty;
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.ENDLESS_TITAN)
                {
                    str2 = str2 + "Time : " + ((this.time - ((int) this.timeTotalServer))).ToString();
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.KILL_TITAN)
                {
                    str2 = "Titan Left: ";
                    str2 = str2 + GameObject.FindGameObjectsWithTag("titan").Length.ToString() + "  Time : ";
                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                    {
                        str2 = str2 + ((int) this.timeTotalServer).ToString();
                    }
                    else
                    {
                        str2 = str2 + ((this.time - ((int) this.timeTotalServer))).ToString();
                    }
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                {
                    str2 = "Titan Left: ";
                    str2 = (str2 + GameObject.FindGameObjectsWithTag("titan").Length.ToString()) + " Wave : " + this.wave;
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.BOSS_FIGHT_CT)
                {
                    str2 = "Time : ";
                    str2 = str2 + ((this.time - ((int) this.timeTotalServer))).ToString() + "\nDefeat the Colossal Titan.\nPrevent abnormal titan from running to the north gate";
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
                {
                    string str3 = "| ";
                    for (int i = 0; i < PVPcheckPoint.chkPts.Count; i++)
                    {
                        str3 = str3 + (PVPcheckPoint.chkPts[i] as PVPcheckPoint).getStateString() + " ";
                    }
                    str3 = str3 + "|";
                    str2 = string.Concat(new object[] { this.PVPtitanScoreMax - this.PVPtitanScore, "  ", str3, "  ", this.PVPhumanScoreMax - this.PVPhumanScore, "\n" }) + "Time : " + ((this.time - ((int) this.timeTotalServer))).ToString();
                }
                this.ShowHUDInfoTopCenter(str2);
                str2 = string.Empty;
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                {
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                    {
                        str2 = "Time : ";
                        str2 = str2 + ((int) this.timeTotalServer).ToString();
                    }
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.ENDLESS_TITAN)
                {
                    object[] objArray10 = new object[] { "Humanity ", this.humanScore, " : Titan ", this.titanScore, " " };
                    str2 = string.Concat(objArray10);
                }
                else if (((IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.KILL_TITAN) || (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.BOSS_FIGHT_CT)) || (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE))
                {
                    object[] objArray11 = new object[] { "Humanity ", this.humanScore, " : Titan ", this.titanScore, " " };
                    str2 = string.Concat(objArray11);
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode != GAMEMODE.CAGE_FIGHT)
                {
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                    {
                        str2 = "Time : ";
                        str2 = str2 + ((this.time - ((int) this.timeTotalServer))).ToString();
                    }
                    else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
                    {
                        for (int j = 0; j < this.teamScores.Length; j++)
                        {
                            str11 = str2;
                            object[] objArray12 = new object[] { str11, (j == 0) ? string.Empty : " : ", "Team", j + 1, " ", this.teamScores[j], string.Empty };
                            str2 = string.Concat(objArray12);
                        }
                        str2 = str2 + "\nTime : " + ((this.time - ((int) this.timeTotalServer))).ToString();
                    }
                }
                this.ShowHUDInfoTopRight(str2);
                string str4 = (IN_GAME_MAIN_CAMERA.difficulty >= 0) ? ((IN_GAME_MAIN_CAMERA.difficulty != 0) ? ((IN_GAME_MAIN_CAMERA.difficulty != 1) ? "Abnormal" : "Hard") : "Normal") : "Trainning";
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.CAGE_FIGHT)
                {
                    this.ShowHUDInfoTopRightMAPNAME(string.Concat(new object[] { (int) this.roundTime, "s\n", level, " : ", str4 }));
                }
                else
                {
                    this.ShowHUDInfoTopRightMAPNAME("\n" + level + " : " + str4);
                }
                this.ShowHUDInfoTopRightMAPNAME("\nCamera(" + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.camera] + "):" + IN_GAME_MAIN_CAMERA.cameraMode.ToString());
                if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && this.needChooseSide)
                {
                    this.ShowHUDInfoTopCenterADD("\n\nPRESS 1 TO ENTER GAME");
                }
            }
            if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && (this.killInfoGO.Count > 0)) && (this.killInfoGO[0] == null))
            {
                this.killInfoGO.RemoveAt(0);
            }
            if (((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && PhotonNetwork.isMasterClient) && (this.timeTotalServer > this.time))
            {
                string str10;
                IN_GAME_MAIN_CAMERA.gametype = GAMETYPE.STOP;
                this.gameStart = false;
                Screen.lockCursor = false;
                Screen.showCursor = true;
                string str5 = string.Empty;
                string str6 = string.Empty;
                string str7 = string.Empty;
                string str8 = string.Empty;
                string str9 = string.Empty;
                foreach (PhotonPlayer player2 in PhotonNetwork.playerList)
                {
                    if (player2 != null)
                    {
                        str5 = str5 + player2.customProperties[PhotonPlayerProperty.name] + "\n";
                        str6 = str6 + player2.customProperties[PhotonPlayerProperty.kills] + "\n";
                        str7 = str7 + player2.customProperties[PhotonPlayerProperty.deaths] + "\n";
                        str8 = str8 + player2.customProperties[PhotonPlayerProperty.max_dmg] + "\n";
                        str9 = str9 + player2.customProperties[PhotonPlayerProperty.total_dmg] + "\n";
                    }
                }
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
                {
                    str10 = string.Empty;
                    for (int k = 0; k < this.teamScores.Length; k++)
                    {
                        str10 = str10 + ((k == 0) ? string.Concat(new object[] { "Team", k + 1, " ", this.teamScores[k], " " }) : " : ");
                    }
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                {
                    str10 = "Highest Wave : " + this.highestwave;
                }
                else
                {
                    object[] objArray15 = new object[] { "Humanity ", this.humanScore, " : Titan ", this.titanScore };
                    str10 = string.Concat(objArray15);
                }
                object[] parameters = new object[] { str5, str6, str7, str8, str9, str10 };
                base.photonView.RPC("showResult", PhotonTargets.AllBuffered, parameters);
            }
        }
    }

    public void gameLose()
    {
        if (!this.isWinning && !this.isLosing)
        {
            this.isLosing = true;
            this.titanScore++;
            this.gameEndCD = this.gameEndTotalCDtime;
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
            {
                object[] parameters = new object[] { this.titanScore };
                base.photonView.RPC("netGameLose", PhotonTargets.Others, parameters);
            }
        }
    }

    public void gameWin()
    {
        if (!this.isLosing && !this.isWinning)
        {
            this.isWinning = true;
            this.humanScore++;
            if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
            {
                this.gameEndCD = 20f;
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                {
                    object[] parameters = new object[] { 0 };
                    base.photonView.RPC("netGameWin", PhotonTargets.Others, parameters);
                }
            }
            else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
            {
                this.gameEndCD = this.gameEndTotalCDtime;
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                {
                    object[] objArray2 = new object[] { this.teamWinner };
                    base.photonView.RPC("netGameWin", PhotonTargets.Others, objArray2);
                }
                this.teamScores[this.teamWinner - 1]++;
            }
            else
            {
                this.gameEndCD = this.gameEndTotalCDtime;
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                {
                    object[] objArray3 = new object[] { this.humanScore };
                    base.photonView.RPC("netGameWin", PhotonTargets.Others, objArray3);
                }
            }
        }
    }

    [RPC]
    private void getRacingResult(string player, float time)
    {
        RacingResult result = new RacingResult {
            name = player,
            time = time
        };
        this.racingResult.Add(result);
        this.refreshRacingResult();
    }

    public bool isPlayerAllDead()
    {
        int num = 0;
        int num2 = 0;
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (((int) player.customProperties[PhotonPlayerProperty.isTitan]) == 1)
            {
                num++;
                if ((bool) player.customProperties[PhotonPlayerProperty.dead])
                {
                    num2++;
                }
            }
        }
        return (num == num2);
    }

    public bool isTeamAllDead(int team)
    {
        int num = 0;
        int num2 = 0;
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if ((((int) player.customProperties[PhotonPlayerProperty.isTitan]) == 1) && (((int) player.customProperties[PhotonPlayerProperty.team]) == team))
            {
                num++;
                if ((bool) player.customProperties[PhotonPlayerProperty.dead])
                {
                    num2++;
                }
            }
        }
        return (num == num2);
    }

    private void kickPhotonPlayer(string name)
    {
        UnityEngine.MonoBehaviour.print("KICK " + name + "!!!");
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if ((player.ID.ToString() == name) && !player.isMasterClient)
            {
                PhotonNetwork.CloseConnection(player);
                return;
            }
        }
    }

    private void kickPlayer(string kickPlayer, string kicker)
    {
        KickState state;
        bool flag = false;
        for (int i = 0; i < this.kicklist.Count; i++)
        {
            if (((KickState) this.kicklist[i]).name == kickPlayer)
            {
                state = (KickState) this.kicklist[i];
                state.addKicker(kicker);
                this.tryKick(state);
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            state = new KickState();
            state.init(kickPlayer);
            state.addKicker(kicker);
            this.kicklist.Add(state);
            this.tryKick(state);
        }
    }

    private void LateUpdate()
    {
        if (this.gameStart)
        {
            IEnumerator enumerator = this.heroes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    ((HERO) enumerator.Current).lateUpdate();
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
	                disposable.Dispose();
                }
            }
            IEnumerator enumerator2 = this.eT.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    ((TITAN_EREN) enumerator2.Current).lateUpdate();
                }
            }
            finally
            {
                IDisposable disposable2 = enumerator2 as IDisposable;
                if (disposable2 != null)
                {
	                disposable2.Dispose();
                }
            }
            IEnumerator enumerator3 = this.titans.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    ((TITAN) enumerator3.Current).lateUpdate();
                }
            }
            finally
            {
                IDisposable disposable3 = enumerator3 as IDisposable;
                if (disposable3 != null)
                {
	                disposable3.Dispose();
                }
            }
            IEnumerator enumerator4 = this.fT.GetEnumerator();
            try
            {
                while (enumerator4.MoveNext())
                {
                    ((FEMALE_TITAN) enumerator4.Current).lateUpdate();
                }
            }
            finally
            {
                IDisposable disposable4 = enumerator4 as IDisposable;
                if (disposable4 != null)
                {
	                disposable4.Dispose();
                }
            }
            this.core();
        }
    }

    public void multiplayerRacingFinsih()
    {
        float time = this.roundTime - 20f;
        if (PhotonNetwork.isMasterClient)
        {
            this.getRacingResult(LoginFengKAI.player.name, time);
        }
        else
        {
            object[] parameters = new object[] { LoginFengKAI.player.name, time };
            base.photonView.RPC("getRacingResult", PhotonTargets.MasterClient, parameters);
        }
        this.gameWin();
    }

    [RPC]
    private void netGameLose(int score)
    {
        this.isLosing = true;
        this.titanScore = score;
        this.gameEndCD = this.gameEndTotalCDtime;
    }

    [RPC]
    private void netGameWin(int score)
    {
        this.humanScore = score;
        this.isWinning = true;
        if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
        {
            this.teamWinner = score;
            this.teamScores[this.teamWinner - 1]++;
            this.gameEndCD = this.gameEndTotalCDtime;
        }
        else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.RACING)
        {
            this.gameEndCD = 20f;
        }
        else
        {
            this.gameEndCD = this.gameEndTotalCDtime;
        }
    }

    [RPC]
    private void netRefreshRacingResult(string tmp)
    {
        this.localRacingResult = tmp;
    }

    [RPC]
    public void netShowDamage(int speed)
    {
        GameObject.Find("Stylish").GetComponent<StylishComponent>().Style(speed);
        GameObject target = GameObject.Find("LabelScore");
        if (target != null)
        {
            target.GetComponent<UILabel>().text = speed.ToString();
            target.transform.localScale = Vector3.zero;
            speed = (int) (speed * 0.1f);
            speed = Mathf.Max(40, speed);
            speed = Mathf.Min(150, speed);
            iTween.Stop(target);
            object[] args = new object[] { "x", speed, "y", speed, "z", speed, "easetype", iTween.EaseType.easeOutElastic, "time", 1f };
            iTween.ScaleTo(target, iTween.Hash(args));
            object[] objArray2 = new object[] { "x", 0, "y", 0, "z", 0, "easetype", iTween.EaseType.easeInBounce, "time", 0.5f, "delay", 2f };
            iTween.ScaleTo(target, iTween.Hash(objArray2));
        }
    }

    public void NOTSpawnNonAITitan(string id)
    {
        this.myLastHero = id.ToUpper();
        ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add("dead", true);
        ExitGames.Client.Photon.Hashtable propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.isTitan, 2);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
        {
            Screen.lockCursor = true;
        }
        else
        {
            Screen.lockCursor = false;
        }
        Screen.showCursor = true;
        this.ShowHUDInfoCenter("the game has started for 60 seconds.\n please wait for next round.\n Click Right Mouse Key to Enter or Exit the Spectator Mode.");
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().enabled = true;
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setMainObject(null, true, false);
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setSpectorMode(true);
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
    }

    public void NOTSpawnPlayer(string id)
    {
        this.myLastHero = id.ToUpper();
        ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add("dead", true);
        ExitGames.Client.Photon.Hashtable propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.isTitan, 1);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
        {
            Screen.lockCursor = true;
        }
        else
        {
            Screen.lockCursor = false;
        }
        Screen.showCursor = false;
        this.ShowHUDInfoCenter("the game has started for 60 seconds.\n please wait for next round.\n Click Right Mouse Key to Enter or Exit the Spectator Mode.");
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().enabled = true;
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setMainObject(null, true, false);
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setSpectorMode(true);
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
    }

    public void OnConnectedToMaster()
    {
        UnityEngine.MonoBehaviour.print("OnConnectedToMaster");
    }

    public void OnConnectedToPhoton()
    {
        UnityEngine.MonoBehaviour.print("OnConnectedToPhoton");
    }

    public void OnConnectionFail(DisconnectCause cause)
    {
        UnityEngine.MonoBehaviour.print("OnConnectionFail : " + cause.ToString());
        Screen.lockCursor = false;
        Screen.showCursor = true;
        IN_GAME_MAIN_CAMERA.gametype = GAMETYPE.STOP;
        this.gameStart = false;
        NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[0], false);
        NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[1], false);
        NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[2], false);
        NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[3], false);
        NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[4], true);
        GameObject.Find("LabelDisconnectInfo").GetComponent<UILabel>().text = "OnConnectionFail : " + cause.ToString();
    }

    public void OnCreatedRoom()
    {
        this.kicklist = new ArrayList();
        this.racingResult = new ArrayList();
        this.teamScores = new int[2];
        UnityEngine.MonoBehaviour.print("OnCreatedRoom");
    }

    public void OnCustomAuthenticationFailed()
    {
        UnityEngine.MonoBehaviour.print("OnCustomAuthenticationFailed");
    }

    public void OnDisconnectedFromPhoton()
    {
        UnityEngine.MonoBehaviour.print("OnDisconnectedFromPhoton");
        Screen.lockCursor = false;
        Screen.showCursor = true;
    }

    [RPC]
    public void oneTitanDown(string name1 = "", bool onPlayerLeave = false)
    {
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE) || PhotonNetwork.isMasterClient)
        {
            if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
            {
                if (name1 != string.Empty)
                {
                    if (name1 == "Titan")
                    {
                        this.PVPhumanScore++;
                    }
                    else if (name1 == "Aberrant")
                    {
                        this.PVPhumanScore += 2;
                    }
                    else if (name1 == "Jumper")
                    {
                        this.PVPhumanScore += 3;
                    }
                    else if (name1 == "Crawler")
                    {
                        this.PVPhumanScore += 4;
                    }
                    else if (name1 == "Female Titan")
                    {
                        this.PVPhumanScore += 10;
                    }
                    else
                    {
                        this.PVPhumanScore += 3;
                    }
                }
                this.checkPVPpts();
                object[] parameters = new object[] { this.PVPhumanScore, this.PVPtitanScore };
                base.photonView.RPC("refreshPVPStatus", PhotonTargets.Others, parameters);
            }
            else if (IN_GAME_MAIN_CAMERA.gamemode != GAMEMODE.CAGE_FIGHT)
            {
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.KILL_TITAN)
                {
                    if (this.checkIsTitanAllDie())
                    {
                        this.gameWin();
                        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
                    }
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                {
                    if (this.checkIsTitanAllDie())
                    {
                        this.wave++;
                        if ((LevelInfo.getInfo(level).respawnMode == RespawnMode.NEWROUND) && (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER))
                        {
                            base.photonView.RPC("respawnHeroInNewRound", PhotonTargets.All, new object[0]);
                        }
                        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                        {
                            this.sendChatContentInfo("<color=#A8FF24>Wave : " + this.wave + "</color>");
                        }
                        if (this.wave > this.highestwave)
                        {
                            this.highestwave = this.wave;
                        }
                        if (PhotonNetwork.isMasterClient)
                        {
                            this.RequireStatus();
                        }
                        if (this.wave > 20)
                        {
                            this.gameWin();
                        }
                        else
                        {
                            int rate = 90;
                            if (this.difficulty == 1)
                            {
                                rate = 70;
                            }
                            if (!LevelInfo.getInfo(level).punk)
                            {
                                this.randomSpawnTitan("titanRespawn", rate, this.wave + 2, false);
                            }
                            else if (this.wave == 5)
                            {
                                this.randomSpawnTitan("titanRespawn", rate, 1, true);
                            }
                            else if (this.wave == 10)
                            {
                                this.randomSpawnTitan("titanRespawn", rate, 2, true);
                            }
                            else if (this.wave == 15)
                            {
                                this.randomSpawnTitan("titanRespawn", rate, 3, true);
                            }
                            else if (this.wave == 20)
                            {
                                this.randomSpawnTitan("titanRespawn", rate, 4, true);
                            }
                            else
                            {
                                this.randomSpawnTitan("titanRespawn", rate, this.wave + 2, false);
                            }
                        }
                    }
                }
                else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.ENDLESS_TITAN)
                {
                    if (!onPlayerLeave)
                    {
                        this.humanScore++;
                        int num2 = 90;
                        if (this.difficulty == 1)
                        {
                            num2 = 70;
                        }
                        this.randomSpawnTitan("titanRespawn", num2, 1, false);
                    }
                }
                else if (LevelInfo.getInfo(level).enemyNumber == -1)
                {
                }
            }
        }
    }

    public void OnFailedToConnectToPhoton()
    {
        UnityEngine.MonoBehaviour.print("OnFailedToConnectToPhoton");
    }

    public void OnJoinedLobby()
    {
        UnityEngine.MonoBehaviour.print("OnJoinedLobby");
        NGUITools.SetActive(GameObject.Find("UIRefer").GetComponent<UIMainReferences>().panelMultiStart, false);
        NGUITools.SetActive(GameObject.Find("UIRefer").GetComponent<UIMainReferences>().panelMultiROOM, true);
    }

    public void OnJoinedRoom()
    {
        char[] separator = new char[] { "`"[0] };
        UnityEngine.MonoBehaviour.print("OnJoinedRoom " + PhotonNetwork.room.name + "    >>>>   " + LevelInfo.getInfo(PhotonNetwork.room.name.Split(separator)[1]).mapName);
        this.gameTimesUp = false;
        char[] chArray2 = new char[] { "`"[0] };
        string[] strArray = PhotonNetwork.room.name.Split(chArray2);
        level = strArray[1];
        if (strArray[2] == "normal")
        {
            this.difficulty = 0;
        }
        else if (strArray[2] == "hard")
        {
            this.difficulty = 1;
        }
        else if (strArray[2] == "abnormal")
        {
            this.difficulty = 2;
        }
        IN_GAME_MAIN_CAMERA.difficulty = this.difficulty;
        this.time = int.Parse(strArray[3]);
        this.time *= 60;
        if (strArray[4] == "day")
        {
            IN_GAME_MAIN_CAMERA.dayLight = DayLight.Day;
        }
        else if (strArray[4] == "dawn")
        {
            IN_GAME_MAIN_CAMERA.dayLight = DayLight.Dawn;
        }
        else if (strArray[4] == "night")
        {
            IN_GAME_MAIN_CAMERA.dayLight = DayLight.Night;
        }
        IN_GAME_MAIN_CAMERA.gamemode = LevelInfo.getInfo(level).type;
        PhotonNetwork.LoadLevel(LevelInfo.getInfo(level).mapName);
        ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.name, LoginFengKAI.player.name);
        ExitGames.Client.Photon.Hashtable propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.guildName, LoginFengKAI.player.guildname);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.kills, 0);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.max_dmg, 0);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.total_dmg, 0);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.deaths, 0);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.dead, true);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.isTitan, 0);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        this.humanScore = 0;
        this.titanScore = 0;
        this.PVPtitanScore = 0;
        this.PVPhumanScore = 0;
        this.wave = 1;
        this.highestwave = 1;
        this.localRacingResult = string.Empty;
        this.needChooseSide = true;
        this.chatContent = new ArrayList();
        this.killInfoGO = new ArrayList();
        InRoomChat.messages = new List<string>();
        if (!PhotonNetwork.isMasterClient)
        {
            base.photonView.RPC("RequireStatus", PhotonTargets.MasterClient, new object[0]);
        }
    }

    public void OnLeftLobby()
    {
        UnityEngine.MonoBehaviour.print("OnLeftLobby");
    }

    public void OnLeftRoom()
    {
        UnityEngine.MonoBehaviour.print("OnLeftRoom");
        if (Application.loadedLevel != 0)
        {
            Time.timeScale = 1f;
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.Disconnect();
            }
            IN_GAME_MAIN_CAMERA.gametype = GAMETYPE.STOP;
            this.gameStart = false;
            Screen.lockCursor = false;
            Screen.showCursor = true;
            GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().menuOn = false;
            UnityEngine.Object.Destroy(GameObject.Find("MultiplayerManager"));
            Application.LoadLevel("menu");
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if ((level != 0) && ((Application.loadedLevelName != "characterCreation") && (Application.loadedLevelName != "SnapShot")))
        {
            ChangeQuality.setCurrentQuality();
            foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("titan"))
            {
                if ((obj2.GetPhotonView() == null) || !obj2.GetPhotonView().owner.isMasterClient)
                {
                    UnityEngine.Object.Destroy(obj2);
                }
            }
            this.isWinning = false;
            this.gameStart = true;
            this.ShowHUDInfoCenter(string.Empty);
            GameObject obj3 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("MainCamera_mono"), GameObject.Find("cameraDefaultPosition").transform.position, GameObject.Find("cameraDefaultPosition").transform.rotation);
            UnityEngine.Object.Destroy(GameObject.Find("cameraDefaultPosition"));
            obj3.name = "MainCamera";
            Screen.lockCursor = true;
            Screen.showCursor = true;
            this.ui = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("UI_IN_GAME"));
            this.ui.name = "UI_IN_GAME";
            this.ui.SetActive(true);
            NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[0], true);
            NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[1], false);
            NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[2], false);
            NGUITools.SetActive(this.ui.GetComponent<UIReferArray>().panels[3], false);
            GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setHUDposition();
            GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setDayLight(IN_GAME_MAIN_CAMERA.dayLight);
            LevelInfo info = LevelInfo.getInfo(FengGameManagerMKII.level);
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                this.single_kills = 0;
                this.single_maxDamage = 0;
                this.single_totalDamage = 0;
                GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().enabled = true;
                GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = true;
                GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = true;
                IN_GAME_MAIN_CAMERA.gamemode = LevelInfo.getInfo(FengGameManagerMKII.level).type;
                this.SpawnPlayer(IN_GAME_MAIN_CAMERA.singleCharacter.ToUpper(), "playerRespawn");
                if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
                {
                    Screen.lockCursor = true;
                }
                else
                {
                    Screen.lockCursor = false;
                }
                Screen.showCursor = false;
                int rate = 90;
                if (this.difficulty == 1)
                {
                    rate = 70;
                }
                this.randomSpawnTitan("titanRespawn", rate, info.enemyNumber, false);
            }
            else
            {
                PVPcheckPoint.chkPts = new ArrayList();
                GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().enabled = false;
                GameObject.Find("MainCamera").GetComponent<CameraShake>().enabled = false;
                IN_GAME_MAIN_CAMERA.gametype = GAMETYPE.MULTIPLAYER;
                if (info.type == GAMEMODE.TROST)
                {
                    GameObject.Find("playerRespawn").SetActive(false);
                    UnityEngine.Object.Destroy(GameObject.Find("playerRespawn"));
                    GameObject.Find("rock").animation["lift"].speed = 0f;
                    GameObject.Find("door_fine").SetActiveRecursively(false);
                    GameObject.Find("door_broke").SetActiveRecursively(true);
                    UnityEngine.Object.Destroy(GameObject.Find("ppl"));
                }
                else if (info.type == GAMEMODE.BOSS_FIGHT_CT)
                {
                    GameObject.Find("playerRespawnTrost").SetActive(false);
                    UnityEngine.Object.Destroy(GameObject.Find("playerRespawnTrost"));
                }
                if (this.needChooseSide)
                {
                    this.ShowHUDInfoTopCenterADD("\n\nPRESS 1 TO ENTER GAME");
                }
                else
                {
                    if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
                    {
                        Screen.lockCursor = true;
                    }
                    else
                    {
                        Screen.lockCursor = false;
                    }
                    if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
                    {
                        if (((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.isTitan]) == 2)
                        {
                            this.checkpoint = GameObject.Find("PVPchkPtT");
                        }
                        else
                        {
                            this.checkpoint = GameObject.Find("PVPchkPtH");
                        }
                    }
                    if (((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.isTitan]) == 2)
                    {
                        this.SpawnNonAITitan(this.myLastHero, "titanRespawn");
                    }
                    else
                    {
                        this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
                    }
                }
                if (info.type == GAMEMODE.BOSS_FIGHT_CT)
                {
                    UnityEngine.Object.Destroy(GameObject.Find("rock"));
                }
                if (PhotonNetwork.isMasterClient)
                {
                    if (info.type == GAMEMODE.TROST)
                    {
                        if (!this.isPlayerAllDead())
                        {
                            PhotonNetwork.Instantiate("TITAN_EREN_trost", new Vector3(-200f, 0f, -194f), Quaternion.Euler(0f, 180f, 0f), 0).GetComponent<TITAN_EREN>().rockLift = true;
                            int num3 = 90;
                            if (this.difficulty == 1)
                            {
                                num3 = 70;
                            }
                            GameObject[] objArray3 = GameObject.FindGameObjectsWithTag("titanRespawn");
                            GameObject obj6 = GameObject.Find("titanRespawnTrost");
                            if (obj6 != null)
                            {
                                foreach (GameObject obj7 in objArray3)
                                {
                                    if (obj7.transform.parent.gameObject == obj6)
                                    {
                                        this.spawnTitan(num3, obj7.transform.position, obj7.transform.rotation, false);
                                    }
                                }
                            }
                        }
                    }
                    else if (info.type == GAMEMODE.BOSS_FIGHT_CT)
                    {
                        if (!this.isPlayerAllDead())
                        {
                            PhotonNetwork.Instantiate("COLOSSAL_TITAN", (Vector3) (-Vector3.up * 10000f), Quaternion.Euler(0f, 180f, 0f), 0);
                        }
                    }
                    else if (((info.type == GAMEMODE.KILL_TITAN) || (info.type == GAMEMODE.ENDLESS_TITAN)) || (info.type == GAMEMODE.SURVIVE_MODE))
                    {
                        if ((info.name == "Annie") || (info.name == "Annie II"))
                        {
                            PhotonNetwork.Instantiate("FEMALE_TITAN", GameObject.Find("titanRespawn").transform.position, GameObject.Find("titanRespawn").transform.rotation, 0);
                        }
                        else
                        {
                            int num5 = 90;
                            if (this.difficulty == 1)
                            {
                                num5 = 70;
                            }
                            this.randomSpawnTitan("titanRespawn", num5, info.enemyNumber, false);
                        }
                    }
                    else if ((info.type != GAMEMODE.TROST) && ((info.type == GAMEMODE.PVP_CAPTURE) && (LevelInfo.getInfo(FengGameManagerMKII.level).mapName == "OutSide")))
                    {
                        GameObject[] objArray5 = GameObject.FindGameObjectsWithTag("titanRespawn");
                        if (objArray5.Length <= 0)
                        {
                            return;
                        }
                        for (int i = 0; i < objArray5.Length; i++)
                        {
                            this.spawnTitanRaw(objArray5[i].transform.position, objArray5[i].transform.rotation).GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_CRAWLER, true);
                        }
                    }
                }
                if (!info.supply)
                {
                    UnityEngine.Object.Destroy(GameObject.Find("aot_supply"));
                }
                if (!PhotonNetwork.isMasterClient)
                {
                    base.photonView.RPC("RequireStatus", PhotonTargets.MasterClient, new object[0]);
                }
                if (LevelInfo.getInfo(FengGameManagerMKII.level).lavaMode)
                {
                    UnityEngine.Object.Instantiate(Resources.Load("levelBottom"), new Vector3(0f, -29.5f, 0f), Quaternion.Euler(0f, 0f, 0f));
                    GameObject.Find("aot_supply").transform.position = GameObject.Find("aot_supply_lava_position").transform.position;
                    GameObject.Find("aot_supply").transform.rotation = GameObject.Find("aot_supply_lava_position").transform.rotation;
                }
            }
        }
    }

    public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        UnityEngine.MonoBehaviour.print("OnMasterClientSwitched");
        if (!this.gameTimesUp && PhotonNetwork.isMasterClient)
        {
            this.restartGame(true);
        }
    }

    public void OnPhotonCreateRoomFailed()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonCreateRoomFailed");
    }

    public void OnPhotonCustomRoomPropertiesChanged()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonCustomRoomPropertiesChanged");
    }

    public void OnPhotonInstantiate()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonInstantiate");
    }

    public void OnPhotonJoinRoomFailed()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonJoinRoomFailed");
    }

    public void OnPhotonMaxCccuReached()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonMaxCccuReached");
    }

    public void OnPhotonPlayerConnected()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonPlayerConnected");
    }

    public void OnPhotonPlayerDisconnected()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonPlayerDisconnected");
        if (!this.gameTimesUp)
        {
            this.oneTitanDown(string.Empty, true);
            this.someOneIsDead(0);
        }
    }

    public void OnPhotonPlayerPropertiesChanged()
    {
    }

    public void OnPhotonRandomJoinFailed()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonRandomJoinFailed");
    }

    public void OnPhotonSerializeView()
    {
        UnityEngine.MonoBehaviour.print("OnPhotonSerializeView");
    }

    public void OnReceivedRoomListUpdate()
    {
    }

    public void OnUpdatedFriendList()
    {
        UnityEngine.MonoBehaviour.print("OnUpdatedFriendList");
    }

    public void playerKillInfoSingleUpdate(int dmg)
    {
        this.single_kills++;
        this.single_maxDamage = Mathf.Max(dmg, this.single_maxDamage);
        this.single_totalDamage += dmg;
    }

    public void playerKillInfoUpdate(PhotonPlayer player, int dmg)
    {
        ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable();
        propertiesToSet.Add(PhotonPlayerProperty.kills, ((int) player.customProperties[PhotonPlayerProperty.kills]) + 1);
        player.SetCustomProperties(propertiesToSet);
        propertiesToSet = new ExitGames.Client.Photon.Hashtable();
        propertiesToSet.Add(PhotonPlayerProperty.max_dmg, Mathf.Max(dmg, (int) player.customProperties[PhotonPlayerProperty.max_dmg]));
        player.SetCustomProperties(propertiesToSet);
        propertiesToSet = new ExitGames.Client.Photon.Hashtable();
        propertiesToSet.Add(PhotonPlayerProperty.total_dmg, ((int) player.customProperties[PhotonPlayerProperty.total_dmg]) + dmg);
        player.SetCustomProperties(propertiesToSet);
    }

    public GameObject randomSpawnOneTitan(string place, int rate)
    {
        GameObject[] objArray = GameObject.FindGameObjectsWithTag(place);
        int index = UnityEngine.Random.Range(0, objArray.Length);
        GameObject obj2 = objArray[index];
        while (objArray[index] == null)
        {
            index = UnityEngine.Random.Range(0, objArray.Length);
            obj2 = objArray[index];
        }
        objArray[index] = null;
        return this.spawnTitan(rate, obj2.transform.position, obj2.transform.rotation, false);
    }

    public void randomSpawnTitan(string place, int rate, int num, bool punk = false)
    {
        if (num == -1)
        {
            num = 1;
        }
        GameObject[] objArray = GameObject.FindGameObjectsWithTag(place);
        if (objArray.Length > 0)
        {
            for (int i = 0; i < num; i++)
            {
                int index = UnityEngine.Random.Range(0, objArray.Length);
                GameObject obj2 = objArray[index];
                while (objArray[index] == null)
                {
                    index = UnityEngine.Random.Range(0, objArray.Length);
                    obj2 = objArray[index];
                }
                objArray[index] = null;
                this.spawnTitan(rate, obj2.transform.position, obj2.transform.rotation, punk);
            }
        }
    }

    [RPC]
    private void refreshPVPStatus(int score1, int score2)
    {
        this.PVPhumanScore = score1;
        this.PVPtitanScore = score2;
    }

    [RPC]
    private void refreshPVPStatus_AHSS(int[] score1)
    {
        UnityEngine.MonoBehaviour.print(score1);
        this.teamScores = score1;
    }

    private void refreshRacingResult()
    {
        this.localRacingResult = "Result\n";
        IComparer comparer = new IComparerRacingResult();
        this.racingResult.Sort(comparer);
        int num = Mathf.Min(this.racingResult.Count, 6);
        for (int i = 0; i < num; i++)
        {
            string localRacingResult = this.localRacingResult;
            object[] objArray1 = new object[] { localRacingResult, "Rank ", i + 1, " : " };
            this.localRacingResult = string.Concat(objArray1);
            this.localRacingResult = this.localRacingResult + (this.racingResult[i] as RacingResult).name;
            this.localRacingResult = this.localRacingResult + "   " + ((((int) ((this.racingResult[i] as RacingResult).time * 100f)) * 0.01f)).ToString() + "s";
            this.localRacingResult = this.localRacingResult + "\n";
        }
        object[] parameters = new object[] { this.localRacingResult };
        base.photonView.RPC("netRefreshRacingResult", PhotonTargets.All, parameters);
    }

    [RPC]
    private void refreshStatus(int score1, int score2, int wav, int highestWav, float time1, float time2, bool startRacin, bool endRacin)
    {
        this.humanScore = score1;
        this.titanScore = score2;
        this.wave = wav;
        this.highestwave = highestWav;
        this.roundTime = time1;
        this.timeTotalServer = time2;
        this.startRacing = startRacin;
        this.endRacing = endRacin;
        if (this.startRacing && (GameObject.Find("door") != null))
        {
            GameObject.Find("door").SetActive(false);
        }
    }

    public void removeCT(COLOSSAL_TITAN titan)
    {
        this.cT.Remove(titan);
    }

    public void removeET(TITAN_EREN hero)
    {
        this.eT.Remove(hero);
    }

    public void removeFT(FEMALE_TITAN titan)
    {
        this.fT.Remove(titan);
    }

    public void removeHero(HERO hero)
    {
        this.heroes.Remove(hero);
    }

    public void removeHook(Bullet h)
    {
        this.hooks.Remove(h);
    }

    public void removeTitan(TITAN titan)
    {
        this.titans.Remove(titan);
    }

    [RPC]
    private void RequireStatus()
    {
        object[] parameters = new object[] { this.humanScore, this.titanScore, this.wave, this.highestwave, this.roundTime, this.timeTotalServer, this.startRacing, this.endRacing };
        base.photonView.RPC("refreshStatus", PhotonTargets.Others, parameters);
        object[] objArray2 = new object[] { this.PVPhumanScore, this.PVPtitanScore };
        base.photonView.RPC("refreshPVPStatus", PhotonTargets.Others, objArray2);
        object[] objArray3 = new object[] { this.teamScores };
        base.photonView.RPC("refreshPVPStatus_AHSS", PhotonTargets.Others, objArray3);
    }

    [RPC]
    private void respawnHeroInNewRound()
    {
        if (!this.needChooseSide && GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver)
        {
            this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
            GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = false;
            this.ShowHUDInfoCenter(string.Empty);
        }
    }

    public void restartGame(bool masterclientSwitched = false)
    {
        UnityEngine.MonoBehaviour.print("reset game :" + this.gameTimesUp);
        if (!this.gameTimesUp)
        {
            this.PVPtitanScore = 0;
            this.PVPhumanScore = 0;
            this.startRacing = false;
            this.endRacing = false;
            this.checkpoint = null;
            this.timeElapse = 0f;
            this.roundTime = 0f;
            this.isWinning = false;
            this.isLosing = false;
            this.isPlayer1Winning = false;
            this.isPlayer2Winning = false;
            this.wave = 1;
            this.myRespawnTime = 0f;
            this.kicklist = new ArrayList();
            this.killInfoGO = new ArrayList();
            this.racingResult = new ArrayList();
            this.ShowHUDInfoCenter(string.Empty);
            PhotonNetwork.DestroyAll();
            base.photonView.RPC("RPCLoadLevel", PhotonTargets.All, new object[0]);
            if (masterclientSwitched)
            {
                this.sendChatContentInfo("<color=#A8FF24>MasterClient has switched to </color>" + PhotonNetwork.player.customProperties[PhotonPlayerProperty.name]);
            }
        }
    }

    [RPC]
    private void restartGameByClient()
    {
        this.restartGame(false);
    }

    public void restartGameSingle()
    {
        this.startRacing = false;
        this.endRacing = false;
        this.checkpoint = null;
        this.single_kills = 0;
        this.single_maxDamage = 0;
        this.single_totalDamage = 0;
        this.timeElapse = 0f;
        this.roundTime = 0f;
        this.timeTotalServer = 0f;
        this.isWinning = false;
        this.isLosing = false;
        this.isPlayer1Winning = false;
        this.isPlayer2Winning = false;
        this.wave = 1;
        this.myRespawnTime = 0f;
        this.ShowHUDInfoCenter(string.Empty);
        Application.LoadLevel(Application.loadedLevel);
    }

    [RPC]
    private void RPCLoadLevel()
    {
        PhotonNetwork.LoadLevel(LevelInfo.getInfo(level).mapName);
    }

    public void sendChatContentInfo(string content)
    {
        object[] parameters = new object[] { content, string.Empty };
        base.photonView.RPC("Chat", PhotonTargets.All, parameters);
    }

    public void sendKillInfo(bool t1, string killer, bool t2, string victim, int dmg = 0)
    {
        object[] parameters = new object[] { t1, killer, t2, victim, dmg };
        base.photonView.RPC("updateKillInfo", PhotonTargets.All, parameters);
    }

    [RPC]
    private void showChatContent(string content)
    {
        this.chatContent.Add(content);
        if (this.chatContent.Count > 10)
        {
            this.chatContent.RemoveAt(0);
        }
        GameObject.Find("LabelChatContent").GetComponent<UILabel>().text = string.Empty;
        for (int i = 0; i < this.chatContent.Count; i++)
        {
            UILabel component = GameObject.Find("LabelChatContent").GetComponent<UILabel>();
            component.text = component.text + this.chatContent[i];
        }
    }

    public void ShowHUDInfoCenter(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoCenter");
        if (obj2 != null)
        {
            obj2.GetComponent<UILabel>().text = content;
        }
    }

    public void ShowHUDInfoCenterADD(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoCenter");
        if (obj2 != null)
        {
            UILabel component = obj2.GetComponent<UILabel>();
            component.text = component.text + content;
        }
    }

    private void ShowHUDInfoTopCenter(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopCenter");
        if (obj2 != null)
        {
            obj2.GetComponent<UILabel>().text = content;
        }
    }

    private void ShowHUDInfoTopCenterADD(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopCenter");
        if (obj2 != null)
        {
            UILabel component = obj2.GetComponent<UILabel>();
            component.text = component.text + content;
        }
    }

    private void ShowHUDInfoTopLeft(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopLeft");
        if (obj2 != null)
        {
            obj2.GetComponent<UILabel>().text = content;
        }
    }

    private void ShowHUDInfoTopRight(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopRight");
        if (obj2 != null)
        {
            obj2.GetComponent<UILabel>().text = content;
        }
    }

    private void ShowHUDInfoTopRightMAPNAME(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopRight");
        if (obj2 != null)
        {
            UILabel component = obj2.GetComponent<UILabel>();
            component.text = component.text + content;
        }
    }

    [RPC]
    private void showResult(string text0, string text1, string text2, string text3, string text4, string text6, PhotonMessageInfo t)
    {
        if (!this.gameTimesUp)
        {
            this.gameTimesUp = true;
            GameObject obj2 = GameObject.Find("UI_IN_GAME");
            NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[0], false);
            NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[1], false);
            NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[2], true);
            NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[3], false);
            GameObject.Find("LabelName").GetComponent<UILabel>().text = text0;
            GameObject.Find("LabelKill").GetComponent<UILabel>().text = text1;
            GameObject.Find("LabelDead").GetComponent<UILabel>().text = text2;
            GameObject.Find("LabelMaxDmg").GetComponent<UILabel>().text = text3;
            GameObject.Find("LabelTotalDmg").GetComponent<UILabel>().text = text4;
            GameObject.Find("LabelResultTitle").GetComponent<UILabel>().text = text6;
            Screen.lockCursor = false;
            Screen.showCursor = true;
            IN_GAME_MAIN_CAMERA.gametype = GAMETYPE.STOP;
            this.gameStart = false;
        }
    }

    private void SingleShowHUDInfoTopCenter(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopCenter");
        if (obj2 != null)
        {
            obj2.GetComponent<UILabel>().text = content;
        }
    }

    private void SingleShowHUDInfoTopLeft(string content)
    {
        GameObject obj2 = GameObject.Find("LabelInfoTopLeft");
        if (obj2 != null)
        {
            content = content.Replace("[0]", "[*^_^*]");
            obj2.GetComponent<UILabel>().text = content;
        }
    }

    [RPC]
    public void someOneIsDead(int id = -1)
    {
        if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
        {
            if (id != 0)
            {
                this.PVPtitanScore += 2;
            }
            this.checkPVPpts();
            object[] parameters = new object[] { this.PVPhumanScore, this.PVPtitanScore };
            base.photonView.RPC("refreshPVPStatus", PhotonTargets.Others, parameters);
        }
        else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.ENDLESS_TITAN)
        {
            this.titanScore++;
        }
        else if (((IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.KILL_TITAN) || (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)) || ((IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.BOSS_FIGHT_CT) || (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.TROST)))
        {
            if (this.isPlayerAllDead())
            {
                this.gameLose();
            }
        }
        else if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
        {
            if (this.isPlayerAllDead())
            {
                this.gameLose();
                this.teamWinner = 0;
            }
            if (this.isTeamAllDead(1))
            {
                this.teamWinner = 2;
                this.gameWin();
            }
            if (this.isTeamAllDead(2))
            {
                this.teamWinner = 1;
                this.gameWin();
            }
        }
    }

    public void SpawnNonAITitan(string id, string tag = "titanRespawn")
    {
        GameObject obj3;
        GameObject[] objArray = GameObject.FindGameObjectsWithTag(tag);
        GameObject obj2 = objArray[UnityEngine.Random.Range(0, objArray.Length)];
        this.myLastHero = id.ToUpper();
        if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
        {
            obj3 = PhotonNetwork.Instantiate("TITAN_VER3.1", this.checkpoint.transform.position + new Vector3((float) UnityEngine.Random.Range(-20, 20), 2f, (float) UnityEngine.Random.Range(-20, 20)), this.checkpoint.transform.rotation, 0);
        }
        else
        {
            obj3 = PhotonNetwork.Instantiate("TITAN_VER3.1", obj2.transform.position, obj2.transform.rotation, 0);
        }
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setMainObjectASTITAN(obj3);
        obj3.GetComponent<TITAN>().nonAI = true;
        obj3.GetComponent<TITAN>().speed = 30f;
        obj3.GetComponent<TITAN_CONTROLLER>().enabled = true;
        if ((id == "RANDOM") && (UnityEngine.Random.Range(0, 100) < 7))
        {
            obj3.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_CRAWLER, true);
        }
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().enabled = true;
        GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = true;
        GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = true;
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = false;
        ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add("dead", false);
        ExitGames.Client.Photon.Hashtable propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        hashtable2 = new ExitGames.Client.Photon.Hashtable();
        hashtable2.Add(PhotonPlayerProperty.isTitan, 2);
        propertiesToSet = hashtable2;
        PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
        {
            Screen.lockCursor = true;
        }
        else
        {
            Screen.lockCursor = false;
        }
        Screen.showCursor = true;
        this.ShowHUDInfoCenter(string.Empty);
    }

    public void SpawnPlayer(string id, string tag = "playerRespawn")
    {
        if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
        {
            this.SpawnPlayerAt(id, this.checkpoint);
        }
        else
        {
            this.myLastRespawnTag = tag;
            GameObject[] objArray = GameObject.FindGameObjectsWithTag(tag);
            GameObject pos = objArray[UnityEngine.Random.Range(0, objArray.Length)];
            this.SpawnPlayerAt(id, pos);
        }
    }

    public void SpawnPlayerAt(string id, GameObject pos)
    {
        IN_GAME_MAIN_CAMERA component = GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>();
        this.myLastHero = id.ToUpper();
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            if (IN_GAME_MAIN_CAMERA.singleCharacter == "TITAN_EREN")
            {
                component.setMainObject((GameObject) UnityEngine.Object.Instantiate(Resources.Load("TITAN_EREN"), pos.transform.position, pos.transform.rotation), true, false);
            }
            else
            {
                component.setMainObject((GameObject) UnityEngine.Object.Instantiate(Resources.Load("AOTTG_HERO 1"), pos.transform.position, pos.transform.rotation), true, false);
                if (((IN_GAME_MAIN_CAMERA.singleCharacter == "SET 1") || (IN_GAME_MAIN_CAMERA.singleCharacter == "SET 2")) || (IN_GAME_MAIN_CAMERA.singleCharacter == "SET 3"))
                {
                    HeroCostume costume = CostumeConeveter.LocalDataToHeroCostume(IN_GAME_MAIN_CAMERA.singleCharacter);
                    costume.checkstat();
                    CostumeConeveter.HeroCostumeToLocalData(costume, IN_GAME_MAIN_CAMERA.singleCharacter);
                    component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().init();
                    if (costume != null)
                    {
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume = costume;
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume.stat = costume.stat;
                    }
                    else
                    {
                        costume = HeroCostume.costumeOption[3];
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume = costume;
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume.stat = HeroStat.getInfo(costume.name.ToUpper());
                    }
                    component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().setCharacterComponent();
                    component.main_object.GetComponent<HERO>().setStat();
                    component.main_object.GetComponent<HERO>().setSkillHUDPosition();
                }
                else
                {
                    for (int i = 0; i < HeroCostume.costume.Length; i++)
                    {
                        if (HeroCostume.costume[i].name.ToUpper() == IN_GAME_MAIN_CAMERA.singleCharacter.ToUpper())
                        {
                            int index = (HeroCostume.costume[i].id + CheckBoxCostume.costumeSet) - 1;
                            if (HeroCostume.costume[index].name != HeroCostume.costume[i].name)
                            {
                                index = HeroCostume.costume[i].id + 1;
                            }
                            component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().init();
                            component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume = HeroCostume.costume[index];
                            component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume.stat = HeroStat.getInfo(HeroCostume.costume[index].name.ToUpper());
                            component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().setCharacterComponent();
                            component.main_object.GetComponent<HERO>().setStat();
                            component.main_object.GetComponent<HERO>().setSkillHUDPosition();
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            component.setMainObject(PhotonNetwork.Instantiate("AOTTG_HERO 1", pos.transform.position, pos.transform.rotation, 0), true, false);
            id = id.ToUpper();
            if (((id == "SET 1") || (id == "SET 2")) || (id == "SET 3"))
            {
                HeroCostume costume2 = CostumeConeveter.LocalDataToHeroCostume(id);
                costume2.checkstat();
                CostumeConeveter.HeroCostumeToLocalData(costume2, id);
                component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().init();
                if (costume2 != null)
                {
                    component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume = costume2;
                    component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume.stat = costume2.stat;
                }
                else
                {
                    costume2 = HeroCostume.costumeOption[3];
                    component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume = costume2;
                    component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume.stat = HeroStat.getInfo(costume2.name.ToUpper());
                }
                component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().setCharacterComponent();
                component.main_object.GetComponent<HERO>().setStat();
                component.main_object.GetComponent<HERO>().setSkillHUDPosition();
            }
            else
            {
                for (int j = 0; j < HeroCostume.costume.Length; j++)
                {
                    if (HeroCostume.costume[j].name.ToUpper() == id.ToUpper())
                    {
                        int num4 = HeroCostume.costume[j].id;
                        if (id.ToUpper() != "AHSS")
                        {
                            num4 += CheckBoxCostume.costumeSet - 1;
                        }
                        if (HeroCostume.costume[num4].name != HeroCostume.costume[j].name)
                        {
                            num4 = HeroCostume.costume[j].id + 1;
                        }
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().init();
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume = HeroCostume.costume[num4];
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume.stat = HeroStat.getInfo(HeroCostume.costume[num4].name.ToUpper());
                        component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().setCharacterComponent();
                        component.main_object.GetComponent<HERO>().setStat();
                        component.main_object.GetComponent<HERO>().setSkillHUDPosition();
                        break;
                    }
                }
            }
            CostumeConeveter.HeroCostumeToPhotonData(component.main_object.GetComponent<HERO>().GetComponent<HERO_SETUP>().myCostume, PhotonNetwork.player);
            if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE)
            {
                Transform transform = component.main_object.transform;
                transform.position += new Vector3((float) UnityEngine.Random.Range(-20, 20), 2f, (float) UnityEngine.Random.Range(-20, 20));
            }
            ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
            hashtable2.Add("dead", false);
            ExitGames.Client.Photon.Hashtable propertiesToSet = hashtable2;
            PhotonNetwork.player.SetCustomProperties(propertiesToSet);
            hashtable2 = new ExitGames.Client.Photon.Hashtable();
            hashtable2.Add(PhotonPlayerProperty.isTitan, 1);
            propertiesToSet = hashtable2;
            PhotonNetwork.player.SetCustomProperties(propertiesToSet);
        }
        component.enabled = true;
        GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().setHUDposition();
        GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = true;
        GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = true;
        component.gameOver = false;
        if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
        {
            Screen.lockCursor = true;
        }
        else
        {
            Screen.lockCursor = false;
        }
        Screen.showCursor = false;
        this.isLosing = false;
        this.ShowHUDInfoCenter(string.Empty);
    }

    public GameObject spawnTitan(int rate, Vector3 position, Quaternion rotation, bool punk = false)
    {
        GameObject obj3;
        GameObject obj2 = this.spawnTitanRaw(position, rotation);
        if (punk)
        {
            obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_PUNK, false);
        }
        else if (UnityEngine.Random.Range(0, 100) < rate)
        {
            if (IN_GAME_MAIN_CAMERA.difficulty == 2)
            {
                if ((UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.7f) || LevelInfo.getInfo(level).noCrawler)
                {
                    obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_JUMPER, false);
                }
                else
                {
                    obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_CRAWLER, false);
                }
            }
        }
        else if (IN_GAME_MAIN_CAMERA.difficulty == 2)
        {
            if ((UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.7f) || LevelInfo.getInfo(level).noCrawler)
            {
                obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_JUMPER, false);
            }
            else
            {
                obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_CRAWLER, false);
            }
        }
        else if (UnityEngine.Random.Range(0, 100) < rate)
        {
            if ((UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.8f) || LevelInfo.getInfo(level).noCrawler)
            {
                obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_I, false);
            }
            else
            {
                obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_CRAWLER, false);
            }
        }
        else if ((UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.8f) || LevelInfo.getInfo(level).noCrawler)
        {
            obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_JUMPER, false);
        }
        else
        {
            obj2.GetComponent<TITAN>().setAbnormalType(AbnormalType.TYPE_CRAWLER, false);
        }
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            obj3 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/FXtitanSpawn"), obj2.transform.position, Quaternion.Euler(-90f, 0f, 0f));
        }
        else
        {
            obj3 = PhotonNetwork.Instantiate("FX/FXtitanSpawn", obj2.transform.position, Quaternion.Euler(-90f, 0f, 0f), 0);
        }
        obj3.transform.localScale = obj2.transform.localScale;
        return obj2;
    }

    private GameObject spawnTitanRaw(Vector3 position, Quaternion rotation)
    {
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            return (GameObject) UnityEngine.Object.Instantiate(Resources.Load("TITAN_VER3.1"), position, rotation);
        }
        return PhotonNetwork.Instantiate("TITAN_VER3.1", position, rotation, 0);
    }

    private void Start()
    {
        base.gameObject.name = "MultiplayerManager";
        HeroCostume.init();
        CharacterMaterials.init();
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        this.heroes = new ArrayList();
        this.eT = new ArrayList();
        this.titans = new ArrayList();
        this.fT = new ArrayList();
        this.cT = new ArrayList();
        this.hooks = new ArrayList();
    }

    [RPC]
    public void titanGetKill(PhotonPlayer player, int Damage, string name)
    {
        Damage = Mathf.Max(10, Damage);
        object[] parameters = new object[] { Damage };
        base.photonView.RPC("netShowDamage", player, parameters);
        object[] objArray2 = new object[] { name, false };
        base.photonView.RPC("oneTitanDown", PhotonTargets.MasterClient, objArray2);
        this.sendKillInfo(false, (string) player.customProperties[PhotonPlayerProperty.name], true, name, Damage);
        this.playerKillInfoUpdate(player, Damage);
    }

    public void titanGetKillbyServer(int Damage, string name)
    {
        Damage = Mathf.Max(10, Damage);
        this.sendKillInfo(false, LoginFengKAI.player.name, true, name, Damage);
        this.netShowDamage(Damage);
        this.oneTitanDown(name, false);
        this.playerKillInfoUpdate(PhotonNetwork.player, Damage);
    }

    private void tryKick(KickState tmp)
    {
        this.sendChatContentInfo(string.Concat(new object[] { "kicking #", tmp.name, ", ", tmp.getKickCount(), "/", (int) (PhotonNetwork.playerList.Length * 0.5f), "vote" }));
        if (tmp.getKickCount() >= ((int) (PhotonNetwork.playerList.Length * 0.5f)))
        {
            this.kickPhotonPlayer(tmp.name.ToString());
        }
    }

    private void Update()
    {
        if ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && (GameObject.Find("LabelNetworkStatus") != null))
        {
            GameObject.Find("LabelNetworkStatus").GetComponent<UILabel>().text = PhotonNetwork.connectionStateDetailed.ToString();
            if (PhotonNetwork.connected)
            {
                UILabel component = GameObject.Find("LabelNetworkStatus").GetComponent<UILabel>();
                component.text = component.text + " ping:" + PhotonNetwork.GetPing();
            }
        }
        if (this.gameStart)
        {
            IEnumerator enumerator = this.heroes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    ((HERO) enumerator.Current).update();
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
	                disposable.Dispose();
                }
            }
            IEnumerator enumerator2 = this.hooks.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    ((Bullet) enumerator2.Current).update();
                }
            }
            finally
            {
                IDisposable disposable2 = enumerator2 as IDisposable;
                if (disposable2 != null)
                {
	                disposable2.Dispose();
                }
            }
            if (this.mainCamera != null)
            {
                this.mainCamera.snapShotUpdate();
            }
            IEnumerator enumerator3 = this.eT.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    ((TITAN_EREN) enumerator3.Current).update();
                }
            }
            finally
            {
                IDisposable disposable3 = enumerator3 as IDisposable;
                if (disposable3 != null)
                {
	                disposable3.Dispose();
                }
            }
            IEnumerator enumerator4 = this.titans.GetEnumerator();
            try
            {
                while (enumerator4.MoveNext())
                {
                    ((TITAN) enumerator4.Current).update();
                }
            }
            finally
            {
                IDisposable disposable4 = enumerator4 as IDisposable;
                if (disposable4 != null)
                {
	                disposable4.Dispose();
                }
            }
            IEnumerator enumerator5 = this.fT.GetEnumerator();
            try
            {
                while (enumerator5.MoveNext())
                {
                    ((FEMALE_TITAN) enumerator5.Current).update();
                }
            }
            finally
            {
                IDisposable disposable5 = enumerator5 as IDisposable;
                if (disposable5 != null)
                {
	                disposable5.Dispose();
                }
            }
            IEnumerator enumerator6 = this.cT.GetEnumerator();
            try
            {
                while (enumerator6.MoveNext())
                {
                    ((COLOSSAL_TITAN) enumerator6.Current).update();
                }
            }
            finally
            {
                IDisposable disposable6 = enumerator6 as IDisposable;
                if (disposable6 != null)
                {
	                disposable6.Dispose();
                }
            }
            if (this.mainCamera != null)
            {
                this.mainCamera.update();
            }
        }
    }

    [RPC]
    private void updateKillInfo(bool t1, string killer, bool t2, string victim, int dmg)
    {
        GameObject obj4;
        GameObject obj2 = GameObject.Find("UI_IN_GAME");
        GameObject obj3 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("UI/KillInfo"));
        for (int i = 0; i < this.killInfoGO.Count; i++)
        {
            obj4 = (GameObject) this.killInfoGO[i];
            if (obj4 != null)
            {
                obj4.GetComponent<KillInfoComponent>().moveOn();
            }
        }
        if (this.killInfoGO.Count > 4)
        {
            obj4 = (GameObject) this.killInfoGO[0];
            if (obj4 != null)
            {
                obj4.GetComponent<KillInfoComponent>().destory();
            }
            this.killInfoGO.RemoveAt(0);
        }
        obj3.transform.parent = obj2.GetComponent<UIReferArray>().panels[0].transform;
        obj3.GetComponent<KillInfoComponent>().show(t1, killer, t2, victim, dmg);
        this.killInfoGO.Add(obj3);
    }
}

