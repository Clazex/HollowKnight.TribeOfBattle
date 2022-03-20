namespace TribeOfBattle.Util;

internal static class MiscUtil {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool EnclosedWith(this string self, string start, string end) =>
		self.StartsWith(start) && self.EndsWith(end);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string StripStart(this string self, string val) =>
		self.StartsWith(val) ? self.Substring(val.Length) : self;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string StripEnd(this string self, string val) =>
		self.EndsWith(val) ? self.Substring(0, self.Length - val.Length) : self;


	internal static GameObject? Child(this GameObject self, string name) =>
		self.transform.Find(name)?.gameObject;
}
