using System;
using UnityEngine;

public class BTN_PAUSE_MENU_CONTINUE : MonoBehaviour
{
    private void OnClick()
    {
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.SINGLE)
        {
            Time.timeScale = 1f;
        }
        GameObject obj2 = GameObject.Find("UI_IN_GAME");
        NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[0], true);
        NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[1], false);
        NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[2], false);
        NGUITools.SetActive(obj2.GetComponent<UIReferArray>().panels[3], false);
        if (!GameObject.Find("MainCamera").GetComponent<IN_GAME_MAIN_CAMERA>().enabled)
        {
            Screen.showCursor = true;
            Screen.lockCursor = true;
            GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().menuOn = false;
            GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = false;
            GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = false;
        }
        else
        {
            IN_GAME_MAIN_CAMERA.isPausing = false;
            if (IN_GAME_MAIN_CAMERA.cameraMode == CAMERA_TYPE.TPS)
            {
                Screen.showCursor = false;
                Screen.lockCursor = true;
            }
            else
            {
                Screen.showCursor = false;
                Screen.lockCursor = false;
            }
            GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().menuOn = false;
            GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().justUPDATEME();
        }
    }
}

