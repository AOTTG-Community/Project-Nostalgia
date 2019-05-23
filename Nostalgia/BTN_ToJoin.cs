using System;
using UnityEngine;

public class BTN_ToJoin : MonoBehaviour
{
    private void OnClick()
    {
        NGUITools.SetActive(base.transform.parent.gameObject, false);
        NGUITools.SetActive(GameObject.Find("UIRefer").GetComponent<UIMainReferences>().PanelMultiJoinPrivate, true);
        GameObject.Find("LabelJoinInfo").GetComponent<UILabel>().text = string.Empty;
        if (PlayerPrefs.HasKey("lastIP"))
        {
            GameObject.Find("InputIP").GetComponent<UIInput>().label.text = PlayerPrefs.GetString("lastIP");
        }
        if (PlayerPrefs.HasKey("lastPort"))
        {
            GameObject.Find("InputPort").GetComponent<UIInput>().label.text = PlayerPrefs.GetString("lastPort");
        }
    }

    private void Start()
    {
        base.gameObject.GetComponent<UIButton>().isEnabled = false;
    }
}

