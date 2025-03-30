using Unity.Netcode;
using UnityEngine;
using UnityChess;
using System.Collections.Generic;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer LocalPlayerInstance;

    public Side PlayerSide;

    public bool IsMyTurn()
    {
        return GameManager.Instance.SideToMove == PlayerSide;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayerInstance = this;

            // FIX: Always assign based on Server's spawn order
            PlayerSide = IsHost ? Side.White : Side.Black;

            Debug.Log($"You are playing as {PlayerSide}");
        }

        NetworkPlayer.RegisterPlayer(this);
    }

    private static Dictionary<ulong, NetworkPlayer> players = new();

    public static void RegisterPlayer(NetworkPlayer player)
    {
        players[player.OwnerClientId] = player;
    }

    public static NetworkPlayer GetPlayerByClientId(ulong clientId)
    {
        return players.TryGetValue(clientId, out var player) ? player : null;
    }
}
