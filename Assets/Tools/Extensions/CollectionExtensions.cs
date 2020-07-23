using System.Collections;
using System.Collections.Generic;

public static class CollectionExtensions
{
  public static List<T> TryAdd<T>(this List<T> list, T add)
  {
    if (list.Contains(add)) return list;
    list.Add(add);
    return list;
  }
}