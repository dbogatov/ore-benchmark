using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Web.Models.Data.Entities;

namespace Test.Web.ControllerTests
{
	public partial class HomeController
	{
		[Fact]
		public async Task SimulationExists()
		{
			// Arrange
			await _context.Simulations.AddAsync(new SingleSimulation { Id = 5 });
			await _context.SaveChangesAsync();

			// Act
			var result = await _controller.Simulation(5);

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<SingleSimulation>(
				viewResult.ViewData.Model
			);
			Assert.Equal(5, model.Id);
		}

		[Fact]
		public async Task SimulationNotFound()
		{
			// Act
			var result = await _controller.Simulation(5);

			// Assert
			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}
	}
}
