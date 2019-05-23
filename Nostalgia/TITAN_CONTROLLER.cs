using System;
using UnityEngine;

public class TITAN_CONTROLLER : MonoBehaviour
{
    public Camera currentCamera;
    public FengCustomInputs inputManager;
    public bool isAttackDown;
    public bool isAttackIIDown;
    public bool isJumpDown;
    public bool isSuicide;
    public bool isWALKDown;
    public float targetDirection;

    private void Start()
    {
        this.inputManager = GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>();
        this.currentCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            base.enabled = false;
        }
    }

    private void Update()
    {
        int num;
        int num2;
        if (this.inputManager.isInput[InputCode.up])
        {
            num = 1;
        }
        else if (this.inputManager.isInput[InputCode.down])
        {
            num = -1;
        }
        else
        {
            num = 0;
        }
        if (this.inputManager.isInput[InputCode.left])
        {
            num2 = -1;
        }
        else if (this.inputManager.isInput[InputCode.right])
        {
            num2 = 1;
        }
        else
        {
            num2 = 0;
        }
        if ((num2 != 0) || (num != 0))
        {
            float y = this.currentCamera.transform.rotation.eulerAngles.y;
            float num5 = Mathf.Atan2((float) num, (float) num2) * 57.29578f;
            num5 = -num5 + 90f;
            float num3 = y + num5;
            this.targetDirection = num3;
        }
        else
        {
            this.targetDirection = -874f;
        }
        this.isAttackDown = false;
        this.isJumpDown = false;
        this.isAttackIIDown = false;
        this.isSuicide = false;
        if (this.inputManager.isInputDown[InputCode.attack0])
        {
            this.isAttackDown = true;
        }
        if (this.inputManager.isInputDown[InputCode.attack1])
        {
            this.isAttackIIDown = true;
        }
        if (this.inputManager.isInputDown[InputCode.bothRope])
        {
            this.isJumpDown = true;
        }
        if (this.inputManager.isInputDown[InputCode.restart])
        {
            this.isSuicide = true;
        }
        this.isWALKDown = this.inputManager.isInput[InputCode.jump];
    }
}

