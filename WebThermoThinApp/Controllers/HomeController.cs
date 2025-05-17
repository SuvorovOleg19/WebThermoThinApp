using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebThermoThinApp.Models;
using WebThermoThinApp.Data;

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
        public IActionResult Calc(HomeCalcViewModel vm, string action)
        {
            // Маппинг входящих данных в модель расчета
            var model = new CalcModel(_context)
            {
                Shape = vm.Shape,
                Orientation = vm.Orientation,
                Length = vm.Length ?? 0,
                Width = vm.Width ?? 0,
                Height = vm.Height ?? 0,
                Radius = vm.Radius ?? 0,
                InitialTemp = vm.InitialTemp ?? 0,
                EnvTemp = vm.EnvTemp ?? 0,
                Material = vm.Material,
                CoolingTime = vm.CoolingTime ?? 0,
                Emissivity = vm.Emissivity ?? 0
            };

            // Выполняем расчет
            var results = model.CalcResult();
            vm.Result = results;

            // При сохранении добавляем вариант в базу
            if (action == "add")
            {
                _context.Variants.Add(new Variant
                {
                    Shape = vm.Shape,
                    Orientation = vm.Orientation,
                    Length = vm.Length ?? 0,
                    Width = vm.Width ?? 0,
                    Height = vm.Height ?? 0,
                    Radius = vm.Radius ?? 0,
                    InitialTemp = vm.InitialTemp ?? 0,
                    EnvTemp = vm.EnvTemp ?? 0,
                    Material = vm.Material,
                    CoolingTime = vm.CoolingTime ?? 0,
                    Emissivity = vm.Emissivity ?? 0
                });
                _context.SaveChanges();
            }

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
