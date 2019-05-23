using Optimization;
using Optimization.Caching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FengGameManagerMKII : Photon.MonoBehaviour
{
    private static readonly Hashtable itweenHash = new Hashtable() { { "x", 0 }, { "y", 0 }, { "z", 0 }, { "easetype", iTween.EaseType.easeInBounce }, { "time", 0.5f }, { "delay", 2f } };
    private static Queue<KillInfoComponent> killInfoList = new Queue<KillInfoComponent>();
    private FEMALE_TITAN annie;
    //private ArrayList chatContent;
    private COLOSSAL_TITAN colossal;
    private float coreUpdate = 0.2f;
    private float currentSpeed;
    private bool endRacing;
    private TITAN_EREN eren;
    private float gameEndCD;
    private float gameEndTotalCDtime = 9f;
    private bool gameTimesUp;
    private List<HERO> heroes;
    private int highestwave = 1;
    private List<Bullet> hooks;
    private int humanScore;
    private bool isLosing;
    private bool isWinning;
    private ArrayList kicklist;
    private TextMesh labelInfoTopRight;
    private TextMesh labelInfoCenter;
    private TextMesh labelInfoTopCenter;
    private TextMesh labelInfoTopLeft;
    private string localRacingResult;
    private IN_GAME_MAIN_CAMERA mainCamera;
    private float maxSpeed;
    public string myLastHero;
    private string myLastRespawnTag = "playerRespawn";
    private PlayerList playerList;
    private int PVPhumanScoreMax = 200;
    private int PVPtitanScoreMax = 200;
    private ArrayList racingResult;
    private int single_kills;
    private int single_maxDamage;
    private int single_totalDamage;
    private bool startRacing;
    private int[] teamScores;
    private int teamWinner;
    private float timeElapse;
    private float timeTotalServer;
    private List<TITAN> titans;
    private int titanScore;

    public const string ApplicationId = "f1f6195c-df4a-40f9-bae5-4744c32901ef";
    public static FengGameManagerMKII FGM;
    public static FPSCounter FPS = new FPSCounter();
    public static bool LAN;
    public static LevelInfo Level;
    public static StylishComponent Stylish;
    public GameObject checkpoint;
    public int Difficulty;
    public bool GameStart;
    public bool JustSuicide;
    public float MyRespawnTime;
    public bool NeedChooseSide;
    public int PVPhumanScore;
    public int PVPtitanScore;
    public float RoundTime;
    public int Time = 600;
    public int Wave = 1;
    public static List<HERO> Heroes => FGM.heroes;
    public static List<TITAN> Titans => FGM.titans;
    public static UIReferArray UIRefer { get; private set; }

    private void Awake()
    {
        FGM = this;
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnConnectionFail, OnConnectionFail);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnConnectedToPhoton, OnConnectedToPhoton);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnCreatedRoom, OnCreatedRoom);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnDisconnectedFromPhoton, OnDisconnectedFromPhoton);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnJoinedLobby, OnJoinedLobby);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnJoinedRoom, OnJoinedRoom);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnLeftRoom, OnLeftRoom);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnMasterClientSwitched, OnMasterClientSwitched);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnPhotonPlayerConnected, OnPhotonPlayerConnected);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnPhotonPlayerDisconnected, OnPhotonPlayerDisconnected);
        NetworkingPeer.RegisterEvent(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, OnPhotonPlayerPropertiesChanged);
    }

    [RPC]
    private void Chat(string content, string sender, PhotonMessageInfo info)
    {
        if (content.Length > 7 && content.Substring(0, 7) == "/kick #")
        {
            if (PhotonNetwork.IsMasterClient)
            {
                this.KickPlayer(content.Remove(0, 7), sender);
            }
            return;
        }
        string message;
        if (sender != string.Empty) message = sender + ":" + content;
        else message = content;
        InRoomChat.Chat.AddLine($"[{info.sender.ID}] {message}");
    }

    private bool CheckIsTitanAllDie()
    {
        foreach(TITAN tit in titans)
        {
            if (!tit.hasDie)
            {
                return false;
            }
        }
        return annie == null;
    }

    private void Core(float dt)
    {
        if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single && this.NeedChooseSide)
        {
            if (FengCustomInputs.Main.isInputDown[InputCode.flare1])
            {
                if (NGUITools.GetActive(UIRefer.panels[3]))
                {
                    Screen.lockCursor = true;
                    Screen.showCursor = true;
                    NGUITools.SetActive(UIRefer.panels[0], true);
                    NGUITools.SetActive(UIRefer.panels[1], false);
                    NGUITools.SetActive(UIRefer.panels[2], false);
                    NGUITools.SetActive(UIRefer.panels[3], false);
                    IN_GAME_MAIN_CAMERA.SpecMov.disable = false;
                    IN_GAME_MAIN_CAMERA.Look.disable = false;
                }
                else
                {
                    Screen.lockCursor = false;
                    Screen.showCursor = true;
                    NGUITools.SetActive(UIRefer.panels[0], false);
                    NGUITools.SetActive(UIRefer.panels[1], false);
                    NGUITools.SetActive(UIRefer.panels[2], false);
                    NGUITools.SetActive(UIRefer.panels[3], true);
                    IN_GAME_MAIN_CAMERA.SpecMov.disable = true;
                    IN_GAME_MAIN_CAMERA.Look.disable = true;
                }
            }
            if (FengCustomInputs.Main.isInputDown[15])
            {
                if (!NGUITools.GetActive(UIRefer.panels[3]))
                {
                    NGUITools.SetActive(UIRefer.panels[0], false);
                    NGUITools.SetActive(UIRefer.panels[1], true);
                    NGUITools.SetActive(UIRefer.panels[2], false);
                    NGUITools.SetActive(UIRefer.panels[3], false);
                    Screen.showCursor = true;
                    Screen.lockCursor = false;
                    IN_GAME_MAIN_CAMERA.SpecMov.disable = true;
                    IN_GAME_MAIN_CAMERA.Look.disable = true;
                    FengCustomInputs.Main.showKeyMap();
                    FengCustomInputs.Main.justUPDATEME();
                    FengCustomInputs.Main.menuOn = true;
                }
            }
        }
        coreUpdate -= dt;
        this.timeElapse += dt;
        this.RoundTime += dt;
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
            {
                if (!this.isWinning)
                {
                    this.timeTotalServer += dt;
                }
            }
            else if (!this.isLosing && !this.isWinning)
            {
                this.timeTotalServer += dt;
            }
        }
        else
        {
            this.timeTotalServer += dt;
        }
        if (coreUpdate > 0f)
        {
            return;
        }
        float delta = coreUpdate - 0.2f;
        coreUpdate = 0.2f;
        if(PhotonNetwork.IsMasterClient)
        {
            CustomLevel.OnUpdate();
            if (CustomLevel.logicLoaded)
            {
                for(int i = 0; i < CustomLevel.titanSpawners.Count; i++)
                {
                    var item = CustomLevel.titanSpawners[i];
                    item.time -= 0.2f;
                    if(item.name == "spawnannie")
                    {
                        Pool.NetworkEnable("FEMALE_TITAN", item.location, Quaternion.identity, 0);
                    }
                    else
                    {
                        TITAN tit = Pool.NetworkEnable("TITAN_VER3.1", item.location, Quaternion.identity, 0).GetComponent<TITAN>();
                        AbnormalType type = AbnormalType.Normal;
                        switch (item.name)
                        {
                            case "spawnAbnormal":
                                type = AbnormalType.Aberrant;
                                break;

                            case "spawnJumper":
                                type = AbnormalType.Jumper;
                                break;

                            case "spawnCrawler":
                                type = AbnormalType.Crawler;
                                break;

                            case "spawnPunk":
                                type = AbnormalType.Punk;
                                break;

                            default:
                                break;
                        }
                        tit.setAbnormalType(type, type == AbnormalType.Crawler);
                        if (item.endless)
                        {
                            item.time = item.delay;
                        }
                        else
                        {
                            CustomLevel.titanSpawners.Remove(item);
                        }
                    }
                }
            }
        }
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && IN_GAME_MAIN_CAMERA.GameMode != GameMode.RACING && IN_GAME_MAIN_CAMERA.MainCamera.gameOver && !this.NeedChooseSide)
        {
            this.ShowHUDInfoCenter(string.Concat(new string[]
            {
                    "Press [F7D358]",
                    FengCustomInputs.Main.inputString[InputCode.flare1],
                    "[-] to spectate the next player. \nPress [F7D358]",
                    FengCustomInputs.Main.inputString[InputCode.flare2],
                    "[-] to spectate the previous player.\nPress [F7D358]",
                    FengCustomInputs.Main.inputString[InputCode.attack1],
                    "[-] to enter the spectator mode.\n\n\n\n"
            }));
            if (FengGameManagerMKII.Level.RespawnMode == RespawnMode.DEATHMATCH)
            {
                this.MyRespawnTime += 0.2f;
                int num = 10;
                if ((int)PhotonNetwork.player.Properties[PhotonPlayerProperty.isTitan] == 2)
                {
                    num = 15;
                }
                this.ShowHUDInfoCenterADD("Respawn in " + ((int)((float)num - this.MyRespawnTime)).ToString() + "s.");
                if (this.MyRespawnTime > (float)num)
                {
                    this.MyRespawnTime = 0f;
                    IN_GAME_MAIN_CAMERA.MainCamera.gameOver = false;
                    if ((int)PhotonNetwork.player.Properties[PhotonPlayerProperty.isTitan] == 2)
                    {
                        this.SpawnNonAITitan(this.myLastHero, "titanRespawn");
                    }
                    else
                    {
                        this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
                    }
                    IN_GAME_MAIN_CAMERA.MainCamera.gameOver = false;
                    this.ShowHUDInfoCenter(string.Empty);
                }
            }
        }
        else if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
            {
                if (!this.isLosing)
                {
                    this.currentSpeed = IN_GAME_MAIN_CAMERA.MainR.velocity.magnitude;
                    this.maxSpeed = Mathf.Max(this.maxSpeed, this.currentSpeed);
                    this.ShowHUDInfoTopLeft(string.Concat(new object[]
                    {
                        "Current Speed : ",
                        (int)this.currentSpeed,
                        "\nMax Speed:",
                        this.maxSpeed
                    }));
                }
            }
            else
            {
                this.ShowHUDInfoTopLeft(string.Concat(new object[]
                {
                    "Kills:",
                    this.single_kills,
                    "\nMax Damage:",
                    this.single_maxDamage,
                    "\nTotal Damage:",
                    this.single_totalDamage
                }));
            }
        }
        if (this.isLosing)
        {
            if (IN_GAME_MAIN_CAMERA.GameMode != GameMode.RACING)
            {
                if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
                {
                    if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
                    {
                        this.ShowHUDInfoCenter(string.Concat(new object[]
                        {
                            "Survive ",
                            this.Wave,
                            " Waves!\n Press ",
                            FengCustomInputs.Main.inputString[InputCode.restart],
                            " to Restart.\n\n\n"
                        }));
                    }
                    else
                    {
                        this.ShowHUDInfoCenter("Humanity Fail!\n Press " + FengCustomInputs.Main.inputString[InputCode.restart] + " to Restart.\n\n\n");
                    }
                }
                else
                {
                    if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
                    {
                        this.ShowHUDInfoCenter(string.Concat(new object[]
                        {
                            "Survive ",
                            this.Wave,
                            " Waves!\nGame Restart in ",
                            (int)this.gameEndCD,
                            "s\n\n"
                        }));
                    }
                    else
                    {
                        this.ShowHUDInfoCenter("Humanity Fail!\nAgain!\nGame Restart in " + (int)this.gameEndCD + "s\n\n");
                    }
                    if (this.gameEndCD <= 0f)
                    {
                        this.gameEndCD = 0f;
                        if (PhotonNetwork.IsMasterClient)
                        {
                            this.RestartGame(false);
                        }
                        this.ShowHUDInfoCenter(string.Empty);
                    }
                    else
                    {
                        this.gameEndCD -= 0.2f;
                    }
                }
            }
        }
        if (this.isWinning)
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
                {
                    this.ShowHUDInfoCenter(((float)((int)(this.timeTotalServer * 10f)) * 0.1f - 5f).ToString() + "s !\n Press " + FengCustomInputs.Main.inputString[InputCode.restart] + " to Restart.\n\n\n");
                }
                else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
                {
                    this.ShowHUDInfoCenter("Survive All Waves!\n Press " + FengCustomInputs.Main.inputString[InputCode.restart] + " to Restart.\n\n\n");
                }
                else
                {
                    this.ShowHUDInfoCenter("Humanity Win!\n Press " + FengCustomInputs.Main.inputString[InputCode.restart] + " to Restart.\n\n\n");
                }
            }
            else
            {
                if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
                {
                    this.ShowHUDInfoCenter(string.Concat(new object[]
                    {
                        this.localRacingResult,
                        "\n\nGame Restart in ",
                        (int)this.gameEndCD,
                        "s"
                    }));
                }
                else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
                {
                    this.ShowHUDInfoCenter("Survive All Waves!\nGame Restart in " + (int)this.gameEndCD + "s\n\n");
                }
                else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_AHSS)
                {
                    this.ShowHUDInfoCenter(string.Concat(new object[]
                    {
                        "Team ",
                        this.teamWinner,
                        " Win!\nGame Restart in ",
                        (int)this.gameEndCD,
                        "s\n\n"
                    }));
                }
                else
                {
                    this.ShowHUDInfoCenter("Humanity Win!\nGame Restart in " + (int)this.gameEndCD + "s\n\n");
                }
                if (this.gameEndCD <= 0f)
                {
                    this.gameEndCD = 0f;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        this.RestartGame(false);
                    }
                    this.ShowHUDInfoCenter(string.Empty);
                }
                else
                {
                    this.gameEndCD -= 0.2f;
                }
            }
        }
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                if (!this.isWinning)
                {
                    this.ShowHUDInfoTopCenter("Time : " + ((float)((int)(this.timeTotalServer * 10f)) * 0.1f - 5f));
                }
                if (this.timeTotalServer < 5f)
                {
                    this.ShowHUDInfoCenter("RACE START IN " + (int)(5f - this.timeTotalServer));
                }
                else if (!this.startRacing)
                {
                    this.ShowHUDInfoCenter(string.Empty);
                    this.startRacing = true;
                    this.endRacing = false;
                    GameObject obj = GameObject.Find("door");
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }
            }
            else
            {
                this.ShowHUDInfoTopCenter("Time : " + ((this.RoundTime >= 20f) ? ((float)((int)(this.RoundTime * 10f)) * 0.1f - 20f).ToString() : "WAITING"));
                if (this.RoundTime < 20f)
                {
                    this.ShowHUDInfoCenter("RACE START IN " + (int)(20f - this.RoundTime) + ((!(this.localRacingResult == string.Empty)) ? ("\nLast Round\n" + this.localRacingResult) : "\n\n"));
                }
                else if (!this.startRacing)
                {
                    this.ShowHUDInfoCenter(string.Empty);
                    this.startRacing = true;
                    this.endRacing = false;
                    GameObject obj = GameObject.Find("door");
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                    if (CustomLevel.racingDoors != null && CustomLevel.customLevelLoaded)
                    {
                        foreach (GameObject gameObject in CustomLevel.racingDoors)
                        {
                            gameObject.SetActive(false);
                        }
                        CustomLevel.racingDoors = null;
                    }
                }
                else if (CustomLevel.racingDoors != null && CustomLevel.customLevelLoaded)
                {
                    foreach (GameObject gameObject2 in CustomLevel.racingDoors)
                    {
                        gameObject2.SetActive(false);
                    }
                    CustomLevel.racingDoors = null;
                }
                else if (this.RoundTime < 50f && this.RoundTime > 20f)
                {
                    if (CustomLevel.racingDoors != null && CustomLevel.customLevelLoaded)
                    {
                        foreach (GameObject gameObject3 in CustomLevel.racingDoors)
                        {
                            gameObject3.SetActive(false);
                        }
                        CustomLevel.racingDoors = null;
                    }
                    GameObject obj2 = GameObject.Find("door");
                    if (obj2 != null)
                    {
                        obj2.SetActive(false);
                    }
                }
            }
            if (IN_GAME_MAIN_CAMERA.MainCamera.gameOver && !this.NeedChooseSide)
            {
                this.MyRespawnTime += 0.2f + dt;
                if (this.MyRespawnTime > 1.5f)
                {
                    this.MyRespawnTime = 0f;
                    IN_GAME_MAIN_CAMERA.MainCamera.gameOver = false;
                    this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
                    IN_GAME_MAIN_CAMERA.MainCamera.gameOver = false;
                    this.ShowHUDInfoCenter(string.Empty);
                }
            }
        }
        if (this.timeElapse > 1f)
        {
            this.timeElapse -= 1f;
            string text3 = string.Empty;
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.ENDLESS_TITAN)
            {
                text3 += "Time : ";
                text3 += ((int)((float)this.Time - this.timeTotalServer)).ToString();
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.KILL_TITAN)
            {
                text3 = "Titan Left: ";
                text3 += titans.Count.ToString();
                text3 += "  Time : ";
                if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
                {
                    text3 += ((int)this.timeTotalServer).ToString();
                }
                else
                {
                    text3 += ((int)((float)this.Time - this.timeTotalServer)).ToString();
                }
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
            {
                text3 = "Titan Left: ";
                text3 += titans.Count.ToString();
                text3 += " Wave : ";
                text3 += this.Wave;
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.BOSS_FIGHT_CT)
            {
                text3 = "Time : ";
                text3 += ((int)((float)this.Time - this.timeTotalServer)).ToString();
                text3 += "\nDefeat the Colossal Titan.\nPrevent abnormal titan from running to the north gate";
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
            {
                string text4 = "| ";
                for (int j = 0; j < PVPcheckPoint.chkPts.Count; j++)
                {
                    text4 = text4 + (PVPcheckPoint.chkPts[j] as PVPcheckPoint).getStateString() + " ";
                }
                text4 += "|";
                text3 = string.Concat(new object[]
                {
                    this.PVPtitanScoreMax - this.PVPtitanScore,
                    "  ",
                    text4,
                    "  ",
                    this.PVPhumanScoreMax - this.PVPhumanScore,
                    "\n"
                });
                text3 += "Time : ";
                text3 += ((int)((float)this.Time - this.timeTotalServer)).ToString();
            }
            if(IN_GAME_MAIN_CAMERA.GameMode != GameMode.RACING) ShowHUDInfoTopCenter(text3);
            text3 = string.Empty;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
                {
                    text3 = "Time : ";
                    text3 += ((int)this.timeTotalServer).ToString();
                }
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.ENDLESS_TITAN)
            {
                text3 = string.Concat(new object[]
                {
                    "Humanity ",
                    this.humanScore,
                    " : Titan ",
                    this.titanScore,
                    " "
                });
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.KILL_TITAN || IN_GAME_MAIN_CAMERA.GameMode == GameMode.BOSS_FIGHT_CT || IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
            {
                text3 = string.Concat(new object[]
                {
                    "Humanity ",
                    this.humanScore,
                    " : Titan ",
                    this.titanScore,
                    " "
                });
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode != GameMode.CAGE_FIGHT)
            {
                if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
                {
                    text3 = "Time : ";
                    text3 += ((int)((float)this.Time - this.timeTotalServer)).ToString();
                }
                else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_AHSS)
                {
                    for (int k = 0; k < this.teamScores.Length; k++)
                    {
                        string text2 = text3;
                        text3 = string.Concat(new object[]
                        {
                            text2,
                            (k == 0) ? string.Empty : " : ",
                            "Team",
                            k + 1,
                            " ",
                            this.teamScores[k],
                            string.Empty
                        });
                    }
                    text3 += "\nTime : ";
                    text3 += ((int)((float)this.Time - this.timeTotalServer)).ToString();
                }
            }
            this.ShowHUDInfoTopRight(text3);
            string text5 = (IN_GAME_MAIN_CAMERA.difficulty >= 0) ? ((IN_GAME_MAIN_CAMERA.difficulty != 0) ? ((IN_GAME_MAIN_CAMERA.difficulty != 1) ? "Abnormal" : "Hard") : "Normal") : "Trainning";
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.CAGE_FIGHT)
            {
                this.ShowHUDInfoTopRightMAPNAME(string.Concat(new object[]
                {
                    (int)this.RoundTime,
                    "s\n",
                    FengGameManagerMKII.Level,
                    " : ",
                    text5
                }));
            }
            else
            {
                this.ShowHUDInfoTopRightMAPNAME("\n" + Level.Name + " : " + text5);
            }
            this.ShowHUDInfoTopRightMAPNAME("\nCamera[" + FengCustomInputs.Main.inputString[InputCode.camera] + "]:" + IN_GAME_MAIN_CAMERA.CameraMode.ToString() + "\nFPS[[DD00DD]" + FPS.FPS + "[-]]");
        }
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            if (NeedChooseSide)
            {
                this.ShowHUDInfoTopCenter("\n\nPRESS 1 TO ENTER GAME");
            }
            if(killInfoList.Count > 0 && killInfoList.Peek() == null) killInfoList.Dequeue();
            if(timeTotalServer > Time && PhotonNetwork.IsMasterClient)
            {

                IN_GAME_MAIN_CAMERA.GameType = GameType.Stop;
                this.GameStart = false;
                Screen.lockCursor = false;
                Screen.showCursor = true;
                string names = string.Empty;
                string kills = string.Empty;
                string deaths = string.Empty;
                string maxs = string.Empty;
                string totals = string.Empty;
                string title = string.Empty;
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    names += player.UIName + "\n";
                    kills += player.Kills + "\n";
                    deaths += player.Deaths + "\n";
                    maxs += player.Max_Dmg + "\n";
                    totals += player.Total_Dmg + "\n";
                }
                switch (IN_GAME_MAIN_CAMERA.GameMode)
                {
                    case GameMode.PVP_AHSS:
                        for (int m = 0; m < this.teamScores.Length; m++)
                        {
                            title += ((m == 0) ? $"Team{m + 1} {teamScores[m]} " : " : ");
                        }
                        break;

                    case GameMode.SURVIVE_MODE:
                        title = "Highest Wave : " + this.highestwave;
                        break;

                    default:
                        title = $"Humanity {humanScore} : Titan {titanScore}";
                        break;
                }
                BasePV.RPC("showResult", PhotonTargets.AllBuffered, new object[] { names, kills, deaths, maxs, totals, title });
            }
        }
    }

    [RPC]
    private void clearlevel(string[] link, int gametype, PhotonMessageInfo info)
    {
        if (info.sender.IsMasterClient)
        {
            if (gametype == 0)
            {
                IN_GAME_MAIN_CAMERA.GameMode = GameMode.KILL_TITAN;
                return;
            }
            if (gametype == 1)
            {
                IN_GAME_MAIN_CAMERA.GameMode = GameMode.SURVIVE_MODE;
                return;
            }
            if (gametype == 2)
            {
                IN_GAME_MAIN_CAMERA.GameMode = GameMode.PVP_AHSS;
                return;
            }
            if (gametype == 3)
            {
                IN_GAME_MAIN_CAMERA.GameMode = GameMode.RACING;
                return;
            }
            if (gametype == 4)
            {
                IN_GAME_MAIN_CAMERA.GameMode = GameMode.None;
            }
        }
    }


    [RPC]
    private void customlevelRPC(string[] content, PhotonMessageInfo info)
    {
        if (!info.sender.IsMasterClient)
        {
            return;
        }
        CustomLevel.RPC(content);
    }

    [RPC]
    private void getRacingResult(string player, float time)
    {
        RacingResult racingResult = new RacingResult();
        racingResult.name = player;
        racingResult.time = time;
        this.racingResult.Add(racingResult);
        this.RefreshRacingResult();
    }

    private void KickPhotonPlayer(string name)
    {
        foreach (PhotonPlayer photonPlayer in PhotonNetwork.playerList)
        {
            if (photonPlayer.ID.ToString() == name && !photonPlayer.IsMasterClient)
            {
                PhotonNetwork.CloseConnection(photonPlayer);
                return;
            }
        }
    }

    private void KickPlayer(string kickPlayer, string kicker)
    {
        bool flag = false;
        for (int i = 0; i < this.kicklist.Count; i++)
        {
            if (((KickState)this.kicklist[i]).name == kickPlayer)
            {
                KickState kickState = (KickState)this.kicklist[i];
                kickState.addKicker(kicker);
                this.TryKick(kickState);
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            KickState kickState = new KickState();
            kickState.init(kickPlayer);
            kickState.addKicker(kicker);
            this.kicklist.Add(kickState);
            this.TryKick(kickState);
        }
    }

    private void LateUpdate()
    {
        if (this.GameStart)
        {
            foreach (var h in heroes)
            {
                h.lateUpdate();
            }
            foreach (var t in titans)
            {
                t.lateUpdate();
            }
            Core(UnityEngine.Time.deltaTime);
        }
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
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_AHSS)
        {
            this.teamWinner = score;
            this.teamScores[this.teamWinner - 1]++;
            this.gameEndCD = this.gameEndTotalCDtime;
        }
        else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
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

    private void OnLevelWasLoaded(int level)
    {
        ChangeQuality.setCurrentQuality();
        if (level == 0)
        {
            return;
        }
        if (Application.loadedLevelName == "characterCreation" || Application.loadedLevelName == "SnapShot")
        {
            return;
        }
        GameObject[] array = GameObject.FindGameObjectsWithTag("titan");
        foreach (GameObject gameObject in array)
        {
            if (gameObject.GetPhotonView() == null || !gameObject.GetPhotonView().owner.IsMasterClient)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
        this.isWinning = false;
        this.GameStart = true;
        RespawnPositions.Dispose();
        this.ShowHUDInfoCenter(string.Empty);
        GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("MainCamera_mono"), CacheGameObject.Find("cameraDefaultPosition").transform.position, CacheGameObject.Find("cameraDefaultPosition").transform.rotation);
        UnityEngine.Object.Destroy(CacheGameObject.Find("cameraDefaultPosition"));
        gameObject2.name = "MainCamera";
        Screen.lockCursor = true;
        Screen.showCursor = true;
        var ui = (GameObject)Instantiate(CacheResources.Load("UI_IN_GAME"));
        ui.name = "UI_IN_GAME";
        ui.SetActive(true);
        UIRefer = ui.GetComponent<UIReferArray>();
        NGUITools.SetActive(UIRefer.panels[0], true);
        NGUITools.SetActive(UIRefer.panels[1], false);
        NGUITools.SetActive(UIRefer.panels[2], false);
        NGUITools.SetActive(UIRefer.panels[3], false);
        IN_GAME_MAIN_CAMERA.MainCamera.setHUDposition();
        IN_GAME_MAIN_CAMERA.MainCamera.setDayLight(IN_GAME_MAIN_CAMERA.DayLight);
        LevelInfo info = Level;
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            this.single_kills = 0;
            this.single_maxDamage = 0;
            this.single_totalDamage = 0;
            IN_GAME_MAIN_CAMERA.MainCamera.enabled = true;
            IN_GAME_MAIN_CAMERA.SpecMov.disable = true;
            IN_GAME_MAIN_CAMERA.Look.disable = true;
            IN_GAME_MAIN_CAMERA.GameMode = FengGameManagerMKII.Level.Mode;
            this.SpawnPlayer(IN_GAME_MAIN_CAMERA.singleCharacter.ToUpper(), "playerRespawn");
            if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
            {
                Screen.lockCursor = true;
            }
            else
            {
                Screen.lockCursor = false;
            }
            Screen.showCursor = false;
            int rate = 90;
            if (this.Difficulty == 1)
            {
                rate = 70;
            }
            this.RandomSpawnTitan("titanRespawn", rate, info.EnemyNumber, false);
            return;
        }
        playerList = new PlayerList();
        PVPcheckPoint.chkPts = new ArrayList();
        IN_GAME_MAIN_CAMERA.MainCamera.enabled = false;
        IN_GAME_MAIN_CAMERA.BaseCamera.GetComponent<CameraShake>().enabled = false;
        IN_GAME_MAIN_CAMERA.GameType = GameType.Multi;
        CustomLevel.loadLevel();
        if (info.Mode == GameMode.TROST)
        {
            CacheGameObject.Find("playerRespawn").SetActive(false);
            UnityEngine.Object.Destroy(CacheGameObject.Find("playerRespawn"));
            GameObject gameObject3 = CacheGameObject.Find("rock");
            gameObject3.animation["lift"].speed = 0f;
            CacheGameObject.Find("door_fine").SetActive(false);
            CacheGameObject.Find("door_broke").SetActive(true);
            UnityEngine.Object.Destroy(CacheGameObject.Find("ppl"));
        }
        else if (info.Mode == GameMode.BOSS_FIGHT_CT)
        {
            CacheGameObject.Find("playerRespawnTrost").SetActive(false);
            UnityEngine.Object.Destroy(CacheGameObject.Find("playerRespawnTrost"));
        }
        if (this.NeedChooseSide)
        {
            this.ShowHUDInfoTopCenterADD("\n\nPRESS 1 TO ENTER GAME");
        }
        else
        {
            if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
            {
                Screen.lockCursor = true;
            }
            else
            {
                Screen.lockCursor = false;
            }
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
            {
                if ((int)PhotonNetwork.player.Properties[PhotonPlayerProperty.isTitan] == 2)
                {
                    this.checkpoint = CacheGameObject.Find("PVPchkPtT");
                }
                else
                {
                    this.checkpoint = CacheGameObject.Find("PVPchkPtH");
                }
            }
            if ((int)PhotonNetwork.player.Properties[PhotonPlayerProperty.isTitan] == 2)
            {
                this.SpawnNonAITitan(this.myLastHero, "titanRespawn");
            }
            else
            {
                this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
            }
        }
        if (info.Mode == GameMode.BOSS_FIGHT_CT)
        {
            UnityEngine.Object.Destroy(CacheGameObject.Find("rock"));
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (info.Mode == GameMode.TROST)
            {
                if (!this.IsPlayerAllDead())
                {
                    GameObject gameObject4 = Optimization.Caching.Pool.NetworkEnable("TITAN_EREN_trost", new Vector3(-200f, 0f, -194f), Quaternion.Euler(0f, 180f, 0f), 0);
                    gameObject4.GetComponent<TITAN_EREN>().rockLift = true;
                    int rate2 = 90;
                    if (this.Difficulty == 1)
                    {
                        rate2 = 70;
                    }
                    GameObject[] array3 = GameObject.FindGameObjectsWithTag("titanRespawn");
                    GameObject gameObject5 = CacheGameObject.Find("titanRespawnTrost");
                    if (gameObject5 != null)
                    {
                        foreach (GameObject gameObject6 in array3)
                        {
                            if (gameObject6.transform.parent.gameObject == gameObject5)
                            {
                                this.SpawnTitan(rate2, gameObject6.transform.position, gameObject6.transform.rotation, false);
                            }
                        }
                    }
                }
            }
            else if (info.Mode == GameMode.BOSS_FIGHT_CT)
            {
                if (!this.IsPlayerAllDead())
                {
                    Optimization.Caching.Pool.NetworkEnable("COLOSSAL_TITAN", -Vectors.up * 10000f, Quaternion.Euler(0f, 180f, 0f), 0);
                }
            }
            else if (info.Mode == GameMode.KILL_TITAN || info.Mode == GameMode.ENDLESS_TITAN || info.Mode == GameMode.SURVIVE_MODE)
            {
                if (info.Name == "Annie" || info.Name == "Annie II")
                {
                    Optimization.Caching.Pool.NetworkEnable("FEMALE_TITAN", CacheGameObject.Find("titanRespawn").transform.position, CacheGameObject.Find("titanRespawn").transform.rotation, 0);
                }
                else
                {
                    int rate3 = 90;
                    if (this.Difficulty == 1)
                    {
                        rate3 = 70;
                    }
                    this.RandomSpawnTitan("titanRespawn", rate3, info.EnemyNumber, false);
                }
            }
            else if (info.Mode != GameMode.TROST)
            {
                if (info.Mode == GameMode.PVP_CAPTURE && FengGameManagerMKII.Level.MapName == "OutSide")
                {
                    GameObject[] array5 = GameObject.FindGameObjectsWithTag("titanRespawn");
                    if (array5.Length <= 0)
                    {
                        return;
                    }
                    for (int k = 0; k < array5.Length; k++)
                    {
                        this.spawnTitanRaw(array5[k].transform.position, array5[k].transform.rotation).setAbnormalType(AbnormalType.Crawler, true);
                    }
                }
            }
        }
        if (!info.Supply)
        {
            UnityEngine.Object.Destroy(CacheGameObject.Find("aot_supply"));
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            BasePV.RPC("RequireStatus", PhotonTargets.MasterClient, new object[0]);
        }
        if (Stylish != null)
        {
            Stylish.enabled = true;
        }
        if (FengGameManagerMKII.Level.LavaMode)
        {
            UnityEngine.Object.Instantiate(CacheResources.Load("levelBottom"), new Vector3(0f, -29.5f, 0f), Quaternion.Euler(0f, 0f, 0f));
            CacheGameObject.Find("aot_supply").transform.position = CacheGameObject.Find("aot_supply_lava_position").transform.position;
            CacheGameObject.Find("aot_supply").transform.rotation = CacheGameObject.Find("aot_supply_lava_position").transform.rotation;
        }
        Resources.UnloadUnusedAssets();
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
        this.teamScores = score1;
    }

    private void RefreshRacingResult()
    {
        this.localRacingResult = "Result\n";
        IComparer comparer = new IComparerRacingResult();
        this.racingResult.Sort(comparer);
        int num = this.racingResult.Count;
        num = Mathf.Min(num, 6);
        for (int i = 0; i < num; i++)
        {
            string text = this.localRacingResult;
            this.localRacingResult = string.Concat(new object[]
            {
                text,
                "Rank ",
                i + 1,
                " : "
            });
            this.localRacingResult += (this.racingResult[i] as RacingResult).name;
            this.localRacingResult = this.localRacingResult + "   " + ((float)((int)((this.racingResult[i] as RacingResult).time * 100f)) * 0.01f).ToString() + "s";
            this.localRacingResult += "\n";
        }
        BasePV.RPC("netRefreshRacingResult", PhotonTargets.All, new object[]
        {
            this.localRacingResult
        });
    }

    [RPC]
    private void refreshStatus(int score1, int score2, int wav, int highestWav, float time1, float time2, bool startRacin, bool endRacin)
    {
        this.humanScore = score1;
        this.titanScore = score2;
        this.Wave = wav;
        this.highestwave = highestWav;
        this.RoundTime = time1;
        this.timeTotalServer = time2;
        this.startRacing = startRacin;
        this.endRacing = endRacin;
        if (this.startRacing && CacheGameObject.Find("door"))
        {
            CacheGameObject.Find("door").SetActive(false);
        }
    }

    [RPC]
    private void RequireStatus()
    {
        BasePV.RPC("refreshStatus", PhotonTargets.Others, new object[]
        {
            this.humanScore,
            this.titanScore,
            this.Wave,
            this.highestwave,
            this.RoundTime,
            this.timeTotalServer,
            this.startRacing,
            this.endRacing
        });
        BasePV.RPC("refreshPVPStatus", PhotonTargets.Others, new object[]
        {
            this.PVPhumanScore,
            this.PVPtitanScore
        });
        BasePV.RPC("refreshPVPStatus_AHSS", PhotonTargets.Others, new object[]
        {
            this.teamScores
        });
    }

    [RPC]
    private void respawnHeroInNewRound()
    {
        if (this.NeedChooseSide)
        {
            return;
        }
        if (IN_GAME_MAIN_CAMERA.MainCamera.gameOver)
        {
            this.SpawnPlayer(this.myLastHero, this.myLastRespawnTag);
            IN_GAME_MAIN_CAMERA.MainCamera.gameOver = false;
            this.ShowHUDInfoCenter(string.Empty);
        }
    }

    [RPC]
    private void restartGameByClient()
    {
        this.RestartGame(false);
    }

    [RPC]
    private void RPCLoadLevel()
    {
        PhotonNetwork.LoadLevel(FengGameManagerMKII.Level.MapName);
    }

    [RPC]
    private void showChatContent(string content)
    {
        //    this.chatContent.Add(content);
        //    if (this.chatContent.Count > 10)
        //    {
        //        this.chatContent.RemoveAt(0);
        //    }
        //    CacheGameObject.Find("LabelChatContent").GetComponent<UILabel>().text = string.Empty;
        //    for (int i = 0; i < this.chatContent.Count; i++)
        //    {
        //        UILabel component = CacheGameObject.Find("LabelChatContent").GetComponent<UILabel>();
        //        component.text += this.chatContent[i];
        //    }
        Debug.Log("showChatContent sent. Content: " + content);
    }

    #region Show UILabels
    internal void ShowHUDInfoCenter(string content)
    {
        Labels.Center = content.ToRGBA();
    }

    internal void ShowHUDInfoCenterADD(string content)
    {
        Labels.Center = content.ToRGBA();
    }

    internal void ShowHUDInfoTopCenter(string content)
    {
        Labels.TopCenter = content.ToRGBA();
    }

    internal void ShowHUDInfoTopCenterADD(string content)
    {
        Labels.TopCenter = content.ToRGBA();
    }

    internal void ShowHUDInfoTopLeft(string content)
    {
        Labels.TopLeft= content.ToRGBA();
    }

    internal void ShowHUDInfoTopRight(string content)
    {
        Labels.TopRight = content.ToRGBA();
    }

    internal void ShowHUDInfoTopRightMAPNAME(string content)
    {
        Labels.TopRight += content.ToRGBA();
    }
    #endregion

    [RPC]
    private void showResult(string text0, string text1, string text2, string text3, string text4, string text6, PhotonMessageInfo t)
    {
        if (this.gameTimesUp)
        {
            return;
        }
        this.gameTimesUp = true;
        NGUITools.SetActive(UIRefer.panels[0], false);
        NGUITools.SetActive(UIRefer.panels[1], false);
        NGUITools.SetActive(UIRefer.panels[2], true);
        NGUITools.SetActive(UIRefer.panels[3], false);
        CacheGameObject.Find<UILabel>("LabelName").text = text0;
        CacheGameObject.Find<UILabel>("LabelKill").text = text1;
        CacheGameObject.Find<UILabel>("LabelDead").text = text2;
        CacheGameObject.Find<UILabel>("LabelMaxDmg").text = text3;
        CacheGameObject.Find<UILabel>("LabelTotalDmg").text = text4;
        CacheGameObject.Find<UILabel>("LabelResultTitle").text = text6;
        Screen.lockCursor = false;
        Screen.showCursor = true;
        IN_GAME_MAIN_CAMERA.GameType = GameType.Stop;
        this.GameStart = false;
    }

    private TITAN spawnTitanRaw(Vector3 position, Quaternion rotation)
    {
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            return ((GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("TITAN_VER3.1"), position, rotation)).GetComponent<TITAN>();
        }
        else
        {
            return Optimization.Caching.Pool.NetworkEnable("TITAN_VER3.1", position, rotation, 0).GetComponent<TITAN>();
        }
    }

    private void Start()
    {
        ChangeQuality.setCurrentQuality();
        name = "MultiplayerManager";
        DontDestroyOnLoad(this);
        HeroCostume.Init();
        CharacterMaterials.init();
        RCManager.Clear();
        this.heroes = new List<HERO>();
        this.titans = new List<TITAN>();
        this.hooks = new List<Bullet>();
        //Application.targetFrameRate = 60;
        //UnityEngine.Time.fixedDeltaTime = 1f / 60f;
    }

    private void TryKick(KickState tmp)
    {
        this.SendChatContentInfo(string.Concat(new object[]
        {
            "kicking #",
            tmp.name,
            ", ",
            tmp.getKickCount(),
            "/",
            (int)((float)PhotonNetwork.playerList.Length * 0.5f),
            "vote"
        }));
        if (tmp.getKickCount() >= (int)((float)PhotonNetwork.playerList.Length * 0.5f))
        {
            this.KickPhotonPlayer(tmp.name.ToString());
        }
    }

    private void Update()
    {
        FPS.FPSUpdate();
        if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
        {
            Labels.NetworkStatus = PhotonNetwork.connectionStateDetailed.ToString() + (PhotonNetwork.connected ? " ping: " + PhotonNetwork.GetPing() : "");
        }
        if (!GameStart) return;
        foreach (var h in heroes)
        {
            h.update();
        }
        foreach (var b in hooks)
        {
            b.update();
        }
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single || PhotonNetwork.IsMasterClient)
        {
            foreach (var t in titans)
            {
                t.update();
            }
        }
        if (mainCamera)
        {
            mainCamera.update();
            mainCamera.snapShotUpdate();
        }
    }

    [RPC]
    private void updateKillInfo(bool t1, string killer, bool t2, string victim, int dmg)
    {
        //var killInfo = Pool.Enable("UI/KillInfo").GetComponent<KillInfoComponent>();
        var killInfo = ((GameObject)Instantiate(CacheResources.Load("UI/KillInfo"))).GetComponent<KillInfoComponent>();
        using (var ien = killInfoList.GetEnumerator())
        {
            while (ien.MoveNext())
            {
                if (ien.Current != null)
                {
                    ien.Current.moveOn();
                }
            }
        }
        if (killInfoList.Count > 4)
        {
            killInfoList.Dequeue().destroy();
        }
        killInfo.SetParent(UIRefer.panels[0].transform);
        killInfo.Show(t1, killer, t2, victim, dmg);
        killInfoList.Enqueue(killInfo);
    }

    public void AddCamera(IN_GAME_MAIN_CAMERA c)
    {
        this.mainCamera = c;
    }

    public void AddCT(COLOSSAL_TITAN titan)
    {
        colossal = titan;
    }

    public void AddET(TITAN_EREN hero)
    {
        eren = hero;
    }

    public void AddFT(FEMALE_TITAN titan)
    {
        annie = titan;
    }

    public void AddHero(HERO hero)
    {
        this.heroes.Add(hero);
    }

    public void AddHook(Bullet h)
    {
        this.hooks.Add(h);
    }

    public void AddTitan(TITAN titan)
    {
        this.titans.Add(titan);
    }

    public void CheckPVPpts()
    {
        if (this.PVPtitanScore >= this.PVPtitanScoreMax)
        {
            this.PVPtitanScore = this.PVPtitanScoreMax;
            this.GameLose();
        }
        else if (this.PVPhumanScore >= this.PVPhumanScoreMax)
        {
            this.PVPhumanScore = this.PVPhumanScoreMax;
            this.GameWin();
        }
    }

    public void GameLose()
    {
        if (this.isWinning)
        {
            return;
        }
        if (this.isLosing)
        {
            return;
        }
        this.isLosing = true;
        this.titanScore++;
        this.gameEndCD = this.gameEndTotalCDtime;
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            BasePV.RPC("netGameLose", PhotonTargets.Others, new object[]
            {
                this.titanScore
            });
        }
    }

    public void GameWin()
    {
        if (this.isLosing)
        {
            return;
        }
        if (this.isWinning)
        {
            return;
        }
        this.isWinning = true;
        this.humanScore++;
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.RACING)
        {
            this.gameEndCD = 20f;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
            {
                BasePV.RPC("netGameWin", PhotonTargets.Others, new object[]
                {
                    0
                });
            }
        }
        else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_AHSS)
        {
            this.gameEndCD = this.gameEndTotalCDtime;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
            {
                BasePV.RPC("netGameWin", PhotonTargets.Others, new object[]
                {
                    this.teamWinner
                });
            }
            this.teamScores[this.teamWinner - 1]++;
        }
        else
        {
            this.gameEndCD = this.gameEndTotalCDtime;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
            {
                BasePV.RPC("netGameWin", PhotonTargets.Others, new object[]
                {
                    this.humanScore
                });
            }
        }
    }

    public bool IsPlayerAllDead()
    {
        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.IsTitan || player.Dead) continue;
            return false;
        }
        return true;
    }

    public bool IsTeamAllDead(int team)
    {
        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.IsTitan) continue;
            if (player.Team == team && !player.Dead) return false;

        }
        return true;
    }

    public void MultiplayerRacingFinsih()
    {
        float num = this.RoundTime - 20f;
        if (PhotonNetwork.IsMasterClient)
        {
            this.getRacingResult(LoginFengKAI.player.name, num);
        }
        else
        {
            BasePV.RPC("getRacingResult", PhotonTargets.MasterClient, new object[]
            {
                LoginFengKAI.player.name,
                num
            });
        }
        this.GameWin();
    }

    [RPC]
    public void netShowDamage(int speed)
    {
        Stylish?.Style(speed);
        //CacheGameObject.Find<StylishComponent>("Stylish").Style(speed);
        var target = CacheGameObject.Find<UILabel>("LabelScore");
        if (target != null)
        {
            target.text = speed.ToString();
            target.transform.localScale = Vectors.zero;
            speed = (int)(speed * 0.1f);
            speed = Mathf.Max(40, speed);
            speed = Mathf.Min(150, speed);
            iTween.Stop(target.cachedGameObject);
            iTween.ScaleTo(target.cachedGameObject, new System.Collections.Hashtable() { { "x", speed }, { "y", speed }, { "z", speed }, { "easetype", iTween.EaseType.easeOutElastic }, { "time", 1f } });
            iTween.ScaleTo(target.cachedGameObject, itweenHash);
        }
    }

    public void NOTSpawnNonAITitan(string id)
    {
        this.myLastHero = id.ToUpper();
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                "dead",
                true
            }
        };
        PhotonNetwork.player.SetCustomProperties(customProperties);
        customProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                PhotonPlayerProperty.isTitan,
                2
            }
        };
        PhotonNetwork.player.SetCustomProperties(customProperties);
        if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
        {
            Screen.lockCursor = true;
        }
        else
        {
            Screen.lockCursor = false;
        }
        Screen.showCursor = true;
        this.ShowHUDInfoCenter("the game has started for 60 seconds.\n please wait for next round.\n Click Right Mouse Key to Enter or Exit the Spectator Mode.");
        IN_GAME_MAIN_CAMERA.MainCamera.enabled = true;
        IN_GAME_MAIN_CAMERA.MainCamera.SetMainObject(null, true, false);
        IN_GAME_MAIN_CAMERA.MainCamera.setSpectorMode(true);
        IN_GAME_MAIN_CAMERA.MainCamera.gameOver = true;
    }

    public void NOTSpawnPlayer(string id)
    {
        this.myLastHero = id.ToUpper();
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                "dead",
                true
            }
        };
        PhotonNetwork.player.SetCustomProperties(customProperties);
        customProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                PhotonPlayerProperty.isTitan,
                1
            }
        };
        PhotonNetwork.player.SetCustomProperties(customProperties);
        if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
        {
            Screen.lockCursor = true;
        }
        else
        {
            Screen.lockCursor = false;
        }
        Screen.showCursor = false;
        this.ShowHUDInfoCenter("the game has started for 60 seconds.\n please wait for next round.\n Click Right Mouse Key to Enter or Exit the Spectator Mode.");
        IN_GAME_MAIN_CAMERA.MainCamera.enabled = true;
        IN_GAME_MAIN_CAMERA.MainCamera.SetMainObject(null, true, false);
        IN_GAME_MAIN_CAMERA.MainCamera.setSpectorMode(true);
        IN_GAME_MAIN_CAMERA.MainCamera.gameOver = true;
    }

    public void OnConnectedToMaster(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnConnectedToMaster");
    }

    public void OnConnectedToPhoton(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnConnectedToPhoton");
    }

    public void OnConnectionFail(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnConnectionFail : " + args.DisconnectCause.ToString());
        Screen.lockCursor = false;
        Screen.showCursor = true;
        IN_GAME_MAIN_CAMERA.GameType = GameType.Stop;
        this.GameStart = false;
        NGUITools.SetActive(UIRefer.panels[0], false);
        NGUITools.SetActive(UIRefer.panels[1], false);
        NGUITools.SetActive(UIRefer.panels[2], false);
        NGUITools.SetActive(UIRefer.panels[3], false);
        NGUITools.SetActive(UIRefer.panels[4], true);
        CacheGameObject.Find("LabelDisconnectInfo").GetComponent<UILabel>().text = "OnConnectionFail : " + args.DisconnectCause.ToString();
    }

    public void OnCreatedRoom(AOTEventArgs args)
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

    private void OnDestroy()
    {
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnConnectionFail, OnConnectionFail);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnConnectedToPhoton, OnConnectedToPhoton);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnCreatedRoom, OnCreatedRoom);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnDisconnectedFromPhoton, OnDisconnectedFromPhoton);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnJoinedLobby, OnJoinedLobby);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnJoinedRoom, OnJoinedRoom);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnLeftRoom, OnLeftRoom);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnMasterClientSwitched, OnMasterClientSwitched);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnPhotonPlayerConnected, OnPhotonPlayerConnected);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnPhotonPlayerDisconnected, OnPhotonPlayerDisconnected);
        NetworkingPeer.RemoveEvent(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, OnPhotonPlayerPropertiesChanged);
    }

    public void OnDisconnectedFromPhoton(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnDisconnectedFromPhoton");
        Screen.lockCursor = false;
        Screen.showCursor = true;
    }

    [RPC]
    public void oneTitanDown(string name1 = "", bool onPlayerLeave = false)
    {
        if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
        }
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
        {
            if (!(name1 == string.Empty))
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
            this.CheckPVPpts();
            BasePV.RPC("refreshPVPStatus", PhotonTargets.Others, new object[]
            {
                this.PVPhumanScore,
                this.PVPtitanScore
            });
        }
        else if (IN_GAME_MAIN_CAMERA.GameMode != GameMode.CAGE_FIGHT)
        {
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.KILL_TITAN)
            {
                if (this.CheckIsTitanAllDie())
                {
                    this.GameWin();
                    IN_GAME_MAIN_CAMERA.MainCamera.gameOver = true;
                }
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE)
            {
                if (this.CheckIsTitanAllDie())
                {
                    this.Wave++;
                    if (FengGameManagerMKII.Level.RespawnMode == RespawnMode.NEWROUND && IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        BasePV.RPC("respawnHeroInNewRound", PhotonTargets.All, new object[0]);
                    }
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        this.SendChatContentInfo("<color=#A8FF24>Wave : " + this.Wave + "</color>");
                    }
                    if (this.Wave > this.highestwave)
                    {
                        this.highestwave = this.Wave;
                    }
                    if (PhotonNetwork.IsMasterClient)
                    {
                        this.RequireStatus();
                    }
                    if (this.Wave > 20)
                    {
                        this.GameWin();
                    }
                    else
                    {
                        int rate = 90;
                        if (this.Difficulty == 1)
                        {
                            rate = 70;
                        }
                        if (!FengGameManagerMKII.Level.PunksEnabled)
                        {
                            this.RandomSpawnTitan("titanRespawn", rate, this.Wave + 2, false);
                        }
                        else if (this.Wave == 5)
                        {
                            this.RandomSpawnTitan("titanRespawn", rate, 1, true);
                        }
                        else if (this.Wave == 10)
                        {
                            this.RandomSpawnTitan("titanRespawn", rate, 2, true);
                        }
                        else if (this.Wave == 15)
                        {
                            this.RandomSpawnTitan("titanRespawn", rate, 3, true);
                        }
                        else if (this.Wave == 20)
                        {
                            this.RandomSpawnTitan("titanRespawn", rate, 4, true);
                        }
                        else
                        {
                            this.RandomSpawnTitan("titanRespawn", rate, this.Wave + 2, false);
                        }
                    }
                }
            }
            else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.ENDLESS_TITAN)
            {
                if (!onPlayerLeave)
                {
                    this.humanScore++;
                    int rate2 = 90;
                    if (this.Difficulty == 1)
                    {
                        rate2 = 70;
                    }
                    this.RandomSpawnTitan("titanRespawn", rate2, 1, false);
                }
            }
        }
    }

    public void OnFailedToConnectToPhoton(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnFailedToConnectToPhoton");
    }

    public void OnJoinedLobby(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnJoinedLobby");
        NGUITools.SetActive(UIMainReferences.Main.panelMultiStart, false);
        NGUITools.SetActive(UIMainReferences.Main.panelMultiROOM, true);
    }

    public void OnJoinedRoom(AOTEventArgs args)
    {
        var strArray = PhotonNetwork.room.name.Split('`');
        Debug.Log("OnJoinedRoom " + PhotonNetwork.room.name + "    >>>>   " + LevelInfo.GetInfo(strArray[1]).MapName);
        gameTimesUp = false;
        Level = LevelInfo.GetInfo(strArray[1]);
        switch (strArray[2].ToLower())
        {
            case "normal":
                Difficulty = 0;
                break;
            case "hard":
                Difficulty = 1;
                break;
            case "abnormal":
                Difficulty = 2;
                break;
            default:
                Difficulty = 1;
                break;
        }
        IN_GAME_MAIN_CAMERA.difficulty = this.Difficulty;
        Time = int.Parse(strArray[3]) * 60;
        switch (strArray[4].ToLower())
        {
            case "day":
            case "день":
                IN_GAME_MAIN_CAMERA.DayLight = DayLight.Day;
                break;
            case "dawn":
            case "вечер":
                IN_GAME_MAIN_CAMERA.DayLight = DayLight.Dawn;
                break;
            case "night":
            case "ночь":
                IN_GAME_MAIN_CAMERA.DayLight = DayLight.Night;
                break;
            default:
                IN_GAME_MAIN_CAMERA.DayLight = DayLight.Dawn;
                break;
        }
        IN_GAME_MAIN_CAMERA.GameMode = Level.Mode;
        PhotonNetwork.LoadLevel(Level.MapName);
        var player = PhotonNetwork.player;
        player.UIName = LoginFengKAI.player.name;
        player.GuildName =LoginFengKAI.player.guildname;
        player.Kills = player.Deaths = player.Max_Dmg = player.Total_Dmg = 0;
        player.Dead = true;
        player.IsTitan = false;
        humanScore = 0;
        titanScore = 0;
        PVPtitanScore = 0;
        PVPhumanScore = 0;
        Wave = 1;
        highestwave = 1;
        localRacingResult = string.Empty;
        NeedChooseSide = true;
        foreach(var info in killInfoList)
        {
            info.destroy();
        }
        killInfoList.Clear();
        InRoomChat.Clear();
        RCManager.racingSpawnPointSet = false;
        if (!PhotonNetwork.IsMasterClient)
        {
            BasePV.RPC("RequireStatus", PhotonTargets.MasterClient, new object[0]);
        }
    }

    public void OnLeftLobby(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnLeftLobby");
    }

    public void OnLeftRoom(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnLeftRoom");
        if (Application.loadedLevel != 0)
        {
            UnityEngine.Time.timeScale = 1f;
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.Disconnect();
            }
            IN_GAME_MAIN_CAMERA.GameType = GameType.Stop;
            this.GameStart = false;
            Screen.lockCursor = false;
            Screen.showCursor = true;
            FengCustomInputs.Main.menuOn = false;
            UnityEngine.Object.Destroy(FengGameManagerMKII.FGM);
            Application.LoadLevel("menu");
        }
    }

    public void OnMasterClientSwitched(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnMasterClientSwitched");
        if (this.gameTimesUp)
        {
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            this.RestartGame(true);
        }
    }

    public void OnPhotonCreateRoomFailed(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnPhotonCreateRoomFailed");
    }

    public void OnPhotonCustomRoomPropertiesChanged(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnPhotonCustomRoomPropertiesChanged");
    }

    public void OnPhotonJoinRoomFailed(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnPhotonJoinRoomFailed");
    }

    public void OnPhotonPlayerConnected(AOTEventArgs args)
    {
        playerList?.Update();
    }

    public void OnPhotonPlayerDisconnected(AOTEventArgs args)
    {
        playerList?.Update();
        if (!this.gameTimesUp)
        {
            this.oneTitanDown(string.Empty, true);
            this.someOneIsDead(0);
        }
    }

    public void OnPhotonPlayerPropertiesChanged(AOTEventArgs args)
    {
        playerList?.Update();
    }

    public void OnPhotonRandomJoinFailed(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnPhotonRandomJoinFailed");
    }

    public void OnPhotonSerializeView(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnPhotonSerializeView");
    }

    public void OnReceivedRoomListUpdate(AOTEventArgs args)
    {
    }

    public void OnUpdatedFriendList(AOTEventArgs args)
    {
        UnityEngine.MonoBehaviour.print("OnUpdatedFriendList");
    }

    public void PlayerKillInfoSingleUpdate(int dmg)
    {
        this.single_kills++;
        this.single_maxDamage = Mathf.Max(dmg, this.single_maxDamage);
        this.single_totalDamage += dmg;
    }

    public void PlayerKillInfoUpdate(PhotonPlayer player, int dmg)
    {
        player.Kills++;
        player.Max_Dmg = Mathf.Max(dmg, player.Max_Dmg);
        player.Total_Dmg += dmg;
    }

    public TITAN RandomSpawnOneTitan(string place, int rate)
    {
        return this.SpawnTitan(rate, RespawnPositions.RandomTitanPos, Quaternion.identity, false);
    }

    public void RandomSpawnTitan(string place, int rate, int num, bool punk = false)
    {
        if (num == -1)
        {
            num = 1;
        }
        List<Vector3> list = new List<Vector3>(RespawnPositions.TitanPositions);
        if (list.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < num; i++)
        {
            if (list.Count <= 0)
            {
                return;
            }
            int pos = Random.Range(0, list.Count);
            this.SpawnTitan(rate, list[pos], Quaternion.identity, punk);
            list.RemoveAt(pos);
        }
    }

    public void RemoveCT(COLOSSAL_TITAN titan)
    {
        titan = null;
    }

    public void RemoveET(TITAN_EREN hero)
    {
        eren = null;
    }

    public void RemoveFT(FEMALE_TITAN titan)
    {
        titan = null;
    }

    public void RemoveHero(HERO hero)
    {
        this.heroes.Remove(hero);
    }

    public void RemoveHook(Bullet h)
    {
        this.hooks.Remove(h);
    }

    public void RemoveTitan(TITAN titan)
    {
        this.titans.Remove(titan);
    }

    public void RestartGame(bool masterclientSwitched = false)
    {
        if (this.gameTimesUp)
        {
            return;
        }
        this.PVPtitanScore = 0;
        this.PVPhumanScore = 0;
        this.startRacing = false;
        this.endRacing = false;
        this.checkpoint = null;
        this.timeElapse = 0f;
        this.RoundTime = 0f;
        this.isWinning = false;
        this.isLosing = false;
        this.Wave = 1;
        this.MyRespawnTime = 0f;
        this.kicklist = new ArrayList();
        foreach (var info in killInfoList)
        {
            info.destroy();
        }
        killInfoList.Clear();
        this.racingResult = new ArrayList();
        this.ShowHUDInfoCenter(string.Empty);
        PhotonNetwork.DestroyAll();
        BasePV.RPC("RPCLoadLevel", PhotonTargets.All, new object[0]);
        if (masterclientSwitched)
        {
            this.SendChatContentInfo("<color=#A8FF24>MasterClient has switched to </color>" + PhotonNetwork.player.Properties[PhotonPlayerProperty.name]);
        }
    }

    public void RestartGameSingle()
    {
        this.startRacing = false;
        this.endRacing = false;
        this.checkpoint = null;
        this.single_kills = 0;
        this.single_maxDamage = 0;
        this.single_totalDamage = 0;
        this.timeElapse = 0f;
        this.RoundTime = 0f;
        this.timeTotalServer = 0f;
        this.isWinning = false;
        this.isLosing = false;
        this.Wave = 1;
        this.MyRespawnTime = 0f;
        this.ShowHUDInfoCenter(string.Empty);
        Application.LoadLevel(Application.loadedLevel);
    }

    public void SendChatContentInfo(string content)
    {
        BasePV.RPC("Chat", PhotonTargets.All, new object[]
        {
            content,
            string.Empty
        });
    }

    public void SendKillInfo(bool t1, string killer, bool t2, string victim, int dmg = 0)
    {
        BasePV.RPC("updateKillInfo", PhotonTargets.All, new object[]
        {
            t1,
            killer,
            t2,
            victim,
            dmg
        });
    }

    [RPC]
    public void someOneIsDead(int id = -1)
    {
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
        {
            if (id != 0)
            {
                this.PVPtitanScore += 2;
            }
            this.CheckPVPpts();
            BasePV.RPC("refreshPVPStatus", PhotonTargets.Others, new object[]
            {
                this.PVPhumanScore,
                this.PVPtitanScore
            });
        }
        else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.ENDLESS_TITAN)
        {
            this.titanScore++;
        }
        else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.KILL_TITAN || IN_GAME_MAIN_CAMERA.GameMode == GameMode.SURVIVE_MODE || IN_GAME_MAIN_CAMERA.GameMode == GameMode.BOSS_FIGHT_CT || IN_GAME_MAIN_CAMERA.GameMode == GameMode.TROST)
        {
            if (this.IsPlayerAllDead())
            {
                this.GameLose();
            }
        }
        else if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_AHSS)
        {
            if (this.IsPlayerAllDead())
            {
                this.GameLose();
                this.teamWinner = 0;
            }
            if (this.IsTeamAllDead(1))
            {
                this.teamWinner = 2;
                this.GameWin();
            }
            if (this.IsTeamAllDead(2))
            {
                this.teamWinner = 1;
                this.GameWin();
            }
        }
    }

    public void SpawnNonAITitan(string id, string tag = "titanRespawn")
    {
        GameObject[] array = GameObject.FindGameObjectsWithTag(tag);
        GameObject gameObject = array[UnityEngine.Random.Range(0, array.Length)];
        this.myLastHero = id.ToUpper();
        GameObject gameObject2;
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
        {
            gameObject2 = Optimization.Caching.Pool.NetworkEnable("TITAN_VER3.1", this.checkpoint.transform.position + new Vector3((float)UnityEngine.Random.Range(-20, 20), 2f, (float)UnityEngine.Random.Range(-20, 20)), this.checkpoint.transform.rotation, 0);
        }
        else
        {
            gameObject2 = Optimization.Caching.Pool.NetworkEnable("TITAN_VER3.1", gameObject.transform.position, gameObject.transform.rotation, 0);
        }
        IN_GAME_MAIN_CAMERA.MainCamera.SetMainObject(gameObject2.GetComponent<TITAN>());
        gameObject2.GetComponent<TITAN>().nonAI = true;
        gameObject2.GetComponent<TITAN>().speed = 30f;
        gameObject2.GetComponent<TITAN_CONTROLLER>().enabled = true;
        if (id == "RANDOM" && UnityEngine.Random.Range(0, 100) < 7)
        {
            gameObject2.GetComponent<TITAN>().setAbnormalType(AbnormalType.Crawler, true);
        }
        IN_GAME_MAIN_CAMERA.MainCamera.enabled = true;
        IN_GAME_MAIN_CAMERA.SpecMov.disable = true;
        IN_GAME_MAIN_CAMERA.Look.disable = true;
        IN_GAME_MAIN_CAMERA.MainCamera.gameOver = false;
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                "dead",
                false
            }
        };
        PhotonNetwork.player.SetCustomProperties(customProperties);
        customProperties = new ExitGames.Client.Photon.Hashtable
        {
            {
                PhotonPlayerProperty.isTitan,
                2
            }
        };
        PhotonNetwork.player.SetCustomProperties(customProperties);
        if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
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
        this.SpawnPlayerAt(id);
    }

    public void SpawnPlayerAt(string id)
    {
        Vector3 pos;
        Quaternion rot = Quaternion.identity;
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
        {
            pos = this.checkpoint.transform.position;
        }
        else if (RCManager.racingSpawnPointSet)
        {
            pos = RCManager.racingSpawnPoint;
            rot = RCManager.racingSpawnPointRotation;
        }
        else if (Level.Name.StartsWith("Custom"))
        {
            List<Vector3> list = new List<Vector3>();
            switch (PhotonNetwork.player.RCteam)
            {
                case 0:
                    for (int i = 0; i < 2; i++)
                    {
                        string type = (i == 0) ? "C" : "M";
                        foreach (Vector3 vec in CustomLevel.spawnPositions["Player" + type])
                        {
                            list.Add(vec);
                        }
                    }
                    break;
                case 1:
                    using (List<Vector3>.Enumerator enumerator = CustomLevel.spawnPositions["PlayerC"].GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Vector3 vec2 = enumerator.Current;
                            list.Add(vec2);
                        }
                    }
                    break;
                case 2:
                    using (List<Vector3>.Enumerator enumerator = CustomLevel.spawnPositions["PlayerM"].GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Vector3 vec2 = enumerator.Current;
                            list.Add(vec2);
                        }
                    }
                    break;
                default:
                    foreach (Vector3 vec3 in CustomLevel.spawnPositions["PlayerM"])
                    {
                        list.Add(vec3);
                    }
                    break;
            }
            if (list.Count > 0)
            {
                pos = list[UnityEngine.Random.Range(0, list.Count)];
            }
            else
            {
                pos = RespawnPositions.RandomHeroPos;
            }
        }
        else
        {
            pos = RespawnPositions.RandomHeroPos;
        }
        IN_GAME_MAIN_CAMERA component = IN_GAME_MAIN_CAMERA.MainCamera;
        this.myLastHero = id.ToUpper();
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            if (IN_GAME_MAIN_CAMERA.singleCharacter == "TITAN_EREN")
            {
                component.SetMainObject(((GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("TITAN_EREN"), pos, Quaternion.identity)).GetComponent<TITAN_EREN>(), true, false);
            }
            else
            {
                component.SetMainObject(((GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("AOTTG_HERO 1"), pos, rot)).GetComponent<HERO>(), true, false);
                if (IN_GAME_MAIN_CAMERA.singleCharacter == "SET 1" || IN_GAME_MAIN_CAMERA.singleCharacter == "SET 2" || IN_GAME_MAIN_CAMERA.singleCharacter == "SET 3")
                {
                    HeroCostume heroCostume = CostumeConeveter.LocalDataToHeroCostume(IN_GAME_MAIN_CAMERA.singleCharacter);
                    heroCostume.Checkstat();
                    CostumeConeveter.HeroCostumeToLocalData(heroCostume, IN_GAME_MAIN_CAMERA.singleCharacter);
                    IN_GAME_MAIN_CAMERA.MainHERO.setup.Init();
                    if (heroCostume != null)
                    {
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume = heroCostume;
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume.stat = heroCostume.stat;
                    }
                    else
                    {
                        heroCostume = HeroCostume.costumeOption[3];
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume = heroCostume;
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume.stat = HeroStat.getInfo(heroCostume.name.ToUpper());
                    }
                    IN_GAME_MAIN_CAMERA.MainHERO.setup.SetCharacterComponent();
                    IN_GAME_MAIN_CAMERA.MainHERO.setStat();
                    IN_GAME_MAIN_CAMERA.MainHERO.setSkillHUDPosition();
                }
                else
                {
                    for (int i = 0; i < HeroCostume.costume.Length; i++)
                    {
                        if (HeroCostume.costume[i].name.ToUpper() == IN_GAME_MAIN_CAMERA.singleCharacter.ToUpper())
                        {
                            int num = HeroCostume.costume[i].id + CheckBoxCostume.costumeSet - 1;
                            if (HeroCostume.costume[num].name != HeroCostume.costume[i].name)
                            {
                                num = HeroCostume.costume[i].id + 1;
                            }
                            IN_GAME_MAIN_CAMERA.MainHERO.setup.Init();
                            IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume = HeroCostume.costume[num];
                            IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume.stat = HeroStat.getInfo(HeroCostume.costume[num].name.ToUpper());
                            IN_GAME_MAIN_CAMERA.MainHERO.setup.SetCharacterComponent();
                            IN_GAME_MAIN_CAMERA.MainHERO.setStat();
                            IN_GAME_MAIN_CAMERA.MainHERO.setSkillHUDPosition();
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            component.SetMainObject(Optimization.Caching.Pool.NetworkEnable("AOTTG_HERO 1", pos, Quaternion.identity, 0).GetComponent<HERO>(), true, false);
            id = id.ToUpper();
            if (id == "SET 1" || id == "SET 2" || id == "SET 3")
            {
                HeroCostume heroCostume2 = CostumeConeveter.LocalDataToHeroCostume(id);
                heroCostume2.Checkstat();
                CostumeConeveter.HeroCostumeToLocalData(heroCostume2, id);
                IN_GAME_MAIN_CAMERA.MainHERO.setup.Init();
                if (heroCostume2 != null)
                {
                    IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume = heroCostume2;
                    IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume.stat = heroCostume2.stat;
                }
                else
                {
                    heroCostume2 = HeroCostume.costumeOption[3];
                    IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume = heroCostume2;
                    IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume.stat = HeroStat.getInfo(heroCostume2.name.ToUpper());
                }
                IN_GAME_MAIN_CAMERA.MainHERO.setup.SetCharacterComponent();
                IN_GAME_MAIN_CAMERA.MainHERO.setStat();
                IN_GAME_MAIN_CAMERA.MainHERO.setSkillHUDPosition();
            }
            else
            {
                for (int j = 0; j < HeroCostume.costume.Length; j++)
                {
                    if (HeroCostume.costume[j].name.ToUpper() == id.ToUpper())
                    {
                        int num2 = HeroCostume.costume[j].id;
                        if (id.ToUpper() != "AHSS")
                        {
                            num2 += CheckBoxCostume.costumeSet - 1;
                        }
                        if (HeroCostume.costume[num2].name != HeroCostume.costume[j].name)
                        {
                            num2 = HeroCostume.costume[j].id + 1;
                        }
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.Init();
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume = HeroCostume.costume[num2];
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume.stat = HeroStat.getInfo(HeroCostume.costume[num2].name.ToUpper());
                        IN_GAME_MAIN_CAMERA.MainHERO.setup.SetCharacterComponent();
                        IN_GAME_MAIN_CAMERA.MainHERO.setStat();
                        IN_GAME_MAIN_CAMERA.MainHERO.setSkillHUDPosition();
                        break;
                    }
                }
            }
            CostumeConeveter.HeroCostumeToPhotonData(IN_GAME_MAIN_CAMERA.MainHERO.setup.myCostume, PhotonNetwork.player);
            if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
            {
                IN_GAME_MAIN_CAMERA.MainT.position += new Vector3((float)UnityEngine.Random.Range(-20, 20), 2f, (float)UnityEngine.Random.Range(-20, 20));
            }
            PhotonNetwork.player.Dead = false;
            PhotonNetwork.player.IsTitan = false;
        }
        component.enabled = true;
        IN_GAME_MAIN_CAMERA.MainCamera.setHUDposition();
        IN_GAME_MAIN_CAMERA.SpecMov.disable = true;
        IN_GAME_MAIN_CAMERA.Look.disable = true;
        component.gameOver = false;
        if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
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

    public TITAN SpawnTitan(int rate, Vector3 position, Quaternion rotation, bool punk = false)
    {
        TITAN tit = this.spawnTitanRaw(position, rotation);
        if (punk)
        {
            tit.setAbnormalType(AbnormalType.Punk, false);
        }
        else if (UnityEngine.Random.Range(0, 100) < rate)
        {
            if (IN_GAME_MAIN_CAMERA.difficulty == 2)
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.7f || FengGameManagerMKII.Level.NoCrawler)
                {
                    tit.setAbnormalType(AbnormalType.Jumper, false);
                }
                else
                {
                    tit.setAbnormalType(AbnormalType.Crawler, false);
                }
            }
        }
        else if (IN_GAME_MAIN_CAMERA.difficulty == 2)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.7f || FengGameManagerMKII.Level.NoCrawler)
            {
                tit.setAbnormalType(AbnormalType.Jumper, false);
            }
            else
            {
                tit.setAbnormalType(AbnormalType.Crawler, false);
            }
        }
        else if (UnityEngine.Random.Range(0, 100) < rate)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.8f || FengGameManagerMKII.Level.NoCrawler)
            {
                tit.setAbnormalType(AbnormalType.Aberrant, false);
            }
            else
            {
                tit.setAbnormalType(AbnormalType.Crawler, false);
            }
        }
        else if (UnityEngine.Random.Range(0f, 1f) < 0.8f || FengGameManagerMKII.Level.NoCrawler)
        {
            tit.setAbnormalType(AbnormalType.Jumper, false);
        }
        else
        {
            tit.setAbnormalType(AbnormalType.Crawler, false);
        }
        GameObject gameObject2;
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            gameObject2 = Pool.Enable("FX/FXtitanSpawn", tit.transform.position, Quaternion.Euler(-90f, 0f, 0f));//(GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("FX/FXtitanSpawn"), tit.transform.position, Quaternion.Euler(-90f, 0f, 0f));
        }
        else
        {
            gameObject2 = Optimization.Caching.Pool.NetworkEnable("FX/FXtitanSpawn", tit.transform.position, Quaternion.Euler(-90f, 0f, 0f), 0);
        }
        gameObject2.transform.localScale = tit.transform.localScale;
        return tit;
    }

    public void spawnTitanAction(int type, float size, int health, int number)
    {
        Vector3 position = new Vector3(UnityEngine.Random.Range(-400f, 400f), 0f, UnityEngine.Random.Range(-400f, 400f));
        Quaternion rotation = new Quaternion(0f, 0f, 0f, 1f);
        if (CustomLevel.spawnPositions["Titan"].Count > 0)
        {
            position = CustomLevel.spawnPositions["Titan"][UnityEngine.Random.Range(0, CustomLevel.spawnPositions["Titan"].Count)];
        }
        else
        {
            GameObject[] objArray = GameObject.FindGameObjectsWithTag("titanRespawn");
            if (objArray.Length != 0)
            {
                int index = UnityEngine.Random.Range(0, objArray.Length);
                GameObject obj2 = objArray[index];
                while (objArray[index] == null)
                {
                    index = UnityEngine.Random.Range(0, objArray.Length);
                    obj2 = objArray[index];
                }
                objArray[index] = null;
                position = obj2.transform.position;
                rotation = obj2.transform.rotation;
            }
        }
        for (int i = 0; i < number; i++)
        {
            TITAN titan = this.spawnTitanRaw(position, rotation);
            titan.hasSetLevel = true;
            titan.resetLevel(size);
            if ((float)health > 0f)
            {
                titan.armor = health;
            }
            switch (type)
            {
                case 0:
                    titan.setAbnormalType(AbnormalType.Normal);
                    break;
                case 1:
                    titan.setAbnormalType(AbnormalType.Aberrant);
                    break;
                case 2:
                    titan.setAbnormalType(AbnormalType.Jumper);
                    break;
                case 3:
                    titan.setAbnormalType(AbnormalType.Crawler);
                    break;
                case 4:
                    titan.setAbnormalType(AbnormalType.Punk);
                    break;
            }
        }
    }

    public void spawnTitanAtAction(int type, float size, int health, int number, float posX, float posY, float posZ)
    {
        Vector3 position = new Vector3(posX, posY, posZ);
        Quaternion rotation = new Quaternion(0f, 0f, 0f, 1f);
        for (int i = 0; i < number; i++)
        {
            TITAN obj2 = this.spawnTitanRaw(position, rotation);
            obj2.resetLevel(size);
            obj2.hasSetLevel = true;
            if ((float)health > 0f)
            {
                obj2.armor = health;
            }
            switch (type)
            {
                case 0:
                    obj2.setAbnormalType(AbnormalType.Normal);
                    break;
                case 1:
                    obj2.setAbnormalType(AbnormalType.Normal);
                    break;
                case 2:
                    obj2.setAbnormalType(AbnormalType.Normal);
                    break;
                case 3:
                    obj2.setAbnormalType(AbnormalType.Normal);
                    break;
                case 4:
                    obj2.setAbnormalType(AbnormalType.Normal);
                    break;
            }
        }
    }

    public void TitanGetKill(PhotonPlayer player, int Damage, string name)
    {
        Damage = Mathf.Max(10, Damage);
        BasePV.RPC("netShowDamage", player, new object[]
        {
            Damage
        });
        BasePV.RPC("oneTitanDown", PhotonTargets.MasterClient, new object[]
        {
            name,
            false
        });
        this.SendKillInfo(false, player.UIName, true, name, Damage);
        this.PlayerKillInfoUpdate(player, Damage);
    }

    public void TitanGetKillbyServer(int Damage, string name)
    {
        Damage = Mathf.Max(10, Damage);
        this.SendKillInfo(false, LoginFengKAI.player.name, true, name, Damage);
        this.netShowDamage(Damage);
        this.oneTitanDown(name, false);
        this.PlayerKillInfoUpdate(PhotonNetwork.player, Damage);
    }
}