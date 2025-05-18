using System.ComponentModel.DataAnnotations;

namespace WebThermoThinApp.Data
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Density { get; set; }

        public double HeatCapacity { get; set; }

        public double ThermalConductivity { get; set; }

        public double Emissivity { get; set; }

    }
}
