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
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double InitialTemp { get; set; }
        public double EnvTemp { get; set; }
        public double MaterialDensity { get; set; }       // Плотность (кг/м³)
        public double MaterialHeatCapacity { get; set; }  // Теплоемкость (Дж/(кг·K))
        public double MaterialConductivity { get; set; }  // Теплопроводность (Вт/(м·K))
        public double CoolingTime { get; set; }
        public double Emissivity { get; set; }

        public List<CalculationResult> CalcResult()
        {
            var results = new List<CalculationResult>();

            // 1. Расчёт площади поверхности
            double surfaceArea = CalculateSurfaceArea();

            // 2. Расчёт коэффициента теплоотдачи излучением
            double alphaRad = CalculateRadiationHeatTransferCoefficient();
            // Рассчитываем свойства среды
            double prandtl = CalculatePrandtlNumber(EnvTemp);
            double thermalCond = CalculateThermalConductivity(EnvTemp);
            double kinematicViscosity = CalculateKinematicViscosity(EnvTemp);
            // 3. Расчёт коэффициента теплоотдачи конвекцией
            double alphaConv = CalculateConvectionHeatTransferCoefficient(kinematicViscosity);


            // 4. Суммарный коэффициент теплоотдачи
            double alphaSum = alphaRad + alphaConv;

            // 5. Расчёт числа Био
            double bioNumber = CalculateBioNumber(alphaSum);

            if (bioNumber >= 0.1)
            {
                throw new InvalidOperationException("Тело не является термически тонким (Bi ≥ 0.1)");
            }

            int steps = 10; // Фиксируем количество шагов (вместо деления времени)
            double timeStep = CoolingTime / steps;

            // 6. Расчёт изменения температуры
            for (int i = 0; i <= steps; i++)
            {
                double t = i * timeStep;
                results.Add(new CalculationResult
                {
                    Time = t,
                    Temperature = CalculateTemperature(t, surfaceArea, alphaSum),
                    BioNumber = bioNumber,
                    KinematicViscosity = kinematicViscosity,
                    PrandtlNumber = prandtl,
                    ThermalConductivity = thermalCond
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

        public double CalculateConvectionHeatTransferCoefficient(double kinematicViscosity)
        {
            double characteristicLength = GetCharacteristicLength();
            double grashof = CalculateGrashofNumber(characteristicLength, kinematicViscosity);
            double prandtl = CalculatePrandtlNumber(EnvTemp); // Для воздуха

            double nusselt = CalculateNusseltNumber(grashof, prandtl);
            double thermalConductivity = CalculateThermalConductivity(EnvTemp); // Для воздуха, Вт/(м·К)

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
        public double CalculateGrashofNumber(double l, double kinematicViscosity)
        {
            double g = 9.81;
            double beta = 1 / 273.0;
            double deltaT = InitialTemp - EnvTemp;

            return g * beta * deltaT * Math.Pow(l, 3) / Math.Pow(kinematicViscosity, 2);
        }
        public double CalculateKinematicViscosity(double temperatureCelsius)
        {
            // Температура в Кельвинах
            double temperatureKelvin = temperatureCelsius + 273.15;

            // Динамическая вязкость воздуха (формула Сатерленда)
            double dynamicViscosity = 1.458e-6 * Math.Pow(temperatureKelvin, 1.5) /
                                    (temperatureKelvin + 110.4);

            // Плотность воздуха (упрощённая формула)
            double density = 1.293 * (273.15 / temperatureKelvin);

            // Кинематическая вязкость (ν = μ/ρ)
            return dynamicViscosity / density;
        }
        // Методы для расчета свойств воздуха
        public double CalculatePrandtlNumber(double temperatureCelsius)
        {
            double T = temperatureCelsius + 273.15; // Переводим в Кельвины

            // Коэффициент теплопроводности воздуха (Вт/(м·K))
            double thermalConductivity = 0.0241 * Math.Pow(T / 273.15, 0.9);

            // Динамическая вязкость (формула Сатерленда)
            double dynamicViscosity = 1.458e-6 * Math.Pow(T, 1.5) / (T + 110.4);

            // Удельная теплоемкость (Дж/(кг·K))
            double specificHeat = 1005 + (T - 273.15) / 10;

            return (dynamicViscosity * specificHeat) / thermalConductivity;
        }

        public double CalculateThermalConductivity(double temperatureCelsius)
        {
            double T = temperatureCelsius + 273.15;
            return 0.0241 * Math.Pow(T / 273.15, 0.9); // Вт/(м·K)
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
                    // Для горизонтальной пластины учитываем разницу между верхней и нижней поверхностью
                    if (rayleigh > 2e4 && rayleigh < 8e6) return 0.54 * Math.Pow(rayleigh, 0.25);
                    if (rayleigh > 8e6 && rayleigh < 1e11) return 0.15 * Math.Pow(rayleigh, 0.33);
                }
                else // vertical
                {
                    if (rayleigh > 1e4 && rayleigh < 1e9) return 0.59 * Math.Pow(rayleigh, 0.25);
                    if (rayleigh > 1e9) return 0.13 * Math.Pow(rayleigh, 0.33);
                }
            }

            return 1;
        }

        public double CalculateBioNumber(double alphaSum)
        {
            return alphaSum * GetCharacteristicLength() / MaterialConductivity;
        }


        private double CalculateTemperature(double time, double surfaceArea, double alphaSum)
        {
            double mass = CalculateMass();

            return EnvTemp + (InitialTemp - EnvTemp) *
                   Math.Exp(-surfaceArea * alphaSum * time / (mass * MaterialHeatCapacity));
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

            return volume * MaterialDensity;
        }
    }
}
