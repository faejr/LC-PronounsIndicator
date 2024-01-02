using BepInEx;
using LC_API.Networking;
using UnityEngine;

namespace PronounsIndicator;

class PronounManager : MonoBehaviour
{
    const string SIG_SEND_PRONOUNS = "PronounIndicatorSendPronouns";
    const string SIG_REQ_PRONOUNS = "PronounIndicatorRequestPronouns";

    private static int playerCount;
    private static float lobbyCheckTimer = 0f;
    private static bool checkPronouns = true;

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

            RequestPronouns();
            SendPronouns();
        }
    }

    [NetworkMessage(SIG_SEND_PRONOUNS)]
    internal static void ReceivedPronounsHandler(ulong senderId, string pronouns)
    {
        Plugin.logger.LogInfo($"{senderId}: {pronouns}");
        AddPronounsToBillboardText(senderId, pronouns);
    }

    static void AddPronounsToBillboardText(ulong senderId, string pronouns)
    {
        var username = StartOfRound.Instance.allPlayerScripts[senderId].playerUsername;
        StartOfRound.Instance.allPlayerScripts[senderId].usernameBillboardText.text = username + $"<br>({pronouns})";
    }

    [NetworkMessage(SIG_REQ_PRONOUNS)]
    internal static void RequestPronounsHandler(ulong senderId)
    {
        Plugin.logger.LogInfo($"{senderId} requested pronouns");
        SendPronouns();
    }

    public static void SendPronouns()
    {
        var pronouns = Plugin.pronouns.Value;
        if (!pronouns.IsNullOrWhiteSpace())
        {
            Plugin.logger.LogInfo("Sending pronouns...");
            Network.Broadcast(SIG_SEND_PRONOUNS, pronouns);
        }
    }

    public static void RequestPronouns()
    {
        Network.Broadcast(SIG_REQ_PRONOUNS);
    }
}