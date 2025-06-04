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
        [Range(0.0001, double.MaxValue, ErrorMessage = "Начальная температура должна быть больше 0")]
        public double? InitialTemp { get; set; }

        [Required(ErrorMessage = "Введите температуру среды")]
        [Range(-20, 40, ErrorMessage = "Температура среды должна быть от -20 до +40")]
        public double? EnvTemp { get; set; }

        [Required(ErrorMessage = "Выберите материал или введите свойства")]
        public string MaterialType { get; set; } = "custom";

        public Dictionary<string, Material> AvailableMaterials { get; } = new()
        {
            {"aluminium", new Material("Алюминий", 2700, 900, 237)},
            {"iron", new Material("Железо", 7870, 450, 80.2)},
            {"copper", new Material("Медь", 8960, 385, 401)},
            {"low_carbon_steel", new Material("Мал. углер. сталь", 7850, 502, 54)},
            {"medium_carbon_steel", new Material("Ср. углер. сталь", 7850, 486, 51)},
            {"high_carbon_steel", new Material("Выс. углер. сталь", 7850, 469, 48)},
            {"custom", new Material("Свой материал", 0, 0, 0)}
        };

        [RangeIf("MaterialType", "custom", 0.0001, double.MaxValue,
            ErrorMessage = "Плотность должна быть положительной")]
        public double MaterialDensity { get; set; }

        [RangeIf("MaterialType", "custom", 0.0001, double.MaxValue,
            ErrorMessage = "Теплоемкость должна быть положительной")]
        public double MaterialHeatCapacity { get; set; }

        [RangeIf("MaterialType", "custom", 0.0001, double.MaxValue,
             ErrorMessage = "Теплопроводность должна быть положительной")]
        public double MaterialConductivity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MaterialType == "custom")
            {
                if (MaterialDensity == null)
                    yield return new ValidationResult("Укажите плотность материала",
                        new[] { nameof(MaterialDensity) });

                if (MaterialHeatCapacity == null)
                    yield return new ValidationResult("Укажите теплоемкость материала",
                        new[] { nameof(MaterialHeatCapacity) });

                if (MaterialConductivity == null)
                    yield return new ValidationResult("Укажите теплопроводность материала",
                        new[] { nameof(MaterialConductivity) });
            }
        }

        [Required(ErrorMessage = "Введите время охлаждения")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Время охлаждения должно быть положительным")]
        public double? CoolingTime { get; set; }

        [Required(ErrorMessage = "Введите коэффициент излучения")]
        [Range(0.0, 1.0, ErrorMessage = "Коэффициент излучения должен быть от 0 до 1")]
        public double? Emissivity { get; set; }

    }
    // Кастомный атрибут валидации
    public class RangeIfAttribute : ValidationAttribute
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }

        public RangeIfAttribute(string propertyName, object value, double minimum, double maximum)
        {
            PropertyName = propertyName;
            Value = value;
            Minimum = minimum;
            Maximum = maximum;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(PropertyName)?.GetValue(instance, null);

            if (propertyValue?.ToString() == Value.ToString() && value != null)
            {
                var val = Convert.ToDouble(value);
                if (val < Minimum || val > Maximum)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
    public class Material
    {
        public string Name { get; }
        public double Density { get; }
        public double HeatCapacity { get; }
        public double Conductivity { get; }

        public Material(string name, double density, double heatCapacity, double conductivity)
        {
            Name = name;
            Density = density;
            HeatCapacity = heatCapacity;
            Conductivity = conductivity;
        }
    }
}
