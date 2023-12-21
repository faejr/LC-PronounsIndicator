using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LC_API.ServerAPI;
using UnityEngine;

namespace PronounsIndicator
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LC_API.MyPluginInfo.PLUGIN_GUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "faejr.pronounsindicator";
        public const string ModName = "Pronouns Indicator";
        public const string ModVersion = "1.0.0";
        public static ManualLogSource logger;
        public static bool Initialized { get; private set; }
        public static ConfigEntry<string> pronouns;

        private void Awake()
        {
            logger = Logger;
            Logger.LogInfo($"Plugin {ModGUID} is loaded!");
            Initialized = false;

            pronouns = Config.Bind<string>("pronouns", "pronouns", "");

            LC_API.ClientAPI.CommandHandler.RegisterCommand("pronouns", (string[] args) =>
            {
                if (args.Length > 0 && args[0].Contains("/") && args[0].Length < 19)
                {
                    pronouns.SetSerializedValue(args[0]);
                    PronounManager.SendPronouns();
                }
            });
        }

        internal void Start()
        {
            Initialize();
        }

        internal void OnDestroy()
        {
            Initialize();
        }

        void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                GameObject gameObject = new GameObject("PronounIndicator");
                DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<PronounManager>();
                Logger.LogInfo("Pronoun Manager Started!");
                Networking.GetString += PronounManager.NetGetString;
            }
        }
    }
}
