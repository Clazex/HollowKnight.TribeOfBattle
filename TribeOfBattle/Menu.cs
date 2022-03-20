using static Modding.IMenuMod;

namespace TribeOfBattle;

public sealed partial class TribeOfBattle : IMenuMod {
	bool IMenuMod.ToggleButtonInsideMenu => true;

	List<MenuEntry> IMenuMod.GetMenuData(MenuEntry? toggleButtonEntry) => new() {
		toggleButtonEntry!.Value,
		new(
			"Option/ModifyPantheons".Localize(),
			new string[] {
				Lang.Get("MOH_OFF", "MainMenu"),
				Lang.Get("MOH_ON", "MainMenu")
			},
			"",
			i => GlobalSettings.modifyPantheons = i != 0,
			() => GlobalSettings.modifyPantheons ? 1 : 0
		),
		new(
			"Option/UnfixBugs".Localize(),
			new string[] {
				Lang.Get("MOH_OFF", "MainMenu"),
				Lang.Get("MOH_ON", "MainMenu")
			},
			"",
			i => GlobalSettings.unfixBugs = i != 0,
			() => GlobalSettings.unfixBugs ? 1 : 0
		)
	};
}
