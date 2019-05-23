using UnityEngine;

public class LevelTriggerCheckPoint : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
            {
                FengGameManagerMKII.FGM.checkpoint = base.gameObject;
            }
            else if (other.gameObject.GetComponent<HERO>().BasePV.IsMine)
            {
                FengGameManagerMKII.FGM.checkpoint = base.gameObject;
            }
        }
    }

    private void Start()
    {
    }
}