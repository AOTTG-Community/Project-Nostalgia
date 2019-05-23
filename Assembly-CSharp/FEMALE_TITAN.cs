﻿using Optimization.Caching;
using System.Collections;
using UnityEngine;

public class FEMALE_TITAN : Photon.MonoBehaviour
{
    private Vector3 abnorma_jump_bite_horizon_v;
    private int AnkleLHPMAX = 200;
    private int AnkleRHPMAX = 200;
    private string attackAnimation;
    private float attackCheckTime;
    private float attackCheckTimeA;
    private float attackCheckTimeB;
    private bool attackChkOnce;
    private bool attacked;
    private float attention = 10f;
    private Transform checkHitCapsuleEnd;
    private Vector3 checkHitCapsuleEndOld;
    private float checkHitCapsuleR;
    private Transform checkHitCapsuleStart;
    private Transform currentGrabHand;
    private float desDeg;
    private float dieTime;
    private GameObject eren;
    private string fxName;
    private Vector3 fxPosition;
    private Quaternion fxRotation;
    private GameObject grabbedTarget;
    private float gravity = 120f;
    private bool grounded;
    private bool hasDieSteam;
    private bool isAttackMoveByCore;
    private bool isGrabHandLeft;
    private bool needFreshCorePosition;
    private string nextAttackAnimation;
    private Vector3 oldCorePosition;
    private float sbtime;
    private bool startJump;
    private string state = "idle";
    private int stepSoundPhase = 2;
    private float tauntTime;
    private string turnAnimation;
    private float turnDeg;
    private GameObject whoHasTauntMe;
    public static float minusDistance = 99999f;
    public static GameObject minusDistanceEnemy;
    public int AnkleLHP = 200;
    public int AnkleRHP = 200;
    public float attackDistance = 13f;
    public float attackWait = 1f;
    public GameObject bottomObject;
    public float chaseDistance = 80f;
    public GameObject currentCamera;
    public GameObject grabTF;
    public bool hasDie;

    public float maxVelocityChange = 80f;
    public float myDistance;
    public GameObject myHero;

    public int NapeArmor = 1000;
    public float speed = 80f;

    private void attack(string type)
    {
        this.state = "attack";
        this.attacked = false;
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
        this.startJump = false;
        this.attackChkOnce = false;
        this.nextAttackAnimation = null;
        this.fxName = null;
        this.isAttackMoveByCore = false;
        this.attackCheckTime = 0f;
        this.attackCheckTimeA = 0f;
        this.attackCheckTimeB = 0f;
        this.fxRotation = Quaternion.Euler(270f, 0f, 0f);
        switch (type)
        {
            case "combo_1":
                this.attackCheckTimeA = 0.63f;
                this.attackCheckTimeB = 0.8f;
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_R/shin_R/foot_R");
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_R");
                this.checkHitCapsuleR = 5f;
                this.isAttackMoveByCore = true;
                this.nextAttackAnimation = "combo_2";
                break;

            case "combo_2":
                this.attackCheckTimeA = 0.27f;
                this.attackCheckTimeB = 0.43f;
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_L/shin_L/foot_L");
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_L");
                this.checkHitCapsuleR = 5f;
                this.isAttackMoveByCore = true;
                this.nextAttackAnimation = "combo_3";
                break;

            case "combo_3":
                this.attackCheckTimeA = 0.15f;
                this.attackCheckTimeB = 0.3f;
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_R/shin_R/foot_R");
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_R");
                this.checkHitCapsuleR = 5f;
                this.isAttackMoveByCore = true;
                break;

            case "combo_blind_1":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.72f;
                this.attackCheckTimeB = 0.83f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                this.nextAttackAnimation = "combo_blind_2";
                break;

            case "combo_blind_2":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.5f;
                this.attackCheckTimeB = 0.6f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                this.nextAttackAnimation = "combo_blind_3";
                break;

            case "combo_blind_3":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.2f;
                this.attackCheckTimeB = 0.28f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                break;

            case "front":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.44f;
                this.attackCheckTimeB = 0.55f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                break;

            case "jumpCombo_1":
                this.isAttackMoveByCore = false;
                this.nextAttackAnimation = "jumpCombo_2";
                this.abnorma_jump_bite_horizon_v = Vectors.zero;
                break;

            case "jumpCombo_2":
                this.isAttackMoveByCore = false;
                this.attackCheckTimeA = 0.48f;
                this.attackCheckTimeB = 0.7f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                this.nextAttackAnimation = "jumpCombo_3";
                break;

            case "jumpCombo_3":
                this.isAttackMoveByCore = false;
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_L/shin_L/foot_L");
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_L");
                this.checkHitCapsuleR = 5f;
                this.attackCheckTimeA = 0.22f;
                this.attackCheckTimeB = 0.42f;
                break;

            case "jumpCombo_4":
                this.isAttackMoveByCore = false;
                break;

            case "sweep":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.39f;
                this.attackCheckTimeB = 0.6f;
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_R/shin_R/foot_R");
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/thigh_R");
                this.checkHitCapsuleR = 5f;
                break;

            case "sweep_back":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.41f;
                this.attackCheckTimeB = 0.48f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                break;

            case "sweep_front_left":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.53f;
                this.attackCheckTimeB = 0.63f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                break;

            case "sweep_front_right":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.5f;
                this.attackCheckTimeB = 0.62f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001");
                this.checkHitCapsuleR = 4f;
                break;

            case "sweep_head_b_l":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.4f;
                this.attackCheckTimeB = 0.51f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001");
                this.checkHitCapsuleR = 4f;
                break;

            case "sweep_head_b_r":
                this.isAttackMoveByCore = true;
                this.attackCheckTimeA = 0.4f;
                this.attackCheckTimeB = 0.51f;
                this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R");
                this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
                this.checkHitCapsuleR = 4f;
                break;
        }
        this.checkHitCapsuleEndOld = this.checkHitCapsuleEnd.transform.position;
        this.needFreshCorePosition = true;
    }

    private bool attackTarget(GameObject target)
    {
        Vector3 vector = target.transform.position - base.transform.position;
        float current = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
        float num = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
        if (this.eren != null && this.myDistance < 35f)
        {
            this.attack("combo_1");
            return true;
        }
        string text = string.Empty;
        ArrayList arrayList = new ArrayList();
        if (this.myDistance < 40f)
        {
            int num2;
            if (Mathf.Abs(num) < 90f)
            {
                if (num > 0f)
                {
                    num2 = 1;
                }
                else
                {
                    num2 = 2;
                }
            }
            else if (num > 0f)
            {
                num2 = 4;
            }
            else
            {
                num2 = 3;
            }
            float num3 = target.transform.position.y - base.transform.position.y;
            if (Mathf.Abs(num) < 90f)
            {
                if (num3 > 0f && num3 < 12f && this.myDistance < 22f)
                {
                    arrayList.Add("attack_sweep");
                }
                if (num3 >= 55f && num3 < 90f)
                {
                    arrayList.Add("attack_jumpCombo_1");
                }
            }
            if (Mathf.Abs(num) < 90f && num3 > 12f && num3 < 40f)
            {
                arrayList.Add("attack_combo_1");
            }
            if (Mathf.Abs(num) < 30f)
            {
                if (num3 > 0f && num3 < 12f && this.myDistance > 20f && this.myDistance < 30f)
                {
                    arrayList.Add("attack_front");
                }
                if (this.myDistance < 12f && num3 > 33f && num3 < 51f)
                {
                    arrayList.Add("grab_up");
                }
            }
            if (Mathf.Abs(num) > 100f && this.myDistance < 11f && num3 >= 15f && num3 < 32f)
            {
                arrayList.Add("attack_sweep_back");
            }
            int num4 = num2;
            switch (num4)
            {
                case 1:
                    if (this.myDistance < 11f)
                    {
                        if (num3 >= 21f && num3 < 32f)
                        {
                            arrayList.Add("attack_sweep_front_right");
                        }
                    }
                    else if (this.myDistance < 20f)
                    {
                        if (num3 >= 12f && num3 < 21f)
                        {
                            arrayList.Add("grab_bottom_right");
                        }
                        else if (num3 >= 21f && num3 < 32f)
                        {
                            arrayList.Add("grab_mid_right");
                        }
                        else if (num3 >= 32f && num3 < 47f)
                        {
                            arrayList.Add("grab_up_right");
                        }
                    }
                    break;

                case 2:
                    if (this.myDistance < 11f)
                    {
                        if (num3 >= 21f && num3 < 32f)
                        {
                            arrayList.Add("attack_sweep_front_left");
                        }
                    }
                    else if (this.myDistance < 20f)
                    {
                        if (num3 >= 12f && num3 < 21f)
                        {
                            arrayList.Add("grab_bottom_left");
                        }
                        else if (num3 >= 21f && num3 < 32f)
                        {
                            arrayList.Add("grab_mid_left");
                        }
                        else if (num3 >= 32f && num3 < 47f)
                        {
                            arrayList.Add("grab_up_left");
                        }
                    }
                    break;

                case 3:
                    if (this.myDistance < 11f)
                    {
                        if (num3 >= 33f && num3 < 51f)
                        {
                            arrayList.Add("attack_sweep_head_b_l");
                        }
                    }
                    else
                    {
                        arrayList.Add("turn180");
                    }
                    break;

                case 4:
                    if (this.myDistance < 11f)
                    {
                        if (num3 >= 33f && num3 < 51f)
                        {
                            arrayList.Add("attack_sweep_head_b_r");
                        }
                    }
                    else
                    {
                        arrayList.Add("turn180");
                    }
                    break;
            }
        }
        if (arrayList.Count > 0)
        {
            text = (string)arrayList[UnityEngine.Random.Range(0, arrayList.Count)];
        }
        else if (UnityEngine.Random.Range(0, 100) < 10)
        {
            GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
            this.myHero = array[UnityEngine.Random.Range(0, array.Length)];
            this.attention = UnityEngine.Random.Range(5f, 10f);
            return true;
        }
        string text2 = text;
        switch (text2)
        {
            case "grab_bottom_left":
                this.grab("bottom_left");
                return true;

            case "grab_bottom_right":
                this.grab("bottom_right");
                return true;

            case "grab_mid_left":
                this.grab("mid_left");
                return true;

            case "grab_mid_right":
                this.grab("mid_right");
                return true;

            case "grab_up":
                this.grab("up");
                return true;

            case "grab_up_left":
                this.grab("up_left");
                return true;

            case "grab_up_right":
                this.grab("up_right");
                return true;

            case "attack_combo_1":
                this.attack("combo_1");
                return true;

            case "attack_front":
                this.attack("front");
                return true;

            case "attack_jumpCombo_1":
                this.attack("jumpCombo_1");
                return true;

            case "attack_sweep":
                this.attack("sweep");
                return true;

            case "attack_sweep_back":
                this.attack("sweep_back");
                return true;

            case "attack_sweep_front_left":
                this.attack("sweep_front_left");
                return true;

            case "attack_sweep_front_right":
                this.attack("sweep_front_right");
                return true;

            case "attack_sweep_head_b_l":
                this.attack("sweep_head_b_l");
                return true;

            case "attack_sweep_head_b_r":
                this.attack("sweep_head_b_r");
                return true;

            case "turn180":
                this.turn180();
                return true;
        }
        return false;
    }

    private void Awake()
    {
        base.rigidbody.freezeRotation = true;
        base.rigidbody.useGravity = false;
    }

    private void chase()
    {
        this.state = "chase";
        this.crossFade("run", 0.5f);
    }

    private RaycastHit[] checkHitCapsule(Vector3 start, Vector3 end, float r)
    {
        return Physics.SphereCastAll(start, r, end - start, Vector3.Distance(start, end));
    }

    private GameObject checkIfHitHand(Transform hand)
    {
        float num = 9.6f;
        Collider[] array = Physics.OverlapSphere(hand.GetComponent<SphereCollider>().transform.position, num + 1f);
        foreach (Collider collider in array)
        {
            if (collider.transform.root.CompareTag("Player"))
            {
                GameObject gameObject = collider.transform.root.gameObject;
                if (gameObject.GetComponent<TITAN_EREN>())
                {
                    if (!gameObject.GetComponent<TITAN_EREN>().isHit)
                    {
                        gameObject.GetComponent<TITAN_EREN>().hitByTitan();
                    }
                    return gameObject;
                }
                if (gameObject.GetComponent<HERO>() && !gameObject.GetComponent<HERO>().IsInvincible())
                {
                    return gameObject;
                }
            }
        }
        return null;
    }

    private GameObject checkIfHitHead(Transform head, float rad)
    {
        float num = rad * 4f;
        GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject gameObject in array)
        {
            if (!(gameObject.GetComponent<TITAN_EREN>() != null) && !gameObject.GetComponent<HERO>().IsInvincible())
            {
                float num2 = gameObject.GetComponent<CapsuleCollider>().height * 0.5f;
                if (Vector3.Distance(gameObject.transform.position + Vectors.up * num2, head.transform.position + Vectors.up * 1.5f * 4f) < num + num2)
                {
                    return gameObject;
                }
            }
        }
        return null;
    }

    private void crossFade(string aniName, float time)
    {
        base.animation.CrossFade(aniName, time);
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            BasePV.RPC("netCrossFade", PhotonTargets.Others, new object[]
            {
                aniName,
                time
            });
        }
    }

    private void eatSet(GameObject grabTarget)
    {
        if (grabTarget.GetComponent<HERO>().isGrabbed)
        {
            return;
        }
        this.grabToRight();
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            grabTarget.GetPhotonView().RPC("netGrabbed", PhotonTargets.All, new object[]
            {
                BasePV.viewID,
                false
            });
            grabTarget.GetPhotonView().RPC("netPlayAnimation", PhotonTargets.All, new object[]
            {
                "grabbed"
            });
            BasePV.RPC("grabToRight", PhotonTargets.Others, new object[0]);
        }
        else
        {
            grabTarget.GetComponent<HERO>().grabbed(base.gameObject, false);
            grabTarget.GetComponent<HERO>().animation.Play("grabbed");
        }
    }

    private void eatSetL(GameObject grabTarget)
    {
        if (grabTarget.GetComponent<HERO>().isGrabbed)
        {
            return;
        }
        this.grabToLeft();
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            grabTarget.GetPhotonView().RPC("netGrabbed", PhotonTargets.All, new object[]
            {
                BasePV.viewID,
                true
            });
            grabTarget.GetPhotonView().RPC("netPlayAnimation", PhotonTargets.All, new object[]
            {
                "grabbed"
            });
            BasePV.RPC("grabToLeft", PhotonTargets.Others, new object[0]);
        }
        else
        {
            grabTarget.GetComponent<HERO>().grabbed(base.gameObject, true);
            grabTarget.GetComponent<HERO>().animation.Play("grabbed");
        }
    }

    private void findNearestFacingHero()
    {
        GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
        GameObject x = null;
        float num = float.PositiveInfinity;
        Vector3 position = base.transform.position;
        float num2 = 180f;
        foreach (GameObject gameObject in array)
        {
            float sqrMagnitude = (gameObject.transform.position - position).sqrMagnitude;
            if (sqrMagnitude < num)
            {
                Vector3 vector = gameObject.transform.position - base.transform.position;
                float current = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
                float f = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                if (Mathf.Abs(f) < num2)
                {
                    x = gameObject;
                    num = sqrMagnitude;
                }
            }
        }
        if (x != null)
        {
            this.myHero = x;
            this.tauntTime = 5f;
        }
    }

    private void findNearestHero()
    {
        this.myHero = this.getNearestHero();
        this.attention = UnityEngine.Random.Range(5f, 10f);
    }

    private void FixedUpdate()
    {
        if (IN_GAME_MAIN_CAMERA.isPausing && IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            return;
        }
        if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
        {
            if (!BasePV.IsMine)
            {
                return;
            }
        }
        if (this.bottomObject.GetComponent<CheckHitGround>().isGrounded)
        {
            this.grounded = true;
            this.bottomObject.GetComponent<CheckHitGround>().isGrounded = false;
        }
        else
        {
            this.grounded = false;
        }
        if (this.needFreshCorePosition)
        {
            this.oldCorePosition = base.transform.position - base.transform.Find("Amarture/Core").position;
            this.needFreshCorePosition = false;
        }
        if ((this.state == "attack" && this.isAttackMoveByCore) || this.state == "hit" || this.state == "turn180" || this.state == "anklehurt")
        {
            Vector3 a = base.transform.position - base.transform.Find("Amarture/Core").position - this.oldCorePosition;
            base.rigidbody.velocity = a / Time.deltaTime + Vectors.up * base.rigidbody.velocity.y;
            this.oldCorePosition = base.transform.position - base.transform.Find("Amarture/Core").position;
        }
        else if (this.state == "chase")
        {
            if (this.myHero == null)
            {
                return;
            }
            Vector3 a2 = base.transform.Forward() * this.speed;
            Vector3 velocity = base.rigidbody.velocity;
            Vector3 force = a2 - velocity;
            force.y = 0f;
            base.rigidbody.AddForce(force, ForceMode.VelocityChange);
            Vector3 vector = this.myHero.transform.position - base.transform.position;
            float current = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
            float num = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
            base.gameObject.transform.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, base.gameObject.transform.rotation.eulerAngles.y + num, 0f), this.speed * Time.deltaTime);
        }
        else if (this.grounded && !base.animation.IsPlaying("attack_jumpCombo_1"))
        {
            base.rigidbody.AddForce(new Vector3(-base.rigidbody.velocity.x, 0f, -base.rigidbody.velocity.z), ForceMode.VelocityChange);
        }
        base.rigidbody.AddForce(new Vector3(0f, -this.gravity * base.rigidbody.mass, 0f));
    }

    private void getDown()
    {
        this.state = "anklehurt";
        this.playAnimation("legHurt");
        this.AnkleRHP = this.AnkleRHPMAX;
        this.AnkleLHP = this.AnkleLHPMAX;
        this.needFreshCorePosition = true;
    }

    private GameObject getNearestHero()
    {
        GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
        GameObject result = null;
        float num = float.PositiveInfinity;
        Vector3 position = base.transform.position;
        foreach (GameObject gameObject in array)
        {
            if (!gameObject.GetComponent<HERO>() || !gameObject.GetComponent<HERO>().HasDied())
            {
                if (!gameObject.GetComponent<TITAN_EREN>() || !gameObject.GetComponent<TITAN_EREN>().hasDied)
                {
                    float sqrMagnitude = (gameObject.transform.position - position).sqrMagnitude;
                    if (sqrMagnitude < num)
                    {
                        result = gameObject;
                        num = sqrMagnitude;
                    }
                }
            }
        }
        return result;
    }

    private float getNearestHeroDistance()
    {
        GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
        float num = float.PositiveInfinity;
        Vector3 position = base.transform.position;
        foreach (GameObject gameObject in array)
        {
            float magnitude = (gameObject.transform.position - position).magnitude;
            if (magnitude < num)
            {
                num = magnitude;
            }
        }
        return num;
    }

    private void grab(string type)
    {
        this.state = "grab";
        this.attacked = false;
        this.attackAnimation = type;
        if (base.animation.IsPlaying("attack_grab_" + type))
        {
            base.animation["attack_grab_" + type].normalizedTime = 0f;
            this.playAnimation("attack_grab_" + type);
        }
        else
        {
            this.crossFade("attack_grab_" + type, 0.1f);
        }
        this.isGrabHandLeft = true;
        this.grabbedTarget = null;
        this.attackCheckTime = 0f;
        switch (type)
        {
            case "bottom_left":
                this.attackCheckTimeA = 0.28f;
                this.attackCheckTimeB = 0.38f;
                this.attackCheckTime = 0.65f;
                this.isGrabHandLeft = false;
                break;

            case "bottom_right":
                this.attackCheckTimeA = 0.27f;
                this.attackCheckTimeB = 0.37f;
                this.attackCheckTime = 0.65f;
                break;

            case "mid_left":
                this.attackCheckTimeA = 0.27f;
                this.attackCheckTimeB = 0.37f;
                this.attackCheckTime = 0.65f;
                this.isGrabHandLeft = false;
                break;

            case "mid_right":
                this.attackCheckTimeA = 0.27f;
                this.attackCheckTimeB = 0.36f;
                this.attackCheckTime = 0.66f;
                break;

            case "up":
                this.attackCheckTimeA = 0.25f;
                this.attackCheckTimeB = 0.32f;
                this.attackCheckTime = 0.67f;
                break;

            case "up_left":
                this.attackCheckTimeA = 0.26f;
                this.attackCheckTimeB = 0.4f;
                this.attackCheckTime = 0.66f;
                break;

            case "up_right":
                this.attackCheckTimeA = 0.26f;
                this.attackCheckTimeB = 0.4f;
                this.attackCheckTime = 0.66f;
                this.isGrabHandLeft = false;
                break;
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

    private void idle(float sbtime = 0f)
    {
        this.sbtime = sbtime;
        this.sbtime = Mathf.Max(0.5f, this.sbtime);
        this.state = "idle";
        this.crossFade("idle", 0.2f);
    }

    private void justEatHero(GameObject target, Transform hand)
    {
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            if (!target.GetComponent<HERO>().HasDied())
            {
                target.GetComponent<HERO>().markDie();
                target.GetComponent<HERO>().BasePV.RPC("netDie2", PhotonTargets.All, new object[]
                {
                    -1,
                    "Female Titan"
                });
            }
        }
        else if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            target.GetComponent<HERO>().die2(hand);
        }
    }

    private void justHitEye()
    {
        this.attack("combo_blind_1");
    }

    private void killPlayer(GameObject hitHero)
    {
        if (hitHero != null)
        {
            Vector3 position = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest").position;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                if (!hitHero.GetComponent<HERO>().HasDied())
                {
                    hitHero.GetComponent<HERO>().die((hitHero.transform.position - position) * 15f * 4f, false);
                }
            }
            else if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient && !hitHero.GetComponent<HERO>().HasDied())
            {
                hitHero.GetComponent<HERO>().markDie();
                hitHero.GetComponent<HERO>().BasePV.RPC("netDie", PhotonTargets.All, new object[]
                {
                    (hitHero.transform.position - position) * 15f * 4f,
                    false,
                    -1,
                    "Female Titan",
                    true
                });
            }
        }
    }

    [RPC]
    private void netCrossFade(string aniName, float time)
    {
        base.animation.CrossFade(aniName, time);
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

    private void OnDestroy()
    {
        if (FengGameManagerMKII.FGM != null)
        {
            FengGameManagerMKII.FGM.RemoveFT(this);
        }
    }

    private void playAnimation(string aniName)
    {
        base.animation.Play(aniName);
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            BasePV.RPC("netPlayAnimation", PhotonTargets.Others, new object[]
            {
                aniName
            });
        }
    }

    private void playAnimationAt(string aniName, float normalizedTime)
    {
        base.animation.Play(aniName);
        base.animation[aniName].normalizedTime = normalizedTime;
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            BasePV.RPC("netPlayAnimationAt", PhotonTargets.Others, new object[]
            {
                aniName,
                normalizedTime
            });
        }
    }

    private void playSound(string sndname)
    {
        this.playsoundRPC(sndname);
        if (Network.peerType == NetworkPeerType.Server)
        {
            BasePV.RPC("playsoundRPC", PhotonTargets.Others, new object[]
            {
                sndname
            });
        }
    }

    [RPC]
    private void playsoundRPC(string sndname)
    {
        Transform transform = base.transform.Find(sndname);
        transform.GetComponent<AudioSource>().Play();
    }

    private void Start()
    {
        FengGameManagerMKII.FGM.AddFT(this);
        base.name = "Female Titan";
        this.grabTF = new GameObject();
        this.grabTF.name = "titansTmpGrabTF";
        this.oldCorePosition = base.transform.position - base.transform.Find("Amarture/Core").position;
        if (this.myHero == null)
        {
            this.findNearestHero();
        }
        foreach (object obj in base.animation)
        {
            AnimationState animationState = (AnimationState)obj;
            animationState.speed = 0.7f;
        }
        base.animation["turn180"].speed = 0.5f;
        this.NapeArmor = 1000;
        this.AnkleLHP = 50;
        this.AnkleRHP = 50;
        this.AnkleLHPMAX = 50;
        this.AnkleRHPMAX = 50;
        bool flag = false;
        if (FengGameManagerMKII.Level.RespawnMode == RespawnMode.NEVER)
        {
            flag = true;
        }
        if (IN_GAME_MAIN_CAMERA.difficulty == 0)
        {
            this.NapeArmor = ((!flag) ? 1000 : 1000);
            this.AnkleLHP = (this.AnkleLHPMAX = ((!flag) ? 50 : 50));
            this.AnkleRHP = (this.AnkleRHPMAX = ((!flag) ? 50 : 50));
        }
        else if (IN_GAME_MAIN_CAMERA.difficulty == 1)
        {
            this.NapeArmor = ((!flag) ? 3000 : 2500);
            this.AnkleLHP = (this.AnkleLHPMAX = ((!flag) ? 200 : 100));
            this.AnkleRHP = (this.AnkleRHPMAX = ((!flag) ? 200 : 100));
            foreach (object obj2 in base.animation)
            {
                AnimationState animationState2 = (AnimationState)obj2;
                animationState2.speed = 0.7f;
            }
            base.animation["turn180"].speed = 0.7f;
        }
        else if (IN_GAME_MAIN_CAMERA.difficulty == 2)
        {
            this.NapeArmor = ((!flag) ? 6000 : 4000);
            this.AnkleLHP = (this.AnkleLHPMAX = ((!flag) ? 1000 : 200));
            this.AnkleRHP = (this.AnkleRHPMAX = ((!flag) ? 1000 : 200));
            foreach (object obj3 in base.animation)
            {
                AnimationState animationState3 = (AnimationState)obj3;
                animationState3.speed = 1f;
            }
            base.animation["turn180"].speed = 0.9f;
        }
        if (IN_GAME_MAIN_CAMERA.GameMode == GameMode.PVP_CAPTURE)
        {
            this.NapeArmor = (int)((float)this.NapeArmor * 0.8f);
        }
        base.animation["legHurt"].speed = 1f;
        base.animation["legHurt_loop"].speed = 1f;
        base.animation["legHurt_getup"].speed = 1f;
    }

    private void turn(float d)
    {
        if (d > 0f)
        {
            this.turnAnimation = "turnaround1";
        }
        else
        {
            this.turnAnimation = "turnaround2";
        }
        this.playAnimation(this.turnAnimation);
        base.animation[this.turnAnimation].time = 0f;
        d = Mathf.Clamp(d, -120f, 120f);
        this.turnDeg = d;
        this.desDeg = base.gameObject.transform.rotation.eulerAngles.y + this.turnDeg;
        this.state = "turn";
    }

    private void turn180()
    {
        this.turnAnimation = "turn180";
        this.playAnimation(this.turnAnimation);
        base.animation[this.turnAnimation].time = 0f;
        this.state = "turn180";
        this.needFreshCorePosition = true;
    }

    public void beTauntedBy(GameObject target, float tauntTime)
    {
        this.whoHasTauntMe = target;
        this.tauntTime = tauntTime;
    }

    public void erenIsHere(GameObject target)
    {
        this.eren = target;
        this.myHero = target;
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
        this.grabTF.transform.parent = transform;
        this.grabTF.transform.position = transform.GetComponent<SphereCollider>().transform.position;
        this.grabTF.transform.rotation = transform.GetComponent<SphereCollider>().transform.rotation;
        this.grabTF.transform.localPosition -= Vectors.right * transform.GetComponent<SphereCollider>().radius * 0.3f;
        this.grabTF.transform.localPosition -= Vectors.up * transform.GetComponent<SphereCollider>().radius * 0.51f;
        this.grabTF.transform.localPosition -= Vectors.forward * transform.GetComponent<SphereCollider>().radius * 0.3f;
        this.grabTF.transform.localRotation = Quaternion.Euler(this.grabTF.transform.localRotation.eulerAngles.x, this.grabTF.transform.localRotation.eulerAngles.y + 180f, this.grabTF.transform.localRotation.eulerAngles.z + 180f);
    }

    [RPC]
    public void grabToRight()
    {
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
        this.grabTF.transform.parent = transform;
        this.grabTF.transform.position = transform.GetComponent<SphereCollider>().transform.position;
        this.grabTF.transform.rotation = transform.GetComponent<SphereCollider>().transform.rotation;
        this.grabTF.transform.localPosition -= Vectors.right * transform.GetComponent<SphereCollider>().radius * 0.3f;
        this.grabTF.transform.localPosition += Vectors.up * transform.GetComponent<SphereCollider>().radius * 0.51f;
        this.grabTF.transform.localPosition -= Vectors.forward * transform.GetComponent<SphereCollider>().radius * 0.3f;
        this.grabTF.transform.localRotation = Quaternion.Euler(this.grabTF.transform.localRotation.eulerAngles.x, this.grabTF.transform.localRotation.eulerAngles.y + 180f, this.grabTF.transform.localRotation.eulerAngles.z);
    }

    public void hit(int dmg)
    {
        this.NapeArmor -= dmg;
        if (this.NapeArmor <= 0)
        {
            this.NapeArmor = 0;
        }
    }

    public void hitAnkleL(int dmg)
    {
        if (this.hasDie)
        {
            return;
        }
        if (this.state == "anklehurt")
        {
            return;
        }
        this.AnkleLHP -= dmg;
        if (this.AnkleLHP <= 0)
        {
            this.getDown();
        }
    }

    [RPC]
    public void hitAnkleLRPC(int viewID, int dmg)
    {
        if (this.hasDie)
        {
            return;
        }
        if (this.state == "anklehurt")
        {
            return;
        }
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }
        if (this.grabbedTarget != null)
        {
            this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
        }
        float magnitude = (photonView.gameObject.transform.position - base.transform.Find("Amarture/Core/Controller_Body").transform.position).magnitude;
        if (magnitude < 20f)
        {
            this.AnkleLHP -= dmg;
            if (this.AnkleLHP <= 0)
            {
                this.getDown();
            }
            FengGameManagerMKII.FGM.SendKillInfo(false, (string)photonView.owner.Properties[PhotonPlayerProperty.name], true, "Female Titan's ankle", dmg);
            FengGameManagerMKII.FGM.BasePV.RPC("netShowDamage", photonView.owner, new object[]
            {
                dmg
            });
        }
    }

    public void hitAnkleR(int dmg)
    {
        if (this.hasDie)
        {
            return;
        }
        if (this.state == "anklehurt")
        {
            return;
        }
        this.AnkleRHP -= dmg;
        if (this.AnkleRHP <= 0)
        {
            this.getDown();
        }
    }

    [RPC]
    public void hitAnkleRRPC(int viewID, int dmg)
    {
        if (this.hasDie)
        {
            return;
        }
        if (this.state == "anklehurt")
        {
            return;
        }
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }
        if (this.grabbedTarget != null)
        {
            this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
        }
        float magnitude = (photonView.gameObject.transform.position - base.transform.Find("Amarture/Core/Controller_Body").transform.position).magnitude;
        if (magnitude < 20f)
        {
            this.AnkleRHP -= dmg;
            if (this.AnkleRHP <= 0)
            {
                this.getDown();
            }
            FengGameManagerMKII.FGM.SendKillInfo(false, (string)photonView.owner.Properties[PhotonPlayerProperty.name], true, "Female Titan's ankle", dmg);
            FengGameManagerMKII.FGM.BasePV.RPC("netShowDamage", photonView.owner, new object[]
            {
                dmg
            });
        }
    }

    public void hitEye()
    {
        if (this.hasDie)
        {
            return;
        }
        this.justHitEye();
    }

    [RPC]
    public void hitEyeRPC(int viewID)
    {
        if (this.hasDie)
        {
            return;
        }
        if (this.grabbedTarget != null)
        {
            this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
        }
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck");
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }
        float magnitude = (photonView.gameObject.transform.position - transform.transform.position).magnitude;
        if (magnitude < 20f)
        {
            this.justHitEye();
        }
    }

    public bool IsGrounded()
    {
        return this.bottomObject.GetComponent<CheckHitGround>().isGrounded;
    }

    public void LateUpdate()
    {
        if (IN_GAME_MAIN_CAMERA.isPausing && IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            return;
        }
        if (base.animation.IsPlaying("run"))
        {
            if (base.animation["run"].normalizedTime % 1f > 0.1f && base.animation["run"].normalizedTime % 1f < 0.6f && this.stepSoundPhase == 2)
            {
                this.stepSoundPhase = 1;
                Transform transform = base.transform.Find("snd_titan_foot");
                transform.GetComponent<AudioSource>().Stop();
                transform.GetComponent<AudioSource>().Play();
            }
            if (base.animation["run"].normalizedTime % 1f > 0.6f && this.stepSoundPhase == 1)
            {
                this.stepSoundPhase = 2;
                Transform transform2 = base.transform.Find("snd_titan_foot");
                transform2.GetComponent<AudioSource>().Stop();
                transform2.GetComponent<AudioSource>().Play();
            }
        }
        if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
        {
            if (!BasePV.IsMine)
            {
                return;
            }
        }
    }

    [RPC]
    public void netDie()
    {
        if (this.hasDie)
        {
            return;
        }
        this.hasDie = true;
        this.crossFade("die", 0.05f);
    }

    [RPC]
    public void titanGetHit(int viewID, int speed)
    {
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck");
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }
        float magnitude = (photonView.gameObject.transform.position - transform.transform.position).magnitude;
        if (magnitude < 20f)
        {
            this.NapeArmor -= speed;
            if (this.NapeArmor <= 0)
            {
                this.NapeArmor = 0;
                if (!this.hasDie)
                {
                    BasePV.RPC("netDie", PhotonTargets.OthersBuffered, new object[0]);
                    if (this.grabbedTarget != null)
                    {
                        this.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All, new object[0]);
                    }
                    this.netDie();
                    FengGameManagerMKII.FGM.TitanGetKill(photonView.owner, speed, base.name);
                }
            }
            else
            {
                FengGameManagerMKII.FGM.SendKillInfo(false, (string)photonView.owner.Properties[PhotonPlayerProperty.name], true, "Female Titan's neck", speed);
                FengGameManagerMKII.FGM.BasePV.RPC("netShowDamage", photonView.owner, new object[]
                {
                    speed
                });
            }
        }
    }

    public void Update()
    {
        if (IN_GAME_MAIN_CAMERA.isPausing && IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            return;
        }
        if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
        {
            if (!BasePV.IsMine)
            {
                return;
            }
        }
        if (!this.hasDie)
        {
            if (this.attention > 0f)
            {
                this.attention -= Time.deltaTime;
                if (this.attention < 0f)
                {
                    this.attention = 0f;
                    GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
                    this.myHero = array[UnityEngine.Random.Range(0, array.Length)];
                    this.attention = UnityEngine.Random.Range(5f, 10f);
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
            }
            if (this.eren != null)
            {
                if (!this.eren.GetComponent<TITAN_EREN>().hasDied)
                {
                    this.myHero = this.eren;
                }
                else
                {
                    this.eren = null;
                    this.myHero = null;
                }
            }
            if (this.myHero == null)
            {
                this.findNearestHero();
                if (this.myHero != null)
                {
                    return;
                }
            }
            if (this.myHero == null)
            {
                this.myDistance = float.MaxValue;
            }
            else
            {
                this.myDistance = Mathf.Sqrt((this.myHero.transform.position.x - base.transform.position.x) * (this.myHero.transform.position.x - base.transform.position.x) + (this.myHero.transform.position.z - base.transform.position.z) * (this.myHero.transform.position.z - base.transform.position.z));
            }
            if (this.state == "idle")
            {
                if (this.myHero == null)
                {
                    return;
                }
                Vector3 vector = this.myHero.transform.position - base.transform.position;
                float current = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
                float num = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                if (this.attackTarget(this.myHero))
                {
                    return;
                }
                if (Mathf.Abs(num) < 90f)
                {
                    this.chase();
                    return;
                }
                if (UnityEngine.Random.Range(0, 100) < 1)
                {
                    this.turn180();
                    return;
                }
                if (Mathf.Abs(num) > 100f)
                {
                    if (UnityEngine.Random.Range(0, 100) < 10)
                    {
                        this.turn180();
                        return;
                    }
                }
                else if (Mathf.Abs(num) > 45f && UnityEngine.Random.Range(0, 100) < 30)
                {
                    this.turn(num);
                    return;
                }
            }
            else if (this.state == "attack")
            {
                if (!this.attacked && this.attackCheckTime != 0f && base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTime)
                {
                    this.attacked = true;
                    this.fxPosition = base.transform.Find("ap_" + this.attackAnimation).position;
                    GameObject gameObject;
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
                    {
                        gameObject = Optimization.Caching.Pool.NetworkEnable("FX/" + this.fxName, this.fxPosition, this.fxRotation, 0);
                    }
                    else
                    {
                        gameObject = Pool.Enable("FX/" + this.fxName, this.fxPosition, this.fxRotation);//(GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("FX/" + this.fxName), this.fxPosition, this.fxRotation);
                    }
                    gameObject.transform.localScale = base.transform.localScale;
                    float num2 = 1f - Vector3.Distance(IN_GAME_MAIN_CAMERA.MainCamera.transform.position, gameObject.transform.position) * 0.05f;
                    num2 = Mathf.Min(1f, num2);
                    IN_GAME_MAIN_CAMERA.MainCamera.startShake(num2, num2, 0.95f);
                }
                if (this.attackCheckTimeA != 0f && ((base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA && base.animation["attack_" + this.attackAnimation].normalizedTime <= this.attackCheckTimeB) || (!this.attackChkOnce && base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA)))
                {
                    if (!this.attackChkOnce)
                    {
                        this.attackChkOnce = true;
                        this.playSound("snd_eren_swing" + UnityEngine.Random.Range(1, 3));
                    }
                    foreach (RaycastHit raycastHit in this.checkHitCapsule(this.checkHitCapsuleStart.position, this.checkHitCapsuleEnd.position, this.checkHitCapsuleR))
                    {
                        GameObject gameObject2 = raycastHit.collider.gameObject;
                        if (gameObject2.CompareTag("Player"))
                        {
                            this.killPlayer(gameObject2);
                        }
                        if (gameObject2.CompareTag("erenHitbox"))
                        {
                            if (this.attackAnimation == "combo_1")
                            {
                                if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
                                {
                                    gameObject2.transform.root.gameObject.GetComponent<TITAN_EREN>().hitByFTByServer(1);
                                }
                            }
                            else if (this.attackAnimation == "combo_2")
                            {
                                if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
                                {
                                    gameObject2.transform.root.gameObject.GetComponent<TITAN_EREN>().hitByFTByServer(2);
                                }
                            }
                            else if (this.attackAnimation == "combo_3" && IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
                            {
                                gameObject2.transform.root.gameObject.GetComponent<TITAN_EREN>().hitByFTByServer(3);
                            }
                        }
                    }
                    foreach (RaycastHit raycastHit2 in this.checkHitCapsule(this.checkHitCapsuleEndOld, this.checkHitCapsuleEnd.position, this.checkHitCapsuleR))
                    {
                        GameObject gameObject3 = raycastHit2.collider.gameObject;
                        if (gameObject3.CompareTag("Player"))
                        {
                            this.killPlayer(gameObject3);
                        }
                    }
                    this.checkHitCapsuleEndOld = this.checkHitCapsuleEnd.position;
                }
                if (this.attackAnimation == "jumpCombo_1" && base.animation["attack_" + this.attackAnimation].normalizedTime >= 0.65f && !this.startJump && this.myHero != null)
                {
                    this.startJump = true;
                    float y = this.myHero.rigidbody.velocity.y;
                    float num3 = -20f;
                    float num4 = this.gravity;
                    float y2 = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck").position.y;
                    float num5 = (num3 - num4) * 0.5f;
                    float num6 = y;
                    float num7 = this.myHero.transform.position.y - y2;
                    float d = Mathf.Abs((Mathf.Sqrt(num6 * num6 - 4f * num5 * num7) - num6) / (2f * num5));
                    Vector3 a = this.myHero.transform.position + this.myHero.rigidbody.velocity * d + Vectors.up * 0.5f * num3 * d * d;
                    float y3 = a.y;
                    if (num7 < 0f || y3 - y2 < 0f)
                    {
                        this.idle(0f);
                        d = 0.5f;
                        a = base.transform.position + (y2 + 5f) * Vectors.up;
                        y3 = a.y;
                    }
                    float num8 = y3 - y2;
                    float num9 = Mathf.Sqrt(2f * num8 / this.gravity);
                    float num10 = this.gravity * num9 + 20f;
                    num10 = Mathf.Clamp(num10, 20f, 90f);
                    Vector3 vector2 = (a - base.transform.position) / d;
                    this.abnorma_jump_bite_horizon_v = new Vector3(vector2.x, 0f, vector2.z);
                    Vector3 velocity = base.rigidbody.velocity;
                    Vector3 a2 = new Vector3(this.abnorma_jump_bite_horizon_v.x, num10, this.abnorma_jump_bite_horizon_v.z);
                    if (a2.magnitude > 90f)
                    {
                        a2 = a2.normalized * 90f;
                    }
                    Vector3 force = a2 - velocity;
                    base.rigidbody.AddForce(force, ForceMode.VelocityChange);
                    float y4 = Vector2.Angle(new Vector2(base.transform.position.x, base.transform.position.z), new Vector2(this.myHero.transform.position.x, this.myHero.transform.position.z));
                    y4 = Mathf.Atan2(this.myHero.transform.position.x - base.transform.position.x, this.myHero.transform.position.z - base.transform.position.z) * 57.29578f;
                    base.gameObject.transform.rotation = Quaternion.Euler(0f, y4, 0f);
                }
                if (this.attackAnimation == "jumpCombo_3")
                {
                    if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 1f && this.IsGrounded())
                    {
                        this.attack("jumpCombo_4");
                        return;
                    }
                }
                else if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 1f)
                {
                    if (this.nextAttackAnimation != null)
                    {
                        this.attack(this.nextAttackAnimation);
                        if (this.eren != null)
                        {
                            base.gameObject.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(this.eren.transform.position - base.transform.position).eulerAngles.y, 0f);
                        }
                        return;
                    }
                    this.findNearestHero();
                    this.idle(0f);
                }
            }
            else if (this.state == "grab")
            {
                if (base.animation["attack_grab_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA && base.animation["attack_grab_" + this.attackAnimation].normalizedTime <= this.attackCheckTimeB && this.grabbedTarget == null)
                {
                    GameObject gameObject4 = this.checkIfHitHand(this.currentGrabHand);
                    if (gameObject4 != null)
                    {
                        if (this.isGrabHandLeft)
                        {
                            this.eatSetL(gameObject4);
                            this.grabbedTarget = gameObject4;
                        }
                        else
                        {
                            this.eatSet(gameObject4);
                            this.grabbedTarget = gameObject4;
                        }
                    }
                }
                if (base.animation["attack_grab_" + this.attackAnimation].normalizedTime > this.attackCheckTime && this.grabbedTarget)
                {
                    this.justEatHero(this.grabbedTarget, this.currentGrabHand);
                    this.grabbedTarget = null;
                }
                if (base.animation["attack_grab_" + this.attackAnimation].normalizedTime >= 1f)
                {
                    this.idle(0f);
                }
            }
            else if (this.state == "turn")
            {
                base.gameObject.transform.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, this.desDeg, 0f), Time.deltaTime * Mathf.Abs(this.turnDeg) * 0.1f);
                if (base.animation[this.turnAnimation].normalizedTime >= 1f)
                {
                    this.idle(0f);
                }
            }
            else if (this.state == "chase")
            {
                if (this.eren != null && this.myDistance < 35f && this.attackTarget(this.myHero))
                {
                    return;
                }
                if (this.getNearestHeroDistance() < 50f && UnityEngine.Random.Range(0, 100) < 20 && this.attackTarget(this.getNearestHero()))
                {
                    return;
                }
                if (this.myDistance < this.attackDistance - 15f)
                {
                    this.idle(UnityEngine.Random.Range(0.05f, 0.2f));
                }
            }
            else if (this.state == "turn180")
            {
                if (base.animation[this.turnAnimation].normalizedTime >= 1f)
                {
                    base.gameObject.transform.rotation = Quaternion.Euler(base.gameObject.transform.rotation.eulerAngles.x, base.gameObject.transform.rotation.eulerAngles.y + 180f, base.gameObject.transform.rotation.eulerAngles.z);
                    this.idle(0f);
                    this.playAnimation("idle");
                }
            }
            else if (this.state == "anklehurt")
            {
                if (base.animation["legHurt"].normalizedTime >= 1f)
                {
                    this.crossFade("legHurt_loop", 0.2f);
                }
                if (base.animation["legHurt_loop"].normalizedTime >= 3f)
                {
                    this.crossFade("legHurt_getup", 0.2f);
                }
                if (base.animation["legHurt_getup"].normalizedTime >= 1f)
                {
                    this.idle(0f);
                    this.playAnimation("idle");
                }
            }
            return;
        }
        this.dieTime += Time.deltaTime;
        if (base.animation["die"].normalizedTime >= 1f)
        {
            this.playAnimation("die_cry");
            if (IN_GAME_MAIN_CAMERA.GameMode != GameMode.PVP_CAPTURE)
            {
                for (int j = 0; j < 15; j++)
                {
                    FengGameManagerMKII.FGM.RandomSpawnOneTitan("titanRespawn", 50).beTauntedBy(base.gameObject, 20f);
                }
            }
        }
        if (this.dieTime > 2f && !this.hasDieSteam)
        {
            this.hasDieSteam = true;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                GameObject gameObject6 = Pool.Enable("FX/FXtitanDie1", base.transform.Find("Amarture/Core/Controller_Body/hip").position, Quaternion.identity);//(GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("FX/FXtitanDie1"));
                gameObject6.transform.localScale = base.transform.localScale;
            }
            else if (BasePV.IsMine)
            {
                GameObject gameObject7 = Optimization.Caching.Pool.NetworkEnable("FX/FXtitanDie1", base.transform.Find("Amarture/Core/Controller_Body/hip").position, Quaternion.Euler(-90f, 0f, 0f), 0);
                gameObject7.transform.localScale = base.transform.localScale;
            }
        }
        if (this.dieTime > ((IN_GAME_MAIN_CAMERA.GameMode != GameMode.PVP_CAPTURE) ? 20f : 5f))
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                GameObject gameObject8 = Pool.Enable("FX/FXtitanDie", base.transform.Find("Amarture/Core/Controller_Body/hip").position, Quaternion.identity);//(GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("FX/FXtitanDie"));
                gameObject8.transform.localScale = base.transform.localScale;
                UnityEngine.Object.Destroy(base.gameObject);
            }
            else if (BasePV.IsMine)
            {
                GameObject gameObject9 = Optimization.Caching.Pool.NetworkEnable("FX/FXtitanDie", base.transform.Find("Amarture/Core/Controller_Body/hip").position, Quaternion.Euler(-90f, 0f, 0f), 0);
                gameObject9.transform.localScale = base.transform.localScale;
                PhotonNetwork.Destroy(base.gameObject);
            }
            return;
        }
    }
}