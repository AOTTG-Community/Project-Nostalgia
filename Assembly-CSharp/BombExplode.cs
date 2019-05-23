using System;
using Photon;
using UnityEngine;
using Optimization;

public class BombExplode : Photon.MonoBehaviour
{
    //!!!!NOT TESTED!!!!

    public GameObject myExplosion;

    public void OnEnable()
    {
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
        {
            if (BasePV != null)
            {
                PhotonPlayer owner = BasePV.owner;
                //Uncomment this when going to make a team mode
                //if (VSettings.TeamMode > 0)
                //{
                //    int rcteam = owner.RCteam;
                //    if (rcteam != 1)
                //    {
                //        if (rcteam != 2)
                //        {
                //            base.GetComponent<ParticleSystem>().startColor = new Color(owner.RCBombR, owner.RCBombG, owner.RCBombB, Mathf.Max(0.5f, owner.RCBombA));
                //        }
                //        else
                //        {
                //            base.GetComponent<ParticleSystem>().startColor = Color.magenta;
                //        }
                //    }
                //    else
                //    {
                //        base.GetComponent<ParticleSystem>().startColor = Color.cyan;
                //    }
                //}
                //else
                //{
                //    base.GetComponent<ParticleSystem>().startColor = new Color(owner.RCBombR, owner.RCBombG, owner.RCBombB, Mathf.Max(0.5f, owner.RCBombA));
                //}
                //Delete this if uncomment upper
                base.GetComponent<ParticleSystem>().startColor = new Color(owner.RCBombR, owner.RCBombG, owner.RCBombB, Mathf.Max(0.5f, owner.RCBombA));
                float num6 = Optimization.Extensions.GetFloat(owner.Properties["RCBombRadius"]) * 2f;
                num6 = Mathf.Clamp(num6, 40f, 120f);
                base.GetComponent<ParticleSystem>().startSize = num6;
            }
        }
    }
}
