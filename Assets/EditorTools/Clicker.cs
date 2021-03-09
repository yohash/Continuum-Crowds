﻿using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Linq;

public class Clicker : MonoBehaviour
{
  void Start()
  {
    DoStuff();
  }

  private async Task DoStuff()
  {
    var handle = ToolsHandle.Instant;
    handle.TileGenButton.onClick.Invoke();

    var tilegen = TileGenerator.Instant;
    tilegen.LoadCsvFiles();
    await tilegen.GenerateTilesAsync();
    handle.BackButton.onClick.Invoke();

    handle.ContinuumCrowdsButton.onClick.Invoke();
    var cctester = CCTester.Instant;
    //cctester.SolveButton.onClick.Invoke();
    cctester.UnitToggleA.isOn = true;
    cctester.UnitToggleA.onValueChanged.Invoke(cctester.UnitToggleA.isOn);

    cctester.DensityToggle.isOn = true;
    cctester.DensityToggle.onValueChanged.Invoke(cctester.DensityToggle.isOn);

    cctester.VelocityToggle.isOn = true;
    cctester.VelocityToggle.onValueChanged.Invoke(cctester.VelocityToggle.isOn);

    cctester.TestUnitA.transform.position = new Vector3(5, 0, 5);
    cctester.TestUnitA.GetComponent<CCTestUnit>().dimensions = new Vector2(5, 5);
    cctester.TestUnitA.GetComponent<CCTestUnit>().falloff = 3;

  }
}
