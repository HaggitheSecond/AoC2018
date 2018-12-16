using System.Collections.Generic;

namespace AoC2018.Extensions
{
    public static class LinqExtensions
    {
        public static void AddRange<T>(this IList<T> self, IList<T> itemsToAdd)
        {
            foreach (var currentItemToAdd in itemsToAdd) self.Add(currentItemToAdd);
        }
    }
}