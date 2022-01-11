// ***********************************************************************
// Assembly         : DataExtractNetCore5
// Author           : Leo Mrozek
// Created          : 09-17-2020
//
// Last Modified By : Leo Mrozek
// Last Modified On : 11-29-2021
// ***********************************************************************
// <copyright file="CommandLineOptions.cs" company="Leo Mrozek Consulting">
//     Leo Mrozek
// </copyright>
// <summary>Class for Commandline Options.</summary>
// ***********************************************************************
using CommandLine;

namespace DataExtract.Classes
{
	/// <summary>
	/// Commandline options for the applications using the CommandLine Parser from Nuget
	///
	/// WaitTorExit - Displays a prompt to hit any key to complete and close the application
	/// ForceReload - Forces a reload on the same days file.
	///		SkipDownload - Uses an already downloaded file for the same day
	///						Otherwise re-downloads a new file for that day from the host
	/// KeepDownload - Saves the file downloaded with a DateStamp and index number
	/// 
	public class CommandLineOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether [wait for exit].
		/// </summary>
		/// <value><c>true</c> if [wait for exit]; otherwise, <c>false</c>.</value>
		[Option('w', "wait", Required = false, HelpText = "Set to whether to wait to continue when done or to close when done.")]
		public bool WaitForExit { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [force reload].
		/// </summary>
		/// <value><c>true</c> if [force reload]; otherwise, <c>false</c>.</value>
		[Option('f', "force", Required = false, HelpText = "Force a reload even if it has already been done and row counts are the same.")]
		public bool ForceReload { get; set; }

		/// <summary>
		/// True to keep the file downloaded from data.world to reuse for other purposes
		/// </summary>
		[Option('d', "keep", Required = false, HelpText = "Keep the downloaded CSV file.")]
		public bool KeepDownload { get; set; }

		/// <summary>
		/// Skips downloading the file in the event that you are reusing a file
		/// </summary>
		[Option('s', "skip", Required = false, HelpText = "Skip Downloading file. This means you have already downloaded the file previously.")]
		public bool SkipDownload { get; set; }

		/// <summary>
		/// Save file as JSON file. Note file could be huge > 500MB
		/// </summary>
		[Option('j', "json", Required = false, HelpText = "Save file as JSON file. Note file could be huge > 500MB.")]
		public bool SaveDataToJsonFile { get; set; }

		/// <summary>
		/// Truncate production and reload entire table, without does only import since last import.
		/// </summary>
		[Option('r', "full", Required = false, HelpText = "Truncate production and reload entire table, without does only import since last import.")]
		public bool FullReload { get; set; }

	}
}
