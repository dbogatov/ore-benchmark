using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Web.Models.Data;
using Web.Models.View;

namespace Web.Controllers
{
	public class HomeController : Controller
	{
		public IDataContext _context { get; set; }

		public HomeController(IDataContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
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
