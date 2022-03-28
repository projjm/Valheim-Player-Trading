using UnityEngine;

namespace PlayerTrading
{
    class ZNetUtils
    {
        public static void UnregisterRPC(string name)
        {
            var m_functions = ZRoutedRpc.instance.m_functions;
            int stableHashCode = StringExtensionMethods.GetStableHashCode(name);
            if (m_functions.ContainsKey(stableHashCode))
                m_functions.Remove(stableHashCode);
        }

        public static bool IsServer() => ZNet.instance.IsServer();

        // Server only
        public static ZDOID GetZDOID(long uid)
        {
            ZNetPeer peer = ZNet.instance.GetPeer(uid);
            if (peer == null)
                Debug.Log("UTIL: Failed to get ZDOID from UID");
            return peer!.m_characterID;
        }

        public static Player GetPlayer(ZDOID ZDOID)
        {
            foreach (Player player in Player.m_players)
            {
                if (player.GetZDOID() == ZDOID)
                    return player;
            }
            Debug.Log("UTIL: Failed to get Player from ZDOID");
            return null!;
        }

        public static Player GetPlayer(long uid)
        {
            foreach (Player player in Player.m_players)
            {
                if (player.GetOwner() == uid)
                    return player;
            }
            Debug.Log("UTIL: Failed to get Player from UID");
            return null!;
        }

        public static long GetUID(Player player) => player.GetOwner();

    }
}
