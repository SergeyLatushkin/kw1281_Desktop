using kw1281Desktop.Models;
using System.Collections.ObjectModel;

namespace kw1281Desktop.Extantions
{
    internal static class ObservableCollectionExtention
    {
        public static string[] FlattenPairs(this ObservableCollection<ObservableAddressValuePair> pairs)
        {
            var result = new List<string>(pairs.Count * 2);
            foreach (ObservableAddressValuePair pair in pairs)
            {
                result.Add(pair.Address ?? string.Empty);
                result.Add(pair.Value ?? string.Empty);
            }
            return result.ToArray();
        }
    }
}
