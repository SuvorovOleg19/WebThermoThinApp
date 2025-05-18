using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebThermoThinApp.Data;
using WebThermoThinApp.Models;

namespace WebThermoThinApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ThermoThinContext _context;

        public HomeController(ILogger<HomeController> logger, ThermoThinContext context)
        {
            _logger = logger;
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
            viewModel.AvailableMaterials = _context.Materials.ToList();

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
                    viewModel.Material = variant.Material;
                    viewModel.CoolingTime = variant.CoolingTime;
                    viewModel.Emissivity = variant.Emissivity;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Calc(HomeCalcViewModel model, string action)
        {
            // Маппинг входящих данных в модель расчета
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
                MaterialName = model.Material,
                CoolingTime = model.CoolingTime ?? 0,
                Emissivity = model.Emissivity ?? 0
            };

            model.Result = calcModel.CalcResult();

            // Выполняем расчет
            model.Result = calcModel.CalcResult();
            //vm.Result = results;
            model.AvailableMaterials = _context.Materials.ToList();

            // При сохранении добавляем вариант в базу
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
                    Material = model.Material,
                    CoolingTime = model.CoolingTime ?? 0,
                    Emissivity = model.Emissivity ?? 0
                });
                _context.SaveChanges();
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
