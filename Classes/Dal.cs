// ***********************************************************************
// Assembly         : DataExtractNetCore5
// Author           : Leo Mrozek
// Created          : 09-17-2020
//
// Last Modified By : Leo Mrozek
// Last Modified On : 11-29-2021
// ***********************************************************************
// <copyright file="Dal.cs" company="Leo Mrozek Consulting">
//     Leo Mrozek
// </copyright>
// <summary></summary>
// ***********************************************************************

#nullable enable
using DataExtract.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataExtract.Classes
{
	/// <summary>
	/// Class Dal.
	/// Implements the <see cref="DataExtract.Interfaces.IDal" />
	/// </summary>
	/// <seealso cref="DataExtract.Interfaces.IDal" />
	public class Dal : IDal
	{
		/// <summary>
		/// Enum DataTables
		/// </summary>
		public enum DataTables { Working = 0, Production = 1 }

		/// <summary>
		/// The configuration
		/// </summary>
		private readonly IConfiguration _config;

		/// <summary>
		/// The logger
		/// </summary>
		private readonly ILogger<ConsoleApplication> _logger;

		/// <summary>
		/// The dal
		/// </summary>
		private readonly IDalService _dal;

		/// <summary>
		/// Initializes a new instance of the <see cref="Dal" /> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="dal">The dal.</param>
		public Dal(IConfiguration config, ILogger<ConsoleApplication> logger, IDalService dal)
		{
			_config = config;
			_logger = logger;
			_dal = dal;
		}

		/// <summary>
		/// Gets the row data.
		/// </summary>
		/// <returns>DataSet.</returns>
		/// <exception cref="DataException"></exception>
		public DataSet GetRowData()
		{
			_logger.LogInformation("    Starting GetRowData");
			var cns = _config.GetConnectionString("DbConnectionString");
			try
			{
				const string sql = "dbo.usp_Covid19_RowData";
				var col = new DalParamsCollection(new List<DalParameters>());
				return _dal.GetDataSet(cns, sql, ref col);
			}
			catch (Exception ex)
			{
				_logger.LogError("    **** ERROR IN function: GetRowData");
				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: {Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");

				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished GetRowData");
			}

		}

		/// <summary>
		/// Truncates the data.
		/// </summary>
		/// <param name="dtEnum">The dt enum.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		/// <exception cref="DataException"></exception>
		public bool TruncateData(DataTables dtEnum)
		{
			_logger.LogInformation("    Starting TruncateData");

			try
			{
				var cns = _config.GetConnectionString("DbConnectionString");
				const string sql = "dbo.usp_Covid19_TruncateData";
				var col = new DalParamsCollection(new List<DalParameters>
				{
					new("@Id", SqlDbType.Int, null, (int)dtEnum)
				});
				return _dal.ExecuteNonQuery(cns, sql, ref col);
			}
			catch (Exception ex)
			{
				_logger.LogError("    **** ERROR IN function: TruncateData");
				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: {Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished TruncateData");
			}
		}

		public void RebuildIndexes()
		{
			_logger.LogInformation("    Starting RebuildIndexes");

			try
			{
				var cns = _config.GetConnectionString("DbConnectionString");
				const string sql = "[dbo].[Covid19-RebuildIndexes]";
				var col = new DalParamsCollection(new List<DalParameters>());

				_dal.ExecuteNonQuery(cns, sql, ref col);
			}
			catch (Exception ex)
			{

				_logger.LogError("    **** ERROR IN function: RebuildIndexes");

				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: {Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished RebuildIndexes");
			}

		}

		/// <summary>
		/// Bulks the load data.
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <exception cref="DataException"></exception>
		public void BulkLoadData(DataTable dt)
		{
			_logger.LogInformation("    Starting BulkLoadData");

			try
			{
				var cns = _config.GetConnectionString("DbConnectionString");
				const string schema = "dbo";
				const string tableName = "Covid19-Activity-Import";
				using var connection = new SqlConnection(cns);
				//using var bulkCopy = new SqlBulkCopy(connection);

				using var bulkCopy = new SqlBulkCopy(cns, SqlBulkCopyOptions.TableLock);

				var dtSchema = GetActivityTableSchema();

				connection.Open();

				bulkCopy.BatchSize = 10000; //Data will be sent to SQL Server in batches of this size
				bulkCopy.EnableStreaming = true;
				bulkCopy.DestinationTableName = $"{schema}.[{tableName}]";

				//SqlBulkCopyOptions.TableLock.

				dtSchema.ForEach(i =>
				{
					bulkCopy.ColumnMappings.Add(i, i);
				});

				bulkCopy.WriteToServer(dt);
			}
			catch (Exception ex)
			{
				_logger.LogError("    **** ERROR IN function: BulkLoadData");
				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: {Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished BulkLoadData");
			}
		}

		/// <summary>
		/// Retrieves the Activity Table Schema
		/// </summary>
		/// <returns></returns>
		/// <exception cref="DataException"></exception>
		public List<string> GetActivityTableSchema()
		{
			_logger.LogInformation("    Starting GetActivityTableSchema");

			try
			{
				const string query = "SELECT * FROM dbo.[Covid19-Activity] WHERE 1=0";
				var lst = new List<string>();
				using var cnn = new SqlConnection(_config.GetConnectionString("DbConnectionString"));
				using var cmd = cnn.CreateCommand();

				cmd.CommandText = query;
				cmd.CommandType = CommandType.Text;
				cnn.Open();

				using var rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo);

				lst.AddRange(from DataRow dr in rdr.GetSchemaTable()!.Rows select dr["ColumnName"].ToString());

				cnn.Close();
				return lst;
			}
			catch (Exception ex)
			{
				_logger.LogError("    **** ERROR IN function: BulkLoadData");
				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: {Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished BulkLoadData");

			}
		}

		/// <summary>
		/// Loads the covid data.
		/// </summary>
		/// <returns>DataSet.</returns>
		/// <exception cref="DataException"></exception>
		public DataSet LoadCovidData()
		{
			try
			{
				_logger.LogInformation("    Starting LoadCovidData");

				var cns = _config.GetConnectionString("DbConnectionString");
				const string sql = "dbo.usp_LoadCovid19ActivityTable";
				var col = new DalParamsCollection(new List<DalParameters>());

				return _dal.GetDataSet(cns, sql, ref col, CommandType.StoredProcedure, 240);
			}
			catch (Exception ex)
			{
				_logger.LogError("    **** ERROR IN function: LoadCovidData");
				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: {Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished LoadCovidData");
			}
		}

		/// <summary>
		/// Gets the data table from CSV file.
		/// </summary>
		/// <param name="csvFilePath">The CSV file path.</param>
		/// <returns>DataTable.</returns>
		/// <exception cref="DataException"></exception>
		public DataTable GetDataTableFromCsvFile(string csvFilePath)
		{
			_logger.LogInformation("    Starting GetDataTableFromCsvFile");
			var csvData = new DataTable();
			try
			{
				using var csvReader = new TextFieldParser(csvFilePath);
				csvReader.SetDelimiters(",", "\t");

				csvReader.HasFieldsEnclosedInQuotes = true;
				csvReader.ReadFields()?.ToList().ForEach(column =>
				{
					csvData.Columns.Add(new DataColumn(column) { AllowDBNull = true });
				});

				while (!csvReader.EndOfData)
				{
					var fieldData = csvReader.ReadFields();
					if (fieldData == null) continue;
					csvData.Rows.Add(fieldData.ToArray<object>());
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("    **** ERROR IN function: GetDataTableFromCsvFile");
				_logger.LogError($"    **** Exception: {ex}");
				_logger.LogError($"    **** Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogError($"    **** Message: {Convert.ToString(ex.Message)}");
				_logger.LogError($"    **** Source: { Convert.ToString(ex.Source)}");
				_logger.LogError($"    **** Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Rethrowing error", ex.InnerException);
			}
			finally
			{
				_logger.LogInformation("    Finished GetDataTableFromCsvFile");
			}

			return csvData;
		}
	}
}

