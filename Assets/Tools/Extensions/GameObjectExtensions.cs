using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
  public static T GetOrAddComponent<T>(this GameObject go) where T : Component
  {
    T component = go.GetComponent<T>();
    if (component != null) { return component; }
    component = go.AddComponent<T>();
    return component;
  }

  public static void SoftSetActive(this GameObject go, bool activeSelf)
  {
    if (go && go.activeSelf != activeSelf) {
      go.SetActive(activeSelf);
    }
  }
}