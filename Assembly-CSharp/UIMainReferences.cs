using Optimization.Caching;
using UnityEngine;

public sealed class UIMainReferences : MonoBehaviour
{
    private static bool isGAMEFirstLaunch = true;
    public const string VersionShow = "AoTTG v. 04.01.2015 (Optimized)";
    public static string ConnectField = "01042015";
    public static UIMainReferences Main;
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

    private void Awake()
    {
        Main = this;
    }

    private System.Collections.IEnumerator OnOpen()
    {
        yield return StartCoroutine(Optimization.RCManager.DownloadAssets());
        yield return StartCoroutine(Optimization.Labels.LoadFonts());
        Pool.Create();
        //Optimization.Labels.CreateLabels();
        Optimization.Labels.VERSION = VersionShow;
    }

    private void Start()
    {
        GameObject.Find("VERSION").GetComponent<UILabel>().text = "Loading...";
        NGUITools.SetActive(this.panelMain, true);
        StartCoroutine(OnOpen());
        if (isGAMEFirstLaunch)
        {
            isGAMEFirstLaunch = false;
            var inputs = (GameObject)Instantiate(CacheResources.Load("InputManagerController"));
            inputs.name = "InputManagerController";
            DontDestroyOnLoad(inputs);
        }
    }
}