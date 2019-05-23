﻿using UnityEngine;

public class BTN_PAUSE_MENU_QUIT : MonoBehaviour
{
    private void OnClick()
    {
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            Time.timeScale = 1f;
        }
        else
        {
            PhotonNetwork.Disconnect();
        }
        Screen.lockCursor = false;
        Screen.showCursor = true;
        IN_GAME_MAIN_CAMERA.GameType = GameType.Stop;
        FengGameManagerMKII.FGM.GameStart = false;
        FengCustomInputs.Main.menuOn = false;
        UnityEngine.Object.Destroy(FengGameManagerMKII.FGM);
        Application.LoadLevel("menu");
    }
}