using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Web.Models.Data;
using Web.Models.Data.Entities;
using Web.Models.View;
using Web.Services;

namespace Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly IDataContext _context;
		private readonly ISimulationService _simulations;
		private readonly IConfiguration _config;


		public HomeController(
			IDataContext context,
			ISimulationService simulations,
			IConfiguration config
		)
		{
			_context = context;
			_simulations = simulations;
			_config = config;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(SimulationViewModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					if (model.Seed == null)
					{
						model.Seed = new Random().Next();
					}

					var simulation = new SingleSimulation(
						model.Dataset,
						model.Queryset,
						Convert.ToInt32(_config["Limits:Dataset"]),
						Convert.ToInt32(_config["Limits:Queryset"]),
						new Random(model.Seed.Value)
					);

					simulation.CacheSize = model.CacheSize.HasValue ? model.CacheSize.Value : 10;
					simulation.Protocol = model.Protocol;
					simulation.Seed = model.Seed.Value;

					var id = await _simulations.EnqueueAsync(simulation);

					if (id < 0)
					{
						ModelState.AddModelError("queue", "Simulation not scheduled. Queue is full.");
					}
					else
					{
						return RedirectToAction("Simulation", new { id = id });
					}
				}
				catch (System.Exception)
				{
					ModelState.AddModelError("input", "Malformed dataset or queryset.");
					return View(model);
				}
			}

			return View(model);
		}

		public IActionResult Simulation(int id)
		{
			return View(_context.Simulations.First(s => s.Id == id));
		}

		public IActionResult Raw(int id)
		{
			return File(
				Encoding.ASCII.GetBytes(
					JsonConvert.SerializeObject(
						_context.Simulations.First(s => s.Id == id).Result,
						new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
					)
				),
				"application/json",
				"simulation-result.json"
			);
		}

		public IActionResult Queue()
		{
			return View(_context.Simulations.ToList());
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
