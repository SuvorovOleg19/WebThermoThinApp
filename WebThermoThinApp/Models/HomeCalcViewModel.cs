using Microsoft.AspNetCore.Mvc;
using WebThermoThinApp.Data;

namespace WebThermoThinApp.Models
{
    public class HomeCalcViewModel
    {
        public List<CalculationResult> Result { get; set; }
        public string Shape { get; set; }
        public string Orientation { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Radius { get; set; }
        public double? InitialTemp { get; set; }
        public double? EnvTemp { get; set; }
        public double MaterialDensity { get; set; }
        public double MaterialHeatCapacity { get; set; }
        public double MaterialConductivity { get; set; }
        public double? CoolingTime { get; set; }
        public double? Emissivity { get; set; }
    }
}
