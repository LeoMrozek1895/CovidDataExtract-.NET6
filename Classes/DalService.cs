// ***********************************************************************
// Assembly         : DataExtractNetCore5
// Author           : Leo Mrozek
// Created          : 09-17-2020
//
// Last Modified By : Leo Mrozek
// Last Modified On : 11-29-2021
// ***********************************************************************
// <copyright file="DalService.cs" company="Leo Mrozek Consulting">
//     Leo Mrozek
// </copyright>
// <summary></summary>
// ***********************************************************************
using DataExtract.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

/// <summary>
/// The Classes namespace.
/// </summary>
namespace DataExtract.Classes
{
	/// <summary>
	/// Class DalService.
	/// Implements the <see cref="DataExtract.Interfaces.IDalService" />
	/// </summary>
	/// <seealso cref="DataExtract.Interfaces.IDalService" />
	public class DalService : IDalService
	{
		/// <summary>
		/// The logger
		/// </summary>
		private readonly ILogger<ConsoleApplication> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="DalService" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public DalService(ILogger<ConsoleApplication> logger) { _logger = logger; }

		/// <summary>
		/// Loads the command parameters.
		/// </summary>
		/// <param name="cn">The cn.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <param name="commandTimeout">The command timeout.</param>
		/// <returns>SqlCommand.</returns>
		/// <exception cref="DataException">Data Exception in LoadCommandParameters</exception>
		public SqlCommand LoadCommandParameters(SqlConnection cn, string strSql, DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
		{
			try
			{
				_logger.LogDebug($"Starting LoadCommandParameters. SQL: {strSql}");
				using var cmd = new SqlCommand(strSql, cn) { CommandType = commandType, CommandTimeout = commandTimeout };

				foreach (var itm in parms)
				{
					var parm = new SqlParameter(itm.ParmName, itm.SqlDbType);
					if (itm.Length.HasValue) { parm.Size = Convert.ToInt32(itm.Length); }

					parm.Value = itm.Value;
					cmd.Parameters.Add(parm);
				}
				return cmd;
			}
			catch (Exception ex)
			{
				_logger.LogDebug($"Exception: {ex}");
				_logger.LogDebug($"Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogDebug($"Message: {Convert.ToString(ex.Message)}");
				_logger.LogDebug($"Source: {Convert.ToString(ex.Source)}");
				_logger.LogDebug($"Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Data Exception in LoadCommandParameters", ex.InnerException);
			}
			finally
			{
				_logger.LogDebug("Finished LoadCommandParameters.");
			}
		}

		/// <summary>
		/// Executes the data reader.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns>SqlDataReader.</returns>
		/// <exception cref="Exception">Could not open connection to database</exception>
		/// <exception cref="DataException">Data Exception in ExecuteDataReader</exception>
		public SqlDataReader ExecuteDataReader(string connString, string strSql, ref DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure)
		{
			using var cn = new SqlConnection(connString);

			var cnt = 0;
			cn.Open();
			do
			{
				Thread.Sleep(100);
				cnt += 1;
				if (cnt == 100) { return null; } // give it 10 seconds and give up
			} while (cn.State != ConnectionState.Open);

			if (cn == null) { throw new Exception("Could not open connection to database"); }

			try
			{
				return LoadCommandParameters(cn, strSql, parms, commandType).ExecuteReader();
			}
			catch (Exception ex)
			{
				_logger.LogDebug($"ERROR IN function: ExecuteDataReader");
				_logger.LogDebug($"Exception: {ex}");
				_logger.LogDebug($"Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogDebug($"Message: {Convert.ToString(ex.Message)}");
				_logger.LogDebug($"Source: {Convert.ToString(ex.Source)}");
				_logger.LogDebug($"Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Data Exception in ExecuteDataReader", ex.InnerException);
			}
			finally
			{
				if (cn.State != ConnectionState.Closed) { cn.Close(); }
			}
		}

		/// <summary>
		/// Executes the non query.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		/// <exception cref="DataException">Could not open connection to database. Connection: {connString}</exception>
		/// <exception cref="DataException">Data Exception in ExecuteNonQuery</exception>
		/// <exception cref="DataException">Could not open connection to database. Connection: {connString}</exception>
		public bool ExecuteNonQuery(string connString, string strSql, ref DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure)
		{
			using var cn = new SqlConnection(connString);

			var cnt = 0;
			cn.Open();
			do
			{
				Thread.Sleep(100);
				cnt += 1;
				if (cnt == 100) { return false; } // give it 10 seconds and give up
			} while (cn.State != ConnectionState.Open);

			if (cn == null) { throw new DataException($"Could not open connection to database. Connection: {connString}"); }

			try
			{
				return Convert.ToBoolean(LoadCommandParameters(cn, strSql, parms, commandType).ExecuteNonQuery());
			}
			catch (Exception ex)
			{
				_logger.LogDebug($"ERROR IN function: ExecuteNonQuery");
				_logger.LogDebug($"Exception: {ex}");
				_logger.LogDebug($"Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogDebug($"Message: {Convert.ToString(ex.Message)}");
				_logger.LogDebug($"Source: {Convert.ToString(ex.Source)}");
				_logger.LogDebug($"Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Data Exception in ExecuteNonQuery", ex.InnerException);
			}
			finally
			{
				if (cn.State != ConnectionState.Closed) { cn.Close(); }
			}
		}

		/// <summary>
		/// Executes the scalar.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns>System.Object.</returns>
		/// <exception cref="Exception">Could not open connection to database</exception>
		/// <exception cref="DataException">Data Exception in ExecuteScalar</exception>
		public object ExecuteScalar(string connString, string strSql, ref DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure)
		{
			using var cn = new SqlConnection(connString);

			var cnt = 0;
			cn.Open();
			do
			{
				Thread.Sleep(100);
				cnt += 1;
				if (cnt == 100) { return null; } // give it 10 seconds and give up
			} while (cn.State != ConnectionState.Open);

			if (cn == null) { throw new Exception("Could not open connection to database"); }
			try
			{
				return LoadCommandParameters(cn, strSql, parms, commandType).ExecuteScalar();
			}
			catch (Exception ex)
			{
				_logger.LogDebug($"ERROR IN function: ExecuteScalar");
				_logger.LogDebug($"Exception: {ex}");
				_logger.LogDebug($"Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogDebug($"Message: {Convert.ToString(ex.Message)}");
				_logger.LogDebug($"Source: {Convert.ToString(ex.Source)}");
				_logger.LogDebug($"Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Data Exception in ExecuteScalar", ex.InnerException);
			}
			finally
			{
				if (cn.State != ConnectionState.Closed) { cn.Close(); }
			}
		}

		/// <summary>
		/// Gets the data set.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <param name="commandTimeout"></param>
		/// <returns>DataSet.</returns>
		/// <exception cref="Exception">Could not open connection to database</exception>
		/// <exception cref="DataException">Data Exception in GetDataSet</exception>
		public DataSet GetDataSet(string connString, string strSql, ref DalParamsCollection parms,
									CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
		{
			using var cn = new SqlConnection(connString);

			var cnt = 0;
			cn.Open();
			do
			{
				Thread.Sleep(100);
				cnt += 1;
				if (cnt == 100) { return null; } // give it 10 seconds and give up
			} while (cn.State != ConnectionState.Open);

			if (cn == null) { throw new Exception("Could not open connection to database"); }

			try
			{
				var da = new SqlDataAdapter(LoadCommandParameters(cn, strSql, parms, commandType, commandTimeout));
				var ds = new DataSet();
				da.Fill(ds);
				return ds;
			}
			catch (Exception ex)
			{
				_logger.LogDebug($"ERROR IN function: GetDataSet");
				_logger.LogDebug($"Exception: {ex}");
				_logger.LogDebug($"Inner Exception: {Convert.ToString(ex.InnerException)}");
				_logger.LogDebug($"Message: {Convert.ToString(ex.Message)}");
				_logger.LogDebug($"Source: {Convert.ToString(ex.Source)}");
				_logger.LogDebug($"Stack Trace: {Convert.ToString(ex.StackTrace)}");
				throw new DataException("Data Exception in GetDataSet", ex.InnerException);
			}
			finally
			{
				if (cn.State != ConnectionState.Closed) { cn.Close(); }
			}
		}
	}

	/// <summary>
	/// Class DalParameters.
	/// </summary>
	public class DalParameters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DalParameters" /> class.
		/// </summary>
		/// <param name="parmName">Name of the parm.</param>
		/// <param name="dbType">Type of the database.</param>
		/// <param name="length">The length.</param>
		/// <param name="value">The value.</param>
		public DalParameters(string parmName, SqlDbType dbType, int? length, object value)
		{
			ParmName = parmName;
			SqlDbType = dbType;
			Length = length;
			Value = value;
		}
		/// <summary>
		/// Gets or sets the length.
		/// </summary>
		/// <value>The length.</value>
		public int? Length { get; set; }
		/// <summary>
		/// Gets or sets the name of the parm.
		/// </summary>
		/// <value>The name of the parm.</value>
		public string ParmName { get; set; }
		/// <summary>
		/// Gets or sets the type of the SQL database.
		/// </summary>
		/// <value>The type of the SQL database.</value>
		public SqlDbType SqlDbType { get; set; }
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public object Value { get; set; }

	}

	/// <summary>
	/// Class DalParamsCollection.
	/// Implements the <see cref="CollectionBase" />
	/// Implements the <see cref="IEnumerable{DalParameters}" />
	/// </summary>
	/// <seealso cref="System.Collections.CollectionBase" />
	/// <seealso cref="System.Collections.Generic.IEnumerable{DalParameters}" />
	public class DalParamsCollection : CollectionBase, IEnumerable<DalParameters>
	{
		/// <summary>
		/// The dal parameters
		/// </summary>
		private readonly List<DalParameters> _dalParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="DalParamsCollection" /> class.
		/// </summary>
		/// <param name="parmsList">The parms list.</param>
		public DalParamsCollection(List<DalParameters> parmsList)
		{
			this._dalParameters = parmsList;
		}

		/// <summary>
		/// Adds the specified parm.
		/// </summary>
		/// <param name="parm">The parm.</param>
		public void Add(DalParameters parm)
		{
			_dalParameters.Add(parm);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="T:System.Collections.CollectionBase" /> instance.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.CollectionBase" /> instance.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public new IEnumerator<DalParameters> GetEnumerator()
		{
			return ((IEnumerable<DalParameters>)_dalParameters).GetEnumerator();
		}
	}

}
