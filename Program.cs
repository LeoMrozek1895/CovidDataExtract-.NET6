// ***********************************************************************
// Assembly         : DataExtractNetCore5
// Author           : Leo Mrozek
// Created          : 09-17-2020
//
// Last Modified By : Leo Mrozek
// Last Modified On : 11-29-2021
// ***********************************************************************
// <copyright file="Program.cs" company="Leo Mrozek Consulting">
//     Leo Mrozek
// </copyright>
// <summary></summary>
// ***********************************************************************
using CommandLine;

using DataExtract.Classes;
using DataExtract.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;

namespace DataExtract
{
	/// <summary>
	/// Class Program.
	/// </summary>
	internal class Program
	{
		/// <summary>
		/// Static variable that stores information from Configuration file
		/// </summary>
		private static IConfiguration _configuration;

		/// <summary>
		/// Starting point of the application.
		/// </summary>
		/// <param name="args">Command Line Arguments: Currently only WaitForExit and ForceReload</param>
		private static void Main(string[] args)
		{
			// Get the system configuration
			_configuration = GetConfiguration();

			var services = ConfigureServices();

			// Create service collection and configure our services
			var serviceProvider = services.BuildServiceProvider();

			// Create console logger
			serviceProvider
				.GetRequiredService<ILoggerFactory>()
				.CreateLogger<ConsoleApplication>();

			// Kick off our actual code
			Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed(opts => { serviceProvider.GetService<ConsoleApplication>()?.Run(opts); });

			Environment.Exit(0);
		}

		/// <summary>
		/// Loads the various classes into the application using DI.
		/// </summary>
		/// <returns>IServiceCollection.</returns>
		private static IServiceCollection ConfigureServices()
		{
			// Get LogLevel from Configuration
			var logLevel = new LoggingInfo(_configuration).GetMinLogLevel();

			var services = new ServiceCollection();
			services.AddLogging(loggingBuilder => loggingBuilder
				.AddSimpleConsole(options =>
				{
					options.IncludeScopes = true;
					options.SingleLine = true;
					//options.TimestampFormat = "MM/dd/yyyy HH:mm:ss ";
					options.TimestampFormat = "HH:mm:ss ";

				})
				.AddConsole()
				.SetMinimumLevel(logLevel));

			services.AddSingleton(_configuration);
			services.AddSingleton<IDalService, DalService>();
			services.AddSingleton<IDal, Dal>();
			services.AddSingleton<ConsoleApplication>();
			return services;
		}

		/// <summary>
		/// Loads the configuration and sets the application environment.
		/// </summary>
		/// <returns>IConfigurationRoot.</returns>
		public static IConfigurationRoot GetConfiguration()
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var json = $"{Directory.GetCurrentDirectory()}\\appsettings.{env}.json";
			if (env == null) { json = $"{Directory.GetCurrentDirectory()}\\appsettings.json"; }

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
				.AddJsonFile(json, false)
				.AddEnvironmentVariables()
				.AddConfiguration(new ConfigurationRoot(new List<IConfigurationProvider>()))
				.Build();

			var appSettings = new AppSettings(configuration);
			configuration.Bind("AppSettings", appSettings);

			var connectionStrings = new ConnectionStrings(configuration);
			configuration.Bind("ConnectionStrings", connectionStrings);

			// Used when you need to see what is being loaded from the appsettings.json file
			//configuration.GetSection("LoggingInfo").GetChildren().ToList().ForEach(l => { Console.WriteLine($"Path: {l.Path}, Value: { l.Value}"); });
			//configuration.GetSection("AppSettings").GetChildren().ToList().ForEach(l => { Console.WriteLine($"Path: {l.Path}, Value: { l.Value}"); });
			//configuration.GetSection("ConnectionStrings").GetChildren().ToList().ForEach(l => { Console.WriteLine($"Path: {l.Path}, Value: { l.Value}"); });

			return configuration;
		}
	}
}
