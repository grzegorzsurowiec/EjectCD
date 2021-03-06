﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EjectCD
{
	class Program
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool SetConsoleIcon(IntPtr hIcon);

		static void Main(string[] args)
		{
			Icon ikona = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

			SetConsoleIcon(ikona.Handle);

			char letter = char.MinValue;

			if (args != null)
			{
				if (args.Length == 1)
				{
					letter = args[0][0];
				}
			}

			Console.WriteLine("Drive to open: " + letter);

			letter = CorrectCDDrive(letter);
			if (letter != char.MinValue) EjectMedia.Run(letter);
			else Console.WriteLine("Wrong parametr or ejectable drive not found.");
		}

		static char CorrectCDDrive(char letter)
		{
			bool correct = false;

			DriveInfo[] drives = DriveInfo.GetDrives();

			if (char.IsLetter(letter))
			{
				foreach (DriveInfo drive in drives)
				{
					if (drive.Name[0] == letter && drive.DriveType == DriveType.CDRom)
					{
						correct = true;
					}

				}
			}
			
			if (!correct)
			{
				foreach (DriveInfo drive in drives)
				{
					if (drive.DriveType == DriveType.CDRom)
					{
						letter = drive.Name[0];
						correct = true;
						break;
					}

				}
			}

			if (!correct) letter = char.MinValue;

			return letter;
		}


	}
}


class EjectMedia
{
	const int OPEN_EXISTING = 3;
	const uint GENERIC_READ = 0x80000000;
	const uint GENERIC_WRITE = 0x40000000;
	const uint IOCTL_STORAGE_EJECT_MEDIA = 2967560;

	[DllImport("kernel32")]
	private static extern IntPtr CreateFile
		(string filename, uint desiredAccess, 
		 uint shareMode, IntPtr securityAttributes,
		 int creationDisposition, int flagsAndAttributes, 
		 IntPtr templateFile);

	[DllImport("kernel32")]
	private static extern int DeviceIoControl
		(IntPtr deviceHandle, uint ioControlCode, 
		 IntPtr inBuffer, int inBufferSize,
		 IntPtr outBuffer, int outBufferSize, 
		 ref int bytesReturned, IntPtr overlapped);

	[DllImport("kernel32")]
	private static extern int CloseHandle(IntPtr handle);

	public static void Run(char driveLetter)
	{
		bool error = false;
		string path = "\\\\.\\" + driveLetter + ":";
		IntPtr handle = CreateFile(path, GENERIC_READ | GENERIC_WRITE, 0,
								IntPtr.Zero, OPEN_EXISTING, 0,
								IntPtr.Zero);
		if ((long)handle == -1)
		{
			Console.WriteLine("Unable to open drive " + driveLetter);
			error = true;
		}

		if (!error)
		{
			int dummy = 0;
			DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0,
							IntPtr.Zero, 0, ref dummy, IntPtr.Zero);
			CloseHandle(handle);
			Console.WriteLine("Ejected drive: " + driveLetter);
		}

		Console.WriteLine("Hit key to exit.");
		Console.ReadKey();
	}
}