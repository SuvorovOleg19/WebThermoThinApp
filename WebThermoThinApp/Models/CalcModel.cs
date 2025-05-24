using System;
using System.Collections.Generic;
using WebThermoThinApp.Data;

namespace WebThermoThinApp.Models
{
    public class CalculationResult
    {
        public double Time { get; set; }
        public double Temperature { get; set; }
        public double BioNumber { get; set; }
        public double KinematicViscosity { get; set; }  // Кинематическая вязкость (м²/с)
        public double PrandtlNumber { get; set; }       // Число Прандтля
        public double ThermalConductivity { get; set; } // Коэффициент теплопроводности (Вт/(м·K))
    }
    public class CalcModel
    {
        private readonly ThermoThinContext _context;

        public CalcModel(ThermoThinContext context)
        {
            _context = context;
        }

        public string Shape { get; set; }
        public string Orientation { get; set; }
        public string MaterialType { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double InitialTemp { get; set; }
        public double EnvTemp { get; set; }
        public double MaterialDensity { get; set; }
        public double MaterialHeatCapacity { get; set; }
        public double MaterialConductivity { get; set; }
        public double CoolingTime { get; set; }
        public double Emissivity { get; set; }

        public List<CalculationResult> CalcResult()
        {
            ValidateInputParameters();

            var results = new List<CalculationResult>();
            double currentTemp = InitialTemp;

            // Получаем свойства с учетом температуры
            double conductivity = GetTemperatureDependentConductivity(currentTemp);
            double heatCapacity = GetTemperatureDependentHeatCapacity(currentTemp);
            double density = MaterialDensity; // Плотность меньше зависит от температуры

            // 1. Расчёт площади поверхности
            double surfaceArea = CalculateSurfaceArea();

            // 2. Расчёт коэффициента теплоотдачи
            double alphaRad = CalculateRadiationHeatTransferCoefficient();
            double kinematicViscosity = CalculateKinematicViscosity(EnvTemp);
            double alphaConv = CalculateConvectionHeatTransferCoefficient(kinematicViscosity);
            double alphaSum = alphaRad + alphaConv;

            // 3. Расчёт числа Био
            double bioNumber = CalculateBioNumber(alphaSum, conductivity);
            if (bioNumber >= 0.1)
            {
                throw new InvalidOperationException("Тело не является термически тонким (Bi ≥ 0.1)");
            }

            // 4. Расчёт температуры с шагами по времени
            int steps = 10;
            double timeStep = CoolingTime / steps;

            for (int i = 0; i <= steps; i++)
            {
                double time = i * timeStep;
                currentTemp = CalculateTemperature(time, surfaceArea, alphaSum, density, heatCapacity);

                // Обновляем свойства материала для новой температуры
                conductivity = GetTemperatureDependentConductivity(currentTemp);
                heatCapacity = GetTemperatureDependentHeatCapacity(currentTemp);

                results.Add(new CalculationResult
                {
                    Time = time,
                    Temperature = currentTemp,
                    BioNumber = bioNumber,
                    KinematicViscosity = kinematicViscosity,
                    PrandtlNumber = CalculatePrandtlNumber(EnvTemp),
                    ThermalConductivity = CalculateThermalConductivity(EnvTemp)
                });
            }

            return results;
        }

        private void ValidateInputParameters()
        {
            if (MaterialConductivity <= 0 || MaterialHeatCapacity <= 0 || MaterialDensity <= 0)
                throw new ArgumentException("Свойства материала должны быть положительными");

            if (CoolingTime <= 0 || Emissivity < 0 || Emissivity > 1)
                throw new ArgumentException("Некорректные параметры расчета");
        }

        #region Material Properties Calculations
        public double GetTemperatureDependentConductivity(double temperature)
        {
            temperature = Math.Clamp(temperature, -273, 3000);

            return MaterialType switch
            {
                "aluminium" => 237 * (1 - 0.0005 * (temperature - 20)),
                "copper" => 401 * (1 - 0.0004 * (temperature - 20)),
                "iron" => 80.2 * (1 - 0.00045 * (temperature - 20)),
                "low_carbon_steel" => 54 * (1 - 0.0003 * (temperature - 20)),
                "medium_carbon_steel" => 51 * (1 - 0.0003 * (temperature - 20)),
                "high_carbon_steel" => 48 * (1 - 0.0003 * (temperature - 20)),
                _ => MaterialConductivity // Для пользовательского материала
            };
        }

        public double GetTemperatureDependentHeatCapacity(double temperature)
        {
            temperature = Math.Clamp(temperature, -273, 3000);

            return MaterialType switch
            {
                "aluminium" => 900 + 0.5 * temperature,
                "copper" => 385 + 0.3 * temperature,
                "iron" => 450 + 0.25 * temperature,
                "low_carbon_steel" => 502 + 0.2 * temperature,
                "medium_carbon_steel" => 486 + 0.2 * temperature,
                "high_carbon_steel" => 469 + 0.2 * temperature,
                _ => MaterialHeatCapacity // Для пользовательского материала
            };
        }
        #endregion

        #region Thermal Calculations
        public double CalculateSurfaceArea()
        {
            return Shape switch
            {
                "cylinder" => 2 * Math.PI * Radius * (Radius + Height),
                "sphere" => 4 * Math.PI * Radius * Radius,
                "plate" => Orientation == "vertical" ? 2 * (Length + Width) * Height : 2 * Length * Width,
                _ => throw new ArgumentException("Неизвестная форма тела")
            };
        }

        public double CalculateRadiationHeatTransferCoefficient()
        {
            const double stefanBoltzmann = 5.670374419e-8;
            double t0 = InitialTemp + 273.15;
            double tn = EnvTemp + 273.15;
            return Emissivity * stefanBoltzmann * (Math.Pow(t0, 4) - Math.Pow(tn, 4)) / (t0 - tn);
        }

        public double CalculateConvectionHeatTransferCoefficient(double kinematicViscosity)
        {
            double characteristicLength = GetCharacteristicLength();
            double grashof = CalculateGrashofNumber(characteristicLength, kinematicViscosity);
            double prandtl = CalculatePrandtlNumber(EnvTemp);
            double nusselt = CalculateNusseltNumber(grashof, prandtl);
            double thermalConductivity = CalculateThermalConductivity(EnvTemp);
            return nusselt * thermalConductivity / characteristicLength;
        }

        public double CalculateBioNumber(double alphaSum, double conductivity)
        {
            return alphaSum * GetCharacteristicLength() / conductivity;
        }

        private double CalculateTemperature(double time, double surfaceArea, double alphaSum,
                                         double density, double heatCapacity)
        {
            double mass = CalculateMass(density);
            return EnvTemp + (InitialTemp - EnvTemp) *
                   Math.Exp(-surfaceArea * alphaSum * time / (mass * heatCapacity));
        }

        public double CalculateMass(double density)
        {
            double volume = Shape switch
            {
                "cylinder" => Math.PI * Radius * Radius * Height,
                "sphere" => 4.0 / 3 * Math.PI * Math.Pow(Radius, 3),
                "plate" => Length * Width * Height,
                _ => throw new ArgumentException("Неизвестная форма тела")
            };
            return volume * density;
        }
        #endregion

        #region Helper Methods
        public double GetCharacteristicLength()
        {
            return Shape switch
            {
                "sphere" => 2 * Radius,
                "cylinder" => Orientation == "horizontal" ? 2 * Radius : Height,
                "plate" => Orientation == "vertical" ? Height : Math.Max(Length, Width),
                _ => throw new ArgumentException("Неизвестная форма тела")
            };
        }

        public double CalculateGrashofNumber(double l, double kinematicViscosity)
        {
            double g = 9.81;
            double beta = 1 / 273.0;
            double deltaT = InitialTemp - EnvTemp;
            return g * beta * deltaT * Math.Pow(l, 3) / Math.Pow(kinematicViscosity, 2);
        }

        public double CalculateKinematicViscosity(double temperatureCelsius)
        {
            double temperatureKelvin = temperatureCelsius + 273.15;
            double dynamicViscosity = 1.458e-6 * Math.Pow(temperatureKelvin, 1.5) / (temperatureKelvin + 110.4);
            double density = 1.293 * (273.15 / temperatureKelvin);
            return dynamicViscosity / density;
        }

        public double CalculatePrandtlNumber(double temperatureCelsius)
        {
            double T = temperatureCelsius + 273.15;
            double thermalConductivity = 0.0241 * Math.Pow(T / 273.15, 0.9);
            double dynamicViscosity = 1.458e-6 * Math.Pow(T, 1.5) / (T + 110.4);
            double specificHeat = 1005 + (T - 273.15) / 10;
            return (dynamicViscosity * specificHeat) / thermalConductivity;
        }

        public double CalculateThermalConductivity(double temperatureCelsius)
        {
            double T = temperatureCelsius + 273.15;
            return 0.0241 * Math.Pow(T / 273.15, 0.9);
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
                if (Orientation == "horizontal")
                {
                    if (rayleigh > 2e4 && rayleigh < 8e6) return 0.54 * Math.Pow(rayleigh, 0.25);
                    if (rayleigh > 8e6 && rayleigh < 1e11) return 0.15 * Math.Pow(rayleigh, 0.33);
                }
                else
                {
                    if (rayleigh > 1e4 && rayleigh < 1e9) return 0.59 * Math.Pow(rayleigh, 0.25);
                    if (rayleigh > 1e9) return 0.13 * Math.Pow(rayleigh, 0.33);
                }
            }

            return 1;
        }
        #endregion
    }
}