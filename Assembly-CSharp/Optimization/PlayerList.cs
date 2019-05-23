using System.Text;
using UnityEngine;

namespace Optimization
{
    class PlayerList
    {
        private TextMesh label;

        public PlayerList()
        {
            new System.Threading.Thread(() =>
            {
                while (label == null)
                {
                    FindLabel();
                    System.Threading.Thread.Sleep(100);
                    if (label != null)
                    {
                        Update();
                        return;
                    }
                }
            })
            { IsBackground = true, Priority = System.Threading.ThreadPriority.Lowest }.Start();
        }

        private void FindLabel()
        {
            label = GameObject.Find("LabelInfoTopLeft").GetComponent<TextMesh>();
        }

        public void Update()
        {
            var bld = new StringBuilder();
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                var player = PhotonNetwork.playerList[i];
                bld.Append($"[FFFFFF]# {player.ID} ");
                bld.Append(player.IsLocal ? "[FFCC00]>>[-] " : "");
                bld.Append(player.IsMasterClient ? "[MC] " : "");
                bld.Append(player.Dead ? $"[{ColorSet.color_red}]*dead* " : "");
                bld.Append(player.IsTitan ? $"[{ColorSet.color_titan_player}][T] " : (player.Team == 2 ? $"[{ColorSet.color_human_1}][A] " : $"[{ColorSet.color_human}][H] "));
                bld.AppendLine($"{player.UIName}[FFFFFF]: {player.Kills}/{player.Deaths}/{player.Max_Dmg}/{player.Total_Dmg}");
            }
            Labels.TopLeft = bld.ToString().ToRGBA();
        }
    }
}
