using HealthShare;

namespace TribeOfBattle;

public sealed partial class TribeOfBattle {
	private static bool running = false;

	private static void EditScene(Scene _, Scene next) {
		if (Instance == null) {
			goto Inactive;
		}

		if (next.name != "GG_Mantis_Lords_V") {
			goto Inactive;
		}

		if (BossSequenceController.IsInSequence && !GlobalSettings.modifyPantheons) {
			goto Inactive;
		}

		running = true;

		if (!didGlobalChange) {
			traitorSprite!.CurrentSprite.material.mainTexture = traitorTex.Value;
			grassPrefab!.SetActive(false);
			didGlobalChange = true;
		}

		GameObject battle = next.GetRootGameObjects().First(go => go.name == "Mantis Battle");

		InstantiateTraitor(battle);

		battle.Child("Mantis Lord Throne 2")
			.LocateMyFSM("Mantis Throne Main")
			.InsertCustomAction("I Stand", () => {
				traitorInstance!.SetActive(true);

				new[] { 1, 2, 3 }
					.Map(i => "Battle Sub/Mantis Lord S" + i)
					.Map(path => battle.Child(path)!)
					.Append(traitorInstance!)
					.ShareHealth(name: "Tribe Of Battle").HP =
						BossSceneController.Instance.BossLevel == 0 ? 3550 : 4750;
			}, 4);

		if (MantisGodsOn()) {
			new[] { 1, 2 }
				.Map(i => "Battle Sub/Mantis Lord S" + i)
				.Map(path => battle.Child(path)!)
				.ForEach(go => go.AddComponent<MantisHPFix>());
		}

		return;

	Inactive:
		running = false;

		if (didGlobalChange) {
			traitorSprite!.CurrentSprite.material.mainTexture = traitorTexOrig;
			grassPrefab!.SetActive(true);
			didGlobalChange = false;
		}

		return;
	}
}
