using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApplication14
{
    public partial class Form1 : Form
    {
        string sql;
        string connetionString = "Provider=SQLOLEDB;Data Source=SADA-PC\\SQLEXPRESS;Initial Catalog=YelpDB; Integrated Security=SSPI; Connection Timeout = 100";
        OleDbConnection connection;

        DataSet ds_location = new DataSet();
        DataSet ds_category = new DataSet();
        DataSet ds_business = new DataSet();

        bool catgFilled = false;
        bool locationFilled = false;
        bool businessFilled = false;

        String paramCategory, paramLatitude, paramLongitude, paramBusinessId, paramDistance, paramKeyword;



   //     OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);

        public Form1()
        {
            InitializeComponent();
            connection = new OleDbConnection(connetionString);
            connection.Open();
            fillDefaultParams();        // fill in the values for drop down menu for paramters used in queries

        }
        private void loadfunctions()
        {

        }
        private void loadSp()
        {

        }

        private void fillDefaultParams()
        {
            string sql;

            sql = "select category_name from Category";
            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
            DataSet ds = ds_category;

            dataadapter.Fill(ds, "Authors");

            comboBox1.DataSource = ds.Tables[0];
            comboBox1.ValueMember = "category_name";
            comboBox1.DisplayMember = "category";

            catgFilled = true;

 
            sql = "select full_address, latitude, longitude from Business order by full_address";
            dataadapter = new OleDbDataAdapter(sql, connection);
 
             ds = ds_location;

            dataadapter.Fill(ds, "location");

            comboBox2.DataSource = ds.Tables[0];
            comboBox2.ValueMember = "full_address";
            comboBox2.DisplayMember = "location";

            locationFilled = true;

            sql = "select  CONCAT (name, ' @ ', full_address ) as name_address, business_id from business order by name";
            dataadapter = new OleDbDataAdapter(sql, connection);
            ds = ds_business;

            dataadapter.Fill(ds, "Authors");

            comboBox3.DataSource = ds.Tables[0];
            comboBox3.ValueMember = "name_address";
            comboBox3.DisplayMember = "au_lname";
 
            businessFilled = true;

            comboBox4.SelectedIndex = 0;
            


        }


        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int i = comboBox.SelectedIndex;

            DataSet ds = ds_business;

            paramBusinessId = ds.Tables[0].Rows[i].ItemArray[1].ToString();

        }
  

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {

            if (! catgFilled)
            {
                string sql = "select category_name from Category";
                OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
                DataSet ds = ds_category;

                dataadapter.Fill(ds, "Authors");

                comboBox1.DataSource = ds.Tables[0];
                comboBox1.ValueMember = "category_name";
                comboBox1.DisplayMember = "au_lname";

                catgFilled = true;
            }

 
        }

 
        private void button10_Click(object sender, EventArgs e)
        {
            if (!locationFilled)
            {
                string sql = "select full_address, latitude, longitude from Business";
                OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
                DataSet ds = ds_location;

                dataadapter.Fill(ds, "Authors");

                comboBox2.DataSource = ds.Tables[0];
                comboBox2.ValueMember = "full_address";
                comboBox2.DisplayMember = "au_lname";

                locationFilled = true;
            }


        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int i = comboBox.SelectedIndex;

            DataSet ds = ds_location;

            paramLatitude = ds.Tables[0].Rows[i].ItemArray[1].ToString();
            paramLongitude = ds.Tables[0].Rows[i].ItemArray[2].ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
             ComboBox comboBox = (ComboBox)sender;
            int i = comboBox.SelectedIndex;

            DataSet ds = ds_category;

            paramCategory = ds.Tables[0].Rows[i].ItemArray[0].ToString();

        }



        private void button9_Click(object sender, EventArgs e)
        {
            if (!businessFilled)
            {
                string sql = "select  CONCAT (name, ' @ ', full_address ) as name_address from business order by name";
                OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
                DataSet ds = ds_business;

                dataadapter.Fill(ds, "Authors");

                comboBox3.DataSource = ds.Tables[0];
                comboBox3.ValueMember = "name_address";
                comboBox3.DisplayMember = "au_lname";

                businessFilled = true;
            }
            

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int i = comboBox.SelectedIndex;

            i = (i + 1) * 1609;  // convert miles to meters

            paramDistance = (i+1).ToString();

        }


        // Handle Query Executions [Q1 ... Q5]


        // handles Q1 - exec Q1 paramCategory, paramLatitude, paramLogitude

        private void Q1_Click(object sender, EventArgs e)
        {
            // initialize the user interface

            dataGridView1.DataSource = null;
            dataGridView1.Refresh();

            // set up the query

            string sql = "exec Q1 " + '"' + paramCategory + '"' + ", " + paramLatitude + ", " + paramLongitude;

            // Update the description and SQL command for the query submitted

            QueryDescription.Text = "Given a category and the current location, finds the businesses within 10 miles of the current location based on  rating, review count & proximity to current location.";
            QueryString.Text = sql;

            QueryDescription.Refresh();
            QueryString.Refresh();

            // submit the query


            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
            DataSet ds = new DataSet();

            dataadapter.Fill(ds, "Q1");

            // Assign the result set to the data source of the dataGridView

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Q1";


        }

        
        // handles Q2a - exec Q2a

        private void Q2a_Click(object sender, EventArgs e)
        {

            // initialize the user interface

            dataGridView1.DataSource = null;
            dataGridView1.Refresh();

            // set up the query

            string sql = "exec Q2_1";

            // Update the description and SQL command for the query submitted

            QueryDescription.Text = "For each business category, finds the businesses that were rated best in June 2011";
            QueryString.Text = sql;

            QueryDescription.Refresh();
            QueryString.Refresh();

            // submit the query

            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
            DataSet ds = new DataSet();

            dataadapter.Fill(ds, "Q2a");

            // Assign the result set to the data source of the dataGridView

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Q2a";

        }

        // handles Q2a - exec Q2b

        private void Q2b_Click(object sender, EventArgs e)
        {

            // initialize the user interface

            dataGridView1.DataSource = null;
            dataGridView1.Refresh();

            // set up the query

            string sql = "exec Q2_2";

            // Update the description and SQL command for the query submitted

            QueryDescription.Text = "Finds the restaurants that steadily improved their ratings during the year of 2012";
            QueryString.Text = sql;

            QueryDescription.Refresh();
            QueryString.Refresh();

            // submit the query

            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);

            dataadapter.SelectCommand.CommandTimeout = 0; // this is a long running query

            DataSet ds = new DataSet();
            dataadapter.Fill(ds, "Q2b");



            // Assign the result set to the data source of the dataGridView

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Q2b";

        }


        // handles Q3 - exec Q3,  business_id,  distance

        private void Q3_Click(object sender, EventArgs e)
        {
            // initialize the user interface

            dataGridView1.DataSource = null;
            dataGridView1.Refresh();


            // set up the query
  
            string sql = "exec Q3 " + '"' + paramBusinessId + '"' + ", " + paramDistance;

            // Update the description and SQL command for the query submitted

            QueryDescription.Text = "Given a “business” and distance, provides competitive rankings against other businesses in all categories for that business.";
            QueryString.Text = sql;

            QueryDescription.Refresh();
            QueryString.Refresh();

            // submit the query

            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
            DataSet ds = new DataSet();

            dataadapter.Fill(ds, "Q3");


            // Assign the result set to the data source of the dataGridView

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Q3";
 
        }


        // handles Q4 - exec Q4 category, distance, keyword, lat, long

        private void Q4_Click(object sender, EventArgs e)
        {
  //          exec Q4 'Food', 5000, 'chicken', 33.4633733188, -111.9269084930 

             // initialize the user interface

            dataGridView1.DataSource = null;
            dataGridView1.Refresh();


            // set up the query

            paramKeyword = "chicken";

            string sql = "exec Q4 " + '"' + paramCategory + '"' + ", " + paramDistance + ", " + '"' + paramKeyword + '"' + ", " + paramLatitude + ", " + paramLongitude;

            // Update the description and SQL command for the query submitted

            QueryDescription.Text = "Given a Category, a location, distance and a list of key words, reports review stats for businesses that has reviews that matches one or more of the keywords";
            QueryString.Text = sql;

            QueryDescription.Refresh();
            QueryString.Refresh();

            // submit the query

            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
            DataSet ds = new DataSet();
            dataadapter.SelectCommand.CommandTimeout = 0; // this can be a long running query

            dataadapter.Fill(ds, "Q4");


            // Assign the result set to the data source of the dataGridView

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Q4";
 
         }

        // handles Q5  - exec Q5

        private void Q5_Click(object sender, EventArgs e)
        {
            // initialize the user interface

            dataGridView1.DataSource = null;
            dataGridView1.Refresh();

            // set up the query

            string sql = "exec Q5";

            // Update the description and SQL command for the query submitted

            QueryDescription.Text = "For each Zipcode, provides the count of restaurants for the 10 most popular international category; ordered by total count descending.";
            QueryString.Text = sql;

            QueryDescription.Refresh();
            QueryString.Refresh();

            // submit the query

            OleDbDataAdapter dataadapter = new OleDbDataAdapter(sql, connection);
            DataSet ds = new DataSet();

            dataadapter.Fill(ds, "Q5");

            // Assign the result set to the data source of the dataGridView

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Q5";

        }
 
    }
}
