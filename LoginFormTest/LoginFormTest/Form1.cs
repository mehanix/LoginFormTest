using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//1. import System.Data.SqlClient
using System.Data.SqlClient;

namespace LoginFormTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }


        private void button1_Click(object sender, EventArgs e)
        {
            //2. create new SqlConnection. 
            /*the @"" thing is a Connection String.*/
            SqlConnection sqlcon = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Nix\Documents\Visual Studio 2015\Projects\LoginFormTest\LoginFormTest\DB\loginDB.mdf; Integrated Security = True; Connect Timeout = 30");
            //3.Make a query. The query is what we're going to ask the DB to do, pretty much.
            string query = "Select * from [Table] Where username= '" + textBox1.Text + "' and password = '" + textBox2.Text + "'";
            //4. Create an object which will send the query through our connection to the sql server
            SqlDataAdapter sda = new SqlDataAdapter(query,sqlcon);
            //5. Create a dataTable in which we'll put the login information that matches
            DataTable dtbl = new DataTable();
            //6.Fill the table with whichever entry in the DB fits the criteria of:having the username put in the textBox1.Text
            //and password from textBox2.Text
            sda.Fill(dtbl);
            //7.If there is 1 element (a match has been found), allow access
            if(dtbl.Rows.Count == 1)
            {
                Form2 f2 = new Form2();
                this.Hide();
                f2.Show();
            }
            //If incorrect, don't allow access
            else
            {
                MessageBox.Show("Incorrect username or password!");
            }

            sqlcon.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            //Same drift, connect to the DB
            SqlConnection sqlcon = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Nix\Documents\Visual Studio 2015\Projects\LoginFormTest\LoginFormTest\DB\loginDB.mdf; Integrated Security = True; Connect Timeout = 30");
            //the query is a bit different this time. we're searching to see if the username already exists
            //the principle is the same though, with the dataTable
            string query = "Select * from [Table] Where username= '" + textBox1.Text + "'";
            //creating the adapter
            SqlDataAdapter sda = new SqlDataAdapter(query, sqlcon);
            //creating the DataTable
            DataTable dtbl = new DataTable();
            //filling it with matching info
            sda.Fill(dtbl);
            //now here's what's different: if there is actually a match, we deny the user the registration
            if (dtbl.Rows.Count > 0)
            {
                MessageBox.Show("Username is taken!");
            }
            //else, we register the user (add its username + password combination in our DB)
            else
            {
                try
                {
                    //write the query
                    string commString = "INSERT INTO [Table](username, password) VALUES (@val1, @val2)";
                    string constring = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Nix\Documents\Visual Studio 2015\Projects\LoginFormTest\LoginFormTest\DB\loginDB.mdf; Integrated Security = True; Connect Timeout = 30";
                    //connect to db
                    using (SqlConnection conn = new SqlConnection(constring))
                    {
                        //create an sqlCommand
                        //Why is this done? To sanitize inputs and prevent injection into the string..
                        using (SqlCommand comm = new SqlCommand())
                        {
                            comm.Connection = conn;
                            comm.CommandText = commString;
                            //where the sanitizing happens
                            //santizing prevents adding malitious pieces of code into the query strings
                            comm.Parameters.AddWithValue("@val1", textBox1.Text);
                            comm.Parameters.AddWithValue("@val2", textBox2.Text);
                            //open the connection
                            conn.Open();
                            //execute the query we just wrote
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }
                    }

                    MessageBox.Show("Sucessfully registered!");
                    sqlcon.Close();
                }
                catch { MessageBox.Show("A registration error has occured. Please contact the developer :("); }
                
            }
            
            
        }

        }
    }

