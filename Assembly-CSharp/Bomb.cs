using System;
using System.Collections;
using ExitGames.Client.Photon;
using Optimization;
using Photon;
using UnityEngine;
using Optimization.Caching;

public class Bomb : Photon.MonoBehaviour
{
    //!!!!NOT TESTED!!!!

    public static bool BombModeEnabled;
    public static int MyBombCD = 5;
    public static int MyBombRad = 5;
    public static int MyBombRange = 5;
    public static int MyBombSpeed = 5;
    
    public Rigidbody baseR;
    public Transform baseT;
    private Vector3 correctPlayerPos = Vectors.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;
    private Vector3 correctPlayerVelocity = Vectors.zero;
    public bool disabled;
    public BombExplode myExplosion;
    private float myRad;
    public float SmoothingDelay = 10f;

    private void AIM()
    {
        foreach (HERO hero in FengGameManagerMKII.Heroes)
        {
            if (Vector3.Distance(hero.baseT.position, this.baseT.position) <= 400f && !hero.IsLocal)
            {
                break;
            }
        }
    }

    public void Awake()
    {
        this.baseT = transform;//GetComponent<Transform>();
        this.baseR = rigidbody;//GetComponent<Rigidbody>();
    }

    public void destroyMe()
    {
        if (this.BasePV.IsMine)
        {
            if (this.myExplosion != null)
            {
                PhotonNetwork.Destroy(this.myExplosion.gameObject);
            }
            PhotonNetwork.Destroy(base.gameObject);
        }
    }

    public void Explode(float radius)
    {
        this.disabled = true;
        this.baseR.velocity = Vectors.zero;
        this.myExplosion = Pool.NetworkEnable("RCAsset/BombExplodeMain", this.baseT.position, Quaternion.Euler(0f, 0f, 0f), 0).GetComponent<BombExplode>();
        foreach (HERO hero in FengGameManagerMKII.Heroes)
        {
            if (Vector3.Distance(hero.baseT.position, this.baseT.position) < radius && !hero.BasePV.IsMine && !hero.bombImmune)
            {
                PhotonPlayer owner = hero.BasePV.owner;
                if (PhotonNetwork.player.RCteam > 0)
                {
                    int num = PhotonNetwork.player.RCteam;
                    int num2 = owner.RCteam;
                    if (num == 0 || num != num2)
                    {
                        hero.markDie();
                        hero.BasePV.RPC("netDie2", PhotonTargets.All, new object[]
                        {
                            -1,
                            $"ID's {owner.ID} Bomb"//(Settings.BombNameOn ? Settings.BombName : Settings.Name) + " "
                        });
                        FengGameManagerMKII.FGM.PlayerKillInfoUpdate(PhotonNetwork.player, 0);
                    }
                }
                else
                {
                    hero.markDie();
                    hero.BasePV.RPC("netDie2", PhotonTargets.All, new object[]
                    {
                        -1,
                        $"ID's {owner.ID} Bomb"//(Settings.BombNameOn ? Settings.BombName : Settings.Name) + " "
                    });
                    FengGameManagerMKII.FGM.PlayerKillInfoUpdate(PhotonNetwork.player, 0);
                }
            }
        }
        base.StartCoroutine(this.WaitAndFade(1.5f));
    }

    private void OnEnable()
    {
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            if (this.BasePV != null)
            {
                this.BasePV.observed = this;
                this.correctPlayerPos = this.baseT.position;
                this.correctPlayerRot = Quaternion.identity;
                PhotonPlayer owner = this.BasePV.owner;
                if (owner.IsLocal)
                {
                    //int rad = Settings.BombRadius;
                    this.myRad = MyBombRad * 4f + 20f;
                }
                //Uncomment when goint to make a team mode
                //if (Settings.TeamMode > 0)
                //{
                //    int rcteam = owner.RCteam;
                //    if (rcteam == 1)
                //    {
                //        base.GetComponent<ParticleSystem>().startColor = Color.cyan;
                //        return;
                //    }
                //    if (rcteam != 2)
                //    {
                //        base.GetComponent<ParticleSystem>().startColor = new Color(owner.RCBombR, owner.RCBombG, owner.RCBombB, (owner.RCBombA < 0.5f) ? 0.5f : owner.RCBombA);
                //        return;
                //    }
                //    base.GetComponent<ParticleSystem>().startColor = Color.magenta;
                //    return;
                //}
                //else
                //{
                //    base.GetComponent<ParticleSystem>().startColor = new Color(owner.RCBombR, owner.RCBombG, owner.RCBombB, (owner.RCBombA < 0.5f) ? 0.5f : owner.RCBombA);
                //}
                //Delete this string if uncomment upper
                base.GetComponent<ParticleSystem>().startColor = new Color(owner.RCBombR, owner.RCBombG, owner.RCBombB, (owner.RCBombA < 0.5f) ? 0.5f : owner.RCBombA);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(this.baseT.position);
            stream.SendNext(this.baseT.rotation);
            stream.SendNext(this.baseR.velocity);
            return;
        }
        if (stream.data.Count != 3)
        {
            //Antis.Response(info.sender, "OnPhotonSerializeView err[<color=magenta>Bomb</color>]");
            return;
        }
        object obj = stream.ReceiveNext();
        this.correctPlayerPos = ((obj is Vector3) ? ((Vector3)obj) : Vector3.zero);
        obj = stream.ReceiveNext();
        this.correctPlayerRot = ((obj is Quaternion) ? ((Quaternion)obj) : Quaternion.identity);
        obj = stream.ReceiveNext();
        this.correctPlayerVelocity = ((obj is Vector3) ? ((Vector3)obj) : Vector3.zero);
    }

    public void Update()
    {
        if (!this.disabled)
        {
            if (!this.BasePV.IsMine)
            {
                float dt = Time.deltaTime;
                this.baseT.position = Vector3.Lerp(this.baseT.position, this.correctPlayerPos, dt * this.SmoothingDelay);
                this.baseT.rotation = Quaternion.Lerp(this.baseT.rotation, this.correctPlayerRot, dt * this.SmoothingDelay);
                this.baseR.velocity = this.correctPlayerVelocity;
            }
        }
    }

    private IEnumerator WaitAndFade(float time)
    {
        yield return new WaitForSeconds(time);
        PhotonNetwork.Destroy(this.myExplosion.gameObject);
        PhotonNetwork.Destroy(base.gameObject);
        yield break;
    }
}
