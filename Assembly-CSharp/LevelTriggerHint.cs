using UnityEngine;

public class LevelTriggerHint : MonoBehaviour
{
    private bool on;
    public string content;

    public HintType myhint;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            this.on = true;
        }
    }

    private void Start()
    {
        if (!FengGameManagerMKII.Level.Hint)
        {
            base.enabled = false;
        }
        if (this.content != string.Empty)
        {
            return;
        }
        switch (this.myhint)
        {
            case HintType.MOVE:
                this.content = string.Concat(new string[]
                {
                "Hello soldier!\nWelcome to Attack On Titan Tribute Game!\n Press [F7D358]",
                FengCustomInputs.Main.inputString[InputCode.up],
                FengCustomInputs.Main.inputString[InputCode.left],
                FengCustomInputs.Main.inputString[InputCode.down],
                FengCustomInputs.Main.inputString[InputCode.right],
                "[-] to Move."
                });
                break;

            case HintType.TELE:
                this.content = "Move to [82FA58]green warp point[-] to proceed.";
                break;

            case HintType.CAMA:
                this.content = string.Concat(new string[]
                {
                "Press [F7D358]",
                FengCustomInputs.Main.inputString[InputCode.camera],
                "[-] to change camera mode\nPress [F7D358]",
                FengCustomInputs.Main.inputString[InputCode.hideCursor],
                "[-] to hide or show the cursor."
                });
                break;

            case HintType.JUMP:
                this.content = "Press [F7D358]" + FengCustomInputs.Main.inputString[InputCode.jump] + "[-] to Jump.";
                break;

            case HintType.JUMP2:
                this.content = "Press [F7D358]" + FengCustomInputs.Main.inputString[InputCode.up] + "[-] towards a wall to perform a wall-run.";
                break;

            case HintType.HOOK:
                this.content = string.Concat(new string[]
                {
                "Press and Hold[F7D358] ",
                FengCustomInputs.Main.inputString[InputCode.leftRope],
                "[-] or [F7D358]",
                FengCustomInputs.Main.inputString[InputCode.rightRope],
                "[-] to launch your grapple.\nNow Try hooking to the [>3<] box. "
                });
                break;

            case HintType.HOOK2:
                this.content = string.Concat(new string[]
                {
                "Press and Hold[F7D358] ",
                FengCustomInputs.Main.inputString[InputCode.bothRope],
                "[-] to launch both of your grapples at the same Time.\n\nNow aim between the two black blocks. \nYou will see the mark '<' and '>' appearing on the blocks. \nThen press ",
                FengCustomInputs.Main.inputString[InputCode.bothRope],
                " to hook the blocks."
                });
                break;

            case HintType.SUPPLY:
                this.content = "Press [F7D358]" + FengCustomInputs.Main.inputString[InputCode.reload] + "[-] to reload your blades.\n Move to the supply station to refill your gas and blades.";
                break;

            case HintType.DODGE:
                this.content = "Press [F7D358]" + FengCustomInputs.Main.inputString[InputCode.dodge] + "[-] to Dodge.";
                break;

            case HintType.ATTACK:
                this.content = string.Concat(new string[]
                {
                "Press [F7D358]",
                FengCustomInputs.Main.inputString[InputCode.attack0],
                "[-] to Attack. \nPress [F7D358]",
                FengCustomInputs.Main.inputString[InputCode.attack1],
                "[-] to use special attack.\n***You can only kill a titan by slashing his [FA5858]NAPE[-].***\n\n"
                });
                break;
        }
    }

    private void Update()
    {
        if (this.on)
        {
            FengGameManagerMKII.FGM.ShowHUDInfoCenter(this.content + "\n\n\n\n\n");
            this.on = false;
        }
    }
}