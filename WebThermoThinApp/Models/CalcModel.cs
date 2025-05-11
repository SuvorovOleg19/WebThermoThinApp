using Microsoft.AspNetCore.Mvc;

namespace WebThermoThinApp.Models
{
    public class CalculationResult
    {
        public double Time { get; set; }
        public double Temperature { get; set; }
        public double BioNumber { get; set; }
    }

    public class CalcModel
    {
        public string Shape { get; set; }
        public string Orientation { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double InitialTemp { get; set; }
        public double EnvTemp { get; set; }
        public string Material { get; set; }
        public double CoolingTime { get; set; }
        public double Emissivity { get; set; }

        public List<CalculationResult> CalcResult()
        {
            var results = new List<CalculationResult>();

            // 1. Расчёт площади поверхности
            double surfaceArea = CalculateSurfaceArea();

            // 2. Расчёт коэффициента теплоотдачи излучением
            double alphaRad = CalculateRadiationHeatTransferCoefficient();

            // 3. Расчёт коэффициента теплоотдачи конвекцией
            double alphaConv = CalculateConvectionHeatTransferCoefficient();

            // 4. Суммарный коэффициент теплоотдачи
            double alphaSum = alphaRad + alphaConv;

            // 5. Расчёт числа Био
            double bioNumber = CalculateBioNumber(alphaSum);

            if (bioNumber >= 0.1)
            {
                throw new InvalidOperationException("Тело не является термически тонким (Bi ≥ 0.1)");
            }

            // 6. Расчёт изменения температуры
            for (double t = 0; t <= CoolingTime; t += CoolingTime / 10)
            {
                double temp = CalculateTemperature(t, surfaceArea, alphaSum);
                results.Add(new CalculationResult
                {
                    Time = t,
                    Temperature = temp,
                    BioNumber = bioNumber
                });
            }

            return results;
        }

        public double CalculateSurfaceArea()
        {
            return Shape switch
            {
                "cylinder" => 2 * Math.PI * Radius * (Radius + Height),
                "sphere" => 4 * Math.PI * Radius * Radius,
                "plate" => Orientation == "vertical" ? 2 * (Length + Width) * Height : 2 * Length * Width,
                _ => 0
            };
        }

        public double CalculateRadiationHeatTransferCoefficient()
        {
            const double stefanBoltzmann = 5.670374419e-8;
            double c = Emissivity * stefanBoltzmann;
            double t0 = InitialTemp + 273.15;
            double tn = EnvTemp + 273.15;

            return c * (Math.Pow(t0 / 100, 4) - Math.Pow(tn / 100, 4)) / (t0 - tn);
        }

        public double CalculateConvectionHeatTransferCoefficient()
        {
            double characteristicLength = GetCharacteristicLength();
            double grashof = CalculateGrashofNumber(characteristicLength);
            double prandtl = 0.7; // Для воздуха

            double nusselt = CalculateNusseltNumber(grashof, prandtl);
            double thermalConductivity = 0.026; // Для воздуха, Вт/(м·К)

            return nusselt * thermalConductivity / characteristicLength;
        }

        public double GetCharacteristicLength()
        {
            return Shape switch
            {
                "sphere" => 2 * Radius,
                "cylinder" => Orientation == "horizontal" ? 2 * Radius : Height,
                "plate" => Orientation == "vertical" ? Height : Math.Max(Length, Width),
                _ => 0
            };
        }

        public double CalculateGrashofNumber(double l)
        {
            double g = 9.81;
            double beta = 1 / 273.0;
            double deltaT = InitialTemp - EnvTemp;
            double kinematicViscosity = 15.89e-6; // Для воздуха при 20°C, м²/с

            return g * beta * deltaT * Math.Pow(l, 3) / Math.Pow(kinematicViscosity, 2);
        }

        public double CalculateNusseltNumber(double gr, double pr)
        {
            double rayleigh = gr * pr;

            if (Shape == "sphere" || (Shape == "cylinder" && Orientation == "horizontal"))
            {
                if (rayleigh > 1e3 && rayleigh < 1e9) return 0.53 * Math.Pow(rayleigh, 0.25);
                if (rayleigh > 1e9 && rayleigh < 1e12) return 0.13 * Math.Pow(rayleigh, 0.33);
            }

            if (Shape == "plate")
            {
                if (rayleigh > 2e4 && rayleigh < 8e6) return 0.54 * Math.Pow(rayleigh, 0.25);
                if (rayleigh > 8e6 && rayleigh < 1e11) return 0.15 * Math.Pow(rayleigh, 0.33);
            }

            return 1; // Минимальное значение по умолчанию
        }

        public double CalculateBioNumber(double alphaSum)
        {
            double characteristicLength = GetCharacteristicLength();
            double materialConductivity = GetMaterialConductivity();

            return alphaSum * characteristicLength / materialConductivity;
        }

        public double GetMaterialConductivity()
        {
            return Material switch
            {
                "steel" => 50,
                "aluminum" => 237,
                "copper" => 401,
                "glass" => 0.8,
                _ => 0.5
            };
        }

        private double CalculateTemperature(double time, double surfaceArea, double alphaSum)
        {
            double mass = CalculateMass();
            double heatCapacity = GetMaterialHeatCapacity();

            return EnvTemp + (InitialTemp - EnvTemp) *
                   Math.Exp(-surfaceArea * alphaSum * time / (mass * heatCapacity));
        }

        public double CalculateMass()
        {
            double volume = Shape switch
            {
                "cylinder" => Math.PI * Radius * Radius * Height,
                "sphere" => 4.0 / 3 * Math.PI * Math.Pow(Radius, 3),
                "plate" => Length * Width * Height,
                _ => 0
            };

            double density = GetMaterialDensity();
            return volume * density;
        }

        public double GetMaterialDensity()
        {
            return Material switch
            {
                "steel" => 7850,
                "aluminum" => 2700,
                "copper" => 8960,
                "glass" => 2500,
                _ => 1000
            };
        }

        public double GetMaterialHeatCapacity()
        {
            return Material switch
            {
                "steel" => 500,
                "aluminum" => 900,
                "copper" => 385,
                "glass" => 840,
                _ => 1000
            };
        }
    }
}
