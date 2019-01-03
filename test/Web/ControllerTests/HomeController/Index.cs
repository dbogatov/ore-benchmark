using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using Web.Models.View;
using Web.Models.Data.Entities;

namespace Test.Web.ControllerTests
{
	public partial class HomeController
	{
		[Fact]
		public void Index()
		{
			// Act
			var result = _controller.Index();

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task IndexGoodInput()
		{
			// Arrange
			_mockSimulationService
				.Setup(mock => mock.EnqueueAsync(It.IsAny<SingleSimulation>()))
				.ReturnsAsync(1);

			// Act
			var result = await _controller.Index(new SimulationViewModel { Seed = 123456 });

			// Assert
			var redirectResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(1, redirectResult.RouteValues["id"]);
			_mockSimulationService.Verify(
				mock => mock.EnqueueAsync(It.Is<SingleSimulation>(s => s.Seed == 123456)),
				Times.Once()
			);
		}

		[Fact]
		public async Task IndexQueueFull()
		{
			// Arrange
			_mockSimulationService
				.Setup(mock => mock.EnqueueAsync(It.IsAny<SingleSimulation>()))
				.ReturnsAsync(-1);

			// Act
			var result = await _controller.Index(new SimulationViewModel { Seed = 123456 });

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<SimulationViewModel>(
				viewResult.ViewData.Model
			);
			Assert.Equal(123456, model.Seed);
			_mockSimulationService.Verify(
				mock => mock.EnqueueAsync(It.IsAny<SingleSimulation>()),
				Times.Once()
			);
			Assert.NotEmpty(_controller.ModelState["queue"].Errors);
		}

		[Fact]
		public async Task IndexMalformedInput()
		{
			// Act
			var result = await _controller.Index(new SimulationViewModel { Dataset = "Jiberish" });

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<SimulationViewModel>(
				viewResult.ViewData.Model
			);
			Assert.Equal("Jiberish", model.Dataset);
			_mockSimulationService.Verify(
				mock => mock.EnqueueAsync(It.IsAny<SingleSimulation>()),
				Times.Never()
			);
			Assert.NotEmpty(_controller.ModelState["input"].Errors);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void IndexTooLongSet(bool dataVsQuery)
		{
			// Arrange
			var goodInput = new SimulationViewModel();
			var badInput = new SimulationViewModel();
			
			if (dataVsQuery)
			{
				badInput.Dataset = new string('D', 64 * 10 * 1000 + 50);
				goodInput.Dataset = new string('D', 64 * 10 * 1000 - 50);
			}
			else
			{
				badInput.Queryset = new string('Q', 64 * 10 * 1000 + 50);
				goodInput.Queryset = new string('Q', 64 * 10 * 1000 - 50);
			}

			// Act and Assert
    		Assert.NotEmpty(Validate(badInput));
			Assert.Empty(Validate(goodInput));
		}
	}
}
