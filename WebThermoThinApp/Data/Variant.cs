using System.ComponentModel.DataAnnotations;

namespace WebThermoThinApp.Data
{
    public class Variant
    {
        [Key]
        public int Id { get; set; }

        public string Shape { get; set; } // "cylinder", "sphere", "plate"
        public string Orientation { get; set; } // "vertical", "horizontal"
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double InitialTemp { get; set; } // Начальная температура тела
        public double EnvTemp { get; set; } // Температура окружающей среды
        public string Material { get; set; } // Материал тела
        public double CoolingTime { get; set; } // Время охлаждения
        public double Emissivity { get; set; } // Коэффициент излучения
    }
}
