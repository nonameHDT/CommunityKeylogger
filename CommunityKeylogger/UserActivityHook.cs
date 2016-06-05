using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace abc
{
	public class UserActivityHook
	{
		/* 
		 * Author: Unknown
		 * 
		 */

		#region Windows structure definitions

		[StructLayout(LayoutKind.Sequential)]
		private class KeyboardHookStruct
		{
			public int vkCode;
			public int scanCode;
			public int flags;
			public int time;
			public int dwExtraInfo;
		}
		#endregion

		#region Windows function imports

		[DllImport("user32.dll", CharSet = CharSet.Auto,
		   CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int SetWindowsHookEx(
			int idHook,
			HookProc lpfn,
			IntPtr hMod,
			int dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int UnhookWindowsHookEx(int idHook);

		[DllImport("user32.dll", CharSet = CharSet.Auto,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern int CallNextHookEx(
			int idHook,
			int nCode,
			int wParam,
			IntPtr lParam);

		private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

		[DllImport("user32")]
		private static extern int ToAscii(
			int uVirtKey,
			int uScanCode,
			byte[] lpbKeyState,
			byte[] lpwTransKey,
			int fuState);

		[DllImport("user32")]
		private static extern int GetKeyboardState(byte[] pbKeyState);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern short GetKeyState(int vKey);

		#endregion

		#region Windows constants


		private const int WH_KEYBOARD_LL = 13;
		private const int WH_KEYBOARD = 2;
		private const int WM_KEYDOWN = 0x100;
		private const int WM_KEYUP = 0x101;
		private const int WM_SYSKEYDOWN = 0x104;
		private const int WM_SYSKEYUP = 0x105;

		private const byte VK_SHIFT = 0x10;
		private const byte VK_CAPITAL = 0x14;
		private const byte VK_NUMLOCK = 0x90;
		private const byte VK_CONTROL = 17;

		#endregion

		public UserActivityHook()
		{
			Start();
		}

		public UserActivityHook(bool InstallMouseHook, bool InstallKeyboardHook)
		{
			Start(InstallMouseHook, InstallKeyboardHook);
		}

		~UserActivityHook()
		{
			Stop(true, true, false);
		}

		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyPress;
		public event KeyEventHandler KeyUp;
		
		private int hKeyboardHook = 0;
		private static HookProc KeyboardHookProcedure;
		
		public void Start()
		{
			this.Start(true, true);
		}

		public void Start(bool InstallMouseHook, bool InstallKeyboardHook)
		{
			// install Keyboard hook only if it is not installed and must be installed
			if (hKeyboardHook == 0 && InstallKeyboardHook)
			{
				// Create an instance of HookProc.
				KeyboardHookProcedure = new HookProc(KeyboardHookProc);
				//install hook
				hKeyboardHook = SetWindowsHookEx(
					WH_KEYBOARD_LL,
					KeyboardHookProcedure,
					IntPtr.Zero,
					0);
				//If SetWindowsHookEx fails.
				if (hKeyboardHook == 0)
				{
					//Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
					int errorCode = Marshal.GetLastWin32Error();
					//do cleanup
					Stop(false, true, false);
					//Initializes and throws a new instance of the Win32Exception class with the specified error. 
					throw new Win32Exception(errorCode);
				}
			}
		}

		public void Stop()
		{
			this.Stop(true, true, true);
		}

		public void Stop(bool UninstallMouseHook, bool UninstallKeyboardHook, bool ThrowExceptions)
		{
			//if keyboard hook set and must be uninstalled
			if (hKeyboardHook != 0 && UninstallKeyboardHook)
			{
				//uninstall hook
				int retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
				//reset invalid handle
				hKeyboardHook = 0;
				//if failed and exception must be thrown
				if (retKeyboard == 0 && ThrowExceptions)
				{
					//Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
					int errorCode = Marshal.GetLastWin32Error();
					//Initializes and throws a new instance of the Win32Exception class with the specified error. 
					throw new Win32Exception(errorCode);
				}
			}
		}

		private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
		{
			//indicates if any of underlaing events set e.Handled flag
			bool handled = false;
			//it was ok and someone listens to events
			if ((nCode >= 0) && (KeyDown != null || KeyUp != null || KeyPress != null))
			{
				//read structure KeyboardHookStruct at lParam
				KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

				// raise KeyPress
				if (KeyPress != null && wParam == WM_KEYDOWN)
				{
					bool isDownShift = ((GetKeyState(VK_SHIFT) & 0x80) == 0x80 ? true : false);
					bool isDownCapslock = (GetKeyState(VK_CAPITAL) != 0 ? true : false);

					byte[] keyState = new byte[256];
					GetKeyboardState(keyState);
					byte[] inBuffer = new byte[2];
					if (ToAscii(MyKeyboardHookStruct.vkCode,
							  MyKeyboardHookStruct.scanCode,
							  keyState,
							  inBuffer,
							  MyKeyboardHookStruct.flags) == 1)
					{
						byte key = inBuffer[0];
						if ((isDownCapslock ^ isDownShift) && Char.IsLetter((char)key)) key = (byte)Char.ToUpper((char)key);
						KeyEventArgs e = new KeyEventArgs((Keys)key);
						KeyPress(this, e);
						handled = handled || e.Handled;
					}
				}

			}

			//if event handled in application do not handoff to other listeners
			if (handled)
				return 1;
			else
				return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
		}
	}
}
