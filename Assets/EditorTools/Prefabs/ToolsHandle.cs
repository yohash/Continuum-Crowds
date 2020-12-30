using UnityEngine;
using UnityEngine.UI;

public class ToolsHandle : MonoBehaviour
{
  public static ToolsHandle Instant;

  public Button HeightMapButton;
  public Button TileGenButton;
  public Button AStarButton;
  public Button ContinuumCrowdsButton;

  public Button BackButton;

  private void Awake()
  {
    Instant = this;
  }
}
