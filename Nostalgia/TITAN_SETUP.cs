using Photon;
using System;
using UnityEngine;

public class TITAN_SETUP : Photon.MonoBehaviour
{
    public GameObject eye;
    private CostumeHair hair;
    private GameObject hair_go_ref;
    private int hairType;
    private GameObject part_hair;

    private void Awake()
    {
        CostumeHair.init();
        CharacterMaterials.init();
        HeroCostume.init();
        this.hair_go_ref = new GameObject();
        this.eye.transform.parent = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head").transform;
        this.hair_go_ref.transform.position = (Vector3) ((this.eye.transform.position + (Vector3.up * 3.5f)) + (base.transform.forward * 5.2f));
        this.hair_go_ref.transform.rotation = this.eye.transform.rotation;
        this.hair_go_ref.transform.RotateAround(this.eye.transform.position, base.transform.right, -20f);
        this.hair_go_ref.transform.localScale = new Vector3(210f, 210f, 210f);
        this.hair_go_ref.transform.parent = base.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head").transform;
    }

    public void setFacialTexture(GameObject go, int id)
    {
        if (id >= 0)
        {
            float num = 0.25f;
            float num2 = 0.125f;
            float x = num2 * ((int) (((float) id) / 8f));
            float y = -num * (id % 4);
            go.renderer.material.mainTextureOffset = new Vector2(x, y);
        }
    }

    public void setHair()
    {
        UnityEngine.Object.Destroy(this.part_hair);
        int index = UnityEngine.Random.Range(0, CostumeHair.hairsM.Length);
        if (index == 3)
        {
            index = 9;
        }
        this.hairType = index;
        this.hair = CostumeHair.hairsM[index];
        if (this.hair.hair == string.Empty)
        {
            this.hair = CostumeHair.hairsM[9];
            this.hairType = 9;
        }
        this.part_hair = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Character/" + this.hair.hair));
        this.part_hair.transform.parent = this.hair_go_ref.transform.parent;
        this.part_hair.transform.position = this.hair_go_ref.transform.position;
        this.part_hair.transform.rotation = this.hair_go_ref.transform.rotation;
        this.part_hair.transform.localScale = this.hair_go_ref.transform.localScale;
        this.part_hair.renderer.material = CharacterMaterials.materials[this.hair.texture];
        this.part_hair.renderer.material.color = HeroCostume.costume[UnityEngine.Random.Range(0, HeroCostume.costume.Length - 5)].hair_color;
        int id = UnityEngine.Random.Range(1, 8);
        this.setFacialTexture(this.eye, id);
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
        {
            object[] parameters = new object[] { this.hairType, id, this.part_hair.renderer.material.color.r, this.part_hair.renderer.material.color.g, this.part_hair.renderer.material.color.b };
            base.photonView.RPC("setHairPRC", PhotonTargets.OthersBuffered, parameters);
        }
    }

    [RPC]
    private void setHairPRC(int type, int eye_type, float c1, float c2, float c3)
    {
        UnityEngine.Object.Destroy(this.part_hair);
        this.hair = CostumeHair.hairsM[type];
        this.hairType = type;
        if (this.hair.hair != string.Empty)
        {
            GameObject obj2 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Character/" + this.hair.hair));
            obj2.transform.parent = this.hair_go_ref.transform.parent;
            obj2.transform.position = this.hair_go_ref.transform.position;
            obj2.transform.rotation = this.hair_go_ref.transform.rotation;
            obj2.transform.localScale = this.hair_go_ref.transform.localScale;
            obj2.renderer.material = CharacterMaterials.materials[this.hair.texture];
            obj2.renderer.material.color = new Color(c1, c2, c3);
            this.part_hair = obj2;
        }
        this.setFacialTexture(this.eye, eye_type);
    }

    public void setPunkHair()
    {
        UnityEngine.Object.Destroy(this.part_hair);
        this.hair = CostumeHair.hairsM[3];
        this.hairType = 3;
        GameObject obj2 = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Character/" + this.hair.hair));
        obj2.transform.parent = this.hair_go_ref.transform.parent;
        obj2.transform.position = this.hair_go_ref.transform.position;
        obj2.transform.rotation = this.hair_go_ref.transform.rotation;
        obj2.transform.localScale = this.hair_go_ref.transform.localScale;
        obj2.renderer.material = CharacterMaterials.materials[this.hair.texture];
        switch (UnityEngine.Random.Range(1, 4))
        {
            case 1:
                obj2.renderer.material.color = FengColor.hairPunk1;
                break;

            case 2:
                obj2.renderer.material.color = FengColor.hairPunk2;
                break;

            case 3:
                obj2.renderer.material.color = FengColor.hairPunk3;
                break;
        }
        this.part_hair = obj2;
        this.setFacialTexture(this.eye, 0);
        if ((IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER) && base.photonView.isMine)
        {
            object[] parameters = new object[] { this.hairType, 0, this.part_hair.renderer.material.color.r, this.part_hair.renderer.material.color.g, this.part_hair.renderer.material.color.b };
            base.photonView.RPC("setHairPRC", PhotonTargets.OthersBuffered, parameters);
        }
    }
}

