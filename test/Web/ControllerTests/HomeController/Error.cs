using Microsoft.AspNetCore.Mvc;
using Xunit;
using Web.Models.View;

namespace Test.Web.ControllerTests
{
	public partial class HomeController
	{
		[Theory]
		[InlineData(404)]
		[InlineData(500)]
		public void Error(int error)
		{
			// Act
			var result = _controller.Error(error);

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<ErrorViewModel>(
				viewResult.ViewData.Model
			);
			Assert.Equal(error, model.Code);
			switch (error)
			{
				case 404:
					Assert.Contains("not found", model.Message.ToLower());
					break;
				default:
					Assert.Contains("generic error", model.Message.ToLower());
					break;
			}
		}
	}
}
