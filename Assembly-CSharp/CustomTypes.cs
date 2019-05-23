using Optimization.Caching;
using ExitGames.Client.Photon;
using UnityEngine;

internal static class CustomTypes
{
    private static bool DeserializePhotonPlayer(byte[] bytes, out object obj)
    {
        if (bytes.Length != 4)
        {
            obj = PhotonNetwork.player;
            return false;
        }
        int num = 0;
        int key;
        Protocol.Deserialize(out key, bytes, ref num);
        PhotonPlayer player = PhotonPlayer.Find(key);
        if (player == null)
        {
            obj = PhotonNetwork.player;
            return false;
        }
        obj = player;
        return true;
    }

    private static bool DeserializeQuaternion(byte[] bytes, out object obj)
    {
        if (bytes.Length != 16)
        {
            obj = Quaternion.identity;
            return false;
        }
        Quaternion quaternion = default(Quaternion);
        int num = 0;
        Protocol.Deserialize(out quaternion.w, bytes, ref num);
        Protocol.Deserialize(out quaternion.x, bytes, ref num);
        Protocol.Deserialize(out quaternion.y, bytes, ref num);
        Protocol.Deserialize(out quaternion.z, bytes, ref num);
        obj = quaternion;
        return true;
    }

    private static bool DeserializeVector2(byte[] bytes, out object obj)
    {
        if (bytes.Length != 8)
        {
            obj = Vectors.v2zero;
            return false;
        }
        Vector2 vector = default(Vector2);
        int num = 0;
        Protocol.Deserialize(out vector.x, bytes, ref num);
        Protocol.Deserialize(out vector.y, bytes, ref num);
        obj = vector;
        return true;
    }

    private static bool DeserializeVector3(byte[] bytes, out object obj)
    {
        if (bytes.Length != 12)
        {
            obj = Vectors.zero;
            return false;
        }
        Vector3 vector = default(Vector3);
        int num = 0;
        Protocol.Deserialize(out vector.x, bytes, ref num);
        Protocol.Deserialize(out vector.y, bytes, ref num);
        Protocol.Deserialize(out vector.z, bytes, ref num);
        obj = vector;
        return true;
    }

    private static byte[] SerializePhotonPlayer(object customobject)
    {
        int id = ((PhotonPlayer)customobject).ID;
        byte[] array = new byte[4];
        int num = 0;
        Protocol.Serialize(id, array, ref num);
        return array;
    }

    private static byte[] SerializeQuaternion(object obj)
    {
        Quaternion quaternion = (Quaternion)obj;
        byte[] array = new byte[16];
        int num = 0;
        Protocol.Serialize(quaternion.w, array, ref num);
        Protocol.Serialize(quaternion.x, array, ref num);
        Protocol.Serialize(quaternion.y, array, ref num);
        Protocol.Serialize(quaternion.z, array, ref num);
        return array;
    }

    private static byte[] SerializeVector2(object customobject)
    {
        Vector2 vector = (Vector2)customobject;
        byte[] array = new byte[8];
        int num = 0;
        Protocol.Serialize(vector.x, array, ref num);
        Protocol.Serialize(vector.y, array, ref num);
        return array;
    }

    private static byte[] SerializeVector3(object customobject)
    {
        Vector3 vector = (Vector3)customobject;
        int num = 0;
        byte[] array = new byte[12];
        Protocol.Serialize(vector.x, array, ref num);
        Protocol.Serialize(vector.y, array, ref num);
        Protocol.Serialize(vector.z, array, ref num);
        return array;
    }

    internal static void Register()
    {
        PhotonPeer.RegisterType(typeof(Vector2), 87, new SerializeMethod(CustomTypes.SerializeVector2), new DeserializeMethod(CustomTypes.DeserializeVector2));
        PhotonPeer.RegisterType(typeof(Vector3), 86, new SerializeMethod(CustomTypes.SerializeVector3), new DeserializeMethod(CustomTypes.DeserializeVector3));
        PhotonPeer.RegisterType(typeof(Quaternion), 81, new SerializeMethod(CustomTypes.SerializeQuaternion), new DeserializeMethod(CustomTypes.DeserializeQuaternion));
        PhotonPeer.RegisterType(typeof(PhotonPlayer), 80, new SerializeMethod(CustomTypes.SerializePhotonPlayer), new DeserializeMethod(CustomTypes.DeserializePhotonPlayer));
    }
}