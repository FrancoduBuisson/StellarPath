namespace StellarPath.API.Core.Models
{
    public class Planet
    {
        public string name { get; set; }
        public double mass { get; set; } 
        public double radius { get; set; }
        public double period { get; set; }
        public double semi_major_axis { get; set; } 
        public double temperature { get; set; }
        public double distance_light_year { get; set; }
        public double host_star_mass {get;set;}
        public double host_star_temperature {get; set;}
    }
}
