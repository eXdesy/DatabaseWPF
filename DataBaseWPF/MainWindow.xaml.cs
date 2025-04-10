using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataBaseWPF.database;
using MySql.Data.MySqlClient;
using DataBaseWPF;


namespace DataBaseWPF
{
    public partial class MainWindow : Window
    {
        // Database connection
        private readonly MySqlConnection? connection;

        // Constructor of the MainWindow class that receives a database connection
        public MainWindow(MySqlConnection con)
        {
            InitializeComponent();
            // Assign the received connection to the class's connection field
            connection = con;
            // Call the method to load tables into the menu
            LoadTablesToMenu();
        }

        // Method to dynamically load menu items with table names
        private void LoadTablesToMenu()
        {
            // Get the list of available table names from the database
            List<string> tableNames = GetTableNames();

            // Dynamically create menu items with table names
            foreach (string tableName in tableNames)
            {
                MenuItem menuItem = new()
                {
                    Header = tableName,
                    FontFamily = new System.Windows.Media.FontFamily("Segoe UI Black"),
                    FontSize = 10,
                    Foreground = System.Windows.Media.Brushes.White,
                    Height = 25
                };
                // Assign the ChangeContentButton event to be triggered when the menu item is clicked
                menuItem.Click += ChangeContentButton;
                // Add the new item to the menu
                MainMenu.Items.Add(menuItem);
            }

            // Add the "EXIT" item at the end of the menu
            MenuItem exitMenuItem = new MenuItem
            {
                Header = "EXIT",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI Black"),
                FontSize = 10,
                Foreground = System.Windows.Media.Brushes.White,
                Height = 25
            };
            // Assign the ExitButton event to be triggered when "EXIT" is clicked
            exitMenuItem.Click += ExitButton;
            // Add the new item to the end of the menu
            MainMenu.Items.Add(exitMenuItem);
        }

        // Method to get the list of table names from the database
        private List<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();
            try
            {
                // Get the list of available tables from the database
                DataTable schema = connection.GetSchema("Tables");
                foreach (DataRow row in schema.Rows)
                {
                    tableNames.Add(row["TABLE_NAME"].ToString());
                }
            }
            catch (Exception ex)
            {
                // Show an error message if there is a problem getting table names
                MessageBox.Show("Error getting table names: " + ex.Message);
            }
            // Return the list of table names
            return tableNames;
        }

        // Method to change the content when a menu item is clicked
        private void ChangeContentButton(object sender, RoutedEventArgs e)
        {
            // Check if the sender is a MenuItem
            if (sender is MenuItem menuItem)
            {
                // Get the table name from the Header of the MenuItem
                string tableName = menuItem.Header.ToString();
                // Create a new UserControl to display the selected table's content
                UserControl newContent = new TableWindow(connection, tableName);
                // Clear the current content and add the new UserControl
                Content.Children.Clear();
                Content.Children.Add(newContent);
            }
        }

        // Method to close the database connection and the current window when "EXIT" is clicked
        private void ExitButton(object sender, RoutedEventArgs e)
        {
            // Close the database connection
            Sql.Close();
            // Create a new instance of LoginWindow
            LoginWindow loginWindow = new LoginWindow();
            // Show the login window and close the current window
            loginWindow.Show();
            this.Close();
        }
    }
}
