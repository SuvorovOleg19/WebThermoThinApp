using System.ComponentModel.DataAnnotations;

namespace WebThermoThinApp.Data
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Плотность (кг/м³)")]
        public double Density { get; set; }

        [Display(Name = "Теплоемкость (Дж/(кг·K))")]
        public double HeatCapacity { get; set; }

        [Display(Name = "Теплопроводность (Вт/(м·K))")]
        public double ThermalConductivity { get; set; }

        [Display(Name = "Коэффициент излучения")]
        public double Emissivity { get; set; }
    }
}