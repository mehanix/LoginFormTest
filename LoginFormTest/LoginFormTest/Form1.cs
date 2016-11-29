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
using System.Security.Cryptography;

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

            //Encryption time!!
            //search for username and get the PW string

            string query = "Select * from [Table] Where username= '" + textBox1.Text + "'";
            //4. Create an object which will send the query through our connection to the sql server
            SqlDataAdapter sda = new SqlDataAdapter(query,sqlcon);
            //5. Create a dataTable in which we'll put the login information that matches
            DataTable dtbl = new DataTable();
            //6.Fill the table with whichever entry in the DB fits the criteria of:having the username put in the textBox1.Text
            //and password from textBox2.Text
            sda.Fill(dtbl);
            //7.If there is 1 element, check for password matching :)
            if(dtbl.Rows.Count == 1)
            {
                //get the saved string
                string savedPasswordHash = dtbl.Rows[0][1].ToString();
                //turn it into bytes
                byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
                //take the salt out of the string
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                //hash the user inputted PW with the salt
                var pbkdf2 = new Rfc2898DeriveBytes(textBox2.Text, salt, 10000);
                //put the damn thing in a byte vector.. instead of a string. why? why is this necessary?
                //who am i to judge cryptography standards i guess
                byte[] hash = pbkdf2.GetBytes(20);
                //oh, this is why
                //compare results! letter by letter!
                //starting from 17 cause 0-16 are the salt
                int ok = 1;
                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                        ok = 0;
                if (ok == 1)
                {
                    Form2 frm2 = new Form2();
                    this.Hide();
                    frm2.ShowDialog();
                    Close();
                }
                else
                {
                    //if wrong password, show
                    MessageBox.Show("Incorrect username or password!");
                }
            }
            //If no match for username, show
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




                    //hashing and stuff
                    //make a new byte array
                    byte[] salt;
                    /*generate salt*/
                    new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
                    /*hash and salt it using PBKDF2*/
                    var pbkdf2 = new Rfc2898DeriveBytes(textBox2.Text, salt, 10000);
                    //place the string in the byte array (thats waht getbytes does)
                    byte[] hash = pbkdf2.GetBytes(20);
                    //make new byte array where to store the hashed password+salt
                    //why 36? cause 20 are for the hash and 16 for the salt
                    byte[] hashBytes = new byte[36];
                    //place the god damn things where they belong
                    Array.Copy(salt, 0, hashBytes, 0, 16);
                    Array.Copy(hash, 0, hashBytes, 16, 20);

                    //now, convert our fancy byte array to a string so i can put it in the db
                    string savedPasswordHash = Convert.ToBase64String(hashBytes);


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
                            comm.Parameters.AddWithValue("@val2", savedPasswordHash);
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

