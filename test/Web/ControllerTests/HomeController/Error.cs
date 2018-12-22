using Microsoft.AspNetCore.Mvc;
using Xunit;
using Web.Models.View;

namespace Test.Web.ControllerTests
{
	public partial class HomeController
	{
		[Fact]
		public void Error()
		{
			// Act
			var result = _controller.Error(404);

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<ErrorViewModel>(
				viewResult.ViewData.Model
			);
			Assert.Equal(404, model.Code);
		}
	}
}
