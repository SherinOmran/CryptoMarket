using System;
using System.Data.OleDb;

public class ExcelReader
{
    public double[,] ReadArrayFromExcel(string filePath, string sheetName, int numRows, int numCols)
    {
        // Create a connection to the Excel file
        var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1;\"";
        using (var connection = new OleDbConnection(connectionString))
        {
            // Open the connection and create a command to select data from the specified worksheet
            connection.Open();
            var command = new OleDbCommand($"SELECT TOP {numRows} * FROM [{sheetName}$]", connection);

            // Read the data from the worksheet into a data reader
            var dataReader = command.ExecuteReader();

            // Create a 2D array to store the data
            var data = new double[numRows, numCols];

            // Loop through the rows of the data reader and populate the array
            int i = 0;
            while (dataReader.Read() && i < numRows)
            {
                for (int j = 0; j < numCols; j++)
                {
                    // Read the value from the data reader and convert it to a double
                    var valueString = dataReader[j].ToString();
                    double value;
                    if (double.TryParse(valueString, out value))
                    {
                        data[i, j] = value;
                    }
                    else
                    {
                        // Handle invalid values here, such as setting them to zero
                        data[i, j] = 0.0;
                    }
                }
                i++;
            }

            return data;
        }
    }
}
