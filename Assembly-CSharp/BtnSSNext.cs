﻿using UnityEngine;

public class BtnSSNext : MonoBehaviour
{
    private void OnClick()
    {
        if (base.gameObject.transform.parent.gameObject.GetComponent<CharacterCreationComponent>())
        {
            base.gameObject.transform.parent.gameObject.GetComponent<CharacterCreationComponent>().nextOption();
        }
        else
        {
            base.gameObject.transform.parent.gameObject.GetComponent<CharacterStatComponent>().nextOption();
        }
    }
}