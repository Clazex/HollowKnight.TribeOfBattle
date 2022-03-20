namespace TribeOfBattle;

public sealed partial class TribeOfBattle : IGlobalSettings<GlobalSettings> {
	public static GlobalSettings GlobalSettings { get; private set; } = new();
	public void OnLoadGlobal(GlobalSettings s) => GlobalSettings = s;
	public GlobalSettings OnSaveGlobal() => GlobalSettings;
}

public sealed class GlobalSettings {
	public bool modifyPantheons = false;
	public bool unfixBugs = false;
}
