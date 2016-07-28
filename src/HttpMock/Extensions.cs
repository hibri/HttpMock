using System.Collections.Generic;
using System.Linq;

namespace HttpMock
{
    public static class DictionaryExtensionMethods
    {
        public static bool ContentEquals<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> otherDictionary)
        {
            dictionary = dictionary ?? new Dictionary<TKey, TValue>();
            otherDictionary = otherDictionary ?? new Dictionary<TKey, TValue>();

            return otherDictionary.OrderBy(kvp => kvp.Key)
                .SequenceEqual(dictionary.OrderBy(kvp => kvp.Key));
        }
    }
}
