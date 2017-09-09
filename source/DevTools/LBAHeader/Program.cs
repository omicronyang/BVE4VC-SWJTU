﻿using System;
using System.IO;

namespace LBAHeader
{
	class FixLBAHeader
	{
		static byte[] data;
		static void Main(string[] args)
		{
			string f = args[0];
			if (!System.IO.File.Exists(f))
			{
				//If we can't find our initial argument, try and locate the openBVE executable in our directory
				string ff = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				f = System.IO.Path.Combine(ff, "OpenBve.exe");
			}
			if (!System.IO.File.Exists(f))
			{
				//Not found, log this rather than crashing
				Console.WriteLine("No suitable executables found....");
				return;
			}
			Console.WriteLine("Adding LBA Flag to executable {0}", f);
			data = File.ReadAllBytes(f);
			var offset = BitConverter.ToInt32(data, 0x3c);
			//Set LBA Flag for the file supplied via Arguments[0]
			data[offset + 4 + 18] |= 0x20;
			File.WriteAllBytes(args[0], data);
		}
	}
}
