
namespace GsdmlLinker.Core;

public static class GroupingExtensions
{
    public static IEnumerable<List<T>> GroupSuccessiveBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        var currentGroup = new List<T> { enumerator.Current };
        var currentKey = keySelector(enumerator.Current);

        while (enumerator.MoveNext())
        {
            var item = enumerator.Current;
            var key = keySelector(item);

            if (!EqualityComparer<TKey>.Default.Equals(key, currentKey))
            {
                // Fin du groupe courant
                yield return currentGroup;

                // Nouveau groupe
                currentGroup = [item];
                currentKey = key;
            }
            else
            {
                currentGroup.Add(item);
            }
        }

        // Dernier groupe
        yield return currentGroup;
    }
}
