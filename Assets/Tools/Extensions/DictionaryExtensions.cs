using System.Collections.Generic;

public static class DictionaryExtensions
{
  public static V TryGetOrDefault<K, V>(this Dictionary<K, V> d, K key, V defaultValue = default(V))
  {
    return d.ContainsKey(key) ? d[key] : defaultValue;
  }
}