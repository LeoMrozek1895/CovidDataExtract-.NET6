using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataExtract.Classes
{
	public class CovidData
	{

		///	[PEOPLE_POSITIVE_CASES_COUNT], [COUNTY_NAME], [REPORT_DATE], [PROVINCE_STATE_NAME], [CONTINENT_NAME], [DATA_SOURCE_NAME],
		/// [PEOPLE_DEATH_NEW_COUNT], [COUNTY_FIPS_NUMBER], [COUNTRY_ALPHA_3_CODE], [COUNTRY_SHORT_NAME], [COUNTRY_ALPHA_2_CODE],
		/// [PEOPLE_POSITIVE_NEW_CASES_COUNT], [PEOPLE_DEATH_COUNT]

		public int People_Positive_Cases_Count { get; set; }
		public string County_Name { get; set; }
		public DateTime Report_Date { get; set; }
		public string Province_State_Name { get; set; }
		public string Continent_Name { get; set; }
		public string Data_Source_Name { get; set; }
		public long People_Death_New_Count { get; set; }
		public int County_Fips_Number { get; set; }
		public string Country_Alpha_3_Code { get; set; }
		public string Country_Short_Name { get; set; }
		public string Country_Alpha_2_Code { get; set; }
		public long People_Positive_New_Cases_Count { get; set; }
		public long People_Death_Count { get; set; }

	}

	public class ListToDataTableConverter
	{
		public DataTable ToDataTable<T>(List<T> items)
		{
			DataTable dataTable = new DataTable(typeof(T).Name);

			//Get all the properties
			PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo prop in props)
			{
				//Setting column names as Property names
				dataTable.Columns.Add(prop.Name);
			}
			foreach (T item in items)
			{
				var values = new object[props.Length];
				for (int i = 0; i < props.Length; i++)
				{
					//inserting property values to data table rows
					values[i] = props[i].GetValue(item, null);
				}
				dataTable.Rows.Add(values);
			}
			//put a breakpoint here and check data table
			return dataTable;
		}
	}
}
