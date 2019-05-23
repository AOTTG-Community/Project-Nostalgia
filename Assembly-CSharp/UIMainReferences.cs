using System;
using UnityEngine;

public class UIMainReferences : MonoBehaviour
{
    private static bool isGAMEFirstLaunch = true;
    public GameObject panelCredits;
    public GameObject PanelDisconnect;
    public GameObject panelMain;
    public GameObject PanelMultiJoinPrivate;
    public GameObject PanelMultiPWD;
    public GameObject panelMultiROOM;
    public GameObject panelMultiSet;
    public GameObject panelMultiStart;
    public GameObject PanelMultiWait;
    public GameObject panelOption;
    public GameObject panelSingleSet;
    public GameObject PanelSnapShot;
    public static string version = "01042015";

    private void Start()
    {
        NGUITools.SetActive(this.panelMain, true);
        GameObject.Find("VERSION").GetComponent<UILabel>().text = version;
        if (isGAMEFirstLaunch)
        {
            isGAMEFirstLaunch = false;
            GameObject target = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("InputManagerController"));
            target.name = "InputManagerController";
            UnityEngine.Object.DontDestroyOnLoad(target);
        }
    }
}

