using ExitGames.Client.Photon;
using Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class TITAN : Photon.MonoBehaviour
{
    [CompilerGenerated]
    private static Dictionary<string, int> f__switchmap5;
    [CompilerGenerated]
    private static Dictionary<string, int> f__switchmap6;
    [CompilerGenerated]
    private static Dictionary<string, int> f__switchmap7;
    private Vector3 abnorma_jump_bite_horizon_v;
    public AbnormalType abnormalType;
    public int activeRad = 0x7fffffff;
    private float angle;
    public bool asClientLookTarget;
    private string attackAnimation;
    private float attackCheckTime;
    private float attackCheckTimeA;
    private float attackCheckTimeB;
    private int attackCount;
    public float attackDistance = 13f;
    private bool attacked;
    private float attackEndWait;
    public float attackWait = 1f;
    private float between2;
    public float chaseDistance = 80f;
    public ArrayList checkPoints = new ArrayList();
    public TITAN_CONTROLLER controller;
    public GameObject currentCamera;
    private Transform currentGrabHand;
    private float desDeg;
    private float dieTime;
    private string fxName;
    private Vector3 fxPosition;
    private Quaternion fxRotation;
    private float getdownTime;
    private GameObject grabbedTarget;
    public GameObject grabTF;
    private float gravity = 120f;
    private bool grounded;
    public bool hasDie;
    private bool hasDieSteam;
    private Transform head;
    private Vector3 headscale = Vector3.one;
    private string hitAnimation;
    private float hitPause;
    public bool isAlarm;
    private bool isAttackMoveByCore;
    private bool isGrabHandLeft;
    private bool leftHandAttack;
    public GameObject mainMaterial;
    private float maxStamina = 320f;
    public float maxVelocityChange = 10f;
    public static float minusDistance = 99999f;
    public static GameObject minusDistanceEnemy;
    public int myDifficulty;
    public float myDistance;
    public GROUP myGroup = GROUP.T;
    public GameObject myHero;
    public float myLevel = 1f;
    private Transform neck;
    private bool needFreshCorePosition;
    private string nextAttackAnimation;
    public bool nonAI;
    private bool nonAIcombo;
    private Vector3 oldCorePosition;
    private Quaternion oldHeadRotation;
    public PVPcheckPoint PVPfromCheckPt;
    private float random_run_time;
    private float rockInterval;
    private string runAnimation;
    private float sbtime;
    private Vector3 spawnPt;
    public float speed = 7f;
    private float stamina = 320f;
    private TitanState state;
    private int stepSoundPhase = 2;
    private bool stuck;
    private float stuckTime;
    private float stuckTurnAngle;
    private Vector3 targetCheckPt;
    private Quaternion targetHeadRotation;
    private float targetR;
    private float tauntTime;
    private GameObject throwRock;
    private string turnAnimation;
    private float turnDeg;
    private GameObject whoHasTauntMe;

    private void attack(string type)
    {
        this.state = TitanState.attack;
        this.attacked = false;
        this.isAlarm = true;
        if (this.attackAnimation == type)
        {
            this.attackAnimation = type;
            this.playAnimationAt("attack_" + type, 0f);
        }
        else
        {
            this.attackAnimation = type;
            this.playAnimationAt("attack_" + type, 0f);
        }
        this.nextAttackAnimation = null;
        this.fxName = null;
        this.isAttackMoveByCore = false;
        this.attackCheckTime = 0f;
        this.attackCheckTimeA = 0f;
        this.attackCheckTimeB = 0f;
        this.attackEndWait = 0f;
        this.fxRotation = Quaternion.Euler(270f, 0f, 0f);
        string key = type;
        if (key != null)
        {
            int num;
            if (f__switchmap6 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(0x16);
                dictionary.Add("abnormal_getup", 0);
                dictionary.Add("abnormal_jump", 1);
                dictionary.Add("combo_1", 2);
                dictionary.Add("combo_2", 3);
                dictionary.Add("combo_3", 4);
                dictionary.Add("front_ground", 5);
                dictionary.Add("kick", 6);
                dictionary.Add("slap_back", 7);
                dictionary.Add("slap_face", 8);
                dictionary.Add("stomp", 9);
                dictionary.Add("bite", 10);
                dictionary.Add("bite_l", 11);
                dictionary.Add("bite_r", 12);
                dictionary.Add("jumper_0", 13);
                dictionary.Add("crawler_jump_0", 14);
                dictionary.Add("anti_AE_l", 15);
                dictionary.Add("anti_AE_r", 0x10);
                dictionary.Add("anti_AE_low_l", 0x11);
                dictionary.Add("anti_AE_low_r", 0x12);
                dictionary.Add("quick_turn_l", 0x13);
                dictionary.Add("quick_turn_r", 20);
                dictionary.Add("throw", 0x15);
                f__switchmap6 = dictionary;
            }
            if (f__switchmap6.TryGetValue(key, out num))
            {
                switch (num)
                {
                    case 0:
                        this.attackCheckTime = 0f;
                        this.fxName = string.Empty;
                        break;

                    case 1:
                        this.nextAttackAnimation = "abnormal_getup";
                        if (!this.nonAI)
                        {
                            this.attackEndWait = (this.myDifficulty <= 0) ? UnityEngine.Random.Range((float) 1f, (float) 4f) : UnityEngine.Random.Range((float) 0f, (float) 1f);
                        }
                        else
                        {
                            this.attackEndWait = 0f;
                        }
                        this.attackCheckTime = 0.75f;
                        this.fxName = "boom4";
                        this.fxRotation = Quaternion.Euler(270f, base.transform.rotation.eulerAngles.y, 0f);
                        break;

                    case 2:
                        this.nextAttackAnimation = "combo_2";
                        this.attackCheckTimeA = 0.54f;
                        this.attackCheckTimeB = 0.76f;
                        this.nonAIcombo = false;
                        this.isAttackMoveByCore = true;
                        this.leftHandAttack = false;
                        break;

                    case 3:
                        if (this.abnormalType != AbnormalType.TYPE_PUNK)
                        {
                            this.nextAttackAnimation = "combo_3";
                        }
                        this.attackCheckTimeA = 0.37f;
                        this.attackCheckTimeB = 0.57f;
                        this.nonAIcombo = false;
                        this.isAttackMoveByCore = true;
                        this.leftHandAttack = true;
                        break;

                    case 4:
                        this.nonAIcombo = false;
                        this.isAttackMoveByCore = true;
                        this.attackCheckTime = 0.21f;
                        this.fxName = "boom1";
                        break;

                    case 5:
                        this.fxName = "boom1";
                        this.attackCheckTime = 0.45f;
                        break;

                    case 6:
                        this.fxName = "boom5";
                        this.fxRotation = base.transform.rotation;
                        this.attackCheckTime = 0.43f;
                        break;

                    case 7:
                        this.fxName = "boom3";
                        this.attackCheckTime = 0.66f;
                        break;

                    case 8:
                        this.fxName = "boom3";
                        this.attackCheckTime = 0.655f;
                        break;

                    case 9:
                        this.fxName = "boom2";
                        this.attackCheckTime = 0.42f;
                        break;

                    case 10:
                        this.fxName = "bite";
                        this.attackCheckTime = 0.6f;
                        break;

                    case 11:
                        this.fxName = "bite";
                        this.attackCheckTime = 0.4f;
                        break;

                    case 12:
                        this.fxName = "bite";
                        this.attackCheckTime = 0.4f;
                        break;

                    case 13:
                        this.abnorma_jump_bite_horizon_v = Vector3.zero;
                        break;

                    case 14:
                        this.abnorma_jump_bite_horizon_v = Vector3.zero;
                        break;

                    case 15:
                        this.attackCheckTimeA = 0.31f;
                        this.attackCheckTimeB = 0.4f;
                        this.leftHandAttack = true;
                        break;

                    case 0x10:
                        this.attackCheckTimeA = 0.31f;
                        this.attackCheckTimeB = 0.4f;
                        this.leftHandAttack = false;
                        break;

                    case 0x11:
                        this.attackCheckTimeA = 0.31f;
                        this.attackCheckTimeB = 0.4f;
                        this.leftHandAttack = true;
                        break;

                    case 0x12:
                        this.attackCheckTimeA = 0.31f;
                        this.attackCheckTimeB = 0.4f;
                        this.leftHandAttack = false;
                        break;

                    case 0x13:
                        this.attackCheckTimeA = 2f;
                        this.attackCheckTimeB = 2f;
                        this.isAttackMoveByCore = true;
                        break;

                    case 20:
                        this.attackCheckTimeA = 2f;
                        this.attackCheckTimeB = 2f;
                        this.isAttackMoveByCore = true;
                        break;

                    case 0x15:
                        this.isAlarm = true;
                        this.chaseDistance = 99999f;
                        break;
                }
            }
        }
        this.needFreshCorePosition = true;
    }

    private void Awake()
    {
        base.rigidbody.freezeRotation = true;
        base.rigidbody.useGravity = false;
    }

    public void beLaughAttacked()
    {
        if (!this.hasDie && (this.abnormalType != AbnormalType.TYPE_CRAWLER))
        {
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
            {
                object[] parameters = new object[] { 0f };
                base.photonView.RPC("laugh", PhotonTargets.All, parameters);
            }
            else if (((this.state == TitanState.idle) || (this.state == TitanState.turn)) || (this.state == TitanState.chase))
            {
                this.laugh(0f);
            }
        }
    }

    public void beTauntedBy(GameObject target, float tauntTime)
    {
        this.whoHasTauntMe = target;
        this.tauntTime = tauntTime;
        this.isAlarm = true;
    }

    private void chase()
    {
        this.state = TitanState.chase;
        this.isAlarm = true;
        this.crossFade(this.runAnimation, 0.5f);
    }

    private GameObject checkIfHitCrawlerMouth(Transform head, float rad)
    {
        float num = rad * this.myLevel;
        foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("Player"))
        {
            if ((obj2.GetComponent<TITAN_EREN>() == null) && ((obj2.GetComponent<HERO>() == null) || !obj2.GetComponent<HERO>().isInvincible()))
            {
                float num3 = obj2.GetComponent<CapsuleCollider>().height * 0.5f;
                if (Vector3.Distance(obj2.transform.position + ((Vector3) (Vector3.up * num3)), head.position - ((Vector3) ((Vector3.up * 1.5f) * this.myLevel))) < (num + num3))
                {
                    return obj2;
                }
            }
        }
        return null;
    }

    private GameObject checkIfHitHand(Transform hand)
    {
        float num = 2.4f * this.myLevel;
        foreach (Collider collider in Physics.OverlapSphere(hand.GetComponent<SphereCollider>().transform.position, num + 1f))
        {
            if (collider.transform.root.tag == "Player")
            {
                GameObject gameObject = collider.transform.root.gameObject;
                if (gameObject.GetComponent<TITAN_EREN>() != null)
                {
                    if (!gameObject.GetComponent<TITAN_EREN>().isHit)
                    {
                        gameObject.GetComponent<TITAN_EREN>().hitByTitan();
                    }
                }
                else if ((gameObject.GetComponent<HERO>() != null) && !gameObject.GetComponent<HERO>().isInvincible())
                {
                    return gameObject;
                }
            }
        }
        return null;
    }

    private GameObject checkIfHitHead(Transform head, float rad)
    {
        float num = rad * this.myLevel;
        foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("Player"))
        {
            if ((obj2.GetComponent<TITAN_EREN>() == null) && ((obj2.GetComponent<HERO>() == null) || !obj2.GetComponent<HERO>().isInvincible()))
            {
                float num3 = obj2.GetComponent<CapsuleCollider>().height * 0.5f;
                if (Vector3.Distance(obj2.transform.position + ((Vector3) (Vector3.up * num3)), head.position + ((Vector3) ((Vector3.up * 1.5f) * this.myLevel))) < (num + num3))
                {
                    return obj2;
                }
            }
        }
        return null;
    }

    private void crossFade(string aniName, float time)
    {
        base.animation.CrossFade(aniName, time);
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
        {
            object[] parameters = new object[] { aniName, time };
            base.photonView.RPC("netCrossFade", PhotonTargets.Others, parameters);
        }
    }

    public bool die()
    {
        if (this.hasDie)
        {
            return false;
        }
        this.hasDie = true;
        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().oneTitanDown(string.Empty, false);
        this.dieAnimation();
        return true;
    }

    private void dieAnimation()
    {
        if (base.animation.IsPlaying("sit_idle") || base.animation.IsPlaying("sit_hit_eye"))
        {
            this.crossFade("sit_die", 0.1f);
        }
        else if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
        {
            this.crossFade("crawler_die", 0.2f);
        }
        else if (this.abnormalType == AbnormalType.NORMAL)
        {
            this.crossFade("die_front", 0.05f);
        }
        else if (((base.animation.IsPlaying("attack_abnormal_jump") && (base.animation["attack_abnormal_jump"].normalizedTime > 0.7f)) || (base.animation.IsPlaying("attack_abnormal_getup") && (base.animation["attack_abnormal_getup"].normalizedTime < 0.7f))) || base.animation.IsPlaying("tired"))
        {
            this.crossFade("die_ground", 0.2f);
        }
        else
        {
            this.crossFade("die_back", 0.05f);
        }
    }

    public void dieBlow(Vector3 attacker, float hitPauseTime)
    {
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            this.dieBlowFunc(attacker, hitPauseTime);
            if (GameObject.FindGameObjectsWithTag("titan").Length <= 1)
            {
                GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
            }
        }
        else
        {
            object[] parameters = new object[] { attacker, hitPauseTime };
            base.photonView.RPC("dieBlowRPC", PhotonTargets.All, parameters);
        }
    }

    public void dieBlowFunc(Vector3 attacker, float hitPauseTime)
    {
        if (!this.hasDie)
        {
            base.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(attacker - base.transform.position).eulerAngles.y, 0f);
            this.hasDie = true;
            this.hitAnimation = "die_blow";
            this.hitPause = hitPauseTime;
            this.playAnimation(this.hitAnimation);
            base.animation[this.hitAnimation].time = 0f;
            base.animation[this.hitAnimation].speed = 0f;
            this.needFreshCorePosition = true;
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().oneTitanDown(string.Empty, false);
            if (base.photonView.isMine)
            {
                if (this.grabbedTarget != null)
                {
                    this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
                }
                if (this.nonAI)
                {
                    this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().setMainObject(null, true, false);
                    this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().setSpectorMode(true);
                    this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
                    ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                    propertiesToSet.Add(PhotonPlayerProperty.dead, true);
                    PhotonNetwork.player.SetCustomProperties(propertiesToSet);
                    propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                    propertiesToSet.Add(PhotonPlayerProperty.deaths, ((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.deaths]) + 1);
                    PhotonNetwork.player.SetCustomProperties(propertiesToSet);
                }
            }
        }
    }

    [RPC]
    private void dieBlowRPC(Vector3 attacker, float hitPauseTime)
    {
        if (base.photonView.isMine)
        {
            Vector3 vector = attacker - base.transform.position;
            if (vector.magnitude < 80f)
            {
                this.dieBlowFunc(attacker, hitPauseTime);
            }
        }
    }

    public void dieHeadBlow(Vector3 attacker, float hitPauseTime)
    {
        if (this.abnormalType != AbnormalType.TYPE_CRAWLER)
        {
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                this.dieHeadBlowFunc(attacker, hitPauseTime);
                if (GameObject.FindGameObjectsWithTag("titan").Length <= 1)
                {
                    GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
                }
            }
            else
            {
                object[] parameters = new object[] { attacker, hitPauseTime };
                base.photonView.RPC("dieHeadBlowRPC", PhotonTargets.All, parameters);
            }
        }
    }

    public void dieHeadBlowFunc(Vector3 attacker, float hitPauseTime)
    {
        if (!this.hasDie)
        {
            GameObject obj2;
            this.playSound("snd_titan_head_blow");
            base.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(attacker - base.transform.position).eulerAngles.y, 0f);
            this.hasDie = true;
            this.hitAnimation = "die_headOff";
            this.hitPause = hitPauseTime;
            this.playAnimation(this.hitAnimation);
            base.animation[this.hitAnimation].time = 0f;
            base.animation[this.hitAnimation].speed = 0f;
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().oneTitanDown(string.Empty, false);
            this.needFreshCorePosition = true;
            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
            {
                obj2 = PhotonNetwork.Instantiate("bloodExplore", this.head.position + ((Vector3) ((Vector3.up * 1f) * this.myLevel)), Quaternion.Euler(270f, 0f, 0f), 0);
            }
            else
            {
                obj2 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("bloodExplore"), this.head.position + ((Vector3) ((Vector3.up * 1f) * this.myLevel)), Quaternion.Euler(270f, 0f, 0f));
            }
            obj2.transform.localScale = base.transform.localScale;
            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
            {
                obj2 = PhotonNetwork.Instantiate("bloodsplatter", this.head.position, Quaternion.Euler(270f + this.neck.rotation.eulerAngles.x, this.neck.rotation.eulerAngles.y, this.neck.rotation.eulerAngles.z), 0);
            }
            else
            {
                obj2 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("bloodsplatter"), this.head.position, Quaternion.Euler(270f + this.neck.rotation.eulerAngles.x, this.neck.rotation.eulerAngles.y, this.neck.rotation.eulerAngles.z));
            }
            obj2.transform.localScale = base.transform.localScale;
            obj2.transform.parent = this.neck;
            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
            {
                obj2 = PhotonNetwork.Instantiate("FX/justSmoke", this.neck.position, Quaternion.Euler(270f, 0f, 0f), 0);
            }
            else
            {
                obj2 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/justSmoke"), this.neck.position, Quaternion.Euler(270f, 0f, 0f));
            }
            obj2.transform.parent = this.neck;
            if (base.photonView.isMine)
            {
                if (this.grabbedTarget != null)
                {
                    this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
                }
                if (this.nonAI)
                {
                    this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().setMainObject(null, true, false);
                    this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().setSpectorMode(true);
                    this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
                    ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                    propertiesToSet.Add(PhotonPlayerProperty.dead, true);
                    PhotonNetwork.player.SetCustomProperties(propertiesToSet);
                    propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                    propertiesToSet.Add(PhotonPlayerProperty.deaths, ((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.deaths]) + 1);
                    PhotonNetwork.player.SetCustomProperties(propertiesToSet);
                }
            }
        }
    }

    [RPC]
    private void dieHeadBlowRPC(Vector3 attacker, float hitPauseTime)
    {
        if (base.photonView.isMine)
        {
            Vector3 vector = attacker - base.transform.position;
            if (vector.magnitude < 80f)
            {
                this.dieHeadBlowFunc(attacker, hitPauseTime);
            }
        }
    }

    private void eat()
    {
        this.state = TitanState.eat;
        this.attacked = false;
        if (this.isGrabHandLeft)
        {
            this.attackAnimation = "eat_l";
            this.crossFade("eat_l", 0.1f);
        }
        else
        {
            this.attackAnimation = "eat_r";
            this.crossFade("eat_r", 0.1f);
        }
    }

    private void eatSet(GameObject grabTarget)
    {
        if (((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.MULTIPLAYER) || !base.photonView.isMine)) || !grabTarget.GetComponent<HERO>().isGrabbed)
        {
            this.grabToRight();
            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
            {
                base.photonView.RPC("grabToRight", PhotonTargets.Others, new object[0]);
                object[] parameters = new object[] { "grabbed" };
                grabTarget.GetPhotonView().RPC("netPlayAnimation", PhotonTargets.All, parameters);
                object[] objArray2 = new object[] { base.photonView.viewID, false };
                grabTarget.GetPhotonView().RPC("netGrabbed", PhotonTargets.All, objArray2);
            }
            else
            {
                grabTarget.GetComponent<HERO>().grabbed(base.gameObject, false);
                grabTarget.GetComponent<HERO>().animation.Play("grabbed");
            }
        }
    }

    private void eatSetL(GameObject grabTarget)
    {
        if (((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.MULTIPLAYER) || !base.photonView.isMine)) || !grabTarget.GetComponent<HERO>().isGrabbed)
        {
            this.grabToLeft();
            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
            {
                base.photonView.RPC("grabToLeft", PhotonTargets.Others, new object[0]);
                object[] parameters = new object[] { "grabbed" };
                grabTarget.GetPhotonView().RPC("netPlayAnimation", PhotonTargets.All, parameters);
                object[] objArray2 = new object[] { base.photonView.viewID, true };
                grabTarget.GetPhotonView().RPC("netGrabbed", PhotonTargets.All, objArray2);
            }
            else
            {
                grabTarget.GetComponent<HERO>().grabbed(base.gameObject, true);
                grabTarget.GetComponent<HERO>().animation.Play("grabbed");
            }
        }
    }

    private bool executeAttack(string decidedAction)
    {
        string key = decidedAction;
        if (key != null)
        {
            int num;
            if (f__switchmap5 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(0x12);
                dictionary.Add("grab_ground_front_l", 0);
                dictionary.Add("grab_ground_front_r", 1);
                dictionary.Add("grab_ground_back_l", 2);
                dictionary.Add("grab_ground_back_r", 3);
                dictionary.Add("grab_head_front_l", 4);
                dictionary.Add("grab_head_front_r", 5);
                dictionary.Add("grab_head_back_l", 6);
                dictionary.Add("grab_head_back_r", 7);
                dictionary.Add("attack_abnormal_jump", 8);
                dictionary.Add("attack_combo", 9);
                dictionary.Add("attack_front_ground", 10);
                dictionary.Add("attack_kick", 11);
                dictionary.Add("attack_slap_back", 12);
                dictionary.Add("attack_slap_face", 13);
                dictionary.Add("attack_stomp", 14);
                dictionary.Add("attack_bite", 15);
                dictionary.Add("attack_bite_l", 0x10);
                dictionary.Add("attack_bite_r", 0x11);
                f__switchmap5 = dictionary;
            }
            if (f__switchmap5.TryGetValue(key, out num))
            {
                switch (num)
                {
                    case 0:
                        this.grab("ground_front_l");
                        return true;

                    case 1:
                        this.grab("ground_front_r");
                        return true;

                    case 2:
                        this.grab("ground_back_l");
                        return true;

                    case 3:
                        this.grab("ground_back_r");
                        return true;

                    case 4:
                        this.grab("head_front_l");
                        return true;

                    case 5:
                        this.grab("head_front_r");
                        return true;

                    case 6:
                        this.grab("head_back_l");
                        return true;

                    case 7:
                        this.grab("head_back_r");
                        return true;

                    case 8:
                        this.attack("abnormal_jump");
                        return true;

                    case 9:
                        this.attack("combo_1");
                        return true;

                    case 10:
                        this.attack("front_ground");
                        return true;

                    case 11:
                        this.attack("kick");
                        return true;

                    case 12:
                        this.attack("slap_back");
                        return true;

                    case 13:
                        this.attack("slap_face");
                        return true;

                    case 14:
                        this.attack("stomp");
                        return true;

                    case 15:
                        this.attack("bite");
                        return true;

                    case 0x10:
                        this.attack("bite_l");
                        return true;

                    case 0x11:
                        this.attack("bite_r");
                        return true;
                }
            }
        }
        return false;
    }

    private void findNearestFacingHero()
    {
        GameObject[] objArray = GameObject.FindGameObjectsWithTag("Player");
        GameObject obj2 = null;
        float positiveInfinity = float.PositiveInfinity;
        Vector3 position = base.transform.position;
        float current = 0f;
        float num3 = (this.abnormalType != AbnormalType.NORMAL) ? 180f : 100f;
        float f = 0f;
        foreach (GameObject obj3 in objArray)
        {
            Vector3 vector2 = obj3.transform.position - position;
            float sqrMagnitude = vector2.sqrMagnitude;
            if (sqrMagnitude < positiveInfinity)
            {
                Vector3 vector3 = obj3.transform.position - base.transform.position;
                current = -Mathf.Atan2(vector3.z, vector3.x) * 57.29578f;
                f = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                if (Mathf.Abs(f) < num3)
                {
                    obj2 = obj3;
                    positiveInfinity = sqrMagnitude;
                }
            }
        }
        if (obj2 != null)
        {
            GameObject myHero = this.myHero;
            this.myHero = obj2;
            if (((myHero != this.myHero) && (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)) && PhotonNetwork.isMasterClient)
            {
                if (this.myHero == null)
                {
                    object[] parameters = new object[] { -1 };
                    base.photonView.RPC("setMyTarget", PhotonTargets.Others, parameters);
                }
                else
                {
                    object[] objArray3 = new object[] { this.myHero.GetPhotonView().viewID };
                    base.photonView.RPC("setMyTarget", PhotonTargets.Others, objArray3);
                }
            }
            this.tauntTime = 5f;
        }
    }

    private void findNearestHero()
    {
        GameObject myHero = this.myHero;
        this.myHero = this.getNearestHero();
        if (((this.myHero != myHero) && (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)) && PhotonNetwork.isMasterClient)
        {
            if (this.myHero == null)
            {
                object[] parameters = new object[] { -1 };
                base.photonView.RPC("setMyTarget", PhotonTargets.Others, parameters);
            }
            else
            {
                object[] objArray2 = new object[] { this.myHero.GetPhotonView().viewID };
                base.photonView.RPC("setMyTarget", PhotonTargets.Others, objArray2);
            }
        }
        this.oldHeadRotation = this.head.rotation;
    }

    private void FixedUpdate()
    {
        if ((!IN_GAME_MAIN_CAMERA.isPausing || (IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE)) && ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE) || base.photonView.isMine))
        {
            base.rigidbody.AddForce(new Vector3(0f, -this.gravity * base.rigidbody.mass, 0f));
            if (this.needFreshCorePosition)
            {
                this.oldCorePosition = base.transform.position - base.transform.Find("Amarture/Core").position;
                this.needFreshCorePosition = false;
            }
            if (this.hasDie)
            {
                if ((this.hitPause <= 0f) && base.animation.IsPlaying("die_headOff"))
                {
                    Vector3 vector = (base.transform.position - base.transform.Find("Amarture/Core").position) - this.oldCorePosition;
                    base.rigidbody.velocity = (Vector3) ((vector / Time.deltaTime) + (Vector3.up * base.rigidbody.velocity.y));
                }
                this.oldCorePosition = base.transform.position - base.transform.Find("Amarture/Core").position;
            }
            else if (((this.state == TitanState.attack) && this.isAttackMoveByCore) || (this.state == TitanState.hit))
            {
                Vector3 vector2 = (base.transform.position - base.transform.Find("Amarture/Core").position) - this.oldCorePosition;
                base.rigidbody.velocity = (Vector3) ((vector2 / Time.deltaTime) + (Vector3.up * base.rigidbody.velocity.y));
                this.oldCorePosition = base.transform.position - base.transform.Find("Amarture/Core").position;
            }
            if (this.hasDie)
            {
                if (this.hitPause > 0f)
                {
                    this.hitPause -= Time.deltaTime;
                    if (this.hitPause <= 0f)
                    {
                        base.animation[this.hitAnimation].speed = 1f;
                        this.hitPause = 0f;
                    }
                }
                else if (base.animation.IsPlaying("die_blow"))
                {
                    if (base.animation["die_blow"].normalizedTime < 0.55f)
                    {
                        base.rigidbody.velocity = (Vector3) ((-base.transform.forward * 300f) + (Vector3.up * base.rigidbody.velocity.y));
                    }
                    else if (base.animation["die_blow"].normalizedTime < 0.83f)
                    {
                        base.rigidbody.velocity = (Vector3) ((-base.transform.forward * 100f) + (Vector3.up * base.rigidbody.velocity.y));
                    }
                    else
                    {
                        base.rigidbody.velocity = (Vector3) (Vector3.up * base.rigidbody.velocity.y);
                    }
                }
            }
            else
            {
                if ((this.nonAI && !IN_GAME_MAIN_CAMERA.isPausing) && ((this.state == TitanState.idle) || ((this.state == TitanState.attack) && (this.attackAnimation == "jumper_1"))))
                {
                    Vector3 zero = Vector3.zero;
                    if (this.controller.targetDirection != -874f)
                    {
                        bool flag = false;
                        if (this.stamina < 5f)
                        {
                            flag = true;
                        }
                        else if (((this.stamina < 40f) && !base.animation.IsPlaying("run_abnormal")) && !base.animation.IsPlaying("crawler_run"))
                        {
                            flag = true;
                        }
                        if (this.controller.isWALKDown || flag)
                        {
                            zero = (Vector3) (((base.transform.forward * this.speed) * Mathf.Sqrt(this.myLevel)) * 0.2f);
                        }
                        else
                        {
                            zero = (Vector3) ((base.transform.forward * this.speed) * Mathf.Sqrt(this.myLevel));
                        }
                        base.gameObject.transform.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, this.controller.targetDirection, 0f), (this.speed * 0.15f) * Time.deltaTime);
                        if (this.state == TitanState.idle)
                        {
                            if (this.controller.isWALKDown || flag)
                            {
                                if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                                {
                                    if (!base.animation.IsPlaying("crawler_run"))
                                    {
                                        this.crossFade("crawler_run", 0.1f);
                                    }
                                }
                                else if (!base.animation.IsPlaying("run_walk"))
                                {
                                    this.crossFade("run_walk", 0.1f);
                                }
                            }
                            else if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                            {
                                if (!base.animation.IsPlaying("crawler_run"))
                                {
                                    this.crossFade("crawler_run", 0.1f);
                                }
                                GameObject obj2 = this.checkIfHitCrawlerMouth(this.head, 2.2f);
                                if (obj2 != null)
                                {
                                    Vector3 position = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                    {
                                        obj2.GetComponent<HERO>().die((Vector3) (((obj2.transform.position - position) * 15f) * this.myLevel), false);
                                    }
                                    else if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine) && !obj2.GetComponent<HERO>().HasDied())
                                    {
                                        obj2.GetComponent<HERO>().markDie();
                                        object[] parameters = new object[] { (Vector3) (((obj2.transform.position - position) * 15f) * this.myLevel), true, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                        obj2.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, parameters);
                                    }
                                }
                            }
                            else if (!base.animation.IsPlaying("run_abnormal"))
                            {
                                this.crossFade("run_abnormal", 0.1f);
                            }
                        }
                    }
                    else if (this.state == TitanState.idle)
                    {
                        if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                        {
                            if (!base.animation.IsPlaying("crawler_idle"))
                            {
                                this.crossFade("crawler_idle", 0.1f);
                            }
                        }
                        else if (!base.animation.IsPlaying("idle"))
                        {
                            this.crossFade("idle", 0.1f);
                        }
                        zero = Vector3.zero;
                    }
                    if (this.state == TitanState.idle)
                    {
                        Vector3 velocity = base.rigidbody.velocity;
                        Vector3 force = zero - velocity;
                        force.x = Mathf.Clamp(force.x, -this.maxVelocityChange, this.maxVelocityChange);
                        force.z = Mathf.Clamp(force.z, -this.maxVelocityChange, this.maxVelocityChange);
                        force.y = 0f;
                        base.rigidbody.AddForce(force, ForceMode.VelocityChange);
                    }
                    else if ((this.state == TitanState.attack) && (this.attackAnimation == "jumper_0"))
                    {
                        Vector3 vector7 = base.rigidbody.velocity;
                        Vector3 vector8 = ((Vector3) (zero * 0.8f)) - vector7;
                        vector8.x = Mathf.Clamp(vector8.x, -this.maxVelocityChange, this.maxVelocityChange);
                        vector8.z = Mathf.Clamp(vector8.z, -this.maxVelocityChange, this.maxVelocityChange);
                        vector8.y = 0f;
                        base.rigidbody.AddForce(vector8, ForceMode.VelocityChange);
                    }
                }
                if (((this.abnormalType == AbnormalType.TYPE_I) || (this.abnormalType == AbnormalType.TYPE_JUMPER)) && ((!this.nonAI && (this.state == TitanState.attack)) && (this.attackAnimation == "jumper_0")))
                {
                    Vector3 vector9 = (Vector3) (((base.transform.forward * this.speed) * this.myLevel) * 0.5f);
                    Vector3 vector10 = base.rigidbody.velocity;
                    if ((base.animation["attack_jumper_0"].normalizedTime <= 0.28f) || (base.animation["attack_jumper_0"].normalizedTime >= 0.8f))
                    {
                        vector9 = Vector3.zero;
                    }
                    Vector3 vector11 = vector9 - vector10;
                    vector11.x = Mathf.Clamp(vector11.x, -this.maxVelocityChange, this.maxVelocityChange);
                    vector11.z = Mathf.Clamp(vector11.z, -this.maxVelocityChange, this.maxVelocityChange);
                    vector11.y = 0f;
                    base.rigidbody.AddForce(vector11, ForceMode.VelocityChange);
                }
                if (((this.state == TitanState.chase) || (this.state == TitanState.wander)) || (((this.state == TitanState.to_check_point) || (this.state == TitanState.to_pvp_pt)) || (this.state == TitanState.random_run)))
                {
                    Vector3 vector12 = (Vector3) (base.transform.forward * this.speed);
                    Vector3 vector13 = base.rigidbody.velocity;
                    Vector3 vector14 = vector12 - vector13;
                    vector14.x = Mathf.Clamp(vector14.x, -this.maxVelocityChange, this.maxVelocityChange);
                    vector14.z = Mathf.Clamp(vector14.z, -this.maxVelocityChange, this.maxVelocityChange);
                    vector14.y = 0f;
                    base.rigidbody.AddForce(vector14, ForceMode.VelocityChange);
                    if ((!this.stuck && (this.abnormalType != AbnormalType.TYPE_CRAWLER)) && !this.nonAI)
                    {
                        if (base.animation.IsPlaying(this.runAnimation) && (base.rigidbody.velocity.magnitude < (this.speed * 0.5f)))
                        {
                            this.stuck = true;
                            this.stuckTime = 2f;
                            this.stuckTurnAngle = (UnityEngine.Random.Range(0, 2) * 140f) - 70f;
                        }
                        if (((this.state == TitanState.chase) && (this.myHero != null)) && ((this.myDistance > this.attackDistance) && (this.myDistance < 150f)))
                        {
                            float num = 0.05f;
                            if (this.myDifficulty > 1)
                            {
                                num += 0.05f;
                            }
                            if (this.abnormalType != AbnormalType.NORMAL)
                            {
                                num += 0.1f;
                            }
                            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < num)
                            {
                                this.stuck = true;
                                this.stuckTime = 1f;
                                float num2 = UnityEngine.Random.Range((float) 20f, (float) 50f);
                                this.stuckTurnAngle = ((UnityEngine.Random.Range(0, 2) * num2) * 2f) - num2;
                            }
                        }
                    }
                    float current = 0f;
                    if (this.state == TitanState.wander)
                    {
                        current = base.transform.rotation.eulerAngles.y - 90f;
                    }
                    else if (((this.state == TitanState.to_check_point) || (this.state == TitanState.to_pvp_pt)) || (this.state == TitanState.random_run))
                    {
                        Vector3 vector15 = this.targetCheckPt - base.transform.position;
                        current = -Mathf.Atan2(vector15.z, vector15.x) * 57.29578f;
                    }
                    else
                    {
                        if (this.myHero == null)
                        {
                            return;
                        }
                        Vector3 vector16 = this.myHero.transform.position - base.transform.position;
                        current = -Mathf.Atan2(vector16.z, vector16.x) * 57.29578f;
                    }
                    if (this.stuck)
                    {
                        this.stuckTime -= Time.deltaTime;
                        if (this.stuckTime < 0f)
                        {
                            this.stuck = false;
                        }
                        if (this.stuckTurnAngle > 0f)
                        {
                            this.stuckTurnAngle -= Time.deltaTime * 10f;
                        }
                        else
                        {
                            this.stuckTurnAngle += Time.deltaTime * 10f;
                        }
                        current += this.stuckTurnAngle;
                    }
                    float num4 = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                    if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                    {
                        base.gameObject.transform.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, base.gameObject.transform.rotation.eulerAngles.y + num4, 0f), ((this.speed * 0.3f) * Time.deltaTime) / this.myLevel);
                    }
                    else
                    {
                        base.gameObject.transform.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, base.gameObject.transform.rotation.eulerAngles.y + num4, 0f), ((this.speed * 0.5f) * Time.deltaTime) / this.myLevel);
                    }
                }
            }
        }
    }

    private string[] GetAttackStrategy()
    {
        string[] strArray = null;
        int num = 0;
        if (this.isAlarm || ((this.myHero.transform.position.y + 3f) <= (this.neck.position.y + (10f * this.myLevel))))
        {
            if (this.myHero.transform.position.y > (this.neck.position.y - (3f * this.myLevel)))
            {
                if (this.myDistance < (this.attackDistance * 0.5f))
                {
                    if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("chkOverHead").position) < (3.6f * this.myLevel))
                    {
                        if (this.between2 > 0f)
                        {
                            strArray = new string[] { "grab_head_front_r" };
                        }
                        else
                        {
                            strArray = new string[] { "grab_head_front_l" };
                        }
                    }
                    else if (Mathf.Abs(this.between2) < 90f)
                    {
                        if (Mathf.Abs(this.between2) < 30f)
                        {
                            if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("chkFront").position) < (2.5f * this.myLevel))
                            {
                                strArray = new string[] { "attack_bite", "attack_bite", "attack_slap_face" };
                            }
                        }
                        else if (this.between2 > 0f)
                        {
                            if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("chkFrontRight").position) < (2.5f * this.myLevel))
                            {
                                strArray = new string[] { "attack_bite_r" };
                            }
                        }
                        else if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("chkFrontLeft").position) < (2.5f * this.myLevel))
                        {
                            strArray = new string[] { "attack_bite_l" };
                        }
                    }
                    else if (this.between2 > 0f)
                    {
                        if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("chkBackRight").position) < (2.8f * this.myLevel))
                        {
                            strArray = new string[] { "grab_head_back_r", "grab_head_back_r", "attack_slap_back" };
                        }
                    }
                    else if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("chkBackLeft").position) < (2.8f * this.myLevel))
                    {
                        strArray = new string[] { "grab_head_back_l", "grab_head_back_l", "attack_slap_back" };
                    }
                }
                if (strArray != null)
                {
                    return strArray;
                }
                if ((this.abnormalType == AbnormalType.NORMAL) || (this.abnormalType == AbnormalType.TYPE_PUNK))
                {
                    if ((this.myDifficulty <= 0) && (UnityEngine.Random.Range(0, 0x3e8) >= 3))
                    {
                        return strArray;
                    }
                    if (Mathf.Abs(this.between2) >= 60f)
                    {
                        return strArray;
                    }
                    return new string[] { "attack_combo" };
                }
                if ((this.abnormalType != AbnormalType.TYPE_I) && (this.abnormalType != AbnormalType.TYPE_JUMPER))
                {
                    return strArray;
                }
                if ((this.myDifficulty <= 0) && (UnityEngine.Random.Range(0, 100) >= 50))
                {
                    return strArray;
                }
                return new string[] { "attack_abnormal_jump" };
            }
            if (Mathf.Abs(this.between2) < 90f)
            {
                if (this.between2 > 0f)
                {
                    num = 1;
                }
                else
                {
                    num = 2;
                }
            }
            else if (this.between2 > 0f)
            {
                num = 4;
            }
            else
            {
                num = 3;
            }
            switch (num)
            {
                case 1:
                    if (this.myDistance >= (this.attackDistance * 0.25f))
                    {
                        if (this.myDistance < (this.attackDistance * 0.5f))
                        {
                            if ((this.abnormalType != AbnormalType.TYPE_PUNK) && (this.abnormalType == AbnormalType.NORMAL))
                            {
                                return new string[] { "grab_ground_front_r", "grab_ground_front_r", "attack_stomp" };
                            }
                            return new string[] { "grab_ground_front_r", "grab_ground_front_r", "attack_abnormal_jump" };
                        }
                        if (this.abnormalType == AbnormalType.TYPE_PUNK)
                        {
                            return new string[] { "attack_combo", "attack_combo", "attack_abnormal_jump" };
                        }
                        if (this.abnormalType == AbnormalType.NORMAL)
                        {
                            if (this.myDifficulty > 0)
                            {
                                return new string[] { "attack_front_ground", "attack_combo", "attack_combo" };
                            }
                            return new string[] { "attack_front_ground", "attack_front_ground", "attack_front_ground", "attack_front_ground", "attack_combo" };
                        }
                        return new string[] { "attack_abnormal_jump" };
                    }
                    if (this.abnormalType != AbnormalType.TYPE_PUNK)
                    {
                        if (this.abnormalType == AbnormalType.NORMAL)
                        {
                            return new string[] { "attack_front_ground", "attack_stomp" };
                        }
                        return new string[] { "attack_kick" };
                    }
                    return new string[] { "attack_kick", "attack_stomp" };

                case 2:
                    if (this.myDistance >= (this.attackDistance * 0.25f))
                    {
                        if (this.myDistance < (this.attackDistance * 0.5f))
                        {
                            if ((this.abnormalType != AbnormalType.TYPE_PUNK) && (this.abnormalType == AbnormalType.NORMAL))
                            {
                                return new string[] { "grab_ground_front_l", "grab_ground_front_l", "attack_stomp" };
                            }
                            return new string[] { "grab_ground_front_l", "grab_ground_front_l", "attack_abnormal_jump" };
                        }
                        if (this.abnormalType == AbnormalType.TYPE_PUNK)
                        {
                            return new string[] { "attack_combo", "attack_combo", "attack_abnormal_jump" };
                        }
                        if (this.abnormalType == AbnormalType.NORMAL)
                        {
                            if (this.myDifficulty > 0)
                            {
                                return new string[] { "attack_front_ground", "attack_combo", "attack_combo" };
                            }
                            return new string[] { "attack_front_ground", "attack_front_ground", "attack_front_ground", "attack_front_ground", "attack_combo" };
                        }
                        return new string[] { "attack_abnormal_jump" };
                    }
                    if (this.abnormalType != AbnormalType.TYPE_PUNK)
                    {
                        if (this.abnormalType == AbnormalType.NORMAL)
                        {
                            return new string[] { "attack_front_ground", "attack_stomp" };
                        }
                        return new string[] { "attack_kick" };
                    }
                    return new string[] { "attack_kick", "attack_stomp" };

                case 3:
                    if (this.myDistance >= (this.attackDistance * 0.5f))
                    {
                        return strArray;
                    }
                    if (this.abnormalType != AbnormalType.NORMAL)
                    {
                        return new string[] { "grab_ground_back_l" };
                    }
                    return new string[] { "grab_ground_back_l" };

                case 4:
                    if (this.myDistance >= (this.attackDistance * 0.5f))
                    {
                        return strArray;
                    }
                    if (this.abnormalType != AbnormalType.NORMAL)
                    {
                        return new string[] { "grab_ground_back_r" };
                    }
                    return new string[] { "grab_ground_back_r" };
            }
        }
        return strArray;
    }

    private void getDown()
    {
        this.state = TitanState.down;
        this.isAlarm = true;
        this.playAnimation("sit_hunt_down");
        this.getdownTime = UnityEngine.Random.Range((float) 3f, (float) 5f);
    }

    private GameObject getNearestHero()
    {
        GameObject[] objArray = GameObject.FindGameObjectsWithTag("Player");
        GameObject obj2 = null;
        float positiveInfinity = float.PositiveInfinity;
        Vector3 position = base.transform.position;
        foreach (GameObject obj3 in objArray)
        {
            Vector3 vector2 = obj3.transform.position - position;
            float sqrMagnitude = vector2.sqrMagnitude;
            if (sqrMagnitude < positiveInfinity)
            {
                obj2 = obj3;
                positiveInfinity = sqrMagnitude;
            }
        }
        return obj2;
    }

    private int getPunkNumber()
    {
        int num = 0;
        foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("titan"))
        {
            if ((obj2.GetComponent<TITAN>() != null) && (obj2.GetComponent<TITAN>().name == "Punk"))
            {
                num++;
            }
        }
        return num;
    }

    private void grab(string type)
    {
        this.state = TitanState.grab;
        this.attacked = false;
        this.isAlarm = true;
        this.attackAnimation = type;
        this.crossFade("grab_" + type, 0.1f);
        this.isGrabHandLeft = true;
        this.grabbedTarget = null;
        string key = type;
        if (key != null)
        {
            int num;
            if (f__switchmap7 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(8);
                dictionary.Add("ground_back_l", 0);
                dictionary.Add("ground_back_r", 1);
                dictionary.Add("ground_front_l", 2);
                dictionary.Add("ground_front_r", 3);
                dictionary.Add("head_back_l", 4);
                dictionary.Add("head_back_r", 5);
                dictionary.Add("head_front_l", 6);
                dictionary.Add("head_front_r", 7);
                f__switchmap7 = dictionary;
            }
            if (f__switchmap7.TryGetValue(key, out num))
            {
                switch (num)
                {
                    case 0:
                        this.attackCheckTimeA = 0.34f;
                        this.attackCheckTimeB = 0.49f;
                        break;

                    case 1:
                        this.attackCheckTimeA = 0.34f;
                        this.attackCheckTimeB = 0.49f;
                        this.isGrabHandLeft = false;
                        break;

                    case 2:
                        this.attackCheckTimeA = 0.37f;
                        this.attackCheckTimeB = 0.6f;
                        break;

                    case 3:
                        this.attackCheckTimeA = 0.37f;
                        this.attackCheckTimeB = 0.6f;
                        this.isGrabHandLeft = false;
                        break;

                    case 4:
                        this.attackCheckTimeA = 0.45f;
                        this.attackCheckTimeB = 0.5f;
                        this.isGrabHandLeft = false;
                        break;

                    case 5:
                        this.attackCheckTimeA = 0.45f;
                        this.attackCheckTimeB = 0.5f;
                        break;

                    case 6:
                        this.attackCheckTimeA = 0.38f;
                        this.attackCheckTimeB = 0.55f;
                        break;

                    case 7:
                        this.attackCheckTimeA = 0.38f;
                        this.attackCheckTimeB = 0.55f;
                        this.isGrabHandLeft = false;
                        break;
                }
            }
        }
        if (this.isGrabHandLeft)
        {
            this.currentGrabHand = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001");
        }
        else
        {
            this.currentGrabHand = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
        }
    }

    [RPC]
    public void grabbedTargetEscape()
    {
        this.grabbedTarget = null;
    }

    [RPC]
    public void grabToLeft()
    {
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001");
        this.grabTF.transform.parent = transform;
        this.grabTF.transform.position = transform.GetComponent<SphereCollider>().transform.position;
        this.grabTF.transform.rotation = transform.GetComponent<SphereCollider>().transform.rotation;
        Transform transform1 = this.grabTF.transform;
        transform1.localPosition -= (Vector3) ((Vector3.right * transform.GetComponent<SphereCollider>().radius) * 0.3f);
        Transform transform2 = this.grabTF.transform;
        transform2.localPosition -= (Vector3) ((Vector3.up * transform.GetComponent<SphereCollider>().radius) * 0.51f);
        Transform transform3 = this.grabTF.transform;
        transform3.localPosition -= (Vector3) ((Vector3.forward * transform.GetComponent<SphereCollider>().radius) * 0.3f);
        this.grabTF.transform.localRotation = Quaternion.Euler(this.grabTF.transform.localRotation.eulerAngles.x, this.grabTF.transform.localRotation.eulerAngles.y + 180f, this.grabTF.transform.localRotation.eulerAngles.z + 180f);
    }

    [RPC]
    public void grabToRight()
    {
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
        this.grabTF.transform.parent = transform;
        this.grabTF.transform.position = transform.GetComponent<SphereCollider>().transform.position;
        this.grabTF.transform.rotation = transform.GetComponent<SphereCollider>().transform.rotation;
        Transform transform1 = this.grabTF.transform;
        transform1.localPosition -= (Vector3) ((Vector3.right * transform.GetComponent<SphereCollider>().radius) * 0.3f);
        Transform transform2 = this.grabTF.transform;
        transform2.localPosition += (Vector3) ((Vector3.up * transform.GetComponent<SphereCollider>().radius) * 0.51f);
        Transform transform3 = this.grabTF.transform;
        transform3.localPosition -= (Vector3) ((Vector3.forward * transform.GetComponent<SphereCollider>().radius) * 0.3f);
        this.grabTF.transform.localRotation = Quaternion.Euler(this.grabTF.transform.localRotation.eulerAngles.x, this.grabTF.transform.localRotation.eulerAngles.y + 180f, this.grabTF.transform.localRotation.eulerAngles.z);
    }

    public void headMovement()
    {
        if (!this.hasDie)
        {
            if (IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE)
            {
                if (base.photonView.isMine)
                {
                    this.targetHeadRotation = this.head.rotation;
                    bool flag = false;
                    if (((((this.abnormalType != AbnormalType.TYPE_CRAWLER) && (this.state != TitanState.attack)) && ((this.state != TitanState.down) && (this.state != TitanState.hit))) && (((this.state != TitanState.recover) && (this.state != TitanState.eat)) && ((this.state != TitanState.hit_eye) && !this.hasDie))) && ((this.myDistance < 100f) && (this.myHero != null)))
                    {
                        Vector3 vector = this.myHero.transform.position - base.transform.position;
                        this.angle = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
                        float num = -Mathf.DeltaAngle(this.angle, base.transform.rotation.eulerAngles.y - 90f);
                        num = Mathf.Clamp(num, -40f, 40f);
                        float y = (this.neck.position.y + (this.myLevel * 2f)) - this.myHero.transform.position.y;
                        float num3 = Mathf.Atan2(y, this.myDistance) * 57.29578f;
                        num3 = Mathf.Clamp(num3, -40f, 30f);
                        this.targetHeadRotation = Quaternion.Euler(this.head.rotation.eulerAngles.x + num3, this.head.rotation.eulerAngles.y + num, this.head.rotation.eulerAngles.z);
                        if (!this.asClientLookTarget)
                        {
                            this.asClientLookTarget = true;
                            object[] parameters = new object[] { true };
                            base.photonView.RPC("setIfLookTarget", PhotonTargets.Others, parameters);
                        }
                        flag = true;
                    }
                    if (!flag && this.asClientLookTarget)
                    {
                        this.asClientLookTarget = false;
                        object[] objArray2 = new object[] { false };
                        base.photonView.RPC("setIfLookTarget", PhotonTargets.Others, objArray2);
                    }
                    if (((this.state == TitanState.attack) || (this.state == TitanState.hit)) || (this.state == TitanState.hit_eye))
                    {
                        this.oldHeadRotation = Quaternion.Lerp(this.oldHeadRotation, this.targetHeadRotation, Time.deltaTime * 20f);
                    }
                    else
                    {
                        this.oldHeadRotation = Quaternion.Lerp(this.oldHeadRotation, this.targetHeadRotation, Time.deltaTime * 10f);
                    }
                }
                else
                {
                    this.targetHeadRotation = this.head.rotation;
                    if (this.asClientLookTarget && (this.myHero != null))
                    {
                        Vector3 vector2 = this.myHero.transform.position - base.transform.position;
                        this.angle = -Mathf.Atan2(vector2.z, vector2.x) * 57.29578f;
                        float num4 = -Mathf.DeltaAngle(this.angle, base.transform.rotation.eulerAngles.y - 90f);
                        num4 = Mathf.Clamp(num4, -40f, 40f);
                        float num5 = (this.neck.position.y + (this.myLevel * 2f)) - this.myHero.transform.position.y;
                        float num6 = Mathf.Atan2(num5, this.myDistance) * 57.29578f;
                        num6 = Mathf.Clamp(num6, -40f, 30f);
                        this.targetHeadRotation = Quaternion.Euler(this.head.rotation.eulerAngles.x + num6, this.head.rotation.eulerAngles.y + num4, this.head.rotation.eulerAngles.z);
                    }
                    if (!this.hasDie)
                    {
                        this.oldHeadRotation = Quaternion.Lerp(this.oldHeadRotation, this.targetHeadRotation, Time.deltaTime * 10f);
                    }
                }
            }
            else
            {
                this.targetHeadRotation = this.head.rotation;
                if (((((this.abnormalType != AbnormalType.TYPE_CRAWLER) && (this.state != TitanState.attack)) && ((this.state != TitanState.down) && (this.state != TitanState.hit))) && (((this.state != TitanState.recover) && (this.state != TitanState.hit_eye)) && (!this.hasDie && (this.myDistance < 100f)))) && (this.myHero != null))
                {
                    Vector3 vector3 = this.myHero.transform.position - base.transform.position;
                    this.angle = -Mathf.Atan2(vector3.z, vector3.x) * 57.29578f;
                    float num7 = -Mathf.DeltaAngle(this.angle, base.transform.rotation.eulerAngles.y - 90f);
                    num7 = Mathf.Clamp(num7, -40f, 40f);
                    float num8 = (this.neck.position.y + (this.myLevel * 2f)) - this.myHero.transform.position.y;
                    float num9 = Mathf.Atan2(num8, this.myDistance) * 57.29578f;
                    num9 = Mathf.Clamp(num9, -40f, 30f);
                    this.targetHeadRotation = Quaternion.Euler(this.head.rotation.eulerAngles.x + num9, this.head.rotation.eulerAngles.y + num7, this.head.rotation.eulerAngles.z);
                }
                if (((this.state == TitanState.attack) || (this.state == TitanState.hit)) || (this.state == TitanState.hit_eye))
                {
                    this.oldHeadRotation = Quaternion.Lerp(this.oldHeadRotation, this.targetHeadRotation, Time.deltaTime * 20f);
                }
                else
                {
                    this.oldHeadRotation = Quaternion.Lerp(this.oldHeadRotation, this.targetHeadRotation, Time.deltaTime * 10f);
                }
            }
            this.head.rotation = this.oldHeadRotation;
        }
        if (!base.animation.IsPlaying("die_headOff"))
        {
            this.head.localScale = this.headscale;
        }
    }

    private void hit(string animationName, Vector3 attacker, float hitPauseTime)
    {
        this.state = TitanState.hit;
        this.hitAnimation = animationName;
        this.hitPause = hitPauseTime;
        this.playAnimation(this.hitAnimation);
        base.animation[this.hitAnimation].time = 0f;
        base.animation[this.hitAnimation].speed = 0f;
        base.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(attacker - base.transform.position).eulerAngles.y, 0f);
        this.needFreshCorePosition = true;
        if (base.photonView.isMine && (this.grabbedTarget != null))
        {
            this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
        }
    }

    public void hitAnkle()
    {
        if (!this.hasDie && (this.state != TitanState.down))
        {
            if (this.grabbedTarget != null)
            {
                this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
            }
            this.getDown();
        }
    }

    [RPC]
    public void hitAnkleRPC(int viewID)
    {
        if (!this.hasDie && (this.state != TitanState.down))
        {
            PhotonView view = PhotonView.Find(viewID);
            if (view != null)
            {
                Vector3 vector = view.gameObject.transform.position - base.transform.position;
                if (vector.magnitude < 20f)
                {
                    if (base.photonView.isMine && (this.grabbedTarget != null))
                    {
                        this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
                    }
                    this.getDown();
                }
            }
        }
    }

    public void hitEye()
    {
        if (!this.hasDie)
        {
            this.justHitEye();
        }
    }

    [RPC]
    public void hitEyeRPC(int viewID)
    {
        if (!this.hasDie)
        {
            Vector3 vector = PhotonView.Find(viewID).gameObject.transform.position - this.neck.position;
            if (vector.magnitude < 20f)
            {
                if (base.photonView.isMine && (this.grabbedTarget != null))
                {
                    this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
                }
                if (!this.hasDie)
                {
                    this.justHitEye();
                }
            }
        }
    }

    public void hitL(Vector3 attacker, float hitPauseTime)
    {
        if (this.abnormalType != AbnormalType.TYPE_CRAWLER)
        {
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                this.hit("hit_eren_L", attacker, hitPauseTime);
            }
            else
            {
                object[] parameters = new object[] { attacker, hitPauseTime };
                base.photonView.RPC("hitLRPC", PhotonTargets.All, parameters);
            }
        }
    }

    [RPC]
    private void hitLRPC(Vector3 attacker, float hitPauseTime)
    {
        if (base.photonView.isMine)
        {
            Vector3 vector = attacker - base.transform.position;
            if (vector.magnitude < 80f)
            {
                this.hit("hit_eren_L", attacker, hitPauseTime);
            }
        }
    }

    public void hitR(Vector3 attacker, float hitPauseTime)
    {
        if (this.abnormalType != AbnormalType.TYPE_CRAWLER)
        {
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                this.hit("hit_eren_R", attacker, hitPauseTime);
            }
            else
            {
                object[] parameters = new object[] { attacker, hitPauseTime };
                base.photonView.RPC("hitRRPC", PhotonTargets.All, parameters);
            }
        }
    }

    [RPC]
    private void hitRRPC(Vector3 attacker, float hitPauseTime)
    {
        if (base.photonView.isMine && !this.hasDie)
        {
            Vector3 vector = attacker - base.transform.position;
            if (vector.magnitude < 80f)
            {
                this.hit("hit_eren_R", attacker, hitPauseTime);
            }
        }
    }

    private void idle(float sbtime = 0f)
    {
        this.stuck = false;
        this.sbtime = sbtime;
        if ((this.myDifficulty == 2) && ((this.abnormalType == AbnormalType.TYPE_JUMPER) || (this.abnormalType == AbnormalType.TYPE_I)))
        {
            this.sbtime = UnityEngine.Random.Range((float) 0f, (float) 1.5f);
        }
        else if (this.myDifficulty >= 1)
        {
            this.sbtime = 0f;
        }
        this.sbtime = Mathf.Max(0.5f, this.sbtime);
        if (this.abnormalType == AbnormalType.TYPE_PUNK)
        {
            this.sbtime = 0.1f;
            if (this.myDifficulty == 1)
            {
                this.sbtime += 0.4f;
            }
        }
        this.state = TitanState.idle;
        if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
        {
            this.crossFade("crawler_idle", 0.2f);
        }
        else
        {
            this.crossFade("idle", 0.2f);
        }
    }

    public bool IsGrounded()
    {
        LayerMask mask = ((int) 1) << LayerMask.NameToLayer("Ground");
        LayerMask mask2 = ((int) 1) << LayerMask.NameToLayer("EnemyAABB");
        LayerMask mask3 = mask2 | mask;
        return Physics.Raycast(base.gameObject.transform.position + ((Vector3) (Vector3.up * 0.1f)), -Vector3.up, (float) 0.3f, mask3.value);
    }

    private void justEatHero(GameObject target, Transform hand)
    {
        if (target != null)
        {
            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
            {
                if (!target.GetComponent<HERO>().HasDied())
                {
                    target.GetComponent<HERO>().markDie();
                    if (this.nonAI)
                    {
                        object[] parameters = new object[] { base.photonView.viewID, base.name };
                        target.GetComponent<HERO>().photonView.RPC("netDie2", PhotonTargets.All, parameters);
                    }
                    else
                    {
                        object[] objArray2 = new object[] { -1, base.name };
                        target.GetComponent<HERO>().photonView.RPC("netDie2", PhotonTargets.All, objArray2);
                    }
                }
            }
            else if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
            {
                target.GetComponent<HERO>().die2(hand);
            }
        }
    }

    private void justHitEye()
    {
        if (this.state != TitanState.hit_eye)
        {
            if ((this.state == TitanState.down) || (this.state == TitanState.sit))
            {
                this.playAnimation("sit_hit_eye");
            }
            else
            {
                this.playAnimation("hit_eye");
            }
            this.state = TitanState.hit_eye;
        }
    }

    public void lateUpdate()
    {
        if (!IN_GAME_MAIN_CAMERA.isPausing || (IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE))
        {
            if (base.animation.IsPlaying("run_walk"))
            {
                if ((((base.animation["run_walk"].normalizedTime % 1f) > 0.1f) && ((base.animation["run_walk"].normalizedTime % 1f) < 0.6f)) && (this.stepSoundPhase == 2))
                {
                    this.stepSoundPhase = 1;
                    Transform transform = base.transform.Find("snd_titan_foot");
                    transform.GetComponent<AudioSource>().Stop();
                    transform.GetComponent<AudioSource>().Play();
                }
                if (((base.animation["run_walk"].normalizedTime % 1f) > 0.6f) && (this.stepSoundPhase == 1))
                {
                    this.stepSoundPhase = 2;
                    Transform transform2 = base.transform.Find("snd_titan_foot");
                    transform2.GetComponent<AudioSource>().Stop();
                    transform2.GetComponent<AudioSource>().Play();
                }
            }
            if (base.animation.IsPlaying("crawler_run"))
            {
                if ((((base.animation["crawler_run"].normalizedTime % 1f) > 0.1f) && ((base.animation["crawler_run"].normalizedTime % 1f) < 0.56f)) && (this.stepSoundPhase == 2))
                {
                    this.stepSoundPhase = 1;
                    Transform transform3 = base.transform.Find("snd_titan_foot");
                    transform3.GetComponent<AudioSource>().Stop();
                    transform3.GetComponent<AudioSource>().Play();
                }
                if (((base.animation["crawler_run"].normalizedTime % 1f) > 0.56f) && (this.stepSoundPhase == 1))
                {
                    this.stepSoundPhase = 2;
                    Transform transform4 = base.transform.Find("snd_titan_foot");
                    transform4.GetComponent<AudioSource>().Stop();
                    transform4.GetComponent<AudioSource>().Play();
                }
            }
            if (base.animation.IsPlaying("run_abnormal"))
            {
                if ((((base.animation["run_abnormal"].normalizedTime % 1f) > 0.47f) && ((base.animation["run_abnormal"].normalizedTime % 1f) < 0.95f)) && (this.stepSoundPhase == 2))
                {
                    this.stepSoundPhase = 1;
                    Transform transform5 = base.transform.Find("snd_titan_foot");
                    transform5.GetComponent<AudioSource>().Stop();
                    transform5.GetComponent<AudioSource>().Play();
                }
                if ((((base.animation["run_abnormal"].normalizedTime % 1f) > 0.95f) || ((base.animation["run_abnormal"].normalizedTime % 1f) < 0.47f)) && (this.stepSoundPhase == 1))
                {
                    this.stepSoundPhase = 2;
                    Transform transform6 = base.transform.Find("snd_titan_foot");
                    transform6.GetComponent<AudioSource>().Stop();
                    transform6.GetComponent<AudioSource>().Play();
                }
            }
            this.headMovement();
            this.grounded = false;
        }
    }

    [RPC]
    private void laugh(float sbtime = 0f)
    {
        if (((this.state == TitanState.idle) || (this.state == TitanState.turn)) || (this.state == TitanState.chase))
        {
            this.sbtime = sbtime;
            this.state = TitanState.laugh;
            this.crossFade("laugh", 0.2f);
        }
    }

    private bool longRangeAttackCheck()
    {
        if ((this.abnormalType == AbnormalType.TYPE_PUNK) && ((this.myHero != null) && (this.myHero.rigidbody != null)))
        {
            Vector3 line = (Vector3) ((this.myHero.rigidbody.velocity * Time.deltaTime) * 30f);
            if (line.sqrMagnitude > 10f)
            {
                if (this.simpleHitTestLineAndBall(line, base.transform.Find("chkAeLeft").position - this.myHero.transform.position, 5f * this.myLevel))
                {
                    this.attack("anti_AE_l");
                    return true;
                }
                if (this.simpleHitTestLineAndBall(line, base.transform.Find("chkAeLLeft").position - this.myHero.transform.position, 5f * this.myLevel))
                {
                    this.attack("anti_AE_low_l");
                    return true;
                }
                if (this.simpleHitTestLineAndBall(line, base.transform.Find("chkAeRight").position - this.myHero.transform.position, 5f * this.myLevel))
                {
                    this.attack("anti_AE_r");
                    return true;
                }
                if (this.simpleHitTestLineAndBall(line, base.transform.Find("chkAeLRight").position - this.myHero.transform.position, 5f * this.myLevel))
                {
                    this.attack("anti_AE_low_r");
                    return true;
                }
            }
            Vector3 vector2 = this.myHero.transform.position - base.transform.position;
            float current = -Mathf.Atan2(vector2.z, vector2.x) * 57.29578f;
            float f = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
            if (this.rockInterval > 0f)
            {
                this.rockInterval -= Time.deltaTime;
            }
            else if (Mathf.Abs(f) < 5f)
            {
                Vector3 vector3 = this.myHero.transform.position + line;
                Vector3 vector5 = vector3 - base.transform.position;
                float sqrMagnitude = vector5.sqrMagnitude;
                if ((sqrMagnitude > 8000f) && (sqrMagnitude < 90000f))
                {
                    this.attack("throw");
                    this.rockInterval = 2f;
                    return true;
                }
            }
        }
        return false;
    }

    [RPC]
    private void netCrossFade(string aniName, float time)
    {
        base.animation.CrossFade(aniName, time);
    }

    [RPC]
    private void netDie()
    {
        this.asClientLookTarget = false;
        if (!this.hasDie)
        {
            this.hasDie = true;
            if (this.nonAI)
            {
                this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().setMainObject(null, true, false);
                this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().setSpectorMode(true);
                this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().gameOver = true;
                ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                propertiesToSet.Add(PhotonPlayerProperty.dead, true);
                PhotonNetwork.player.SetCustomProperties(propertiesToSet);
                propertiesToSet = new ExitGames.Client.Photon.Hashtable();
                propertiesToSet.Add(PhotonPlayerProperty.deaths, ((int) PhotonNetwork.player.customProperties[PhotonPlayerProperty.deaths]) + 1);
                PhotonNetwork.player.SetCustomProperties(propertiesToSet);
            }
            this.dieAnimation();
        }
    }

    [RPC]
    private void netPlayAnimation(string aniName)
    {
        base.animation.Play(aniName);
    }

    [RPC]
    private void netPlayAnimationAt(string aniName, float normalizedTime)
    {
        base.animation.Play(aniName);
        base.animation[aniName].normalizedTime = normalizedTime;
    }

    [RPC]
    private void netSetAbnormalType(int type)
    {
        if (type == 0)
        {
            this.abnormalType = AbnormalType.NORMAL;
            base.name = "Titan";
            this.runAnimation = "run_walk";
            base.GetComponent<TITAN_SETUP>().setHair();
        }
        else if (type == 1)
        {
            this.abnormalType = AbnormalType.TYPE_I;
            base.name = "Aberrant";
            this.runAnimation = "run_abnormal";
            base.GetComponent<TITAN_SETUP>().setHair();
        }
        else if (type == 2)
        {
            this.abnormalType = AbnormalType.TYPE_JUMPER;
            base.name = "Jumper";
            this.runAnimation = "run_abnormal";
            base.GetComponent<TITAN_SETUP>().setHair();
        }
        else if (type == 3)
        {
            this.abnormalType = AbnormalType.TYPE_CRAWLER;
            base.name = "Crawler";
            this.runAnimation = "crawler_run";
            base.GetComponent<TITAN_SETUP>().setHair();
        }
        else if (type == 4)
        {
            this.abnormalType = AbnormalType.TYPE_PUNK;
            base.name = "Punk";
            this.runAnimation = "run_abnormal_1";
            base.GetComponent<TITAN_SETUP>().setPunkHair();
        }
        if (((this.abnormalType == AbnormalType.TYPE_I) || (this.abnormalType == AbnormalType.TYPE_JUMPER)) || (this.abnormalType == AbnormalType.TYPE_PUNK))
        {
            this.speed = 18f;
            if (this.myLevel > 1f)
            {
                this.speed *= Mathf.Sqrt(this.myLevel);
            }
            if (this.myDifficulty == 1)
            {
                this.speed *= 1.4f;
            }
            if (this.myDifficulty == 2)
            {
                this.speed *= 1.6f;
            }
            base.animation["turnaround1"].speed = 2f;
            base.animation["turnaround2"].speed = 2f;
        }
        if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
        {
            this.chaseDistance += 50f;
            this.speed = 25f;
            if (this.myLevel > 1f)
            {
                this.speed *= Mathf.Sqrt(this.myLevel);
            }
            if (this.myDifficulty == 1)
            {
                this.speed *= 2f;
            }
            if (this.myDifficulty == 2)
            {
                this.speed *= 2.2f;
            }
            base.transform.Find("AABB").gameObject.GetComponent<CapsuleCollider>().height = 10f;
            base.transform.Find("AABB").gameObject.GetComponent<CapsuleCollider>().radius = 5f;
            base.transform.Find("AABB").gameObject.GetComponent<CapsuleCollider>().center = new Vector3(0f, 5.05f, 0f);
        }
        if (this.nonAI)
        {
            if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
            {
                this.speed = Mathf.Min(70f, this.speed);
            }
            else
            {
                this.speed = Mathf.Min(60f, this.speed);
            }
            base.animation["attack_jumper_0"].speed = 7f;
            base.animation["attack_crawler_jump_0"].speed = 4f;
        }
        base.animation["attack_combo_1"].speed = 1f;
        base.animation["attack_combo_2"].speed = 1f;
        base.animation["attack_combo_3"].speed = 1f;
        base.animation["attack_quick_turn_l"].speed = 1f;
        base.animation["attack_quick_turn_r"].speed = 1f;
        base.animation["attack_anti_AE_l"].speed = 1.1f;
        base.animation["attack_anti_AE_low_l"].speed = 1.1f;
        base.animation["attack_anti_AE_r"].speed = 1.1f;
        base.animation["attack_anti_AE_low_r"].speed = 1.1f;
        this.idle(0f);
    }

    [RPC]
    private void netSetLevel(float level, int AI, int skinColor)
    {
        this.setLevel(level, AI, skinColor);
    }

    private void OnCollisionStay()
    {
        this.grounded = true;
    }

    private void OnDestroy()
    {
        if (GameObject.Find("MultiplayerManager") != null)
        {
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().removeTitan(this);
        }
    }

    private void playAnimation(string aniName)
    {
        base.animation.Play(aniName);
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
        {
            object[] parameters = new object[] { aniName };
            base.photonView.RPC("netPlayAnimation", PhotonTargets.Others, parameters);
        }
    }

    private void playAnimationAt(string aniName, float normalizedTime)
    {
        base.animation.Play(aniName);
        base.animation[aniName].normalizedTime = normalizedTime;
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
        {
            object[] parameters = new object[] { aniName, normalizedTime };
            base.photonView.RPC("netPlayAnimationAt", PhotonTargets.Others, parameters);
        }
    }

    private void playSound(string sndname)
    {
        this.playsoundRPC(sndname);
        if (base.photonView.isMine)
        {
            object[] parameters = new object[] { sndname };
            base.photonView.RPC("playsoundRPC", PhotonTargets.Others, parameters);
        }
    }

    [RPC]
    private void playsoundRPC(string sndname)
    {
        base.transform.Find(sndname).GetComponent<AudioSource>().Play();
    }

    public void randomRun(Vector3 targetPt, float r)
    {
        this.state = TitanState.random_run;
        this.targetCheckPt = targetPt;
        this.targetR = r;
        this.random_run_time = UnityEngine.Random.Range((float) 1f, (float) 2f);
        this.crossFade(this.runAnimation, 0.5f);
    }

    private void recover()
    {
        this.state = TitanState.recover;
        this.playAnimation("idle_recovery");
        this.getdownTime = UnityEngine.Random.Range((float) 2f, (float) 5f);
    }

    private void remainSitdown()
    {
        this.state = TitanState.sit;
        this.playAnimation("sit_idle");
        this.getdownTime = UnityEngine.Random.Range((float) 10f, (float) 30f);
    }

    public void setAbnormalType(AbnormalType type, bool forceCrawler = false)
    {
        int num = 0;
        float num2 = 0.02f * (IN_GAME_MAIN_CAMERA.difficulty + 1);
        if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_AHSS)
        {
            num2 = 100f;
        }
        if (type == AbnormalType.NORMAL)
        {
            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < num2)
            {
                num = 4;
            }
            else
            {
                num = 0;
            }
        }
        else if (type == AbnormalType.TYPE_I)
        {
            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < num2)
            {
                num = 4;
            }
            else
            {
                num = 1;
            }
        }
        else if (type == AbnormalType.TYPE_JUMPER)
        {
            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < num2)
            {
                num = 4;
            }
            else
            {
                num = 2;
            }
        }
        else if (type == AbnormalType.TYPE_CRAWLER)
        {
            num = 3;
            if ((GameObject.Find("Crawler") != null) && (UnityEngine.Random.Range(0, 0x3e8) > 5))
            {
                num = 2;
            }
        }
        else if (type == AbnormalType.TYPE_PUNK)
        {
            num = 4;
        }
        if (forceCrawler)
        {
            num = 3;
        }
        if (num == 4)
        {
            if (!LevelInfo.getInfo(FengGameManagerMKII.level).punk)
            {
                num = 1;
            }
            else
            {
                if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE) && (this.getPunkNumber() >= 3))
                {
                    num = 1;
                }
                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE)
                {
                    int wave = GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().wave;
                    if (((wave != 5) && (wave != 10)) && ((wave != 15) && (wave != 20)))
                    {
                        num = 1;
                    }
                }
            }
        }
        if ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && base.photonView.isMine)
        {
            object[] parameters = new object[] { num };
            base.photonView.RPC("netSetAbnormalType", PhotonTargets.AllBuffered, parameters);
        }
        else if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            this.netSetAbnormalType(num);
        }
    }

    [RPC]
    private void setIfLookTarget(bool bo)
    {
        this.asClientLookTarget = bo;
    }

    private void setLevel(float level, int AI, int skinColor)
    {
        this.myLevel = level;
        this.myLevel = Mathf.Clamp(this.myLevel, 0.7f, 3f);
        this.attackWait += UnityEngine.Random.Range((float) 0f, (float) 2f);
        this.chaseDistance += this.myLevel * 10f;
        base.transform.localScale = new Vector3(this.myLevel, this.myLevel, this.myLevel);
        float x = Mathf.Min(Mathf.Pow(2f / this.myLevel, 0.35f), 1.25f);
        this.headscale = new Vector3(x, x, x);
        this.head = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head");
        this.head.localScale = this.headscale;
        if (skinColor != 0)
        {
            this.mainMaterial.GetComponent<SkinnedMeshRenderer>().material.color = (skinColor != 1) ? ((skinColor != 2) ? FengColor.titanSkin3 : FengColor.titanSkin2) : FengColor.titanSkin1;
        }
        float num2 = 1.4f - ((this.myLevel - 0.7f) * 0.15f);
        num2 = Mathf.Clamp(num2, 0.9f, 1.5f);
        IEnumerator enumerator = base.animation.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                AnimationState current = (AnimationState) enumerator.Current;
                current.speed = num2;
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
        Rigidbody rigidbody = base.rigidbody;
        rigidbody.mass *= this.myLevel;
        base.rigidbody.rotation = Quaternion.Euler(0f, (float) UnityEngine.Random.Range(0, 360), 0f);
        if (this.myLevel > 1f)
        {
            this.speed *= Mathf.Sqrt(this.myLevel);
        }
        this.myDifficulty = AI;
        if ((this.myDifficulty == 1) || (this.myDifficulty == 2))
        {
            IEnumerator enumerator2 = base.animation.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    AnimationState state2 = (AnimationState) enumerator2.Current;
                    state2.speed = num2 * 1.05f;
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
            if (this.nonAI)
            {
                this.speed *= 1.1f;
            }
            else
            {
                this.speed *= 1.4f;
            }
            this.chaseDistance *= 1.15f;
        }
        if (this.myDifficulty == 2)
        {
            IEnumerator enumerator3 = base.animation.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    AnimationState state3 = (AnimationState) enumerator3.Current;
                    state3.speed = num2 * 1.05f;
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
            if (this.nonAI)
            {
                this.speed *= 1.1f;
            }
            else
            {
                this.speed *= 1.5f;
            }
            this.chaseDistance *= 1.3f;
        }
        if ((IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.ENDLESS_TITAN) || (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.SURVIVE_MODE))
        {
            this.chaseDistance = 999999f;
        }
        if (this.nonAI)
        {
            if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
            {
                this.speed = Mathf.Min(70f, this.speed);
            }
            else
            {
                this.speed = Mathf.Min(60f, this.speed);
            }
        }
        this.attackDistance = Vector3.Distance(base.transform.position, base.transform.Find("ap_front_ground").position) * 1.65f;
    }

    private void setmyLevel()
    {
        base.animation.cullingType = AnimationCullingType.BasedOnRenderers;
        if ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE) && base.photonView.isMine)
        {
            object[] parameters = new object[] { this.myLevel, GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().difficulty, UnityEngine.Random.Range(0, 4) };
            base.photonView.RPC("netSetLevel", PhotonTargets.AllBuffered, parameters);
            base.animation.cullingType = AnimationCullingType.AlwaysAnimate;
        }
        else if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            this.setLevel(this.myLevel, IN_GAME_MAIN_CAMERA.difficulty, UnityEngine.Random.Range(0, 4));
        }
    }

    [RPC]
    private void setMyTarget(int ID)
    {
        if (ID == -1)
        {
            this.myHero = null;
        }
        PhotonView view = PhotonView.Find(ID);
        if (view != null)
        {
            this.myHero = view.gameObject;
        }
    }

    public void setRoute(GameObject route)
    {
        this.checkPoints = new ArrayList();
        for (int i = 1; i <= 10; i++)
        {
            this.checkPoints.Add(route.transform.Find("r" + i).position);
        }
        this.checkPoints.Add("end");
    }

    private bool simpleHitTestLineAndBall(Vector3 line, Vector3 ball, float R)
    {
        Vector3 rhs = Vector3.Project(ball, line);
        Vector3 vector2 = ball - rhs;
        if (vector2.magnitude > R)
        {
            return false;
        }
        if (Vector3.Dot(line, rhs) < 0f)
        {
            return false;
        }
        if (rhs.sqrMagnitude > line.sqrMagnitude)
        {
            return false;
        }
        return true;
    }

    private void sitdown()
    {
        this.state = TitanState.sit;
        this.playAnimation("sit_down");
        this.getdownTime = UnityEngine.Random.Range((float) 10f, (float) 30f);
    }

    private void Start()
    {
        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().addTitan(this);
        this.currentCamera = GameObject.Find("MainCamera");
        this.runAnimation = "run_walk";
        this.grabTF = new GameObject();
        this.grabTF.name = "titansTmpGrabTF";
        this.head = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head");
        this.neck = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck");
        this.oldHeadRotation = this.head.rotation;
        if ((IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.MULTIPLAYER) || base.photonView.isMine)
        {
            this.myLevel = UnityEngine.Random.Range((float) 0.5f, (float) 3.5f);
            this.spawnPt = base.transform.position;
            this.setmyLevel();
            this.setAbnormalType(this.abnormalType, false);
            if (this.myHero == null)
            {
                this.findNearestHero();
            }
            this.controller = base.gameObject.GetComponent<TITAN_CONTROLLER>();
        }
    }

    public void suicide()
    {
        this.netDie();
        if (this.nonAI)
        {
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().sendKillInfo(false, string.Empty, true, (string) PhotonNetwork.player.customProperties[PhotonPlayerProperty.name], 0);
        }
        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().needChooseSide = true;
        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().justSuicide = true;
    }

    [RPC]
    public void titanGetHit(int viewID, int speed)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null)
        {
            Vector3 vector = view.gameObject.transform.position - this.neck.position;
            if ((vector.magnitude < 30f) && !this.hasDie)
            {
                base.photonView.RPC("netDie", PhotonTargets.OthersBuffered, new object[0]);
                if (this.grabbedTarget != null)
                {
                    this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
                }
                this.netDie();
                if (this.nonAI)
                {
                    GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().titanGetKill(view.owner, speed, (string) PhotonNetwork.player.customProperties[PhotonPlayerProperty.name]);
                }
                else
                {
                    GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().titanGetKill(view.owner, speed, base.name);
                }
            }
        }
    }

    public void toCheckPoint(Vector3 targetPt, float r)
    {
        this.state = TitanState.to_check_point;
        this.targetCheckPt = targetPt;
        this.targetR = r;
        this.crossFade(this.runAnimation, 0.5f);
    }

    public void toPVPCheckPoint(Vector3 targetPt, float r)
    {
        this.state = TitanState.to_pvp_pt;
        this.targetCheckPt = targetPt;
        this.targetR = r;
        this.crossFade(this.runAnimation, 0.5f);
    }

    private void turn(float d)
    {
        if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
        {
            if (d > 0f)
            {
                this.turnAnimation = "crawler_turnaround_R";
            }
            else
            {
                this.turnAnimation = "crawler_turnaround_L";
            }
        }
        else if (d > 0f)
        {
            this.turnAnimation = "turnaround2";
        }
        else
        {
            this.turnAnimation = "turnaround1";
        }
        this.playAnimation(this.turnAnimation);
        base.animation[this.turnAnimation].time = 0f;
        d = Mathf.Clamp(d, -120f, 120f);
        this.turnDeg = d;
        this.desDeg = base.gameObject.transform.rotation.eulerAngles.y + this.turnDeg;
        this.state = TitanState.turn;
    }

    public void update()
    {
        if (((!IN_GAME_MAIN_CAMERA.isPausing || (IN_GAME_MAIN_CAMERA.gametype != GAMETYPE.SINGLE)) && (this.myDifficulty >= 0)) && ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE) || base.photonView.isMine))
        {
            if (!this.nonAI)
            {
                if ((this.activeRad < 0x7fffffff) && (((this.state == TitanState.idle) || (this.state == TitanState.wander)) || (this.state == TitanState.chase)))
                {
                    if (this.checkPoints.Count > 1)
                    {
                        if (Vector3.Distance((Vector3) this.checkPoints[0], base.transform.position) > this.activeRad)
                        {
                            this.toCheckPoint((Vector3) this.checkPoints[0], 10f);
                        }
                    }
                    else if (Vector3.Distance(this.spawnPt, base.transform.position) > this.activeRad)
                    {
                        this.toCheckPoint(this.spawnPt, 10f);
                    }
                }
                if (this.whoHasTauntMe != null)
                {
                    this.tauntTime -= Time.deltaTime;
                    if (this.tauntTime <= 0f)
                    {
                        this.whoHasTauntMe = null;
                    }
                    this.myHero = this.whoHasTauntMe;
                    if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && PhotonNetwork.isMasterClient)
                    {
                        object[] parameters = new object[] { this.myHero.GetPhotonView().viewID };
                        base.photonView.RPC("setMyTarget", PhotonTargets.Others, parameters);
                    }
                }
            }
            if (this.hasDie)
            {
                this.dieTime += Time.deltaTime;
                if ((this.dieTime > 2f) && !this.hasDieSteam)
                {
                    this.hasDieSteam = true;
                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                    {
                        GameObject obj2 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/FXtitanDie1"));
                        obj2.transform.position = base.transform.Find("Amarture/Core/Controller_Body/hip").position;
                        obj2.transform.localScale = base.transform.localScale;
                    }
                    else if (base.photonView.isMine)
                    {
                        PhotonNetwork.Instantiate("FX/FXtitanDie1", base.transform.Find("Amarture/Core/Controller_Body/hip").position, Quaternion.Euler(-90f, 0f, 0f), 0).transform.localScale = base.transform.localScale;
                    }
                }
                if (this.dieTime > 5f)
                {
                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                    {
                        GameObject obj4 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/FXtitanDie"));
                        obj4.transform.position = base.transform.Find("Amarture/Core/Controller_Body/hip").position;
                        obj4.transform.localScale = base.transform.localScale;
                        UnityEngine.Object.Destroy(base.gameObject);
                    }
                    else if (base.photonView.isMine)
                    {
                        PhotonNetwork.Instantiate("FX/FXtitanDie", base.transform.Find("Amarture/Core/Controller_Body/hip").position, Quaternion.Euler(-90f, 0f, 0f), 0).transform.localScale = base.transform.localScale;
                        PhotonNetwork.Destroy(base.gameObject);
                        this.myDifficulty = -1;
                    }
                }
            }
            else
            {
                if (this.state == TitanState.hit)
                {
                    if (this.hitPause > 0f)
                    {
                        this.hitPause -= Time.deltaTime;
                        if (this.hitPause <= 0f)
                        {
                            base.animation[this.hitAnimation].speed = 1f;
                            this.hitPause = 0f;
                        }
                    }
                    if (base.animation[this.hitAnimation].normalizedTime >= 1f)
                    {
                        this.idle(0f);
                    }
                }
                if (!this.nonAI)
                {
                    if (this.myHero == null)
                    {
                        this.findNearestHero();
                    }
                    if ((((this.state == TitanState.idle) || (this.state == TitanState.chase)) || (this.state == TitanState.wander)) && ((this.whoHasTauntMe == null) && (UnityEngine.Random.Range(0, 100) < 10)))
                    {
                        this.findNearestFacingHero();
                    }
                    if (this.myHero == null)
                    {
                        this.myDistance = float.MaxValue;
                    }
                    else
                    {
                        this.myDistance = Mathf.Sqrt(((this.myHero.transform.position.x - base.transform.position.x) * (this.myHero.transform.position.x - base.transform.position.x)) + ((this.myHero.transform.position.z - base.transform.position.z) * (this.myHero.transform.position.z - base.transform.position.z)));
                    }
                }
                else
                {
                    if (this.stamina < this.maxStamina)
                    {
                        if (base.animation.IsPlaying("idle"))
                        {
                            this.stamina += Time.deltaTime * 30f;
                        }
                        if (base.animation.IsPlaying("crawler_idle"))
                        {
                            this.stamina += Time.deltaTime * 35f;
                        }
                        if (base.animation.IsPlaying("run_walk"))
                        {
                            this.stamina += Time.deltaTime * 10f;
                        }
                    }
                    if (base.animation.IsPlaying("run_abnormal_1"))
                    {
                        this.stamina -= Time.deltaTime * 5f;
                    }
                    if (base.animation.IsPlaying("crawler_run"))
                    {
                        this.stamina -= Time.deltaTime * 15f;
                    }
                    if (this.stamina < 0f)
                    {
                        this.stamina = 0f;
                    }
                    if (!IN_GAME_MAIN_CAMERA.isPausing)
                    {
                        GameObject.Find("stamina_titan").transform.localScale = new Vector3(this.stamina, 16f);
                    }
                }
                if (this.state == TitanState.laugh)
                {
                    if (base.animation["laugh"].normalizedTime >= 1f)
                    {
                        this.idle(2f);
                    }
                }
                else if (this.state == TitanState.idle)
                {
                    if (this.nonAI)
                    {
                        if (!IN_GAME_MAIN_CAMERA.isPausing)
                        {
                            if (this.abnormalType != AbnormalType.TYPE_CRAWLER)
                            {
                                if (this.controller.isAttackDown && (this.stamina > 25f))
                                {
                                    this.stamina -= 25f;
                                    this.attack("combo_1");
                                }
                                else if (this.controller.isAttackIIDown && (this.stamina > 50f))
                                {
                                    this.stamina -= 50f;
                                    this.attack("abnormal_jump");
                                }
                                else if (this.controller.isJumpDown && (this.stamina > 15f))
                                {
                                    this.stamina -= 15f;
                                    this.attack("jumper_0");
                                }
                            }
                            else if (this.controller.isAttackDown && (this.stamina > 40f))
                            {
                                this.stamina -= 40f;
                                this.attack("crawler_jump_0");
                            }
                            if (this.controller.isSuicide)
                            {
                                this.suicide();
                            }
                        }
                    }
                    else if (this.sbtime > 0f)
                    {
                        this.sbtime -= Time.deltaTime;
                    }
                    else
                    {
                        if (!this.isAlarm)
                        {
                            if (((this.abnormalType != AbnormalType.TYPE_PUNK) && (this.abnormalType != AbnormalType.TYPE_CRAWLER)) && (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.005f))
                            {
                                this.sitdown();
                                return;
                            }
                            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.02f)
                            {
                                this.wander(0f);
                                return;
                            }
                            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.01f)
                            {
                                this.turn((float) UnityEngine.Random.Range(30, 120));
                                return;
                            }
                            if (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.01f)
                            {
                                this.turn((float) UnityEngine.Random.Range(-30, -120));
                                return;
                            }
                        }
                        this.angle = 0f;
                        this.between2 = 0f;
                        if ((this.myDistance < this.chaseDistance) || (this.whoHasTauntMe != null))
                        {
                            Vector3 vector = this.myHero.transform.position - base.transform.position;
                            this.angle = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
                            this.between2 = -Mathf.DeltaAngle(this.angle, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                            if (this.myDistance >= this.attackDistance)
                            {
                                if (this.isAlarm || (Mathf.Abs(this.between2) < 90f))
                                {
                                    this.chase();
                                    return;
                                }
                                if (!this.isAlarm && (this.myDistance < (this.chaseDistance * 0.1f)))
                                {
                                    this.chase();
                                    return;
                                }
                            }
                        }
                        if (!this.longRangeAttackCheck())
                        {
                            if (this.myDistance < this.chaseDistance)
                            {
                                if (((this.abnormalType == AbnormalType.TYPE_JUMPER) && ((this.myDistance > this.attackDistance) || (this.myHero.transform.position.y > (this.head.position.y + (4f * this.myLevel))))) && ((Mathf.Abs(this.between2) < 120f) && (Vector3.Distance(base.transform.position, this.myHero.transform.position) < (1.5f * this.myHero.transform.position.y))))
                                {
                                    this.attack("jumper_0");
                                    return;
                                }
                                if ((((this.abnormalType == AbnormalType.TYPE_CRAWLER) && (this.myDistance < (this.attackDistance * 3f))) && ((Mathf.Abs(this.between2) < 90f) && (this.myHero.transform.position.y < (this.neck.position.y + (30f * this.myLevel))))) && (this.myHero.transform.position.y > (this.neck.position.y + (10f * this.myLevel))))
                                {
                                    this.attack("crawler_jump_0");
                                    return;
                                }
                            }
                            if (((this.abnormalType == AbnormalType.TYPE_PUNK) && (this.myDistance < 90f)) && (Mathf.Abs(this.between2) > 90f))
                            {
                                if (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.4f)
                                {
                                    this.randomRun(base.transform.position + new Vector3(UnityEngine.Random.Range((float) -50f, (float) 50f), UnityEngine.Random.Range((float) -50f, (float) 50f), UnityEngine.Random.Range((float) -50f, (float) 50f)), 10f);
                                }
                                if (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.2f)
                                {
                                    this.recover();
                                }
                                else if (UnityEngine.Random.Range(0, 2) == 0)
                                {
                                    this.attack("quick_turn_l");
                                }
                                else
                                {
                                    this.attack("quick_turn_r");
                                }
                            }
                            else
                            {
                                if (this.myDistance < this.attackDistance)
                                {
                                    if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                                    {
                                        if (((this.myHero.transform.position.y + 3f) <= (this.neck.position.y + (20f * this.myLevel))) && (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.1f))
                                        {
                                            this.chase();
                                            return;
                                        }
                                        return;
                                    }
                                    string decidedAction = string.Empty;
                                    string[] attackStrategy = this.GetAttackStrategy();
                                    if (attackStrategy != null)
                                    {
                                        decidedAction = attackStrategy[UnityEngine.Random.Range(0, attackStrategy.Length)];
                                    }
                                    if (((this.abnormalType == AbnormalType.TYPE_JUMPER) || (this.abnormalType == AbnormalType.TYPE_I)) && (Mathf.Abs(this.between2) > 40f))
                                    {
                                        if ((decidedAction.Contains("grab") || decidedAction.Contains("kick")) || (decidedAction.Contains("slap") || decidedAction.Contains("bite")))
                                        {
                                            if (UnityEngine.Random.Range(0, 100) < 30)
                                            {
                                                this.turn(this.between2);
                                                return;
                                            }
                                        }
                                        else if (UnityEngine.Random.Range(0, 100) < 90)
                                        {
                                            this.turn(this.between2);
                                            return;
                                        }
                                    }
                                    if (this.executeAttack(decidedAction))
                                    {
                                        return;
                                    }
                                    if (this.abnormalType == AbnormalType.NORMAL)
                                    {
                                        if ((UnityEngine.Random.Range(0, 100) < 30) && (Mathf.Abs(this.between2) > 45f))
                                        {
                                            this.turn(this.between2);
                                            return;
                                        }
                                    }
                                    else if (Mathf.Abs(this.between2) > 45f)
                                    {
                                        this.turn(this.between2);
                                        return;
                                    }
                                }
                                if (this.PVPfromCheckPt != null)
                                {
                                    if (this.PVPfromCheckPt.state == CheckPointState.Titan)
                                    {
                                        GameObject chkPtNext;
                                        if (UnityEngine.Random.Range(0, 100) > 0x30)
                                        {
                                            chkPtNext = this.PVPfromCheckPt.chkPtNext;
                                            if ((chkPtNext != null) && ((chkPtNext.GetComponent<PVPcheckPoint>().state != CheckPointState.Titan) || (UnityEngine.Random.Range(0, 100) < 20)))
                                            {
                                                this.toPVPCheckPoint(chkPtNext.transform.position, (float) (5 + UnityEngine.Random.Range(0, 10)));
                                                this.PVPfromCheckPt = chkPtNext.GetComponent<PVPcheckPoint>();
                                            }
                                        }
                                        else
                                        {
                                            chkPtNext = this.PVPfromCheckPt.chkPtPrevious;
                                            if ((chkPtNext != null) && ((chkPtNext.GetComponent<PVPcheckPoint>().state != CheckPointState.Titan) || (UnityEngine.Random.Range(0, 100) < 5)))
                                            {
                                                this.toPVPCheckPoint(chkPtNext.transform.position, (float) (5 + UnityEngine.Random.Range(0, 10)));
                                                this.PVPfromCheckPt = chkPtNext.GetComponent<PVPcheckPoint>();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        this.toPVPCheckPoint(this.PVPfromCheckPt.transform.position, (float) (5 + UnityEngine.Random.Range(0, 10)));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (this.state == TitanState.attack)
                {
                    if (this.attackAnimation == "combo")
                    {
                        if (this.nonAI)
                        {
                            if (this.controller.isAttackDown)
                            {
                                this.nonAIcombo = true;
                            }
                            if (!this.nonAIcombo && (base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.385f))
                            {
                                this.idle(0f);
                                return;
                            }
                        }
                        if ((base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.11f) && (base.animation["attack_" + this.attackAnimation].normalizedTime <= 0.16f))
                        {
                            GameObject obj7 = this.checkIfHitHand(base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001"));
                            if (obj7 != null)
                            {
                                Vector3 position = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                {
                                    obj7.GetComponent<HERO>().die((Vector3) (((obj7.transform.position - position) * 15f) * this.myLevel), false);
                                }
                                else if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine) && !obj7.GetComponent<HERO>().HasDied())
                                {
                                    obj7.GetComponent<HERO>().markDie();
                                    object[] objArray2 = new object[] { (Vector3) (((obj7.transform.position - position) * 15f) * this.myLevel), false, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                    obj7.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, objArray2);
                                }
                            }
                        }
                        if ((base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.27f) && (base.animation["attack_" + this.attackAnimation].normalizedTime <= 0.32f))
                        {
                            GameObject obj8 = this.checkIfHitHand(base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001"));
                            if (obj8 != null)
                            {
                                Vector3 vector3 = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                {
                                    obj8.GetComponent<HERO>().die((Vector3) (((obj8.transform.position - vector3) * 15f) * this.myLevel), false);
                                }
                                else if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine) && !obj8.GetComponent<HERO>().HasDied())
                                {
                                    obj8.GetComponent<HERO>().markDie();
                                    object[] objArray3 = new object[] { (Vector3) (((obj8.transform.position - vector3) * 15f) * this.myLevel), false, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                    obj8.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, objArray3);
                                }
                            }
                        }
                    }
                    if (((this.attackCheckTimeA != 0f) && (base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA)) && (base.animation["attack_" + this.attackAnimation].normalizedTime <= this.attackCheckTimeB))
                    {
                        if (this.leftHandAttack)
                        {
                            GameObject obj9 = this.checkIfHitHand(base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001"));
                            if (obj9 != null)
                            {
                                Vector3 vector4 = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                {
                                    obj9.GetComponent<HERO>().die((Vector3) (((obj9.transform.position - vector4) * 15f) * this.myLevel), false);
                                }
                                else if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine) && !obj9.GetComponent<HERO>().HasDied())
                                {
                                    obj9.GetComponent<HERO>().markDie();
                                    object[] objArray4 = new object[] { (Vector3) (((obj9.transform.position - vector4) * 15f) * this.myLevel), false, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                    obj9.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, objArray4);
                                }
                            }
                        }
                        else
                        {
                            GameObject obj10 = this.checkIfHitHand(base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001"));
                            if (obj10 != null)
                            {
                                Vector3 vector5 = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                {
                                    obj10.GetComponent<HERO>().die((Vector3) (((obj10.transform.position - vector5) * 15f) * this.myLevel), false);
                                }
                                else if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine) && !obj10.GetComponent<HERO>().HasDied())
                                {
                                    obj10.GetComponent<HERO>().markDie();
                                    object[] objArray5 = new object[] { (Vector3) (((obj10.transform.position - vector5) * 15f) * this.myLevel), false, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                    obj10.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, objArray5);
                                }
                            }
                        }
                    }
                    if ((!this.attacked && (this.attackCheckTime != 0f)) && (base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTime))
                    {
                        GameObject obj11;
                        this.attacked = true;
                        this.fxPosition = base.transform.Find("ap_" + this.attackAnimation).position;
                        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
                        {
                            obj11 = PhotonNetwork.Instantiate("FX/" + this.fxName, this.fxPosition, this.fxRotation, 0);
                        }
                        else
                        {
                            obj11 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/" + this.fxName), this.fxPosition, this.fxRotation);
                        }
                        if (this.nonAI)
                        {
                            obj11.transform.localScale = (Vector3) (base.transform.localScale * 1.5f);
                            if (obj11.GetComponent<EnemyfxIDcontainer>() != null)
                            {
                                obj11.GetComponent<EnemyfxIDcontainer>().myOwnerViewID = base.photonView.viewID;
                            }
                        }
                        else
                        {
                            obj11.transform.localScale = base.transform.localScale;
                        }
                        if (obj11.GetComponent<EnemyfxIDcontainer>() != null)
                        {
                            obj11.GetComponent<EnemyfxIDcontainer>().titanName = base.name;
                        }
                        float b = 1f - (Vector3.Distance(this.currentCamera.transform.position, obj11.transform.position) * 0.05f);
                        b = Mathf.Min(1f, b);
                        this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().startShake(b, b, 0.95f);
                    }
                    if (this.attackAnimation == "throw")
                    {
                        if (!this.attacked && (base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.11f))
                        {
                            this.attacked = true;
                            Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
                            {
                                this.throwRock = PhotonNetwork.Instantiate("FX/rockThrow", transform.position, transform.rotation, 0);
                            }
                            else
                            {
                                this.throwRock = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/rockThrow"), transform.position, transform.rotation);
                            }
                            this.throwRock.transform.localScale = base.transform.localScale;
                            Transform transform1 = this.throwRock.transform;
                            transform1.position -= (Vector3) ((this.throwRock.transform.forward * 2.5f) * this.myLevel);
                            if (this.throwRock.GetComponent<EnemyfxIDcontainer>() != null)
                            {
                                if (this.nonAI)
                                {
                                    this.throwRock.GetComponent<EnemyfxIDcontainer>().myOwnerViewID = base.photonView.viewID;
                                }
                                this.throwRock.GetComponent<EnemyfxIDcontainer>().titanName = base.name;
                            }
                            this.throwRock.transform.parent = transform;
                            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
                            {
                                object[] objArray6 = new object[] { base.photonView.viewID, base.transform.localScale, this.throwRock.transform.localPosition, this.myLevel };
                                this.throwRock.GetPhotonView().RPC("initRPC", PhotonTargets.Others, objArray6);
                            }
                        }
                        if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.11f)
                        {
                            float y = Mathf.Atan2(this.myHero.transform.position.x - base.transform.position.x, this.myHero.transform.position.z - base.transform.position.z) * 57.29578f;
                            base.gameObject.transform.rotation = Quaternion.Euler(0f, y, 0f);
                        }
                        if ((this.throwRock != null) && (base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.62f))
                        {
                            Vector3 vector6;
                            float num3 = 1f;
                            float num4 = -20f;
                            if (this.myHero != null)
                            {
                                vector6 = ((Vector3) ((this.myHero.transform.position - this.throwRock.transform.position) / num3)) + this.myHero.rigidbody.velocity;
                                float num5 = this.myHero.transform.position.y + (2f * this.myLevel);
                                float num6 = num5 - this.throwRock.transform.position.y;
                                vector6 = new Vector3(vector6.x, (num6 / num3) - ((0.5f * num4) * num3), vector6.z);
                            }
                            else
                            {
                                vector6 = (Vector3) ((base.transform.forward * 60f) + (Vector3.up * 10f));
                            }
                            this.throwRock.GetComponent<RockThrow>().launch(vector6);
                            this.throwRock.transform.parent = null;
                            this.throwRock = null;
                        }
                    }
                    if ((this.attackAnimation == "jumper_0") || (this.attackAnimation == "crawler_jump_0"))
                    {
                        if (!this.attacked)
                        {
                            if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.68f)
                            {
                                this.attacked = true;
                                if ((this.myHero == null) || this.nonAI)
                                {
                                    float num7 = 120f;
                                    Vector3 vector7 = (Vector3) ((base.transform.forward * this.speed) + (Vector3.up * num7));
                                    if (this.nonAI && (this.abnormalType == AbnormalType.TYPE_CRAWLER))
                                    {
                                        num7 = 100f;
                                        float a = this.speed * 2.5f;
                                        a = Mathf.Min(a, 100f);
                                        vector7 = (Vector3) ((base.transform.forward * a) + (Vector3.up * num7));
                                    }
                                    base.rigidbody.velocity = vector7;
                                }
                                else
                                {
                                    float num18;
                                    float num9 = this.myHero.rigidbody.velocity.y;
                                    float num10 = -20f;
                                    float gravity = this.gravity;
                                    float num12 = this.neck.position.y;
                                    float num13 = (num10 - gravity) * 0.5f;
                                    float num14 = num9;
                                    float num15 = this.myHero.transform.position.y - num12;
                                    float num16 = Mathf.Abs((float) ((Mathf.Sqrt((num14 * num14) - ((4f * num13) * num15)) - num14) / (2f * num13)));
                                    Vector3 vector8 = (Vector3) ((this.myHero.transform.position + (this.myHero.rigidbody.velocity * num16)) + ((((Vector3.up * 0.5f) * num10) * num16) * num16));
                                    float num17 = vector8.y;
                                    if ((num15 < 0f) || ((num17 - num12) < 0f))
                                    {
                                        num18 = 60f;
                                        float num19 = this.speed * 2.5f;
                                        num19 = Mathf.Min(num19, 100f);
                                        Vector3 vector9 = (Vector3) ((base.transform.forward * num19) + (Vector3.up * num18));
                                        base.rigidbody.velocity = vector9;
                                        return;
                                    }
                                    float num20 = num17 - num12;
                                    float num21 = Mathf.Sqrt((2f * num20) / this.gravity);
                                    num18 = this.gravity * num21;
                                    num18 = Mathf.Max(30f, num18);
                                    Vector3 vector10 = (Vector3) ((vector8 - base.transform.position) / num16);
                                    this.abnorma_jump_bite_horizon_v = new Vector3(vector10.x, 0f, vector10.z);
                                    Vector3 velocity = base.rigidbody.velocity;
                                    Vector3 force = new Vector3(this.abnorma_jump_bite_horizon_v.x, velocity.y, this.abnorma_jump_bite_horizon_v.z) - velocity;
                                    base.rigidbody.AddForce(force, ForceMode.VelocityChange);
                                    base.rigidbody.AddForce((Vector3) (Vector3.up * num18), ForceMode.VelocityChange);
                                    float num22 = Vector2.Angle(new Vector2(base.transform.position.x, base.transform.position.z), new Vector2(this.myHero.transform.position.x, this.myHero.transform.position.z));
                                    num22 = Mathf.Atan2(this.myHero.transform.position.x - base.transform.position.x, this.myHero.transform.position.z - base.transform.position.z) * 57.29578f;
                                    base.gameObject.transform.rotation = Quaternion.Euler(0f, num22, 0f);
                                }
                            }
                            else
                            {
                                base.rigidbody.velocity = Vector3.zero;
                            }
                        }
                        if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 1f)
                        {
                            Debug.DrawLine(base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head").position + ((Vector3) ((Vector3.up * 1.5f) * this.myLevel)), (Vector3) ((base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head").position + ((Vector3.up * 1.5f) * this.myLevel)) + ((Vector3.up * 3f) * this.myLevel)), Color.green);
                            Debug.DrawLine(base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head").position + ((Vector3) ((Vector3.up * 1.5f) * this.myLevel)), (Vector3) ((base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head").position + ((Vector3.up * 1.5f) * this.myLevel)) + ((Vector3.forward * 3f) * this.myLevel)), Color.green);
                            GameObject obj12 = this.checkIfHitHead(this.head, 3f);
                            if (obj12 != null)
                            {
                                Vector3 vector13 = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                {
                                    obj12.GetComponent<HERO>().die((Vector3) (((obj12.transform.position - vector13) * 15f) * this.myLevel), false);
                                }
                                else if (((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine) && !obj12.GetComponent<HERO>().HasDied())
                                {
                                    obj12.GetComponent<HERO>().markDie();
                                    object[] objArray7 = new object[] { (Vector3) (((obj12.transform.position - vector13) * 15f) * this.myLevel), true, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                    obj12.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, objArray7);
                                }
                                if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                                {
                                    this.attackAnimation = "crawler_jump_1";
                                }
                                else
                                {
                                    this.attackAnimation = "jumper_1";
                                }
                                this.playAnimation("attack_" + this.attackAnimation);
                            }
                            if (((Mathf.Abs(base.rigidbody.velocity.y) < 0.5f) || (base.rigidbody.velocity.y < 0f)) || this.IsGrounded())
                            {
                                if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                                {
                                    this.attackAnimation = "crawler_jump_1";
                                }
                                else
                                {
                                    this.attackAnimation = "jumper_1";
                                }
                                this.playAnimation("attack_" + this.attackAnimation);
                            }
                        }
                    }
                    else if ((this.attackAnimation == "jumper_1") || (this.attackAnimation == "crawler_jump_1"))
                    {
                        if ((base.animation["attack_" + this.attackAnimation].normalizedTime >= 1f) && this.grounded)
                        {
                            GameObject obj13;
                            if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                            {
                                this.attackAnimation = "crawler_jump_2";
                            }
                            else
                            {
                                this.attackAnimation = "jumper_2";
                            }
                            this.crossFade("attack_" + this.attackAnimation, 0.1f);
                            this.fxPosition = base.transform.position;
                            if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
                            {
                                obj13 = PhotonNetwork.Instantiate("FX/boom2", this.fxPosition, this.fxRotation, 0);
                            }
                            else
                            {
                                obj13 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("FX/boom2"), this.fxPosition, this.fxRotation);
                            }
                            obj13.transform.localScale = (Vector3) (base.transform.localScale * 1.6f);
                            float num23 = 1f - (Vector3.Distance(this.currentCamera.transform.position, obj13.transform.position) * 0.05f);
                            num23 = Mathf.Min(1f, num23);
                            this.currentCamera.GetComponent<IN_GAME_MAIN_CAMERA>().startShake(num23, num23, 0.95f);
                        }
                    }
                    else if ((this.attackAnimation == "jumper_2") || (this.attackAnimation == "crawler_jump_2"))
                    {
                        if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 1f)
                        {
                            this.idle(0f);
                        }
                    }
                    else if (base.animation.IsPlaying("tired"))
                    {
                        if (base.animation["tired"].normalizedTime >= (1f + Mathf.Max((float) (this.attackEndWait * 2f), (float) 3f)))
                        {
                            this.idle(UnityEngine.Random.Range((float) (this.attackWait - 1f), (float) 3f));
                        }
                    }
                    else if (base.animation["attack_" + this.attackAnimation].normalizedTime >= (1f + this.attackEndWait))
                    {
                        if (this.nextAttackAnimation != null)
                        {
                            this.attack(this.nextAttackAnimation);
                        }
                        else if ((this.attackAnimation == "quick_turn_l") || (this.attackAnimation == "quick_turn_r"))
                        {
                            base.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x, base.transform.rotation.eulerAngles.y + 180f, base.transform.rotation.eulerAngles.z);
                            this.idle(UnityEngine.Random.Range((float) 0.5f, (float) 1f));
                            this.playAnimation("idle");
                        }
                        else if ((this.abnormalType == AbnormalType.TYPE_I) || (this.abnormalType == AbnormalType.TYPE_JUMPER))
                        {
                            this.attackCount++;
                            if ((this.attackCount > 3) && (this.attackAnimation == "abnormal_getup"))
                            {
                                this.attackCount = 0;
                                this.crossFade("tired", 0.5f);
                            }
                            else
                            {
                                this.idle(UnityEngine.Random.Range((float) (this.attackWait - 1f), (float) 3f));
                            }
                        }
                        else
                        {
                            this.idle(UnityEngine.Random.Range((float) (this.attackWait - 1f), (float) 3f));
                        }
                    }
                }
                else if (this.state == TitanState.grab)
                {
                    if (((base.animation["grab_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA) && (base.animation["grab_" + this.attackAnimation].normalizedTime <= this.attackCheckTimeB)) && (this.grabbedTarget == null))
                    {
                        GameObject grabTarget = this.checkIfHitHand(this.currentGrabHand);
                        if (grabTarget != null)
                        {
                            if (this.isGrabHandLeft)
                            {
                                this.eatSetL(grabTarget);
                                this.grabbedTarget = grabTarget;
                            }
                            else
                            {
                                this.eatSet(grabTarget);
                                this.grabbedTarget = grabTarget;
                            }
                        }
                    }
                    if (base.animation["grab_" + this.attackAnimation].normalizedTime >= 1f)
                    {
                        if (this.grabbedTarget != null)
                        {
                            this.eat();
                        }
                        else
                        {
                            this.idle(UnityEngine.Random.Range((float) (this.attackWait - 1f), (float) 2f));
                        }
                    }
                }
                else if (this.state == TitanState.eat)
                {
                    if (!this.attacked && (base.animation[this.attackAnimation].normalizedTime >= 0.48f))
                    {
                        this.attacked = true;
                        this.justEatHero(this.grabbedTarget, this.currentGrabHand);
                    }
                    if (this.grabbedTarget == null)
                    {
                    }
                    if (base.animation[this.attackAnimation].normalizedTime >= 1f)
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.chase)
                {
                    if (this.myHero == null)
                    {
                        this.idle(0f);
                    }
                    else if (!this.longRangeAttackCheck())
                    {
                        if (((IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.PVP_CAPTURE) && (this.PVPfromCheckPt != null)) && (this.myDistance > this.chaseDistance))
                        {
                            this.idle(0f);
                        }
                        else if (this.abnormalType == AbnormalType.TYPE_CRAWLER)
                        {
                            Vector3 vector14 = this.myHero.transform.position - base.transform.position;
                            float current = -Mathf.Atan2(vector14.z, vector14.x) * 57.29578f;
                            float f = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                            if ((((this.myDistance < (this.attackDistance * 3f)) && (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.1f)) && ((Mathf.Abs(f) < 90f) && (this.myHero.transform.position.y < (this.neck.position.y + (30f * this.myLevel))))) && (this.myHero.transform.position.y > (this.neck.position.y + (10f * this.myLevel))))
                            {
                                this.attack("crawler_jump_0");
                            }
                            else
                            {
                                GameObject obj15 = this.checkIfHitCrawlerMouth(this.head, 2.2f);
                                if (obj15 != null)
                                {
                                    Vector3 vector15 = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
                                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
                                    {
                                        obj15.GetComponent<HERO>().die((Vector3) (((obj15.transform.position - vector15) * 15f) * this.myLevel), false);
                                    }
                                    else if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
                                    {
                                        if (obj15.GetComponent<TITAN_EREN>() != null)
                                        {
                                            obj15.GetComponent<TITAN_EREN>().hitByTitan();
                                        }
                                        else if (!obj15.GetComponent<HERO>().HasDied())
                                        {
                                            obj15.GetComponent<HERO>().markDie();
                                            object[] objArray8 = new object[] { (Vector3) (((obj15.transform.position - vector15) * 15f) * this.myLevel), true, !this.nonAI ? -1 : base.photonView.viewID, base.name, true };
                                            obj15.GetComponent<HERO>().photonView.RPC("netDie", PhotonTargets.All, objArray8);
                                        }
                                    }
                                }
                                if ((this.myDistance < this.attackDistance) && (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.02f))
                                {
                                    this.idle(UnityEngine.Random.Range((float) 0.05f, (float) 0.2f));
                                }
                            }
                        }
                        else if (((this.abnormalType == AbnormalType.TYPE_JUMPER) && (((this.myDistance > this.attackDistance) && (this.myHero.transform.position.y > (this.head.position.y + (4f * this.myLevel)))) || (this.myHero.transform.position.y > (this.head.position.y + (4f * this.myLevel))))) && (Vector3.Distance(base.transform.position, this.myHero.transform.position) < (1.5f * this.myHero.transform.position.y)))
                        {
                            this.attack("jumper_0");
                        }
                        else if (this.myDistance < this.attackDistance)
                        {
                            this.idle(UnityEngine.Random.Range((float) 0.05f, (float) 0.2f));
                        }
                    }
                }
                else if (this.state == TitanState.wander)
                {
                    float num26 = 0f;
                    float num27 = 0f;
                    if ((this.myDistance < this.chaseDistance) || (this.whoHasTauntMe != null))
                    {
                        Vector3 vector16 = this.myHero.transform.position - base.transform.position;
                        num26 = -Mathf.Atan2(vector16.z, vector16.x) * 57.29578f;
                        num27 = -Mathf.DeltaAngle(num26, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                        if (this.isAlarm || (Mathf.Abs(num27) < 90f))
                        {
                            this.chase();
                            return;
                        }
                        if (!this.isAlarm && (this.myDistance < (this.chaseDistance * 0.1f)))
                        {
                            this.chase();
                            return;
                        }
                    }
                    if (UnityEngine.Random.Range((float) 0f, (float) 1f) < 0.01f)
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.turn)
                {
                    base.gameObject.transform.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, this.desDeg, 0f), (Time.deltaTime * Mathf.Abs(this.turnDeg)) * 0.015f);
                    if (base.animation[this.turnAnimation].normalizedTime >= 1f)
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.hit_eye)
                {
                    if (base.animation.IsPlaying("sit_hit_eye") && (base.animation["sit_hit_eye"].normalizedTime >= 1f))
                    {
                        this.remainSitdown();
                    }
                    else if (base.animation.IsPlaying("hit_eye") && (base.animation["hit_eye"].normalizedTime >= 1f))
                    {
                        if (this.nonAI)
                        {
                            this.idle(0f);
                        }
                        else
                        {
                            this.attack("combo_1");
                        }
                    }
                }
                else if (this.state == TitanState.to_check_point)
                {
                    if ((this.checkPoints.Count <= 0) && (this.myDistance < this.attackDistance))
                    {
                        string str2 = string.Empty;
                        string[] strArray2 = this.GetAttackStrategy();
                        if (strArray2 != null)
                        {
                            str2 = strArray2[UnityEngine.Random.Range(0, strArray2.Length)];
                        }
                        if (this.executeAttack(str2))
                        {
                            return;
                        }
                    }
                    if (Vector3.Distance(base.transform.position, this.targetCheckPt) < this.targetR)
                    {
                        if (this.checkPoints.Count > 0)
                        {
                            if (this.checkPoints.Count == 1)
                            {
                                if (IN_GAME_MAIN_CAMERA.gamemode == GAMEMODE.BOSS_FIGHT_CT)
                                {
                                    GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().gameLose();
                                    this.checkPoints = new ArrayList();
                                    this.idle(0f);
                                }
                            }
                            else
                            {
                                if (this.checkPoints.Count == 4)
                                {
                                    GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().sendChatContentInfo("<color=#A8FF24>*WARNING!* An abnormal titan is approaching the north gate!</color>");
                                }
                                Vector3 vector17 = (Vector3) this.checkPoints[0];
                                this.targetCheckPt = vector17;
                                this.checkPoints.RemoveAt(0);
                            }
                        }
                        else
                        {
                            this.idle(0f);
                        }
                    }
                }
                else if (this.state == TitanState.to_pvp_pt)
                {
                    if (this.myDistance < (this.chaseDistance * 0.7f))
                    {
                        this.chase();
                    }
                    if (Vector3.Distance(base.transform.position, this.targetCheckPt) < this.targetR)
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.random_run)
                {
                    this.random_run_time -= Time.deltaTime;
                    if ((Vector3.Distance(base.transform.position, this.targetCheckPt) < this.targetR) || (this.random_run_time <= 0f))
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.down)
                {
                    this.getdownTime -= Time.deltaTime;
                    if (base.animation.IsPlaying("sit_hunt_down") && (base.animation["sit_hunt_down"].normalizedTime >= 1f))
                    {
                        this.playAnimation("sit_idle");
                    }
                    if (this.getdownTime <= 0f)
                    {
                        this.crossFade("sit_getup", 0.1f);
                    }
                    if (base.animation.IsPlaying("sit_getup") && (base.animation["sit_getup"].normalizedTime >= 1f))
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.sit)
                {
                    this.getdownTime -= Time.deltaTime;
                    this.angle = 0f;
                    this.between2 = 0f;
                    if ((this.myDistance < this.chaseDistance) || (this.whoHasTauntMe != null))
                    {
                        if (this.myDistance < 50f)
                        {
                            this.isAlarm = true;
                        }
                        else
                        {
                            Vector3 vector18 = this.myHero.transform.position - base.transform.position;
                            this.angle = -Mathf.Atan2(vector18.z, vector18.x) * 57.29578f;
                            this.between2 = -Mathf.DeltaAngle(this.angle, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                            if (Mathf.Abs(this.between2) < 100f)
                            {
                                this.isAlarm = true;
                            }
                        }
                    }
                    if (base.animation.IsPlaying("sit_down") && (base.animation["sit_down"].normalizedTime >= 1f))
                    {
                        this.playAnimation("sit_idle");
                    }
                    if (((this.getdownTime <= 0f) || this.isAlarm) && base.animation.IsPlaying("sit_idle"))
                    {
                        this.crossFade("sit_getup", 0.1f);
                    }
                    if (base.animation.IsPlaying("sit_getup") && (base.animation["sit_getup"].normalizedTime >= 1f))
                    {
                        this.idle(0f);
                    }
                }
                else if (this.state == TitanState.recover)
                {
                    this.getdownTime -= Time.deltaTime;
                    if (this.getdownTime <= 0f)
                    {
                        this.idle(0f);
                    }
                    if (base.animation.IsPlaying("idle_recovery") && (base.animation["idle_recovery"].normalizedTime >= 1f))
                    {
                        this.idle(0f);
                    }
                }
            }
        }
    }

    private void wander(float sbtime = 0f)
    {
        this.state = TitanState.wander;
        this.crossFade(this.runAnimation, 0.5f);
    }
}

