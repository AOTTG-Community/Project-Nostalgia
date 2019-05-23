using Optimization.Caching;
using UnityEngine;

public class BTN_PAUSE_MENU_CONTINUE : MonoBehaviour
{
    private void OnClick()
    {
        if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        {
            Time.timeScale = 1f;
        }
        NGUITools.SetActive(FengGameManagerMKII.UIRefer.panels[0], true);
        NGUITools.SetActive(FengGameManagerMKII.UIRefer.panels[1], false);
        NGUITools.SetActive(FengGameManagerMKII.UIRefer.panels[2], false);
        NGUITools.SetActive(FengGameManagerMKII.UIRefer.panels[3], false);
        if (!IN_GAME_MAIN_CAMERA.MainCamera.enabled)
        {
            Screen.showCursor = true;
            Screen.lockCursor = true;
            FengCustomInputs.Main.menuOn = false;
            IN_GAME_MAIN_CAMERA.SpecMov.disable = false;
            IN_GAME_MAIN_CAMERA.Look.disable = false;
            return;
        }
        IN_GAME_MAIN_CAMERA.isPausing = false;
        if (IN_GAME_MAIN_CAMERA.CameraMode == CameraType.TPS)
        {
            Screen.showCursor = false;
            Screen.lockCursor = true;
        }
        else
        {
            Screen.showCursor = false;
            Screen.lockCursor = false;
        }
        FengCustomInputs.Main.menuOn = false;
        FengCustomInputs.Main.justUPDATEME();
    }
}