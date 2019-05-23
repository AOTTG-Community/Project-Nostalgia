using UnityEngine;

public class supplyCheck : MonoBehaviour
{
    private float elapsedTime;
    private float stepTime = 1f;

    private void Start()
    {
    }

    private void Update()
    {
        this.elapsedTime += Time.deltaTime;
        if (this.elapsedTime > this.stepTime)
        {
            this.elapsedTime -= this.stepTime;
            GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject gameObject in array)
            {
                if (gameObject.GetComponent<HERO>() != null)
                {
                    if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
                    {
                        if (Vector3.Distance(gameObject.transform.position, base.transform.position) < 1.5f)
                        {
                            gameObject.GetComponent<HERO>().getSupply();
                        }
                    }
                    else if (gameObject.GetPhotonView().IsMine && Vector3.Distance(gameObject.transform.position, base.transform.position) < 1.5f)
                    {
                        gameObject.GetComponent<HERO>().getSupply();
                    }
                }
            }
        }
    }
}