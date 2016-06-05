using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace abc
{
	class Disk
	{
		public Disk()
		{

		}

		public static bool HaveDDrive()
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo di in drives)
			{
				if (di.Name == @"D:\" && di.DriveType == DriveType.Fixed)
				{
					return true;
				}
			}

			return false;
		}

		public static bool HaveEnoughSpace()
		{
			long totalsize = 0;

			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo di in drives)
			{
				if (di.DriveType == DriveType.Fixed)
					totalsize += di.TotalSize;
			}
			// convert bytes to gigabytes and if the size is large than 100GB, the computer is real
			if (totalsize / 1073741824 >= 100)  
				return true;
			return false;
		}
	}
}
