using UnityEngine;

public class BTN_BackToMain : MonoBehaviour
{
    private void OnClick()
    {
        NGUITools.SetActive(base.transform.parent.gameObject, false);
        NGUITools.SetActive(UIMainReferences.Main.panelMain, true);
        FengCustomInputs.Main.menuOn = false;
        PhotonNetwork.Disconnect();
    }
}