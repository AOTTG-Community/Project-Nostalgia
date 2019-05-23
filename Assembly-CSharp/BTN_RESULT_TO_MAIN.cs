using UnityEngine;

public class BTN_RESULT_TO_MAIN : MonoBehaviour
{
    private void OnClick()
    {
        Time.timeScale = 1f;
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Disconnect();
        }
        IN_GAME_MAIN_CAMERA.GameType = GameType.Stop;
        FengGameManagerMKII.FGM.GameStart = false;
        Screen.lockCursor = false;
        Screen.showCursor = true;
        FengCustomInputs.Main.menuOn = false;
        UnityEngine.Object.Destroy(FengGameManagerMKII.FGM);
        Application.LoadLevel("menu");
    }
}