using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Region
{
  [SerializeField] private List<Vector2> locations;

  public Region()
  {
    locations = new List<Vector2>();
  }

  public void AddLocation(Vector2 location)
  {
    if (locations.Contains(location)) {
      Debug.LogWarning("Region already contains location: " + location);
      return;
    }
    locations.Add(location);
  }
}