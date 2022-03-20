namespace TribeOfBattle.Util;

internal static class EnumerableUtil {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IEnumerable<U> Map<T, U>(this IEnumerable<T> self, Func<T, U> f) =>
		self.Select(f);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IEnumerable<T> Filter<T>(this IEnumerable<T> self, Func<T, bool> f) =>
		self.Where(f);
}
