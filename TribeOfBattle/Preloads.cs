using System.IO;

using HutongGames.PlayMaker.Actions;

namespace TribeOfBattle;

public sealed partial class TribeOfBattle {
	private static GameObject? mantisPreload = null;

	private static GameObject? traitorPrefab = null;
	private static GameObject? traitorInstance = null;

	private static bool didGlobalChange = false;

	private static GameObject? grassPrefab = null;

	private static tk2dSprite? traitorSprite = null;
	private static Texture2D? traitorTexOrig = null;
	private static readonly Lazy<Texture2D> traitorTex = new(() => {
		Stream stream = typeof(TribeOfBattle).Assembly
			.GetManifestResourceStream("TribeOfBattle.Resources.TraitorLord.png");
		MemoryStream ms = new((int) stream.Length);

		stream.CopyTo(ms);
		stream.Close();

		byte[] bytes = ms.ToArray();
		ms.Close();

		Texture2D texture2D = new(2, 2);
		texture2D.LoadImage(bytes, true);

		return texture2D;
	});

	public override List<(string, string)> GetPreloadNames() => new() {
		("GG_Mantis_Lords_V", "Mantis Battle/Battle Main/Mantis Lord"),
		("GG_Traitor_Lord", "Battle Scene/Wave 3/Mantis Traitor Lord")
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> preloads) {
		if (preloads == null) {
			return;
		}

		mantisPreload = preloads["GG_Mantis_Lords_V"]["Mantis Battle/Battle Main/Mantis Lord"];
		traitorPrefab = CreateTraitorPrefab(preloads["GG_Traitor_Lord"]["Battle Scene/Wave 3/Mantis Traitor Lord"]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static GameObject CreateTraitorPrefab(GameObject preload) {
		var prefab = GameObject.Instantiate(preload);
		GameObject.DontDestroyOnLoad(prefab);

		grassPrefab = prefab.GetComponent<PersonalObjectPool>().startupPool
			.Filter(pool => pool.prefab.name == "mega_mantis_tall_slash")
			.Single().prefab.Child("Grass")!;

		traitorSprite = prefab.GetComponent<tk2dSprite>();
		traitorTexOrig = traitorSprite.CurrentSprite.material.mainTexture as Texture2D;

		prefab.name = "TOB Traitor Lord";
		prefab.transform.SetPosition2D(35f, 22f);
		UObject.DestroyImmediate(prefab.Child("Pt Mist")!);
		UObject.DestroyImmediate(prefab.GetComponent<EnemyDeathEffects>());
		UObject.DestroyImmediate(prefab.GetComponent<InfectedEnemyEffects>());
		prefab.AddComponent<DontClinkGates>();
		prefab.AddComponent<NonBouncer>().active = false;

		#region Dream Convo

		EnemyDreamnailReaction dreamConvo = prefab.GetComponent<EnemyDreamnailReaction>();
		EnemyDreamnailReaction mantisDreamConvo = mantisPreload!.GetComponent<EnemyDreamnailReaction>();

		ReflectionHelper.SetField(
			dreamConvo,
			"convoAmount",
			ReflectionHelper.GetField<EnemyDreamnailReaction, int>(mantisDreamConvo, "convoAmount")
		);
		dreamConvo.SetConvoTitle(
			ReflectionHelper.GetField<EnemyDreamnailReaction, string>(mantisDreamConvo, "convoTitle")
		);

		#endregion

		#region Constrain Position

		ConstrainPosition constraint = prefab.AddComponent<ConstrainPosition>();
		constraint.constrainX = true;
		constraint.xMin = 24.4f;
		constraint.xMax = 36.7f;

		#endregion

		#region Hit Effect

		EnemyHitEffectsUninfected hitEffect = prefab.AddComponent<EnemyHitEffectsUninfected>();
		ReflectionHelper.SetField(prefab.GetComponent<HealthManager>(), "hitEffectReceiver", hitEffect as IHitEffectReciever);

		EnemyHitEffectsUninfected mantisEffect = mantisPreload!.GetComponent<EnemyHitEffectsUninfected>();
		hitEffect.effectOrigin = mantisEffect.effectOrigin;
		hitEffect.audioPlayerPrefab = mantisEffect.audioPlayerPrefab;
		hitEffect.enemyDamage = mantisEffect.enemyDamage;
		hitEffect.uninfectedHitPt = mantisEffect.uninfectedHitPt;
		hitEffect.slashEffectGhost1 = mantisEffect.slashEffectGhost1;
		hitEffect.slashEffectGhost2 = mantisEffect.slashEffectGhost2;
		hitEffect.uninfectedHitPt = mantisEffect.uninfectedHitPt;

		#endregion

		return prefab;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void InstantiateTraitor(GameObject parent) {
		traitorInstance = GameObject.Instantiate(traitorPrefab)!;
		traitorInstance!.transform.parent = parent.transform;

		HealthManager hm = traitorInstance.GetComponent<HealthManager>();
		DamageHero dh = traitorInstance.GetComponent<DamageHero>();
		NonBouncer nb = traitorInstance.GetComponent<NonBouncer>();
		PlayMakerFSM fsm = traitorInstance.LocateMyFSM("Mantis");

		#region FSM Changes

		#region Intro

		// Sync with Mantis Lords roar
		fsm.RemoveAction("Cloth?", 1);
		fsm.RemoveAction("Cloth?", 0);
		fsm.GetAction<Wait>("Emerge Dust", 1).time = 1.6f;
		fsm.RemoveAction("Emerge Dust", 2);
		fsm.RemoveTransition("Intro Land", FsmEvent.Finished.Name);
		fsm.AddTransition("Intro Land", "SUBS ROAR 2", "Roar");
		fsm.RemoveTransition("Roar", FsmEvent.Finished.Name);
		fsm.AddTransition("Roar", "MLORD START SUB", "Roar End");
		fsm.AddCustomAction("Roar End", () => {
			hm.IsInvincible = false;
			dh.damageDealt = 1;
		});

		// Remove roar & title
		fsm.RemoveAction("Roar", 9);
		fsm.RemoveAction("Roar", 8);
		fsm.RemoveAction("Roar", 7);
		fsm.RemoveAction("Roar", 6);

		#endregion

		#region Death

		FsmState stateDefeated = fsm.AddState("Defeated");
		fsm.ChangeTransition("Idle", "DEATH SCENE", stateDefeated.Name);
		fsm.AddAction(stateDefeated.Name, fsm.GetAction("Walk", 2));
		fsm.AddAction(stateDefeated.Name, fsm.GetAction("Idle", 1));
		fsm.AddCustomAction(stateDefeated.Name, () => {
			hm.IsInvincible = true;
			dh.damageDealt = 0;
			nb.active = true;
			traitorInstance.Child("Head Box")!.SetActive(false);
		});

		FsmState stateBowAntic = fsm.AddState("Bow Antic");
		fsm.AddTransition(stateDefeated.Name, "MANTIS DEFEAT", stateBowAntic.Name);
		fsm.AddAction(stateBowAntic.Name, new Wait() { time = 1.6f, finishEvent = FsmEvent.Finished });

		FsmState stateBow = fsm.AddState("Bow");
		fsm.AddTransition(stateBowAntic.Name, FsmEvent.Finished.Name, stateBow.Name);
		fsm.AddAction(stateBow.Name, fsm.GetAction("Roar Recover", 0));

		#endregion

		#region Change attacks

		// Fix Y
		fsm.GetAction<FloatCompare>("Fall", 10).float2 = 9.6f;
		fsm.GetAction<SetPosition>("Intro Land", 0).y = 9.6f;
		fsm.GetAction<FloatCompare>("DSlash", 13).float2 = 9.6f;
		fsm.GetAction<SetPosition>("Land", 0).y = 9.6f;

		// No feint
		fsm.ChangeTransition("Attack Choice", "SLASH", "Attack Antic");

		// No walk
		fsm.RemoveAction("Idle", 5);

		// No back feint
		fsm.RemoveAction("Too Close?", 1);
		fsm.RemoveAction("Too Close?", 0);

		// Slam ignore HP
		fsm.RemoveAction("Slam?", 2);

		#endregion

		#endregion

		if (!GlobalSettings.unfixBugs) {
			// roar lock
			fsm.RemoveAction("Roar", 3);
			fsm.RemoveAction("Roar", 2);

			// Not interactable until roar finish
			hm.IsInvincible = true;
			dh.damageDealt = 0;
			nb.active = true;
			fsm.RemoveAction("Fall", 6);
			fsm.AddCustomAction("Roar End", () => nb.active = false);

			// Fix slamming after death
			fsm.InsertAction("Slam?", new BoolTest() {
				boolVariable = fsm.FsmVariables.FindFsmBool("Zero HP"),
				isTrue = FsmEvent.Finished
			}, 0);
			fsm.InsertAction("Repeat?", new BoolTest() {
				boolVariable = fsm.FsmVariables.FindFsmBool("Zero HP"),
				isTrue = FsmEvent.Finished
			}, 0);
		}
	}
}
