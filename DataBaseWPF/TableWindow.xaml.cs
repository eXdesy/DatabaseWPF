using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System.Runtime.Intrinsics.X86;
using System.Collections;

namespace DataBaseWPF
{
    public partial class TableWindow : UserControl
    {
        // Connection to the database
        private readonly MySqlConnection? connection;
        private readonly string tableName;

        public TableWindow(MySqlConnection con, string table)
        {
            InitializeComponent();
            connection = con;
            tableName = table;

            // Method that loads data into the table when the window initializes
            LoadDataTable();
        }

        private void LoadDataTable()
        {
            try
            {
                // Query to get all data from the table
                string query = $"SELECT * FROM {tableName}";
                MySqlDataAdapter dataAdapter = new(query, connection);

                // Create a DataSet to store the data
                DataSet dataSet = new();
                dataAdapter.Fill(dataSet, "DataTable");

                // Bind the data to the DataGrid control (or any other control of your choice)
                dataGrid.ItemsSource = dataSet.Tables["DataTable"].DefaultView;
            }
            catch (Exception ex)
            {
                // Error handling when loading data
                MessageBox.Show($"Error loading data from table {tableName}: {ex.Message}");
            }
        }

        private string[] GetColumnNames()
        {
            // Get the column names of the table
            DataTable schemaTable = connection.GetSchema("Columns", new[] { null, null, tableName, null });
            return schemaTable.AsEnumerable().Select(row => row.Field<string>("COLUMN_NAME")).ToArray();
        }

        private void CreateButton(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get column names
                string[] columnNames = GetColumnNames();

                // Build the insert query
                string insertQuery = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", columnNames.Select(c => $"@{c}"))})";
                MySqlCommand insertCommand = new(insertQuery, connection);

                // Create a new row in the DataTable
                DataTable dataTable = ((DataView)dataGrid.ItemsSource)?.Table;
                DataRow newRow = dataTable?.NewRow();

                if (dataTable != null && newRow != null)
                {
                    // Get the selected row from the DataGrid
                    DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

                    // Get the cell values from the DataGrid
                    foreach (var columnName in columnNames)
                    {
                        // Use the column name to get the value from the selected row
                        if (selectedRow != null && selectedRow.Row.Table.Columns.Contains(columnName))
                        {
                            object cellValue = selectedRow[columnName];
                            newRow[columnName] = cellValue;
                            insertCommand.Parameters.AddWithValue($"@{columnName}", cellValue);
                        }
                    }

                    // Add the new row to the DataTable
                    dataTable.Rows.Add(newRow);

                    insertCommand.ExecuteNonQuery();

                    // Reload data after the insertion
                    LoadDataTable();
                }
                else
                {
                    MessageBox.Show("Could not access the DataTable or create a new row.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding basedate: " + ex.Message);
            }
        }

        // Similar methods for Read, Update, Delete buttons and other events...
        private void ReadButton(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDataTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading basedate: " + ex.Message);
            }
        }

        private void UpdateButton(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get column names
                string[] columnNames = GetColumnNames();

                // Build the update query
                string updateQuery = $"UPDATE {tableName} SET {string.Join(", ", columnNames.Skip(1).Select(c => $"{c} = @{c}"))} WHERE {columnNames[0]} = @{columnNames[0]}";
                MySqlCommand updateCommand = new(updateQuery, connection);

                // Get the selected row from the DataGrid
                DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

                if (selectedRow != null)
                {
                    // Set parameters for the update query
                    foreach (var columnName in columnNames.Skip(1))
                    {
                        if (selectedRow.Row.Table.Columns.Contains(columnName))
                        {
                            object cellValue = selectedRow[columnName];
                            updateCommand.Parameters.AddWithValue($"@{columnName}", cellValue);
                        }
                    }

                    // Use the value of the first column as a unique identifier
                    string firstColumnName = columnNames[0];
                    object firstColumnValue = selectedRow[firstColumnName];
                    updateCommand.Parameters.AddWithValue($"@{firstColumnName}", firstColumnValue);

                    // Execute the update query
                    updateCommand.ExecuteNonQuery();

                    // Reload data after the update
                    LoadDataTable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating basedate: " + ex.Message);
            }
        }

        private void DeleteButton(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get column names
                string[] columnNames = GetColumnNames();

                // Build the delete query
                string deleteQuery = $"DELETE FROM {tableName} WHERE {columnNames[0]} = @{columnNames[0]}";
                MySqlCommand deleteCommand = new(deleteQuery, connection);

                // Get the selected row from the DataGrid
                DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

                if (selectedRow != null)
                {
                    // Use the value of the first column as a unique identifier
                    string firstColumnName = columnNames[0];
                    object firstColumnValue = selectedRow[firstColumnName];
                    deleteCommand.Parameters.AddWithValue($"@{firstColumnName}", firstColumnValue);

                    // Execute the delete query
                    deleteCommand.ExecuteNonQuery();

                    // Reload data after deletion
                    LoadDataTable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting basedate: " + ex.Message);
            }
        }

        // Method to execute queries and display results in the DataGrid
        private void ExecuteQuery(string CRUD)
        {
            DataTable resultDataTable = new DataTable();
            try
            {
                // Execute the query and load results into the DataTable
                using (MySqlCommand cmd = new MySqlCommand(CRUD, connection))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    resultDataTable.Load(reader);

                    // Assign the result to the DataGrid
                    dataGrid.ItemsSource = resultDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                // Error handling when executing queries
                MessageBox.Show($"Error executing query: {ex.Message}");
            }
        }

        // Event for the button "Show top 5 best-selling products"
        private void FirstButton(object sender, RoutedEventArgs e)
        {
            // Execute a specific query when the button is clicked
            ExecuteQuery("SELECT * FROM Products ORDER BY UnitsInStock DESC LIMIT 5");
        }

        // Event for the button "Show products out of stock"
        private void SecondButton(object sender, RoutedEventArgs e)
        {
            // Execute a specific query when the button is clicked
            ExecuteQuery("SELECT * FROM Products WHERE UnitsInStock = 0");
        }

        // Event for the button "Show top 5 most expensive products"
        private void ThirdButton(object sender, RoutedEventArgs e)
        {
            // Execute a specific query when the button is clicked
            ExecuteQuery("SELECT * FROM Products ORDER BY UnitPrice DESC LIMIT 5");
        }

        // Event to execute custom queries
        private void SendButton(object sender, RoutedEventArgs e)
        {
            // Execute the query entered by the user
            ExecuteQuery(query.Text);
        }
    }
}
