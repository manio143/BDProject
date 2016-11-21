﻿using System;
using Nancy.Hosting.Self;

namespace BDProject
{
	public class Program
	{
		static void Main()
		{
			var nancyHost = new NancyHost(new Uri("http://localhost:8080/"));
			nancyHost.Start();

			Console.WriteLine("Nancy now listening on http://localhost:8080/. \nPress enter to stop...");
			Console.ReadKey();

			nancyHost.Stop();

			Console.WriteLine("Stopped. Good bye!");
		}
	}
}

