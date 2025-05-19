using System.ComponentModel.DataAnnotations;

namespace WebThermoThinApp.Models
{
    public class HomeCalcViewModel
    {
        public List<CalculationResult>? Result { get; set; }

        [Required(ErrorMessage = "Выберите форму тела")]
        public string Shape { get; set; }

        public string Orientation { get; set; }

        // Только для пластины
        public double? Length { get; set; }

        // Только для пластины
        public double? Width { get; set; }

        // Для цилиндра и пластины
        public double? Height { get; set; }

        // Для шара и цилиндра
        public double? Radius { get; set; }

        [Required(ErrorMessage = "Введите начальную температуру")]
        public double? InitialTemp { get; set; }

        [Required(ErrorMessage = "Введите температуру среды")]
        public double? EnvTemp { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Плотность должна быть положительной")]
        public double MaterialDensity { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Теплоемкость должна быть положительной")]
        public double MaterialHeatCapacity { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "Теплопроводность должна быть положительной")]
        public double MaterialConductivity { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Время охлаждения должно быть положительным")]
        public double? CoolingTime { get; set; }

        [Required(ErrorMessage = "Введите коэффициент излучения")]
        [Range(0.0, 1.0, ErrorMessage = "Коэффициент излучения должен быть от 0 до 1")]
        public double? Emissivity { get; set; }
    }
}
