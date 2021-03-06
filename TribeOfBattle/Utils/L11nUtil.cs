using Language;

namespace TribeOfBattle.Utils;

internal static class L11nUtil {
	private const string langPrefix = "TribeOfBattle.Resources.Lang.";
	private const string langSuffix = ".json";

	internal static Lazy<Dictionary<string, Lazy<Dictionary<string, string>>>> Dict = new(() => {
		Assembly asm = typeof(TribeOfBattle).Assembly;
		return LangUtil.LoadLangs(asm, asm
			.GetManifestResourceNames()
			.Filter(name => name.EnclosedWith(langPrefix, langSuffix))
			.Map(name => (
				name.StripStart(langPrefix).StripEnd(langSuffix),
				name
			)));
	});

	internal static string Localize(this string key) {
		_ = Dict.Value.TryGetValue(LangUtil.CurrentLang, out Lazy<Dictionary<string, string>>? table);
		table ??= Dict.Value[LanguageCode.EN.ToIdentifier()];
		return table.Value.TryGetValue(key, out string value) ? value : key;
	}
}
