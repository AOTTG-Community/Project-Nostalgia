using Optimization.Caching;
using System;
using Photon;
using UnityEngine;

class SmoothSyncMovement : Photon.MonoBehaviour, IPunObservable
{
    private Rigidbody baseR;
    private Transform baseT;
    private Vector3 correctPlayerPos = Vectors.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;
    private Vector3 correctPlayerVelocity = Vectors.zero;
    public Quaternion CorrectCameraRot;
    public bool Disabled;
    public bool PhotonCamera;
    public float SmoothingDelay = 5f;


    public void Awake()
    {
        if (base.BasePV == null || base.BasePV.observed != this)
        {
            Debug.LogWarning(this + " is not observed by this object's photonView! OnPhotonSerializeView() in this class won't be used.");
        }
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            base.enabled = false;
        }
        baseT = transform;
        baseR = rigidbody;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(baseT.position);
            stream.SendNext(baseT.rotation);
            stream.SendNext(baseR.velocity);
            if (PhotonCamera)
            {
                stream.SendNext(IN_GAME_MAIN_CAMERA.MainT.rotation);
            }
        }
        else
        {
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
            this.correctPlayerVelocity = (Vector3)stream.ReceiveNext();
            if (PhotonCamera)
            {
                CorrectCameraRot = (Quaternion)stream.ReceiveNext();
            }
        }
    }

    public void Update()
    {
        if (Disabled || BasePV.IsMine) return;
        var dt = Time.deltaTime;
        if (baseT != null)
        {
            baseT.position = Vector3.Lerp(baseT.position, this.correctPlayerPos, dt * this.SmoothingDelay);
            baseT.rotation = Quaternion.Lerp(baseT.rotation, this.correctPlayerRot, dt * this.SmoothingDelay);
        }
        if (baseR != null) baseR.velocity = this.correctPlayerVelocity;
    }

}
