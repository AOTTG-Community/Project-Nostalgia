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
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            base.enabled = false;
        }
    }

    private void Update()
    {
        int num;
        if (FengCustomInputs.Main.isInput[InputCode.up])
        {
            num = 1;
        }
        else if (FengCustomInputs.Main.isInput[InputCode.down])
        {
            num = -1;
        }
        else
        {
            num = 0;
        }
        int num2;
        if (FengCustomInputs.Main.isInput[InputCode.left])
        {
            num2 = -1;
        }
        else if (FengCustomInputs.Main.isInput[InputCode.right])
        {
            num2 = 1;
        }
        else
        {
            num2 = 0;
        }
        if (num2 != 0 || num != 0)
        {
            float y = IN_GAME_MAIN_CAMERA.MainCamera.transform.rotation.eulerAngles.y;
            float num3 = Mathf.Atan2((float)num, (float)num2) * 57.29578f;
            num3 = -num3 + 90f;
            float num4 = y + num3;
            this.targetDirection = num4;
        }
        else
        {
            this.targetDirection = -874f;
        }
        this.isAttackDown = false;
        this.isJumpDown = false;
        this.isAttackIIDown = false;
        this.isSuicide = false;
        if (FengCustomInputs.Main.isInputDown[InputCode.attack0])
        {
            this.isAttackDown = true;
        }
        if (FengCustomInputs.Main.isInputDown[InputCode.attack1])
        {
            this.isAttackIIDown = true;
        }
        if (FengCustomInputs.Main.isInputDown[InputCode.bothRope])
        {
            this.isJumpDown = true;
        }
        if (FengCustomInputs.Main.isInputDown[InputCode.restart])
        {
            this.isSuicide = true;
        }
        this.isWALKDown = FengCustomInputs.Main.isInput[InputCode.jump];
    }
}