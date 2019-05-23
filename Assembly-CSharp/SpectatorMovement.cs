using Optimization.Caching;
using UnityEngine;

public class SpectatorMovement : MonoBehaviour
{
    private float speed = 100f;
    public bool disable;

    private void Update()
    {
        if (this.disable)
        {
            return;
        }
        float num;
        if (FengCustomInputs.Main.isInput[InputCode.up])
        {
            num = 1f;
        }
        else if (FengCustomInputs.Main.isInput[InputCode.down])
        {
            num = -1f;
        }
        else
        {
            num = 0f;
        }
        float num2;
        if (FengCustomInputs.Main.isInput[InputCode.left])
        {
            num2 = -1f;
        }
        else if (FengCustomInputs.Main.isInput[InputCode.right])
        {
            num2 = 1f;
        }
        else
        {
            num2 = 0f;
        }
        if (num > 0f)
        {
            base.transform.position += base.transform.Forward() * this.speed * Time.deltaTime;
        }
        if (num < 0f)
        {
            base.transform.position -= base.transform.Forward() * this.speed * Time.deltaTime;
        }
        if (num2 > 0f)
        {
            base.transform.position += base.transform.right * this.speed * Time.deltaTime;
        }
        if (num2 < 0f)
        {
            base.transform.position -= base.transform.right * this.speed * Time.deltaTime;
        }
        if (FengCustomInputs.Main.isInput[InputCode.leftRope])
        {
            base.transform.position -= base.transform.Up() * this.speed * Time.deltaTime;
        }
        if (FengCustomInputs.Main.isInput[InputCode.rightRope])
        {
            base.transform.position += base.transform.Up() * this.speed * Time.deltaTime;
        }
    }
}