using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Web.Models.Data.Entities;

namespace Test.Web.ControllerTests
{
	public partial class HomeController
	{
		[Fact]
		public async Task Queue()
		{
			// Arrange
			await _context.Simulations.AddAsync(new SingleSimulation());
			await _context.SaveChangesAsync();
			
			// Act
			var result = await _controller.Queue();

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<IEnumerable<SingleSimulation>>(
				viewResult.ViewData.Model
			);
			Assert.NotEmpty(model);
		}
	}
}
