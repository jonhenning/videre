using System.Collections.Generic;
namespace Videre.Core.Extensions
{
    public static class CollectionExtensions
    {
        //todo: move to CodeEndeavors.Extensions
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> copyFrom, bool newDict = false)
        {
            //todo: use linq with SelectMany?
            IDictionary<TKey, TValue> result;
            result = newDict ? new Dictionary<TKey, TValue>() : source;
            if (newDict)
            {
                foreach (var x in source)
                    result[x.Key] = x.Value;
            }
            foreach (var x in copyFrom)
                result[x.Key] = x.Value;
            return result;
        }
    }
}