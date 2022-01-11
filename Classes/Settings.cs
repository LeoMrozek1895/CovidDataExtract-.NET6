// ***********************************************************************
// Assembly         : DataExtractNetCore5
// Author           : Leo Mrozek
// Created          : 09-17-2020
//
// Last Modified By : Leo Mrozek
// Last Modified On : 11-29-2021
// ***********************************************************************
// <copyright file="Settings.cs" company="Leo Mrozek Consulting">
//     Leo Mrozek
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable 1587
/// <summary>
/// The Classes namespace.
/// </summary>
#pragma warning restore 1587

namespace DataExtract.Classes
{
	/// <summary>
	/// Class AppSettings.
	/// </summary>
	public class AppSettings
	{
		///// <summary>
		///// Initializes a new instance of the <see cref="AppSettings" /> class.
		///// </summary>
		//public AppSettings() { }

		///// <summary>
		///// Gets or sets the environment.
		///// </summary>
		///// <value>The environment.</value>
		//public string Environment { get; set; }

		///// <summary>
		///// Gets or sets the title.
		///// </summary>
		///// <value>The title.</value>
		//public string Title { get; set; }

		///// <summary>
		///// Gets or sets the version.
		///// </summary>
		///// <value>The version.</value>
		//public string Version { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AppSettings" /> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		public AppSettings(IConfiguration config) => config?.GetSection(nameof(AppSettings)).Bind(this);
	}

	/// <summary>
	/// The Classes namespace.
	/// </summary>
	public class ConnectionStrings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionStrings" /> class.
		/// </summary>
		public ConnectionStrings() { }

		/// <summary>
		/// Gets or sets the connection string value.
		/// </summary>
		/// <value>The connection string.</value>
		public string DbConnectionString { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionStrings" /> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		public ConnectionStrings(IConfiguration config) => config?.GetSection(nameof(ConnectionStrings)).Bind(this);
	}

	/// <summary>
	/// The Classes namespace.
	/// </summary>
	public class LoggingInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingInfo" /> class.
		/// Uses the Configuration to get the section of AppSettings and loads the logging values.
		/// </summary>
		/// <param name="config">The configuration.</param>
		public LoggingInfo(IConfiguration config)
		{
			config?.GetSection(nameof(LoggingInfo)).Bind(this);
		}

		/// <summary>
		/// Returns and stores as a string the minimum logging value for the application
		/// </summary>
		/// <value>The minimum log level.</value>
		public string MinLogLevel { get; set; }

		/// <summary>
		/// Converts the minimum logging string value to a value in LogLevel
		/// </summary>
		/// <returns>LogLevel.</returns>
		public LogLevel GetMinLogLevel()
		{
			return MinLogLevel.ToUpper() switch
			{
				"TRACE" => LogLevel.Trace,
				"DEBUG" => LogLevel.Debug,
				"INFORMATION" => LogLevel.Information,
				"WARNING" => LogLevel.Warning,
				"ERROR" => LogLevel.Error,
				"CRITICAL" => LogLevel.Critical,
				"NONE" => LogLevel.None,
				_ => LogLevel.Debug
			};
		}
	}
}