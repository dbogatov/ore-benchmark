using System;
using Xunit;
using ORESchemes.Shared;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class NoEncryptionTests : GenericORETests<long>
	{
		protected override void SetScheme()
		{
			_scheme = new NoEncryptionScheme();
		}
	}
}
