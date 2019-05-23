using System;
using UnityEngine;

class RacingKillTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject gameObject = other.gameObject;
        if (gameObject.layer == 8)
        {
            gameObject = gameObject.transform.root.gameObject;
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi && gameObject.GetPhotonView() != null && gameObject.GetPhotonView().IsMine)
            {
                HERO component = gameObject.GetComponent<HERO>();
                if (component != null && !component.HasDied())
                {
                    component.markDie();
                    component.BasePV.RPC("netDie2", PhotonTargets.All, new object[]
                    {
                        -1,
                         "Server "
                    });
                }
            }
        }
    }
}
