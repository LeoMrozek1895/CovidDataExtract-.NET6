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
using DataExtract.Classes;

using System.Data;
using System.Data.SqlClient;

namespace DataExtract.Interfaces
{
	/// <summary>
	/// Interface IDalService
	/// </summary>
	public interface IDalService
	{
		/// <summary>
		/// Loads the command parameters.
		/// </summary>
		/// <param name="cn">The cn.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <param name="commandTimeout">The command timeout.</param>
		/// <returns>SqlCommand.</returns>
		SqlCommand LoadCommandParameters(SqlConnection cn, string strSql, DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30);

		/// <summary>
		/// Executes the data reader.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns>SqlDataReader.</returns>
		SqlDataReader ExecuteDataReader(string connString, string strSql, ref DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure);

		/// <summary>
		/// Executes the non query.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		bool ExecuteNonQuery(string connString, string strSql, ref DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure);

		/// <summary>
		/// Executes the scalar.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns>System.Object.</returns>
		object ExecuteScalar(string connString, string strSql, ref DalParamsCollection parms, CommandType commandType = CommandType.StoredProcedure);

		/// <summary>
		/// Gets the data set.
		/// </summary>
		/// <param name="connString">The connection string.</param>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="parms">The parms.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <param name="commandTimeout"></param>
		/// <returns>DataSet.</returns>
		DataSet GetDataSet(string connString, string strSql, ref DalParamsCollection parms,
			CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30);
	}
}
