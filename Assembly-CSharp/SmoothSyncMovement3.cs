using Optimization.Caching;
using UnityEngine;

public class SmoothSyncMovement3 : Photon.MonoBehaviour
{
    private Transform baseT;
    private Vector3 correctPlayerPos = Vectors.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;
    public bool disabled;
    public float SmoothingDelay = 5f;

    public void Awake()
    {
        if (BasePV == null || BasePV.observed != this)
        {
            Debug.LogWarning(this + " is not observed by this object's photonView! OnPhotonSerializeView() in this class won't be used.");
        }
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            base.enabled = false;
        }
        baseT = transform;
    }

    private void OnEnable()
    {
        baseT = transform;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(baseT.position);
            stream.SendNext(baseT.rotation);
        }
        else
        {
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }

    public void Update()
    {
        if (this.disabled)
        {
            return;
        }
        if (!BasePV.IsMine)
        {
            baseT.position = Vector3.Lerp(baseT.position, this.correctPlayerPos, Time.deltaTime * this.SmoothingDelay);
            baseT.rotation = Quaternion.Lerp(baseT.rotation, this.correctPlayerRot, Time.deltaTime * this.SmoothingDelay);
        }
    }
}