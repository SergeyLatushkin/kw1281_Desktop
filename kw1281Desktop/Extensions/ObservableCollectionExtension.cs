using System.Collections.ObjectModel;
using BitFab.KW1281Test.Models;
using kw1281Desktop.Models;

namespace kw1281Desktop.Extensions
{
    internal static class ObservableCollectionExtension
    {
        public static Arg[] FlattenPairs(this ObservableCollection<ObservableAddressValuePair> pairs)
        {
            var result = new List<Arg>(pairs.Count * 2);
            foreach (ObservableAddressValuePair pair in pairs)
            {
                result.Add(pair.Address ?? string.Empty);
                result.Add(pair.Value ?? string.Empty);
            }

            return [.. result];
        }
    }
}
