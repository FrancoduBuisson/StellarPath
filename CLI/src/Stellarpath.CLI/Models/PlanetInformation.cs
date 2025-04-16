using System;
using System.Collections.Generic;
using System.Linq;
namespace Stellarpath.CLI.Models;

public class PlanetInformation  
{
    public string Name { get; set; }
    public double Mass { get; set; }
    public double Radius { get; set; }
    public double Period { get; set; }
    public double SemiMajorAxis { get; set; }
    public double Temperature { get; set; }
    public double DistanceLightYear { get; set; }
    public double HostStarMass { get; set; }
    public double HostStarTemperature { get; set; }
}
