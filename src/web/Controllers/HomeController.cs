using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		public IActionResult Index() => View();

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
						Convert.ToInt32(_config["Daemon:SimulationService:PageSize"]),
						model.ProtocolReal,
						new Random(model.Seed.Value)
					);

					simulation.CacheSize = model.CacheSize.HasValue ? model.CacheSize.Value : 0;
					simulation.Seed = model.Seed.Value;
					if (model.ElementsPerPage.HasValue)
					{
						simulation.ElementsPerPage = model.ElementsPerPage.Value;
					}

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
				catch (SingleSimulation.MalformedSetException e)
				{
					ModelState.AddModelError("input", $"Malformed {e.Set}.");
					return View(model);
				}
			}

			return View(model);
		}

		public async Task<IActionResult> Simulation(int id)
		{
			var simulation = await _context.Simulations.FirstOrDefaultAsync(s => s.Id == id);
			if (simulation != null)
			{
				return View(simulation);
			}
			else
			{
				return NotFound();
			}
		}

		public async Task<IActionResult> Raw(int id)
		{
			var simulation = await _context.Simulations.FirstOrDefaultAsync(s => s.Id == id);

			if (simulation != null)
			{
				return File(
					Encoding.ASCII.GetBytes(
						JsonConvert.SerializeObject(
							simulation.Result,
							new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
						)
					),
					"application/json",
					"simulation-result.json"
				);
			}
			else
			{
				return NotFound();
			}
		}

		public async Task<IActionResult> Queue()
			=> View(await _context.Simulations.OrderBy(s => s.Created).ToListAsync());

		public IActionResult Error(int id)
			=> View(new ErrorViewModel { Code = id });
	}
}
