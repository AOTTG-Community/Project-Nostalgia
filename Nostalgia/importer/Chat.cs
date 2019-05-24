// Decompiled with JetBrains decompiler
// Type: BRS.Chat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BRS
{
  public class Chat
  {
    public bool isActive;

    public bool ProcessCommand(string Command)
    {
      if (!this.isActive)
        return false;
      switch (Command.Split(' ')[0])
      {
        case "/ChatTest":
          Chat.smethod_0("Congrats, you can use trusted user commands!");
          return true;
        case "/killtitans":
          foreach (TITAN titan in ((IEnumerable<GameObject>) GameObject.FindGameObjectsWithTag("titan")).Where<GameObject>((Func<GameObject, bool>) (titan => !titan.GetComponent<TITAN>().hasDie)).Select<GameObject, TITAN>((Func<GameObject, TITAN>) (titan => titan.GetComponent<TITAN>())))
          {
            titan.photonView.RPC("netDie", PhotonTargets.OthersBuffered);
            if ((UnityEngine.Object) titan.grabbedTarget != (UnityEngine.Object) null)
              titan.grabbedTarget.GetPhotonView().RPC("netUngrabbed", PhotonTargets.All);
            titan.netDie();
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().sendKillInfo(false, "[FFCC00]Dead Master[-]", true, titan.nonAI ? (string) titan.photonView.owner.customProperties[(object) "name"] : titan.name, UnityEngine.Random.Range(2000, 9999));
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().oneTitanDown("", false);
          }
          return true;
        default:
          return false;
      }
    }

    private static void smethod_0(string Line)
    {
      GameObject.Find("Chatroom").GetComponent<InRoomChat>().addLINE(Line);
    }
  }
}
