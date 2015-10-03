using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Cecil.Inject
{
    public static class CollectionUtils
    {
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (T var in self)
            {
                action(var);
            }
        }

        public static IEnumerable<T> Range<T>(this IEnumerable<T> self, int start, int end)
        {
            return self.Where((e, i) => start <= i && i <= end);
        }

        public static IEnumerable<T> Slice<T>(this IEnumerable<T> self, int start, int count)
        {
            return self.Where((e, i) => start <= i && i < start + count);
        }
    }
}