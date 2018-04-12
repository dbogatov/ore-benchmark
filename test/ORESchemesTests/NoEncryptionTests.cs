using System;
using Xunit;
using ORESchemes.Shared;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Test.ORESchemes
{
	public class NoEncryptionTests : GenericORETests<int>
	{
		protected override void SetScheme()
		{
			_scheme = new NoEncryptionScheme();
		}
	}
}
