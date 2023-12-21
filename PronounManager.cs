using System;
using BepInEx;
using LC_API.ServerAPI;
using UnityEngine;

namespace PronounsIndicator;

class PronounManager : MonoBehaviour
{
    private static int playerCount;
    private static float lobbyCheckTimer = 0f;
    private static bool checkPronouns = true;
    public static string SIG_SEND_PRONOUNS = "PronounIndicatorSendPronouns";
    public static string SIG_REQ_PRONOUNS = "PronounIndicatorRequestPronouns";

    public void Update()
    {
        if (GameNetworkManager.Instance != null)
        {
            if (playerCount < GameNetworkManager.Instance.connectedPlayers)
            {
                lobbyCheckTimer = -4.5f;
                checkPronouns = true;
            }
            playerCount = GameNetworkManager.Instance.connectedPlayers;
        }
        if (lobbyCheckTimer < 0)
        {
            lobbyCheckTimer += Time.deltaTime;
        }
        else if (checkPronouns && HUDManager.Instance != null)
        {
            checkPronouns = false;
            Plugin.logger.LogInfo("Requesting pronouns...");
            Networking.Broadcast("Pronouns_Broadcast", SIG_REQ_PRONOUNS);

            SendPronouns();
        }
    }

    public static void SendPronouns()
    {
        ulong localClientId = GameNetworkManager.Instance.localPlayerController.playerClientId;
        var pronouns = Plugin.pronouns.Value.Replace("/", "#");
        if (!pronouns.IsNullOrWhiteSpace()) {
            Plugin.logger.LogInfo("Sending pronouns...");
            Networking.Broadcast($"{localClientId}:{pronouns}", SIG_SEND_PRONOUNS);
        }
    }

    public static void NetGetString(string data, string signature)
    {
        Plugin.logger.LogInfo($"{signature} Received data: {data}");
        if (signature == SIG_SEND_PRONOUNS)
        {
            Plugin.logger.LogInfo($"Received {data}");
            var split = data.Split(":", 2);
            var clientId = Int32.Parse(split[0]);
            var pronouns = split[1].Replace("#", "/");
            StartOfRound.Instance.allPlayerScripts[clientId].usernameBillboardText.text = StartOfRound.Instance.allPlayerScripts[clientId].playerUsername + $"<br>({pronouns})";
        }

        if (signature == SIG_REQ_PRONOUNS)
        {
            SendPronouns();
        }
    }
}