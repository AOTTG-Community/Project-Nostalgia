// Decompiled with JetBrains decompiler
// Type: BRS.Gameplay.Bomb
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using BRS.Extensions;
using ns8;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BRS.Gameplay
{
  public class Bomb : Photon.MonoBehaviour
  {
    private float float_0 = 3f;
    private float float_1 = 30f;
    private float float_2 = 15f;
    private float float_3 = 0.5f;
    private BombType bombType_0;
    private ParticleSystem particleSystem_0;
    private ParticleSystem particleSystem_1;
    public HERO owner;

    public bool Stuck { get; private set; }

    public bool exploded { get; private set; }

    [Attribute5]
    private void Start()
    {
      this.collider.material = new PhysicMaterial("bouncy")
      {
        bounciness = 0.45f,
        bounceCombine = PhysicMaterialCombine.Maximum
      };
      this.owner = HERO.FindMyHero(this.photonView.owner.ID);
      this.bombType_0 = this.owner.bombType;
      if (this.photonView.isMine)
      {
        Ray ray = this.owner.currentCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask layerMask1 = (LayerMask) (1 << LayerMask.NameToLayer("Ground"));
        LayerMask layerMask2 = (LayerMask) (1 << LayerMask.NameToLayer("EnemyBox"));
        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo, 999999f, (int) layerMask1 | (int) layerMask2);
        this.rigidbody.AddForce((hitInfo.point - this.transform.position).normalized * 300f * this.float_3, ForceMode.Impulse);
      }
      GameObject gameObject1 = new GameObject();
      gameObject1.name = "bomb_sparks";
      GameObject go1 = gameObject1;
      this.particleSystem_0 = go1.AddComponent<ParticleSystem>(this.owner.sparks);
      go1.transform.parent = this.transform;
      go1.transform.localPosition = Vector3.zero;
      go1.GetComponent<ParticleSystem>().renderer.material = this.owner.sparks.renderer.material;
      GameObject gameObject2 = new GameObject();
      gameObject2.name = "bomb_smoke";
      GameObject go2 = gameObject2;
      this.particleSystem_1 = go2.AddComponent<ParticleSystem>(this.owner.smoke_3dmg);
      go2.transform.parent = this.transform;
      go2.transform.localPosition = Vector3.zero;
      go2.GetComponent<ParticleSystem>().renderer.material = this.owner.smoke_3dmg.renderer.material;
      go2.GetComponent<ParticleSystem>().startSize = 0.8f;
      go2.GetComponent<ParticleSystem>().emissionRate = 50f;
      this.particleSystem_0 = this.transform.Find("bomb_sparks").GetComponent<ParticleSystem>();
      this.particleSystem_1 = this.transform.Find("bomb_smoke").GetComponent<ParticleSystem>();
      this.particleSystem_0.enableEmission = true;
      this.particleSystem_1.enableEmission = true;
    }

    [Attribute5]
    private void OnCollisionEnter(Collision other)
    {
      if (this.exploded || (UnityEngine.Object) other.gameObject == (UnityEngine.Object) this.gameObject || !this.photonView.isMine)
        return;
      if (this.bombType_0 == BombType.Contact)
      {
        this.photonView.RPC("Detonate", PhotonTargets.All);
      }
      else
      {
        if (this.bombType_0 != BombType.Sticky || this.Stuck)
          return;
        foreach (ContactPoint contact in other.contacts)
          this.transform.parent = contact.otherCollider.gameObject.transform;
        this.rigidbody.isKinematic = true;
        this.collider.enabled = false;
        this.Stuck = true;
        this.photonView.RPC("SetSmokeState", PhotonTargets.All, new object[1]
        {
          (object) false
        });
      }
    }

    [RPC]
    [Attribute5]
    private void SetSmokeState(bool state)
    {
      this.particleSystem_0.enableEmission = state;
      this.particleSystem_1.enableEmission = state;
    }

    [Attribute5]
    [RPC]
    public void Detonate()
    {
      if (this.exploded)
        return;
      this.exploded = true;
      UnityEngine.Object.Instantiate(Resources.Load("FX/Thunder"), this.transform.position, Quaternion.Euler(270f, 0.0f, 0.0f));
      UnityEngine.Object.Instantiate(Resources.Load("FX/boom1"), this.transform.position, Quaternion.Euler(270f, 0.0f, 0.0f));
      if (this.photonView.isMine)
      {
        foreach (HERO hero in ((IEnumerable<GameObject>) GameObject.FindGameObjectsWithTag("Player")).Select<GameObject, HERO>((Func<GameObject, HERO>) (human => human.GetComponent<HERO>())).Where<HERO>((Func<HERO, bool>) (human =>
        {
          if (!human.HasDied())
            return (double) Vector3.SqrMagnitude(human.transform.position - this.transform.position) < (double) this.float_1 * (double) this.float_1;
          return false;
        })))
        {
          Vector3 vector3 = hero.transform.position - this.transform.position;
          string customProperty = (string) PhotonNetwork.player.customProperties[(object) "name"];
          object[] objArray = new object[5]
          {
            (object) (hero.rigidbody.velocity * 15f + vector3.normalized * (this.float_1 - vector3.magnitude) * 15f),
            (object) false,
            (object) this.owner.photonView.viewID,
            (object) customProperty,
            (object) false
          };
          hero.photonView.RPC("netDie", PhotonTargets.All, objArray);
        }
        foreach (Photon.MonoBehaviour monoBehaviour in ((IEnumerable<Bomb>) UnityEngine.Object.FindObjectsOfType<Bomb>()).Where<Bomb>((Func<Bomb, bool>) (bomb => (double) Vector3.SqrMagnitude(bomb.transform.position - this.transform.position) < (double) this.float_1 * (double) this.float_1)))
          monoBehaviour.photonView.RPC(nameof (Detonate), PhotonTargets.All);
      }
      this.particleSystem_0.enableEmission = false;
      this.particleSystem_1.enableEmission = false;
      this.gameObject.collider.enabled = false;
      this.gameObject.renderer.enabled = false;
      if (!this.photonView.isMine)
        return;
      this.StartCoroutine(this.method_0(5f));
    }

    [Attribute5]
    private void Update()
    {
      this.rigidbody.AddForce(Vector3.down * this.float_2, ForceMode.Acceleration);
      if ((UnityEngine.Object) this.transform.parent != (UnityEngine.Object) null && (UnityEngine.Object) this.transform.root.gameObject.GetComponent<TITAN>() != (UnityEngine.Object) null && this.transform.root.gameObject.GetComponent<TITAN>().hasDie)
      {
        this.transform.parent = (Transform) null;
        this.rigidbody.isKinematic = false;
        this.collider.enabled = true;
        this.Stuck = false;
        this.photonView.RPC("SetSmokeState", PhotonTargets.All, new object[1]
        {
          (object) true
        });
      }
      if (this.bombType_0 != BombType.Timed || !this.photonView.isMine)
        return;
      this.float_0 -= Time.deltaTime;
      if ((double) this.float_0 > 0.0 || this.exploded)
        return;
      this.photonView.RPC("Detonate", PhotonTargets.All);
    }

    private IEnumerator method_0(float time)
    {
      yield return (object) new WaitForSeconds(time);
      PhotonNetwork.Destroy(this.gameObject);
    }
  }
}
