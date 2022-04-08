namespace TribeOfBattle;

public sealed partial class TribeOfBattle : Mod, ITogglableMod {
	public static TribeOfBattle? Instance { get; private set; }

	private static readonly Lazy<string> Version = new(() => Assembly
		.GetExecutingAssembly()
		.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
		.InformationalVersion
#if DEBUG
		+ "-dev"
#endif
	);

	public override string GetVersion() => Version.Value;

	public TribeOfBattle() =>
		USceneManager.activeSceneChanged += EditScene;

	public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloads) {
		if (Instance != null) {
			LogWarn("Attempting to initialize multiple times, operation rejected");
			return;
		}

		Instance = this;
		SavePreloads(preloads);
		HealthShare.HealthShare.GlobalSettings.modifyBosses = false;

		ModHooks.LanguageGetHook += Localize;
	}

	public void Unload() {
		ModHooks.LanguageGetHook -= Localize;

		Instance = null;
	}

	private static string Localize(string key, string sheet, string orig) =>
		running || GameManager.instance.sceneName == "GG_Workshop"
		? sheet switch {
			"CP3" => key switch {
				"NAME_MANTIS_LORD_V" => "Name".Localize(),
				"GG_S_MANTIS_LORD_V" => "Desc".Localize(),
				_ => orig
			},
			"Titles" => key switch {
				"SISTERS_SUPER" => "Title/Super".Localize(),
				"SISTERS_MAIN" => "Title/Main".Localize(),
				"SISTERS_SUB" => "Title/Sub".Localize(),
				_ => orig,
			},
			_ => orig,
		}
		: orig;
}
