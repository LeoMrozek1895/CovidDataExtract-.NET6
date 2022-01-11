// ***********************************************************************
// Assembly         : DataExtractNetCore5
// Author           : Leo Mrozek
// Created          : 09-17-2020
//
// Last Modified By : Leo Mrozek
// Last Modified On : 11-29-2021
// ***********************************************************************
// <copyright file="ConsoleApplication.cs" company="Leo Mrozek Consulting">
//     Leo Mrozek
// </copyright>
// <summary></summary>
// ***********************************************************************
using DataExtract.Classes;
using DataExtract.Interfaces;

using System;
using System.Data;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using Microsoft.Extensions.Logging;

using System.Linq;
using System.Text.RegularExpressions;

using File = System.IO.File;


#if NET5_0
using System.Net;
#endif
#if NET6_0_OR_GREATER
using System.Net.Http;
#endif


namespace DataExtract
{
	/// <summary>
	/// Class ConsoleApplication.
	/// </summary>
	public class ConsoleApplication
	{
		/// <summary>
		/// The logger for the application
		/// </summary>
		private readonly ILogger<ConsoleApplication> _logger;

		/// <summary>
		/// The Data Access Layer (DAL) for the application
		/// </summary>
		private readonly IDal _dal;

		/// <summary>
		/// Stores CommandLineOptions passed in as formatted arguments
		/// </summary>
		private CommandLineOptions _opts;

		/// <summary>
		/// Initializes class with needed injected classes
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="dal">The dal.</param>
		public ConsoleApplication(ILogger<ConsoleApplication> logger, IDal dal)
		{
			_logger = logger;
			_dal = dal;
		}

		// Link to the CSV file that contains the data on world.data
		private const string Url = "https://query.data.world/s/omp3uuql6d4u2cugslj7kdtbhs4q4k";

		// Spreadsheet name on world.data
		private const string FileName = "Covid-19-Activity.csv";
		private readonly string _fileFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		/// <summary>
		/// Entry point of the application that download, extracts from CSV and saves to database.
		/// </summary>
		/// <param name="opts">CommandLineParameters passed from main</param>
		public void Run(CommandLineOptions opts)
		{
			_logger.LogInformation("Starting application");

			// Save CommandLineOptions into local variable
			_opts = opts;

			var drRowData = _dal.GetRowData().Tables[0].Rows[0];
			var rowCounts = (int)drRowData["RowCount"];
			var lastDate = drRowData["LastDataDate"].ToString();

			// Set the path to the CSV File
			var csvFile = _fileFolder + $@"\{FileName}";

			if (opts.SkipDownload)
			{
				_logger.LogError("Skipping downloading CSV File");
				if (!GetLatestSavedFile(csvFile))
				{
					if (!_opts.WaitForExit)
					{
						Environment.Exit(0);
					}
					Console.ReadKey(false);
					Environment.Exit(0);
				}
			}

			switch (opts.SkipDownload)
			{
				// If we were skipping Download and the file doesn't exist
				case true when !File.Exists(csvFile):
					// delete the existing file if it exists
					if (!DeleteCsvFile(csvFile))
					{
						if (!_opts.WaitForExit)
						{
							Environment.Exit(0);
						}
						Console.ReadKey(false);
						Environment.Exit(0);
					}
					break;
				case false:
					{
						DownloadFile(csvFile);

						if (!File.Exists(FileName))
						{
							_logger.LogError("Error Downloading CSV File");
							return;
						}

						break;
					}
			}

			// This works with Any CSV File to load into a data table
			_logger.LogInformation("Loading CSV File to DataTable");

			var dt = _dal.GetDataTableFromCsvFile(csvFile);

			_logger.LogInformation("Completed loading CSV File to DataTable");

			_logger.LogInformation("Get Row Counts from CSV File");
			if (LogRowCounts(dt, rowCounts, lastDate)) return;


			// Truncate working data table of all data
			_logger.LogInformation("Truncating Working Table");
			_dal.TruncateData(Dal.DataTables.Working);

			// Bulk Load Data into working data table
			_logger.LogInformation("Loading Data to Working Table");

			if (opts.FullReload || opts.ForceReload)
			{
				// Bulk load full All data to staging table
				_logger.LogInformation("Loading Data with Full Reload");
				_dal.BulkLoadData(dt);

				// Truncating Production data table of all data
				_logger.LogInformation("Truncating Data in Production Table");
				_dal.TruncateData(Dal.DataTables.Production);
			}
			else
			{
				// Bulk load only today's data to staging table
				_logger.LogInformation("Loading Data with Today Only Load");
				_dal.BulkLoadData(dt.Select($"REPORT_DATE > #{lastDate}#").CopyToDataTable());
			}

			// Load production data table from the staging table
			_logger.LogInformation("Loading Production Table");
			LoadProductionData();
			_logger.LogInformation("Finished Loading Production Table");

			// Reindex production tables
			_logger.LogInformation(@"Re-Indexing Production Table");
			_dal.RebuildIndexes();

			// Clean up CSV File
			if (opts.KeepDownload)
			{
				_logger.LogInformation("Saving CSV File.");
				SaveActivityFile(csvFile);
			}

			_logger.LogInformation("Cleaning up downloaded file");
			DeleteCsvFile(csvFile);

			if (!_opts.WaitForExit) return;

			_logger.LogInformation("Finished, Press any key to continue...");
			Console.ReadKey(false);
		}

		/// <summary>
		/// Download the file from world.data based on .NET version
		/// </summary>
		/// <param name="csvFile"></param>
		private void DownloadFile(string csvFile)
		{
			_logger.LogInformation("    Downloading CSV File");
#if NET6_0_OR_GREATER
			if (!DownloadFileNet(Url, csvFile))
			{
				if (!_opts.WaitForExit) return;

				_logger.LogInformation("Finished, Press any key to continue...");
				Console.ReadKey(false);
			}
#elif NET5_0
            if(!(DownloadFileNet(Url, csvFile))
				if (!_opts.WaitForExit) return;
				_logger.LogInformation("Finished, Press any key to continue...");
				Console.ReadKey(false);
			}
#else
	// Code here for version <.NET 5 (Like Core 3.1, Core 3.0, etc.
#endif
			_logger.LogInformation("Finished downloading CSV File");
		}

		/// <summary>
		/// Deletes the CSV file that is downloaded (if it exists)
		/// </summary>
		/// <param name="csvFile">Name of the CSV file to delete</param>
		private bool DeleteCsvFile(string csvFile)
		{
			if (!File.Exists(csvFile)) { return true; }

			_logger.LogInformation("    Deleting existing Download file");
			File.Delete(csvFile);

			if (!File.Exists(csvFile)) { return true; }

			_logger.LogError("    Error deleting existing CSV File");
			return false;
		}

		/// <summary>
		/// Saving the downloaded CSV file and giving it a time/date stamp.
		/// Also allows for reusing the saved file instead of downloading a new file.
		/// </summary>
		/// <param name="csvFile"></param>
		private void SaveActivityFile(string csvFile)
		{
			var regex = new Regex("_[0-9]+[.]");
			var cnt = 1;
			var rename = $"{csvFile.Replace(".csv", string.Empty)}_{DateTime.Now:yyyyMMdd}_{cnt}.csv";

			var di = new DirectoryInfo(_fileFolder).EnumerateFiles("*_??.csv").ToList();
			var file = di.OrderByDescending(f => f.Name).FirstOrDefault();

			_logger.LogInformation("    Renaming csvFile to save it.");
			if (file == null)
			{
				File.Copy(csvFile, rename);
			}
			else
			{
				var match = regex.Match(file.Name);
				cnt = Convert.ToInt32(match.Value.Replace("_", "").Replace(".", "")) + 1;
				rename = $"{csvFile.Replace(".csv", string.Empty)}_{DateTime.Now:yyyyMMdd}_{cnt}.csv";
				File.Copy(csvFile, rename);
			}
		}

		/// <summary>
		/// Gets the latest saved file for today
		/// </summary>
		/// <param name="csvFile"></param>
		/// <returns>true/false if the file exists and is renamed to the same name as the downloaded file.</returns>
		private bool GetLatestSavedFile(string csvFile)
		{
			var di = new DirectoryInfo(_fileFolder).EnumerateFiles("*_??.csv").ToList();
			var file = di.OrderByDescending(f => f.Name).FirstOrDefault();

			_logger.LogInformation("    Getting latest Today csvFile.");
			if (file != null) { File.Copy(file.FullName, csvFile); }
			if (!File.Exists(csvFile)) { return true; }

			_logger.LogError("    Error getting latest Today csvFile.");
			return false;

		}

#if NET6_0_OR_GREATER
		/// <summary>
		/// Using .NET 6, downloads the file needed for import
		/// </summary>
		/// <param name="url"></param>
		/// <param name="csvFile"></param>
		/// <returns></returns>
		private bool DownloadFileNet(string url, string csvFile)
		{
			// webComTimeoutTimeout is in milliseconds
			const int webComTimeoutTimeout = 1000000;

			var getTask = new HttpClient().GetAsync(url);
			getTask.Wait(webComTimeoutTimeout);

			if (!getTask.Result.IsSuccessStatusCode)
			{
				_logger.LogError("    **** Error Downloading CSV File ****");
				return false;
			}

			if (File.Exists(csvFile))
			{
				try
				{
					_logger.LogInformation("    CSV File to download exists, deleting file.");
					File.Delete(csvFile);
					if (File.Exists(csvFile))
					{
						_logger.LogError("    **** Error deleting existing CSV File ****");
						return false;
					}
				}
				catch
				{
					_logger.LogError("    **** Error deleting existing CSV File ****");
					return false;
				}
			}

			using var fs = new FileStream(csvFile, FileMode.CreateNew);
			var responseTask = getTask.Result.Content.CopyToAsync(fs);
			responseTask.Wait(webComTimeoutTimeout);

			if (File.Exists(csvFile)) { return true; }

			_logger.LogError("    **** Error Downloading CSV File ****");
			return false;
		}
#elif NET5_0
		/// <summary>
		/// Using .NET 5, downloads the file needed for import
		/// </summary>
		/// <param name="url"></param>
		/// <param name="csvFile"></param>
		/// <returns></returns>
        private bool DownloadFileNet(string url, string csvFile)
        {
            using var client = new WebClient();
            client.DownloadFile(url, csvFile);

			if (File.Exists(csvFile)) { return true; }

			_logger.LogError("    **** Error Downloading CSV File ****");
			return false;
        }
#else
		/// Using other versions of .NET need to go here, downloads the file needed for import
		/// </summary>
		/// <param name="url"></param>
		/// <param name="csvFile"></param>
		/// <returns></returns>

		private bool DownloadFileNet(string url, string csvFile)
		{
			// Need to code for versions < .NET 5

			_logger.LogError("    **** Error Downloading CSV File ****");
			return false;
		}
#endif
		/// <summary>
		/// Logs the row counts.
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="rowCounts">The row counts.</param>
		/// <param name="lastDate">The last date.</param>
		/// <returns><c>true</c> if row counts are same as previous run and force is false else <c>false</c> otherwise.</returns>
		private bool LogRowCounts(DataTable dt, int rowCounts, string lastDate)
		{
			// Get the RowCounts from the CSV DataTable
			var csvCounts = dt.Rows.Count;

			// This is temp code to extract the data set as JSON. The file is about 250+MB so it
			// is only run from Visual Studio for when needed.
			if (_opts.SaveDataToJsonFile) { SaveDataToJsonFile(dt); }

			// Display stats from current data and new data
			_logger.LogInformation("    **** Current Row Counts ****");
			_logger.LogInformation($"        Row Count: {rowCounts}");
			_logger.LogInformation($"        Last Date: {lastDate}");
			_logger.LogInformation("    **** CSV File Row Counts ****");
			_logger.LogInformation($"        Row Count: {csvCounts}");

			// If there is no row count change, there is no new data
			if (rowCounts == csvCounts && !_opts.ForceReload)
			{
				_logger.LogInformation("    **** NO CHANGE IN DATA, ABORTING LOAD ****");

				var csvFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"\{FileName}";
				if (File.Exists(csvFile))
				{
					if (_opts.KeepDownload)
					{
						SaveActivityFile(csvFile);
					}

					File.Delete(csvFile);
				}

				if (!_opts.WaitForExit) return true;

				_logger.LogInformation("Finished, Press any key to continue...");
				Console.ReadKey(false);
				Environment.Exit(0);
			}
			else if (rowCounts == csvCounts && _opts.ForceReload)
			{
				_logger.LogInformation("    **** FORCE RELOADING DATA, ROW COUNT SAME  ****");
			}

			return false;
		}

		/// <summary>
		/// Load production data table from the staging table
		/// </summary>
		private void LoadProductionData()
		{
			var ds = _dal.LoadCovidData();

			// Using returned data, compare import count with copy count to verify all data was copied
			var dt2 = ds?.Tables[0];
			if (dt2 == null) return;

			var dr = dt2.Rows[0];
			_logger.LogInformation($"    Rows imported from Data World: {dr["ImportCount"]}");
			_logger.LogInformation($"    Rows copied to production Table: {dr["CopyCount"]}");

			if ((int)dr["ImportCount"] != (int)dr["CopyCount"])
			{
				_logger.LogInformation(_opts.FullReload
					? "    Data copied from staging to production table does not equal rows imported. Re-Run when completed or investigate why mismatch."
					: "    Loaded today's data to production");
			}
		}

		/// <summary>
		/// Saves data that was imported from Data.World to JSON. File is huge (250MB+ when saved).
		/// </summary>
		/// <param name="dt">The dt.</param>
		private static void SaveDataToJsonFile(DataTable dt)
		{
			var json = JsonConvert.SerializeObject(dt);
			var jsonFile = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Extract.json";
			using var sw = new StreamWriter(jsonFile);
			sw.Write(json);
			sw.Close();
			sw.Dispose();
		}
	}
}