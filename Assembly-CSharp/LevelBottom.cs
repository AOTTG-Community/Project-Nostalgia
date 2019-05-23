﻿using Optimization.Caching;
using UnityEngine;

public class LevelBottom : MonoBehaviour
{
    public GameObject link;

    public BottomType type;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (this.type == BottomType.Die)
            {
                if (other.gameObject.GetComponent<HERO>())
                {
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Multi)
                    {
                        if (other.gameObject.GetPhotonView().IsMine)
                        {
                            other.gameObject.GetComponent<HERO>().netDie(base.rigidbody.velocity * 50f, false, -1, string.Empty, true);
                        }
                    }
                    else
                    {
                        other.gameObject.GetComponent<HERO>().die(other.gameObject.rigidbody.velocity * 50f, false);
                    }
                }
            }
            else if (this.type == BottomType.Teleport)
            {
                if (this.link != null)
                {
                    other.gameObject.transform.position = this.link.transform.position;
                }
                else
                {
                    other.gameObject.transform.position = Vectors.zero;
                }
            }
        }
    }
}