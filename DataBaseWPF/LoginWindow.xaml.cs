using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using DataBaseWPF;
using DataBaseWPF.database;

namespace DataBaseWPF
{
    // Declaration of the LoginWindow class that inherits from the Window class
    public partial class LoginWindow : Window
    {
        // Constructor of the LoginWindow class
        public LoginWindow()
        {
            // Initializes the visual components defined in XAML (User Interface)
            InitializeComponent();
        }

        // Method that handles the Click event of the login button
        private void LoginButton(object sender, RoutedEventArgs e)
        {
            // Verifies the connection to the database using the Sql class and its Connect method
            if (Sql.Connect(DataBase.Text, Username.Text, Password.Password))
            {
                // If the connection is successful, create a new instance of MainWindow passing the connection as a parameter
                MainWindow mainWindow = new(Sql.con);
                // Show the main window (MainWindow)
                mainWindow.Show();
                // Close the login window (LoginWindow)
                this.Close();
            }
            else
            {
                // If the connection fails, show an error message in a MessageBox
                MessageBox.Show("Username or password is incorrect.");
            }
        }
    }
}
