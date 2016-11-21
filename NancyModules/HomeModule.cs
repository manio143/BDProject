using System;
using Nancy;

namespace BDProject.NancyModules
{
	public class HomeModule : NancyModule
	{
		public HomeModule()
		{
			Get["/"] = (@params) => "Hello World!";
		}
	}
}

