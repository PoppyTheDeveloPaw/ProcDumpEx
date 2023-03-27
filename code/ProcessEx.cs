using ProcDumpExExceptions;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcDumpEx
{
	internal static class ProcessEx
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetBinaryType(string lpApplicationName, out BinaryType lpBinaryType);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

		enum BinaryType : uint
		{
			SCS_32BIT_BINARY = 0,
			SCS_64BIT_BINARY = 6,
			SCS_DOS_BINARY = 1,
			SCS_OS216_BINARY = 5,
			SCS_PIF_BINARY = 3,
			SCS_POSIX_BINARY = 4,
			SCS_WOW_BINARY = 2
		}

		[Flags]
		enum ProcessAccessFlags : uint
		{
			QueryInformation = 0x0400,
			VirtualMemoryRead = 0x0010
		}

		const int IMAGE_FILE_MACHINE_ARM64 = 0xAA64;

		public static Architecture GetProcessArchitecture(this Process process)
		{
			if (!Environment.Is64BitOperatingSystem)
				return Architecture.X86;

			// 64-bit windows

			IntPtr processHandle = IntPtr.Zero;

			try
			{
				processHandle = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead, false, process.Id);

				if (processHandle == IntPtr.Zero)
					throw new GetArchitectureException();

				if (process.MainModule?.FileName is null)
					throw new GetArchitectureException();

				if (!GetBinaryType(process.MainModule.FileName, out BinaryType binaryTypeValue))
					throw new GetArchitectureException();

				if (binaryTypeValue == BinaryType.SCS_32BIT_BINARY)
					return Architecture.X86;

				if (binaryTypeValue == BinaryType.SCS_64BIT_BINARY)
				{
					if (IsProcessArm64(process))
						return Architecture.Arm64;
					return Architecture.X64;
				}

				throw new GetArchitectureException();
			}
			catch
			{
				throw new GetArchitectureException();
			}
			finally
			{
				if (processHandle != IntPtr.Zero)
					CloseHandle(processHandle);
			}
		}

		static bool IsProcessArm64(Process process)
		{
			if (process.MainModule?.BaseAddress is not { } baseAddress)
				throw new GetArchitectureException();

			byte[] buffer = new byte[2];

			if (!ReadProcessMemory(process.Handle, baseAddress + 0x3C, buffer, buffer.Length, out IntPtr bytesRead))
				return false;

			int peHeaderOffset = BitConverter.ToInt16(buffer, 0);

			buffer = new byte[4];

			if (!ReadProcessMemory(process.Handle, baseAddress + peHeaderOffset + 4 + 20 + 2, buffer, buffer.Length, out bytesRead))
				return false;

			ushort machine = BitConverter.ToUInt16(buffer, 0);

			return machine == IMAGE_FILE_MACHINE_ARM64;
		}

	}

	
}
