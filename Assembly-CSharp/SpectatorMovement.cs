using System;
using UnityEngine;

public class SpectatorMovement : MonoBehaviour
{
    public bool disable;
    public FengCustomInputs inputManager;
    private float speed = 100f;

    private void Start()
    {
        this.inputManager = GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>();
    }

    private void Update()
    {
        if (!this.disable)
        {
            float num;
            float num2;
            if (this.inputManager.isInput[InputCode.up])
            {
                num2 = 1f;
            }
            else if (this.inputManager.isInput[InputCode.down])
            {
                num2 = -1f;
            }
            else
            {
                num2 = 0f;
            }
            if (this.inputManager.isInput[InputCode.left])
            {
                num = -1f;
            }
            else if (this.inputManager.isInput[InputCode.right])
            {
                num = 1f;
            }
            else
            {
                num = 0f;
            }
            if (num2 > 0f)
            {
                Transform transform = base.transform;
                transform.position += (Vector3) ((base.transform.forward * this.speed) * Time.deltaTime);
            }
            if (num2 < 0f)
            {
                Transform transform2 = base.transform;
                transform2.position -= (Vector3) ((base.transform.forward * this.speed) * Time.deltaTime);
            }
            if (num > 0f)
            {
                Transform transform3 = base.transform;
                transform3.position += (Vector3) ((base.transform.right * this.speed) * Time.deltaTime);
            }
            if (num < 0f)
            {
                Transform transform4 = base.transform;
                transform4.position -= (Vector3) ((base.transform.right * this.speed) * Time.deltaTime);
            }
            if (this.inputManager.isInput[InputCode.leftRope])
            {
                Transform transform5 = base.transform;
                transform5.position -= (Vector3) ((base.transform.up * this.speed) * Time.deltaTime);
            }
            if (this.inputManager.isInput[InputCode.rightRope])
            {
                Transform transform6 = base.transform;
                transform6.position += (Vector3) ((base.transform.up * this.speed) * Time.deltaTime);
            }
        }
    }
}

