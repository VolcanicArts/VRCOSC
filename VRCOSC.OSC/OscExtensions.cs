namespace VRCOSC.OSC
{
    internal static class OscExtensions
    {
        public static int FirstIndexAfter<T>(this IEnumerable<T> items, int start, Func<T, bool> predicate)
        {
            var itemsList = items.ToList();

            if (items == null) throw new ArgumentNullException(nameof(items));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (start >= itemsList.Count) throw new ArgumentOutOfRangeException(nameof(start));

            int retVal = 0;

            foreach (var item in itemsList)
            {
                if (retVal >= start && predicate(item)) return retVal;

                retVal++;
            }

            return -1;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
