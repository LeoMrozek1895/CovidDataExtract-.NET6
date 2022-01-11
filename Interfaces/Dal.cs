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

using DataExtract.Classes;

using System.Data;

namespace DataExtract.Interfaces
{
	/// <summary>
	/// Interface IDal
	/// </summary>
	public interface IDal
	{
		/// <summary>
		/// Gets the row data.
		/// </summary>
		/// <returns>DataSet.</returns>
		DataSet GetRowData();

		/// <summary>
		/// Truncates the data.
		/// </summary>
		/// <param name="dtEnum">The dt enum.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		bool TruncateData(Dal.DataTables dtEnum);

		/// <summary>
		/// Bulks the load data.
		/// </summary>
		/// <param name="dt">The dt.</param>
		void BulkLoadData(DataTable dt);

		/// <summary>
		/// Rebuilds the indexes on the Covid19-Activity Table.
		/// </summary>
		public void RebuildIndexes();

		/// <summary>
		/// Loads the covid data.
		/// </summary>
		/// <returns>DataSet.</returns>
		DataSet LoadCovidData();

		/// <summary>
		/// Gets the data table from CSV file.
		/// </summary>
		/// <param name="csvFilePath">The CSV file path.</param>
		/// <returns>DataTable.</returns>
		DataTable GetDataTableFromCsvFile(string csvFilePath);

	}
}
