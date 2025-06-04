using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebThermoThinApp.Data;
using WebThermoThinApp.Models;

namespace WebThermoThinApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ThermoThinContext _context;

        public HomeController(ILogger<HomeController> logger, ThermoThinContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var variants = _context.Variants.ToList();
            return View(variants);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var variant = _context.Variants.FirstOrDefault(x => x.Id == id);
            if (variant != null)
            {
                _context.Variants.Remove(variant);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Calc(int? id)
        {
            var viewModel = new HomeCalcViewModel();

            if (id.HasValue)
            {
                var variant = _context.Variants.FirstOrDefault(x => x.Id == id);

                if (variant != null)
                {
                    viewModel.Shape = variant.Shape;
                    viewModel.Orientation = variant.Orientation;
                    viewModel.Length = variant.Length;
                    viewModel.Width = variant.Width;
                    viewModel.Height = variant.Height;
                    viewModel.Radius = variant.Radius;
                    viewModel.InitialTemp = variant.InitialTemp;
                    viewModel.EnvTemp = variant.EnvTemp;
                    viewModel.CoolingTime = variant.CoolingTime;
                    viewModel.Emissivity = variant.Emissivity;
                    viewModel.MaterialType = variant.Material;
                    if (variant.Material == "custom")
                    {
                        viewModel.MaterialDensity = variant.MaterialDensity ?? 0;
                        viewModel.MaterialHeatCapacity = variant.MaterialHeatCapacity ?? 0;
                        viewModel.MaterialConductivity = variant.MaterialConductivity ?? 0;
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Calc(HomeCalcViewModel model, string action)
        {
            // Сначала базовая проверка по атрибутам
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Дополнительная ручная валидация по выбранной форме
            if (model.Shape == "cylinder")
            {
                if (model.Radius == null || model.Radius <= 0)
                    ModelState.AddModelError("Radius", "Радиус обязателен и должен быть больше 0.");

                if (model.Height == null || model.Height <= 0)
                    ModelState.AddModelError("Height", "Высота обязательна и должна быть больше 0.");
            }
            else if (model.Shape == "sphere")
            {
                if (model.Radius == null || model.Radius <= 0)
                    ModelState.AddModelError("Radius", "Радиус обязателен и должен быть больше 0.");
            }
            else if (model.Shape == "plate")
            {
                if (model.Length == null || model.Length <= 0)
                    ModelState.AddModelError("Length", "Длина обязательна и должна быть больше 0.");

                if (model.Width == null || model.Width <= 0)
                    ModelState.AddModelError("Width", "Ширина обязательна и должна быть больше 0.");

                if (model.Height == null || model.Height <= 0)
                    ModelState.AddModelError("Height", "Толщина обязательна и должна быть больше 0.");
            }

            if (!ModelState.IsValid)
            {
                return View(model); // покажет ошибки на форме
            }
            // Автоматически заполняем свойства для выбранного материала
            if (model.MaterialType != "custom" && model.AvailableMaterials.TryGetValue(model.MaterialType, out var material))
            {
                model.MaterialDensity = material.Density;
                model.MaterialHeatCapacity = material.HeatCapacity;
                model.MaterialConductivity = material.Conductivity;
            }

            // Подготовка расчётной модели
            var calcModel = new CalcModel(_context)
            {
                Shape = model.Shape,
                Orientation = model.Orientation,
                Length = model.Length ?? 0,
                Width = model.Width ?? 0,
                Height = model.Height ?? 0,
                Radius = model.Radius ?? 0,
                InitialTemp = model.InitialTemp ?? 0,
                EnvTemp = model.EnvTemp ?? 0,
                MaterialType = model.MaterialType,
                MaterialDensity = model.MaterialDensity,
                MaterialHeatCapacity = model.MaterialHeatCapacity,
                MaterialConductivity = model.MaterialConductivity,
                CoolingTime = model.CoolingTime ?? 0,
                Emissivity = model.Emissivity ?? 0
            };

            try
            {
                model.Result = calcModel.CalcResult(); // запускаем расчёт

                if (action == "add")
                {
                    _context.Variants.Add(new Variant
                    {
                        Shape = model.Shape,
                        Orientation = model.Orientation,
                        Length = model.Length ?? 0,
                        Width = model.Width ?? 0,
                        Height = model.Height ?? 0,
                        Radius = model.Radius ?? 0,
                        InitialTemp = model.InitialTemp ?? 0,
                        EnvTemp = model.EnvTemp ?? 0,
                        Material = model.MaterialType,
                        CoolingTime = model.CoolingTime ?? 0,
                        Emissivity = model.Emissivity ?? 0,
                        MaterialDensity = model.MaterialType == "custom" ? model.MaterialDensity : (double?)null,
                        MaterialHeatCapacity = model.MaterialType == "custom" ? model.MaterialHeatCapacity : (double?)null,
                        MaterialConductivity = model.MaterialType == "custom" ? model.MaterialConductivity : (double?)null
                    });

                    _context.SaveChanges();
                }
            }
            catch (InvalidOperationException ex)
            {
                // Например, если Bi ≥ 0.1
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
