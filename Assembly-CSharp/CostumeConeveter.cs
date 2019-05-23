using ExitGames.Client.Photon;
using UnityEngine;

public class CostumeConeveter
{
    private static int DivisionToInt(DIVISION id)
    {
        if (id == DIVISION.TheGarrison)
        {
            return 0;
        }
        if (id == DIVISION.TheMilitaryPolice)
        {
            return 1;
        }
        if (id == DIVISION.TheSurveryCorps)
        {
            return 2;
        }
        if (id == DIVISION.TraineesSquad)
        {
            return 3;
        }
        return 2;
    }

    private static DIVISION IntToDivision(int id)
    {
        if (id == 0)
        {
            return DIVISION.TheGarrison;
        }
        if (id == 1)
        {
            return DIVISION.TheMilitaryPolice;
        }
        if (id == 2)
        {
            return DIVISION.TheSurveryCorps;
        }
        if (id == 3)
        {
            return DIVISION.TraineesSquad;
        }
        return DIVISION.TheSurveryCorps;
    }

    private static Sex IntToSex(int id)
    {
        if (id == 0)
        {
            return Sex.Female;
        }
        if (id == 1)
        {
            return Sex.Male;
        }
        return Sex.Male;
    }

    private static UNIFORM_TYPE IntToUniformType(int id)
    {
        if (id == 0)
        {
            return UNIFORM_TYPE.CasualA;
        }
        if (id == 1)
        {
            return UNIFORM_TYPE.CasualB;
        }
        if (id == 2)
        {
            return UNIFORM_TYPE.UniformA;
        }
        if (id == 3)
        {
            return UNIFORM_TYPE.UniformB;
        }
        if (id == 4)
        {
            return UNIFORM_TYPE.CasualAHSS;
        }
        return UNIFORM_TYPE.UniformA;
    }

    private static int SexToInt(Sex id)
    {
        if (id == Sex.Female)
        {
            return 0;
        }
        if (id == Sex.Male)
        {
            return 1;
        }
        return 1;
    }

    private static int UniformTypeToInt(UNIFORM_TYPE id)
    {
        if (id == UNIFORM_TYPE.CasualA)
        {
            return 0;
        }
        if (id == UNIFORM_TYPE.CasualB)
        {
            return 1;
        }
        if (id == UNIFORM_TYPE.UniformA)
        {
            return 2;
        }
        if (id == UNIFORM_TYPE.UniformB)
        {
            return 3;
        }
        if (id == UNIFORM_TYPE.CasualAHSS)
        {
            return 4;
        }
        return 2;
    }

    public static void HeroCostumeToLocalData(HeroCostume costume, string slot)
    {
        slot = slot.ToUpper();
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.sex, CostumeConeveter.SexToInt(costume.sex));
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.costumeId, costume.costumeId);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.heroCostumeId, costume.id);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.cape, (!costume.cape) ? 0 : 1);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.hairInfo, costume.hairInfo.id);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.eye_texture_id, costume.eye_texture_id);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.beard_texture_id, costume.beard_texture_id);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.glass_texture_id, costume.glass_texture_id);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.skin_color, costume.skin_color);
        PlayerPrefs.SetFloat(slot + PhotonPlayerProperty.hair_color1, costume.hair_color.r);
        PlayerPrefs.SetFloat(slot + PhotonPlayerProperty.hair_color2, costume.hair_color.g);
        PlayerPrefs.SetFloat(slot + PhotonPlayerProperty.hair_color3, costume.hair_color.b);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.division, CostumeConeveter.DivisionToInt(costume.division));
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.statSPD, costume.stat.Spd);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.statGAS, costume.stat.Gas);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.statBLA, costume.stat.Bla);
        PlayerPrefs.SetInt(slot + PhotonPlayerProperty.statACL, costume.stat.Acl);
        PlayerPrefs.SetString(slot + PhotonPlayerProperty.statSKILL, costume.stat.skillID);
    }

    public static void HeroCostumeToPhotonData(HeroCostume costume, PhotonPlayer player)
    {
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.sex,
                CostumeConeveter.SexToInt(costume.sex)
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.costumeId,
                costume.costumeId
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.heroCostumeId,
                costume.id
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.cape,
                costume.cape
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.hairInfo,
                costume.hairInfo.id
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.eye_texture_id,
                costume.eye_texture_id
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.beard_texture_id,
                costume.beard_texture_id
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.glass_texture_id,
                costume.glass_texture_id
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.skin_color,
                costume.skin_color
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.hair_color1,
                costume.hair_color.r
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.hair_color2,
                costume.hair_color.g
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.hair_color3,
                costume.hair_color.b
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.division,
                CostumeConeveter.DivisionToInt(costume.division)
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.statSPD,
                costume.stat.Spd
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.statGAS,
                costume.stat.Gas
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.statBLA,
                costume.stat.Bla
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.statACL,
                costume.stat.Acl
            }
        });
        player.SetCustomProperties(new Hashtable
        {
            {
                PhotonPlayerProperty.statSKILL,
                costume.stat.skillID
            }
        });
    }

    public static HeroCostume LocalDataToHeroCostume(string slot)
    {
        slot = slot.ToUpper();
        if (!PlayerPrefs.HasKey(slot + PhotonPlayerProperty.sex))
        {
            return HeroCostume.costume[0];
        }
        HeroCostume heroCostume = new HeroCostume();
        heroCostume = new HeroCostume();
        heroCostume.sex = CostumeConeveter.IntToSex(PlayerPrefs.GetInt(slot + PhotonPlayerProperty.sex));
        heroCostume.id = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.heroCostumeId);
        heroCostume.costumeId = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.costumeId);
        heroCostume.cape = (PlayerPrefs.GetInt(slot + PhotonPlayerProperty.cape) == 1);
        heroCostume.hairInfo = ((heroCostume.sex != Sex.Male) ? CostumeHair.hairsF[PlayerPrefs.GetInt(slot + PhotonPlayerProperty.hairInfo)] : CostumeHair.hairsM[PlayerPrefs.GetInt(slot + PhotonPlayerProperty.hairInfo)]);
        heroCostume.eye_texture_id = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.eye_texture_id);
        heroCostume.beard_texture_id = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.beard_texture_id);
        heroCostume.glass_texture_id = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.glass_texture_id);
        heroCostume.skin_color = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.skin_color);
        heroCostume.hair_color = new Color(PlayerPrefs.GetFloat(slot + PhotonPlayerProperty.hair_color1), PlayerPrefs.GetFloat(slot + PhotonPlayerProperty.hair_color2), PlayerPrefs.GetFloat(slot + PhotonPlayerProperty.hair_color3));
        heroCostume.division = CostumeConeveter.IntToDivision(PlayerPrefs.GetInt(slot + PhotonPlayerProperty.division));
        heroCostume.stat = new HeroStat();
        heroCostume.stat.Spd = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.statSPD);
        heroCostume.stat.Gas = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.statGAS);
        heroCostume.stat.Bla = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.statBLA);
        heroCostume.stat.Acl = PlayerPrefs.GetInt(slot + PhotonPlayerProperty.statACL);
        heroCostume.stat.skillID = PlayerPrefs.GetString(slot + PhotonPlayerProperty.statSKILL);
        heroCostume.setBodyByCostumeId(-1);
        heroCostume.setMesh();
        heroCostume.setTexture();
        return heroCostume;
    }

    public static HeroCostume PhotonDataToHeroCostume(PhotonPlayer player)
    {
        HeroCostume heroCostume = new HeroCostume();
        heroCostume = new HeroCostume();
        heroCostume.sex = CostumeConeveter.IntToSex((int)player.Properties[PhotonPlayerProperty.sex]);
        heroCostume.costumeId = (int)player.Properties[PhotonPlayerProperty.costumeId];
        heroCostume.id = (int)player.Properties[PhotonPlayerProperty.heroCostumeId];
        heroCostume.cape = (bool)player.Properties[PhotonPlayerProperty.cape];
        heroCostume.hairInfo = ((heroCostume.sex != Sex.Male) ? CostumeHair.hairsF[(int)player.Properties[PhotonPlayerProperty.hairInfo]] : CostumeHair.hairsM[(int)player.Properties[PhotonPlayerProperty.hairInfo]]);
        heroCostume.eye_texture_id = (int)player.Properties[PhotonPlayerProperty.eye_texture_id];
        heroCostume.beard_texture_id = (int)player.Properties[PhotonPlayerProperty.beard_texture_id];
        heroCostume.glass_texture_id = (int)player.Properties[PhotonPlayerProperty.glass_texture_id];
        heroCostume.skin_color = (int)player.Properties[PhotonPlayerProperty.skin_color];
        heroCostume.hair_color = new Color((float)player.Properties[PhotonPlayerProperty.hair_color1], (float)player.Properties[PhotonPlayerProperty.hair_color2], (float)player.Properties[PhotonPlayerProperty.hair_color3]);
        heroCostume.division = CostumeConeveter.IntToDivision((int)player.Properties[PhotonPlayerProperty.division]);
        heroCostume.stat = new HeroStat();
        heroCostume.stat.Spd = (int)player.Properties[PhotonPlayerProperty.statSPD];
        heroCostume.stat.Gas = (int)player.Properties[PhotonPlayerProperty.statGAS];
        heroCostume.stat.Bla = (int)player.Properties[PhotonPlayerProperty.statBLA];
        heroCostume.stat.Acl = (int)player.Properties[PhotonPlayerProperty.statACL];
        heroCostume.stat.skillID = (string)player.Properties[PhotonPlayerProperty.statSKILL];
        heroCostume.setBodyByCostumeId(-1);
        heroCostume.setMesh();
        heroCostume.setTexture();
        return heroCostume;
    }
}