using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Web.Models.Data.Entities;

namespace Test.Web.ControllerTests
{
	public partial class HomeController
	{
		[Fact]
		public async Task RawExists()
		{
			// Arrange
			await _context.Simulations.AddAsync(new SingleSimulation { Id = 5 });
			await _context.SaveChangesAsync();

			// Act
			var result = await _controller.Raw(5);

			// Assert
			var fileContentResult = Assert.IsType<FileContentResult>(result);

			Assert.Equal("application/json", fileContentResult.ContentType);
			Assert.Equal("simulation-result.json", fileContentResult.FileDownloadName);
		}

		[Fact]
		public async Task RawNotFound()
		{
			// Act
			var result = await _controller.Raw(5);

			// Assert
			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}
	}
}
