using Optimization.Caching;
using System.Collections;
using UnityEngine;

public class COLOSSAL_TITAN : Photon.MonoBehaviour
{
    private string actionName;
    private string attackAnimation;
    private float attackCheckTime;
    private float attackCheckTimeA;
    private float attackCheckTimeB;
    private bool attackChkOnce;
    private int attackCount;
    private int attackPattern = -1;
    private Transform checkHitCapsuleEnd;
    private Vector3 checkHitCapsuleEndOld;
    private float checkHitCapsuleR;
    private Transform checkHitCapsuleStart;
    private bool isSteamNeed;
    private string state = "idle";
    private float waitTime = 2f;
    public static float minusDistance = 99999f;
    public static GameObject minusDistanceEnemy;
    public GameObject bottomObject;
    public GameObject door_broken;
    public GameObject door_closed;
    public bool hasDie;
    public float myDistance;
    public GameObject myHero;
    public int NapeArmor = 10000;
    public int NapeArmorTotal = 10000;
    public GameObject neckSteamObject;
    public GameObject sweepSmokeObject;

    private void attack_sweep(string type = "")
    {
        this.callTitanHAHA();
        this.state = "attack_sweep";
        this.attackAnimation = "sweep" + type;
        this.attackCheckTimeA = 0.4f;
        this.attackCheckTimeB = 0.57f;
        this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R");
        this.checkHitCapsuleEnd = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
        this.checkHitCapsuleR = 20f;
        this.crossFade("attack_" + this.attackAnimation, 0.1f);
        this.attackChkOnce = false;
        this.sweepSmokeObject.GetComponent<ParticleSystem>().enableEmission = true;
        this.sweepSmokeObject.GetComponent<ParticleSystem>().Play();
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            if (FengGameManagerMKII.LAN)
            {
                if (Network.peerType == NetworkPeerType.Server)
                {
                }
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                BasePV.RPC("startSweepSmoke", PhotonTargets.Others, new object[0]);
            }
        }
    }

    private void Awake()
    {
        base.rigidbody.freezeRotation = true;
        base.rigidbody.useGravity = false;
        base.rigidbody.isKinematic = true;
    }

    private void callTitan(bool special = false)
    {
        if (!special && GameObject.FindGameObjectsWithTag("titan").Length > 6)
        {
            return;
        }
        GameObject[] array = GameObject.FindGameObjectsWithTag("titanRespawn");
        ArrayList arrayList = new ArrayList();
        foreach (GameObject gameObject in array)
        {
            if (gameObject.transform.parent.name == "titanRespawnCT")
            {
                arrayList.Add(gameObject);
            }
        }
        GameObject gameObject2 = (GameObject)arrayList[UnityEngine.Random.Range(0, arrayList.Count)];
        string[] array3 = new string[]
        {
            "TITAN_VER3.1"
        };
        GameObject gameObject3;
        if (FengGameManagerMKII.LAN)
        {
            gameObject3 = (GameObject)Network.Instantiate(CacheResources.Load(array3[UnityEngine.Random.Range(0, array3.Length)]), gameObject2.transform.position, gameObject2.transform.rotation, 0);
        }
        else
        {
            gameObject3 = Optimization.Caching.Pool.NetworkEnable(array3[UnityEngine.Random.Range(0, array3.Length)], gameObject2.transform.position, gameObject2.transform.rotation, 0);
        }
        if (special)
        {
            GameObject[] array4 = GameObject.FindGameObjectsWithTag("route");
            GameObject gameObject4 = array4[UnityEngine.Random.Range(0, array4.Length)];
            while (gameObject4.name != "routeCT")
            {
                gameObject4 = array4[UnityEngine.Random.Range(0, array4.Length)];
            }
            gameObject3.GetComponent<TITAN>().setRoute(gameObject4);
            gameObject3.GetComponent<TITAN>().setAbnormalType(AbnormalType.Aberrant, false);
            gameObject3.GetComponent<TITAN>().activeRad = 0;
            gameObject3.GetComponent<TITAN>().toCheckPoint((Vector3)gameObject3.GetComponent<TITAN>().checkPoints[0], 10f);
        }
        else
        {
            float num = 0.7f;
            float num2 = 0.7f;
            if (IN_GAME_MAIN_CAMERA.difficulty != 0)
            {
                if (IN_GAME_MAIN_CAMERA.difficulty == 1)
                {
                    num = 0.4f;
                    num2 = 0.7f;
                }
                else if (IN_GAME_MAIN_CAMERA.difficulty == 2)
                {
                    num = -1f;
                    num2 = 0.7f;
                }
            }
            if (GameObject.FindGameObjectsWithTag("titan").Length == 5)
            {
                gameObject3.GetComponent<TITAN>().setAbnormalType(AbnormalType.Jumper, false);
            }
            else if (UnityEngine.Random.Range(0f, 1f) >= num)
            {
                if (UnityEngine.Random.Range(0f, 1f) < num2)
                {
                    gameObject3.GetComponent<TITAN>().setAbnormalType(AbnormalType.Jumper, false);
                }
                else
                {
                    gameObject3.GetComponent<TITAN>().setAbnormalType(AbnormalType.Crawler, false);
                }
            }
            gameObject3.GetComponent<TITAN>().activeRad = 200;
        }
        if (FengGameManagerMKII.LAN)
        {
            GameObject gameObject5 = (GameObject)Network.Instantiate(CacheResources.Load("FX/FXtitanSpawn"), gameObject3.transform.position, Quaternion.Euler(-90f, 0f, 0f), 0);
            gameObject5.transform.localScale = gameObject3.transform.localScale;
        }
        else
        {
            GameObject gameObject6 = Optimization.Caching.Pool.NetworkEnable("FX/FXtitanSpawn", gameObject3.transform.position, Quaternion.Euler(-90f, 0f, 0f), 0);
            gameObject6.transform.localScale = gameObject3.transform.localScale;
        }
    }

    private void callTitanHAHA()
    {
        this.attackCount++;
        int num = 4;
        int num2 = 7;
        if (IN_GAME_MAIN_CAMERA.difficulty != 0)
        {
            if (IN_GAME_MAIN_CAMERA.difficulty == 1)
            {
                num = 4;
                num2 = 6;
            }
            else if (IN_GAME_MAIN_CAMERA.difficulty == 2)
            {
                num = 3;
                num2 = 5;
            }
        }
        if (this.attackCount % num == 0)
        {
            this.callTitan(false);
        }
        if ((double)this.NapeArmor < (double)this.NapeArmorTotal * 0.3)
        {
            if (this.attackCount % (int)((float)num2 * 0.5f) == 0)
            {
                this.callTitan(true);
            }
        }
        else if (this.attackCount % num2 == 0)
        {
            this.callTitan(true);
        }
    }

    [RPC]
    private void changeDoor()
    {
        this.door_broken.SetActive(true);
        this.door_closed.SetActive(false);
    }

    private RaycastHit[] checkHitCapsule(Vector3 start, Vector3 end, float r)
    {
        return Physics.SphereCastAll(start, r, end - start, Vector3.Distance(start, end));
    }

    private GameObject checkIfHitHand(Transform hand)
    {
        float num = 30f;
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

    private void crossFade(string aniName, float time)
    {
        base.animation.CrossFade(aniName, time);
        if (!FengGameManagerMKII.LAN)
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
            {
                BasePV.RPC("netCrossFade", PhotonTargets.Others, new object[]
                {
                    aniName,
                    time
                });
            }
        }
    }

    private void findNearestHero()
    {
        this.myHero = this.getNearestHero();
    }

    private GameObject getNearestHero()
    {
        GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
        GameObject result = null;
        float num = float.PositiveInfinity;
        foreach (GameObject gameObject in array)
        {
            if (!gameObject.GetComponent<HERO>() || !gameObject.GetComponent<HERO>().HasDied())
            {
                if (!gameObject.GetComponent<TITAN_EREN>() || !gameObject.GetComponent<TITAN_EREN>().hasDied)
                {
                    float num2 = Mathf.Sqrt((gameObject.transform.position.x - base.transform.position.x) * (gameObject.transform.position.x - base.transform.position.x) + (gameObject.transform.position.z - base.transform.position.z) * (gameObject.transform.position.z - base.transform.position.z));
                    if (gameObject.transform.position.y - base.transform.position.y < 450f && num2 < num)
                    {
                        result = gameObject;
                        num = num2;
                    }
                }
            }
        }
        return result;
    }

    private void idle()
    {
        this.state = "idle";
        this.crossFade("idle", 0.2f);
    }

    private void kick()
    {
        this.state = "kick";
        this.actionName = "attack_kick_wall";
        this.attackCheckTime = 0.64f;
        this.attackChkOnce = false;
        this.crossFade(this.actionName, 0.1f);
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
            else if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
            {
                if (FengGameManagerMKII.LAN)
                {
                    if (!hitHero.GetComponent<HERO>().HasDied())
                    {
                        hitHero.GetComponent<HERO>().markDie();
                    }
                }
                else if (!hitHero.GetComponent<HERO>().HasDied())
                {
                    hitHero.GetComponent<HERO>().markDie();
                    hitHero.GetComponent<HERO>().BasePV.RPC("netDie", PhotonTargets.All, new object[]
                    {
                        (hitHero.transform.position - position) * 15f * 4f,
                        false,
                        -1,
                        "Colossal Titan",
                        true
                    });
                }
            }
        }
    }

    private void neckSteam()
    {
        this.neckSteamObject.GetComponent<ParticleSystem>().Stop();
        this.neckSteamObject.GetComponent<ParticleSystem>().Play();
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            if (FengGameManagerMKII.LAN)
            {
                if (Network.peerType == NetworkPeerType.Server)
                {
                }
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                BasePV.RPC("startNeckSteam", PhotonTargets.Others, new object[0]);
            }
        }
        this.isSteamNeed = true;
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck");
        float radius = 30f;
        Collider[] array = Physics.OverlapSphere(transform.transform.position - base.transform.Forward() * 10f, radius);
        foreach (Collider collider in array)
        {
            if (collider.transform.root.CompareTag("Player"))
            {
                GameObject gameObject = collider.transform.root.gameObject;
                if (!gameObject.GetComponent<TITAN_EREN>())
                {
                    if (gameObject.GetComponent<HERO>())
                    {
                        this.blowPlayer(gameObject, transform);
                    }
                }
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
            FengGameManagerMKII.FGM.RemoveCT(this);
        }
    }

    private void playAnimation(string aniName)
    {
        base.animation.Play(aniName);
        if (!FengGameManagerMKII.LAN)
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
            {
                BasePV.RPC("netPlayAnimation", PhotonTargets.Others, new object[]
                {
                    aniName
                });
            }
        }
    }

    private void playAnimationAt(string aniName, float normalizedTime)
    {
        base.animation.Play(aniName);
        base.animation[aniName].normalizedTime = normalizedTime;
        if (!FengGameManagerMKII.LAN)
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
            {
                BasePV.RPC("netPlayAnimationAt", PhotonTargets.Others, new object[]
                {
                    aniName,
                    normalizedTime
                });
            }
        }
    }

    private void playSound(string sndname)
    {
        this.playsoundRPC(sndname);
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            if (FengGameManagerMKII.LAN)
            {
                if (Network.peerType == NetworkPeerType.Server)
                {
                }
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                BasePV.RPC("playsoundRPC", PhotonTargets.Others, new object[]
                {
                    sndname
                });
            }
        }
    }

    [RPC]
    private void playsoundRPC(string sndname)
    {
        Transform transform = base.transform.Find(sndname);
        transform.GetComponent<AudioSource>().Play();
    }

    [RPC]
    private void removeMe()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void slap(string type)
    {
        this.callTitanHAHA();
        this.state = "slap";
        this.attackAnimation = type;
        if (type == "r1" || type == "r2")
        {
            this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/hand_R_001");
        }
        if (type == "l1" || type == "l2")
        {
            this.checkHitCapsuleStart = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/shoulder_L/upper_arm_L/forearm_L/hand_L/hand_L_001");
        }
        this.attackCheckTime = 0.57f;
        this.attackChkOnce = false;
        this.crossFade("attack_slap_" + this.attackAnimation, 0.1f);
    }

    private void Start()
    {
        FengGameManagerMKII.FGM.AddCT(this);
        if (this.myHero == null)
        {
            this.findNearestHero();
        }
        base.name = "COLOSSAL_TITAN";
        this.NapeArmor = 1000;
        bool flag = false;
        if (FengGameManagerMKII.Level.RespawnMode == RespawnMode.NEVER)
        {
            flag = true;
        }
        if (IN_GAME_MAIN_CAMERA.difficulty == 0)
        {
            this.NapeArmor = ((!flag) ? 5000 : 2000);
        }
        else if (IN_GAME_MAIN_CAMERA.difficulty == 1)
        {
            this.NapeArmor = ((!flag) ? 8000 : 3500);
            foreach (object obj in base.animation)
            {
                AnimationState animationState = (AnimationState)obj;
                animationState.speed = 1.02f;
            }
        }
        else if (IN_GAME_MAIN_CAMERA.difficulty == 2)
        {
            this.NapeArmor = ((!flag) ? 12000 : 5000);
            foreach (object obj2 in base.animation)
            {
                AnimationState animationState2 = (AnimationState)obj2;
                animationState2.speed = 1.05f;
            }
        }
        this.NapeArmorTotal = this.NapeArmor;
        this.state = "wait";
        base.transform.position += -Vectors.up * 10000f;
        if (FengGameManagerMKII.LAN)
        {
            base.GetComponent<PhotonView>().enabled = false;
        }
        else
        {
            base.GetComponent<NetworkView>().enabled = false;
        }
        this.door_broken = CacheGameObject.Find("door_broke");
        this.door_closed = CacheGameObject.Find("door_fine");
        this.door_broken.SetActive(false);
        this.door_closed.SetActive(true);
    }

    [RPC]
    private void startNeckSteam()
    {
        this.neckSteamObject.GetComponent<ParticleSystem>().Stop();
        this.neckSteamObject.GetComponent<ParticleSystem>().Play();
    }

    [RPC]
    private void startSweepSmoke()
    {
        this.sweepSmokeObject.GetComponent<ParticleSystem>().enableEmission = true;
        this.sweepSmokeObject.GetComponent<ParticleSystem>().Play();
    }

    private void steam()
    {
        this.callTitanHAHA();
        this.state = "steam";
        this.actionName = "attack_steam";
        this.attackCheckTime = 0.45f;
        this.crossFade(this.actionName, 0.1f);
        this.attackChkOnce = false;
    }

    [RPC]
    private void stopSweepSmoke()
    {
        this.sweepSmokeObject.GetComponent<ParticleSystem>().enableEmission = false;
        this.sweepSmokeObject.GetComponent<ParticleSystem>().Stop();
    }

    public void beTauntedBy(GameObject target, float tauntTime)
    {
    }

    public void blowPlayer(GameObject player, Transform neck)
    {
        Vector3 vector = -(neck.position + base.transform.Forward() * 50f - player.transform.position);
        float d = 20f;
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            player.GetComponent<HERO>().blowAway(vector.normalized * d + Vectors.up * 1f);
        }
        else if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && PhotonNetwork.IsMasterClient)
        {
            player.GetComponent<HERO>().BasePV.RPC("blowAway", PhotonTargets.All, new object[]
            {
                vector.normalized * d + Vectors.up * 1f
            });
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
    }

    [RPC]
    public void titanGetHit(int viewID, int speed)
    {
        if (FengGameManagerMKII.LAN)
        {
        }
        Transform transform = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck");
        PhotonView photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            return;
        }
        float magnitude = (photonView.gameObject.transform.position - transform.transform.position).magnitude;
        if (magnitude < 40f)
        {
            this.NapeArmor -= speed;
            this.neckSteam();
            if (this.NapeArmor <= 0)
            {
                this.NapeArmor = 0;
                if (!this.hasDie)
                {
                    if (FengGameManagerMKII.LAN)
                    {
                        this.netDie();
                    }
                    else
                    {
                        BasePV.RPC("netDie", PhotonTargets.OthersBuffered, new object[0]);
                        this.netDie();
                        FengGameManagerMKII.FGM.TitanGetKill(photonView.owner, speed, base.name);
                    }
                }
            }
            else
            {
                FengGameManagerMKII.FGM.SendKillInfo(false, (string)photonView.owner.Properties[PhotonPlayerProperty.name], true, "Colossal Titan's neck", speed);
                FengGameManagerMKII.FGM.BasePV.RPC("netShowDamage", photonView.owner, new object[]
                {
                    speed
                });
            }
        }
    }

    public void Update()
    {
        if (this.state == "null")
        {
            return;
        }
        if (this.state == "wait")
        {
            this.waitTime -= Time.deltaTime;
            if (this.waitTime <= 0f)
            {
                base.transform.position = new Vector3(30f, 0f, 784f);
                //UnityEngine.Object.Instantiate(CacheResources.Load("FX/ThunderCT"), base.transform.position + Vectors.up * 350f, Quaternion.Euler(270f, 0f, 0f));
                Pool.Enable("FX/ThunderCT", base.transform.position + Vectors.up * 350f, Quaternion.Euler(270f, 0f, 0f));
                IN_GAME_MAIN_CAMERA.MainCamera.flashBlind();
                if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
                {
                    this.idle();
                }
                else if ((!FengGameManagerMKII.LAN) ? BasePV.IsMine : base.networkView.isMine)
                {
                    this.idle();
                }
                else
                {
                    this.state = "null";
                }
            }
        }
        else
        {
            if (this.state == "idle")
            {
                if (this.attackPattern == -1)
                {
                    this.slap("r1");
                    this.attackPattern++;
                }
                else if (this.attackPattern == 0)
                {
                    this.attack_sweep(string.Empty);
                    this.attackPattern++;
                }
                else if (this.attackPattern == 1)
                {
                    this.steam();
                    this.attackPattern++;
                }
                else if (this.attackPattern == 2)
                {
                    this.kick();
                    this.attackPattern++;
                }
                else
                {
                    if (this.isSteamNeed || this.hasDie)
                    {
                        this.steam();
                        this.isSteamNeed = false;
                        return;
                    }
                    if (this.myHero == null)
                    {
                        this.findNearestHero();
                    }
                    else
                    {
                        Vector3 vector = this.myHero.transform.position - base.transform.position;
                        float current = -Mathf.Atan2(vector.z, vector.x) * 57.29578f;
                        float f = -Mathf.DeltaAngle(current, base.gameObject.transform.rotation.eulerAngles.y - 90f);
                        this.myDistance = Mathf.Sqrt((this.myHero.transform.position.x - base.transform.position.x) * (this.myHero.transform.position.x - base.transform.position.x) + (this.myHero.transform.position.z - base.transform.position.z) * (this.myHero.transform.position.z - base.transform.position.z));
                        float num = this.myHero.transform.position.y - base.transform.position.y;
                        if (this.myDistance < 85f && UnityEngine.Random.Range(0, 100) < 5)
                        {
                            this.steam();
                            return;
                        }
                        if (num > 310f && num < 350f)
                        {
                            if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("APL1").position) < 40f)
                            {
                                this.slap("l1");
                                return;
                            }
                            if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("APL2").position) < 40f)
                            {
                                this.slap("l2");
                                return;
                            }
                            if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("APR1").position) < 40f)
                            {
                                this.slap("r1");
                                return;
                            }
                            if (Vector3.Distance(this.myHero.transform.position, base.transform.Find("APR2").position) < 40f)
                            {
                                this.slap("r2");
                                return;
                            }
                            if (this.myDistance < 150f && Mathf.Abs(f) < 80f)
                            {
                                this.attack_sweep(string.Empty);
                                return;
                            }
                        }
                        if (num < 300f && Mathf.Abs(f) < 80f && this.myDistance < 85f)
                        {
                            this.attack_sweep("_vertical");
                            return;
                        }
                        int num2 = UnityEngine.Random.Range(0, 7);
                        if (num2 == 0)
                        {
                            this.slap("l1");
                        }
                        else if (num2 == 1)
                        {
                            this.slap("l2");
                        }
                        else if (num2 == 2)
                        {
                            this.slap("r1");
                        }
                        else if (num2 == 3)
                        {
                            this.slap("r2");
                        }
                        else if (num2 == 4)
                        {
                            this.attack_sweep(string.Empty);
                        }
                        else if (num2 == 5)
                        {
                            this.attack_sweep("_vertical");
                        }
                        else if (num2 == 6)
                        {
                            this.steam();
                        }
                    }
                }
                return;
            }
            if (this.state == "attack_sweep")
            {
                if (this.attackCheckTimeA != 0f && ((base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA && base.animation["attack_" + this.attackAnimation].normalizedTime <= this.attackCheckTimeB) || (!this.attackChkOnce && base.animation["attack_" + this.attackAnimation].normalizedTime >= this.attackCheckTimeA)))
                {
                    if (!this.attackChkOnce)
                    {
                        this.attackChkOnce = true;
                    }
                    foreach (RaycastHit raycastHit in this.checkHitCapsule(this.checkHitCapsuleStart.position, this.checkHitCapsuleEnd.position, this.checkHitCapsuleR))
                    {
                        GameObject gameObject = raycastHit.collider.gameObject;
                        if (gameObject.CompareTag("Player"))
                        {
                            this.killPlayer(gameObject);
                        }
                        if (gameObject.CompareTag("erenHitbox") && this.attackAnimation == "combo_3" && IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && ((!FengGameManagerMKII.LAN) ? PhotonNetwork.IsMasterClient : Network.isServer))
                        {
                            gameObject.transform.root.gameObject.GetComponent<TITAN_EREN>().hitByFTByServer(3);
                        }
                    }
                    foreach (RaycastHit raycastHit2 in this.checkHitCapsule(this.checkHitCapsuleEndOld, this.checkHitCapsuleEnd.position, this.checkHitCapsuleR))
                    {
                        GameObject gameObject2 = raycastHit2.collider.gameObject;
                        if (gameObject2.CompareTag("Player"))
                        {
                            this.killPlayer(gameObject2);
                        }
                    }
                    this.checkHitCapsuleEndOld = this.checkHitCapsuleEnd.position;
                }
                if (base.animation["attack_" + this.attackAnimation].normalizedTime >= 1f)
                {
                    this.sweepSmokeObject.GetComponent<ParticleSystem>().enableEmission = false;
                    this.sweepSmokeObject.GetComponent<ParticleSystem>().Stop();
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (!FengGameManagerMKII.LAN)
                        {
                            BasePV.RPC("stopSweepSmoke", PhotonTargets.Others, new object[0]);
                        }
                    }
                    this.findNearestHero();
                    this.idle();
                    this.playAnimation("idle");
                }
            }
            else if (this.state == "kick")
            {
                if (!this.attackChkOnce && base.animation[this.actionName].normalizedTime >= this.attackCheckTime)
                {
                    this.attackChkOnce = true;
                    this.door_broken.SetActive(true);
                    this.door_closed.SetActive(false);
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (!FengGameManagerMKII.LAN)
                        {
                            BasePV.RPC("changeDoor", PhotonTargets.OthersBuffered, new object[0]);
                        }
                    }
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (FengGameManagerMKII.LAN)
                        {
                            Network.Instantiate(CacheResources.Load("FX/boom1_CT_KICK"), base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Network.Instantiate(CacheResources.Load("rock"), base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(0f, 0f, 0f), 0);
                        }
                        else
                        {
                            Optimization.Caching.Pool.NetworkEnable("FX/boom1_CT_KICK", base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Optimization.Caching.Pool.NetworkEnable("rock", base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(0f, 0f, 0f), 0);
                        }
                    }
                    else
                    {
                        Pool.Enable("FX/boom1_CT_KICK", base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/boom1_CT_KICK"), base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(270f, 0f, 0f));
                        Pool.Enable("rock", base.transform.position + base.transform.Forward() * 120f + base.transform.right * 30f, Quaternion.Euler(0f, 0f, 0f));
                    }
                }
                if (base.animation[this.actionName].normalizedTime >= 1f)
                {
                    this.findNearestHero();
                    this.idle();
                    this.playAnimation("idle");
                }
            }
            else if (this.state == "slap")
            {
                if (!this.attackChkOnce && base.animation["attack_slap_" + this.attackAnimation].normalizedTime >= this.attackCheckTime)
                {
                    this.attackChkOnce = true;
                    GameObject gameObject3;
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (FengGameManagerMKII.LAN)
                        {
                            gameObject3 = (GameObject)Network.Instantiate(CacheResources.Load("FX/boom1"), this.checkHitCapsuleStart.position, Quaternion.Euler(270f, 0f, 0f), 0);
                        }
                        else
                        {
                            gameObject3 = Optimization.Caching.Pool.NetworkEnable("FX/boom1", this.checkHitCapsuleStart.position, Quaternion.Euler(270f, 0f, 0f), 0);
                        }
                        if (gameObject3.GetComponent<EnemyfxIDcontainer>())
                        {
                            gameObject3.GetComponent<EnemyfxIDcontainer>().titanName = base.name;
                        }
                    }
                    else
                    {
                        gameObject3 = Pool.Enable("FX/boom1", this.checkHitCapsuleStart.position, Quaternion.Euler(270f, 0f, 0f));//(GameObject)UnityEngine.Object.Instantiate(CacheResources.Load("FX/boom1"), this.checkHitCapsuleStart.position, Quaternion.Euler(270f, 0f, 0f));
                    }
                    gameObject3.transform.localScale = new Vector3(5f, 5f, 5f);
                }
                if (base.animation["attack_slap_" + this.attackAnimation].normalizedTime >= 1f)
                {
                    this.findNearestHero();
                    this.idle();
                    this.playAnimation("idle");
                }
            }
            else if (this.state == "steam")
            {
                if (!this.attackChkOnce && base.animation[this.actionName].normalizedTime >= this.attackCheckTime)
                {
                    this.attackChkOnce = true;
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (FengGameManagerMKII.LAN)
                        {
                            Network.Instantiate(CacheResources.Load("FX/colossal_steam"), base.transform.position + base.transform.Up() * 185f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Network.Instantiate(CacheResources.Load("FX/colossal_steam"), base.transform.position + base.transform.Up() * 303f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Network.Instantiate(CacheResources.Load("FX/colossal_steam"), base.transform.position + base.transform.Up() * 50f, Quaternion.Euler(270f, 0f, 0f), 0);
                        }
                        else
                        {
                            Optimization.Caching.Pool.NetworkEnable("FX/colossal_steam", base.transform.position + base.transform.Up() * 185f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Optimization.Caching.Pool.NetworkEnable("FX/colossal_steam", base.transform.position + base.transform.Up() * 303f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Optimization.Caching.Pool.NetworkEnable("FX/colossal_steam", base.transform.position + base.transform.Up() * 50f, Quaternion.Euler(270f, 0f, 0f), 0);
                        }
                    }
                    else
                    {
                        Pool.Enable("FX/colossal_steam", base.transform.position + base.transform.Forward() * 185f, Quaternion.Euler(270f, 0f, 0f));
                        Pool.Enable("FX/colossal_steam", base.transform.position + base.transform.Forward() * 303f, Quaternion.Euler(270f, 0f, 0f));
                        Pool.Enable("FX/colossal_steam", base.transform.position + base.transform.Forward() * 50f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/colossal_steam"), base.transform.position + base.transform.Forward() * 185f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/colossal_steam"), base.transform.position + base.transform.Forward() * 303f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/colossal_steam"), base.transform.position + base.transform.Forward() * 50f, Quaternion.Euler(270f, 0f, 0f));
                    }
                }
                if (base.animation[this.actionName].normalizedTime >= 1f)
                {
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (FengGameManagerMKII.LAN)
                        {
                            Network.Instantiate(CacheResources.Load("FX/colossal_steam_dmg"), base.transform.position + base.transform.Up() * 185f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Network.Instantiate(CacheResources.Load("FX/colossal_steam_dmg"), base.transform.position + base.transform.Up() * 303f, Quaternion.Euler(270f, 0f, 0f), 0);
                            Network.Instantiate(CacheResources.Load("FX/colossal_steam_dmg"), base.transform.position + base.transform.Up() * 50f, Quaternion.Euler(270f, 0f, 0f), 0);
                        }
                        else
                        {
                            GameObject gameObject4 = Optimization.Caching.Pool.NetworkEnable("FX/colossal_steam_dmg", base.transform.position + base.transform.Up() * 185f, Quaternion.Euler(270f, 0f, 0f), 0);
                            if (gameObject4.GetComponent<EnemyfxIDcontainer>())
                            {
                                gameObject4.GetComponent<EnemyfxIDcontainer>().titanName = base.name;
                            }
                            gameObject4 = Optimization.Caching.Pool.NetworkEnable("FX/colossal_steam_dmg", base.transform.position + base.transform.Up() * 303f, Quaternion.Euler(270f, 0f, 0f), 0);
                            if (gameObject4.GetComponent<EnemyfxIDcontainer>())
                            {
                                gameObject4.GetComponent<EnemyfxIDcontainer>().titanName = base.name;
                            }
                            gameObject4 = Optimization.Caching.Pool.NetworkEnable("FX/colossal_steam_dmg", base.transform.position + base.transform.Up() * 50f, Quaternion.Euler(270f, 0f, 0f), 0);
                            if (gameObject4.GetComponent<EnemyfxIDcontainer>())
                            {
                                gameObject4.GetComponent<EnemyfxIDcontainer>().titanName = base.name;
                            }
                        }
                    }
                    else
                    {
                        Pool.Enable("FX/colossal_steam_dmg", base.transform.position + base.transform.Forward() * 185f, Quaternion.Euler(270f, 0f, 0f));
                        Pool.Enable("FX/colossal_steam_dmg", base.transform.position + base.transform.Forward() * 303f, Quaternion.Euler(270f, 0f, 0f));
                        Pool.Enable("FX/colossal_steam_dmg", base.transform.position + base.transform.Forward() * 50f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/colossal_steam_dmg"), base.transform.position + base.transform.Forward() * 185f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/colossal_steam_dmg"), base.transform.position + base.transform.Forward() * 303f, Quaternion.Euler(270f, 0f, 0f));
                        //UnityEngine.Object.Instantiate(CacheResources.Load("FX/colossal_steam_dmg"), base.transform.position + base.transform.Forward() * 50f, Quaternion.Euler(270f, 0f, 0f));
                    }
                    if (this.hasDie)
                    {
                        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
                        {
                            UnityEngine.Object.Destroy(base.gameObject);
                        }
                        else if (FengGameManagerMKII.LAN)
                        {
                            if (base.networkView.isMine)
                            {
                            }
                        }
                        else if (PhotonNetwork.IsMasterClient)
                        {
                            PhotonNetwork.Destroy(BasePV);
                        }
                        FengGameManagerMKII.FGM.GameWin();
                    }
                    this.findNearestHero();
                    this.idle();
                    this.playAnimation("idle");
                }
            }
            else if (this.state == string.Empty)
            {
            }
        }
    }
}