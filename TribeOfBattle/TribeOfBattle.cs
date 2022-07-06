using Osmi;

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
		OsmiHooks.SceneChangeHook += EditScene;

	public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloads) {
		if (Instance != null) {
			LogWarn("Attempting to initialize multiple times, operation rejected");
			return;
		}

		Instance = this;
		SavePreloads(preloads);

		On.HeroController.Start += HookLangGetDelayed;
	}

	public void Unload() {
		On.HeroController.Start -= HookLangGetDelayed;
		ModHooks.LanguageGetHook -= Localize;

		Instance = null;
	}

	private static void HookLangGetDelayed(On.HeroController.orig_Start orig, HeroController self) {
		orig(self);

		ModHooks.LanguageGetHook += Localize;
		On.HeroController.Start -= HookLangGetDelayed;
	}

	private static bool MantisGodsOn() => ModHooks.GetMod("MantisGods", true) != null;

	private static string Localize(string key, string sheet, string orig) =>
		(running || GameManager.instance.sceneName == "GG_Workshop") switch {
			false => orig,
			true => MantisGodsOn() switch {
				false => sheet switch {
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
				},
				true => sheet switch {
					"CP3" => key switch {
						"NAME_MANTIS_LORD_V" => "Gods/Name".Localize(),
						"GG_S_MANTIS_LORD_V" => "Gods/Desc".Localize(),
						_ => orig
					},
					"Titles" => key switch {
						"MANTIS_LORDS_SUPER" => "Gods/Title/P1/Super".Localize(),
						"MANTIS_LORDS_MAIN" => "Gods/Title/P1/Main".Localize(),
						"MANTIS_LORDS_SUB" => "Gods/Title/P1/Sub".Localize(),
						"SISTERS_SUPER" => "Gods/Title/P2/Super".Localize(),
						"SISTERS_MAIN" => "Gods/Title/P2/Main".Localize(),
						"SISTERS_SUB" => "Gods/Title/P2/Sub".Localize(),
						_ => orig,
					},
					_ => orig,
				}
			}
		};
}
