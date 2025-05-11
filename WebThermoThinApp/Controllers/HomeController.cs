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
        public IActionResult Calc(CalcModel model, string action)
        {
            var result = model.CalcResult();

            var viewModel = new HomeCalcViewModel()
            {
                Result = result,
                Shape = model.Shape,
                Orientation = model.Orientation,
                Length = model.Length,
                Width = model.Width,
                Height = model.Height,
                Radius = model.Radius,
                InitialTemp = model.InitialTemp,
                EnvTemp = model.EnvTemp,
                Material = model.Material,
                CoolingTime = model.CoolingTime,
                Emissivity = model.Emissivity
            };

            if (action == "add")
            {
                _context.Variants.Add(new Variant
                {
                    Shape = model.Shape,
                    Orientation = model.Orientation,
                    Length = model.Length,
                    Width = model.Width,
                    Height = model.Height,
                    Radius = model.Radius,
                    InitialTemp = model.InitialTemp,
                    EnvTemp = model.EnvTemp,
                    Material = model.Material,
                    CoolingTime = model.CoolingTime,
                    Emissivity = model.Emissivity
                });
                _context.SaveChanges();
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
