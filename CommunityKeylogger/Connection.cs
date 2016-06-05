using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace abc
{
	class Connection
	{
		public static bool HaveInternet()
		{
			DateTime now = DateTime.Now;
			// against malware analyser
			/*if (now.Hour > 23 || now.Hour < 5)
				return false;*/
			
			try
			{
				IPHostEntry ipe = Dns.Resolve("google.com");
				if (ipe.AddressList.Length > 0)
					return true;
			}
			catch {}
			return false;

		}
	}
}
