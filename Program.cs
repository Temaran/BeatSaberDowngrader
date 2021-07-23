using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;

#nullable enable

namespace BeatSaberDowngrader
{
	class Program
	{
		[DllImport("kernel32.dll", ExactSpelling = true)]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		static Dictionary<string, string> GVersionsToManifests = new Dictionary<string, string>()
		{
			{ "0.10.1", "6316038906315325420" },
			{ "0.10.2", "2542095265882143144" },
			{ "0.10.2p1", "5611588554149133260" },
			{ "0.11.0", "8700049030626148111" },
			{ "0.11.2", "2707973953401625222" },
			{ "0.12.0", "6094599000655593822" },
			{ "0.12.0p1", "2068421223689664394" },
			{ "0.12.1", "2472041066434647526" },
			{ "0.12.2", "5325635033564462932" },
			{ "0.13.0", "3102409495238838111" },
			{ "0.13.0p1", "6827433614670733798" },
			{ "0.13.1", "6033025349617217666" },
			{ "0.13.2", "5325635033564462932" },
			{ "1.0.0", "152937782137361764" },
			{ "1.0.1", "7950322551526208347" },
			{ "1.1.0", "1400454104881094752" },
			{ "1.0.0p1", "1041583928494277430" },
			{ "1.2.0", "3820905673516362176" },
			{ "1.3.0", "2440312204809283162" },
			{ "1.4.0", "3532596684905902618" },
			{ "1.4.2", "1199049250928380207" },
			{ "1.5.0", "2831333980042022356" },
			{ "1.6.0", "1869974316274529288" },
			{ "1.6.1", "6122319670026856947" },
			{ "1.6.2", "4932559146183937357" },
			{ "1.7.0", "3516084911940449222" },
			{ "1.8.0", "3177969677109016846" },
			{ "1.9.0", "7885463693258878294" },
			{ "1.9.1", "6222769774084748916" },
			{ "1.10.0", "6711131863503994755" },
			{ "1.11.0", "1919603726987963829" },
			{ "1.11.1", "3268824881806146387" },
			{ "1.12.1", "2928416283534881313" },
			{ "1.12.2", "543439039654962432" },
			{ "1.13.0", "4635119747389290346" },
			{ "1.13.2", "8571679771389514488" },
			{ "1.13.4", "1257277263145069282" },
			{ "1.13.5", "7007516983116400336" },
			{ "1.14.0", "9218225910501819399" },
			{ "1.15.0", "7624554893344753887" },
			{ "1.16.0", "3667184295685865706" },
			{ "1.16.1", "9201874499606445062" },
			{ "1.16.2", "3692829915208062825" },
			{ "1.16.3", "6392596175313869009" }
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "The DepotDownloader dll only works on Windows anyhow.")]
		static void Main()
		{
			Console.SetWindowSize(200, 50);
			SetForegroundWindow(GetConsoleWindow());

			// Download Depot Downloader if not already present...
			DownloadDepotDownloader();

			string version = AcquireVersion();
			string manifest = GVersionsToManifests[version];

			// Prompt for user credentials
			string userName = AcquireUserName();
			string password = AcquirePassword();

			// Download Beat Saber
			string outputDirectory = AcquireOutputDirectory(version);

			Process downloadProcess = Process.Start("dotnet", "DepotDownloader\\DepotDownloader.dll -app 620980 -depot 620981 -manifest " + manifest +	" -username " + userName + " -password " + password + " -dir " + outputDirectory + " -validate");
			if (downloadProcess != null)
			{
				Console.WriteLine("Downloading Beat Saber version " + version);
				Console.WriteLine("Storing downloaded files at " + outputDirectory);
				Console.WriteLine("...");
				downloadProcess.WaitForExit();

				Console.WriteLine("Download successful!");
			}
			else
			{
				Console.WriteLine("Could not download. Giving up...");
			}
		}

		static void DownloadDepotDownloader()
		{
			string depotDownloaderDllName = "./DepotDownloader/DepotDownloader.dll";
			if (!File.Exists(depotDownloaderDllName))
			{
				Console.WriteLine("Downloading the latest version of DepotDownloader (https://github.com/SteamRE/DepotDownloader)");
				string depotDownloaderFileName = "DepotDownloader.zip";
				bool downloadSuccess = true;
				try
				{
					using var client = new WebClient();
					client.DownloadFile("https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_2.4.3/depotdownloader-2.4.3-hotfix1.zip", depotDownloaderFileName);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					downloadSuccess = false;
				}
				if (File.Exists(depotDownloaderFileName))
				{
					ZipFile.ExtractToDirectory(depotDownloaderFileName, "./DepotDownloader");
					File.Delete(depotDownloaderFileName);
				}

				if (!downloadSuccess || !File.Exists(depotDownloaderDllName))
				{
					Console.WriteLine("Could not get depot downloader automatically. Please download it manually and unzip it in the DepotDownloader subfolder and then proceed.");
					Console.WriteLine("Press enter to continue");
					Console.ReadLine();
				}
			}
		}

		static string AcquireVersion()
		{
			Console.WriteLine("Available Beat Saber versions in depot: ");
			Console.WriteLine("-----");
			foreach (KeyValuePair<string, string> versionToManifest in GVersionsToManifests)
			{
				Console.WriteLine(versionToManifest.Key);
			}

			string? wantedVersion;
			while (true)
			{
				Console.WriteLine("Which version would you like to download?");
				Console.Write("> ");

				wantedVersion = Console.ReadLine();

				if (wantedVersion != null && GVersionsToManifests.ContainsKey(wantedVersion))
				{
					break;
				}

				Console.WriteLine("You must input a version from the list.");
			}

			return wantedVersion;
		}

		static string AcquireUserName()
		{
			string? userName = null;
			while (userName == null)
			{
				Console.WriteLine("Please enter your steam user name (Not the display name)");
				Console.Write("> ");
				userName = Console.ReadLine();
			}

			return userName;
		}

		static string AcquirePassword()
		{
			Console.WriteLine("Please enter your steam password. I know this is scary, but please check the source if you are worried. This app does virtually nothing :)");
			Console.Write("> ");

			string password = string.Empty;
			ConsoleKey key;

			do
			{
				var keyInfo = Console.ReadKey(intercept: true);
				key = keyInfo.Key;

				if (key == ConsoleKey.Backspace && password.Length > 0)
				{
					Console.Write("\b \b");
					password = password[0..^1];
				}
				else if (!char.IsControl(keyInfo.KeyChar))
				{
					Console.Write("*");
					password += keyInfo.KeyChar;
				}
			} while (key != ConsoleKey.Enter);

			return password;
		}

		static string AcquireOutputDirectory(string wantedVersion)
		{
			string outputDirectory = Path.Combine(Environment.CurrentDirectory, "Beat_Saber_" + wantedVersion);
			while (true)
			{
				Console.WriteLine("We will currently save the download to this path:");
				Console.WriteLine(outputDirectory);
				Console.WriteLine("Enter another path if you don't like this one, or just press enter to accept the current one");
				string? userInput = Console.ReadLine();
				if (userInput == null || userInput == string.Empty)
				{
					break;
				}
				else
				{
					outputDirectory = userInput;
				}
			}

			return outputDirectory;
		}
	}
}
