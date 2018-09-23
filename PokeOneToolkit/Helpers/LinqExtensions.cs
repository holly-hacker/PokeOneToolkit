using System.Collections.Generic;
using System.Linq;

namespace PokeOneToolkit.Helpers
{
    internal static class LinqExtensions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> haystack, IEnumerable<T> needles) => needles.All(haystack.Contains);
        public static bool ContainsAll<T>(this IEnumerable<T> haystack, params T[] needles) => needles.All(haystack.Contains);

        public static IEnumerable<T> AllButLast<T>(this IEnumerable<T> enumerable)
        {
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator()) {
                if (!enumerator.MoveNext()) yield break;
                T prev = enumerator.Current;
            
                while (enumerator.MoveNext()) {
                    yield return prev;
                    prev = enumerator.Current;
                }
            }
        }
    }
}
