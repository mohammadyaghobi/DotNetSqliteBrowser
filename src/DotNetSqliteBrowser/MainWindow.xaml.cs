/*
 * DotNetSqliteBrowser 
 * Copyright (C) 2014 "Mohammad Yaghobi Beyrami" m.yaghobi.abc@gmail.com

 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data.SQLite;
using Microsoft.Win32;
using System.Data;

namespace DotNetSqliteBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GetSQLite getSQLite;
        private List<string> changedRows;
        public MainWindow()
        {
            InitializeComponent();
            changedRows = null;
            getSQLite = new GetSQLite();
        }

        public void loadTables()
        {
            try
            {
                string query = "SELECT name, sql from sqlite_master WHERE type='table';";

                DataTable tables = getSQLite.getValueByQuery(query);
                
                tables_lb.Items.Clear();
                ListBoxItem lbi;

                if (tables == null || tables.Rows.Count == 0)
                {
                    lbi = new ListBoxItem();
                    lbi.Name = "NoTable";
                    lbi.Content = "no any table";
                    tables_lb.Items.Add(lbi);
                }
                else
                    foreach(DataRow tablesRow in tables.Rows)
                    {
                        lbi = new ListBoxItem();
                        lbi.Name = tablesRow[0].ToString();
                        lbi.Content = tablesRow[0].ToString();
                        lbi.MouseLeftButtonUp += tables_MouseLeftButtonUp;
                        tables_lb.Items.Add(lbi);
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: "+ex.ToString());
            }
        }

        private void tables_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string tableName = (sender as ListBoxItem).Content.ToString();
            if (sender != null)
            {
                getTableColumns(tableName);
                getTableData(tableName);
            }
        }

        private void getTableData(string _tableName)
        {
            try
            {
                string query = "SELECT rowid, * From '" + _tableName + "' Order By rowid;";
                getSQLite.FillGrid(fulldatagrid_grd, query);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
                    
        public void getTableColumns(string _tableName)
        {
            try
            {
                ListBoxItem lbi;
                string query = "PRAGMA table_info('" + _tableName + "');";
                columns_lb.Items.Clear();
                DataTable tableInfo = getSQLite.getValueByQuery(query);

                string strPrimaryKey;
                foreach (DataRow tablesRow in tableInfo.Rows)
                {
                    lbi = new ListBoxItem();
                    lbi.Name = tablesRow[1].ToString();
                    lbi.Tag = _tableName;
                    strPrimaryKey = (int.Parse(tablesRow[5].ToString()) > 0) ? " PrimaryKey" : "";
                    lbi.Content = tablesRow[1].ToString() + " (" + tablesRow[2].ToString() + ")" + strPrimaryKey;
                    lbi.MouseLeftButtonUp += columns_MouseLeftButtonUp;
                    columns_lb.Items.Add(lbi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void columns_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListBoxItem lbi = sender as ListBoxItem;
                if (lbi != null)
                {
                    DataTable dt = getSQLite.getDB().GetSchema("Columns");
                    dt.DefaultView.RowFilter = "table_name='" + lbi.Tag  + "' and column_name='" + lbi.Name + "'";
                    fulldatagrid_grd.ItemsSource = dt.DefaultView;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void openDB()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog().Value)
                if (openDialog.FileName != null)
                {
                    getSQLite = new GetSQLite(openDialog.FileName);
                    loadTables();
                }
        }

        private void newCommand(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            if (saveDialog.ShowDialog().Value)
                if (saveDialog.FileName != null)
                {
                    SQLiteConnection.CreateFile(saveDialog.FileName);
                    getSQLite = new GetSQLite(saveDialog.FileName);
                    tables_lb.Items.Clear();
                    addTable at = new addTable(getSQLite);
                    at.Show();
                }
        }
        private void openCommand(object sender, RoutedEventArgs e)
        {
            openDB();
        }
        private void exitCommand(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void refreshCommand(object sender, RoutedEventArgs e)
        {
            this.loadTables();
        }

        private void queryexecute_btn_Click(object sender, RoutedEventArgs e)
        {
            string query = queryfield_txt.Text;
            queryerrors_txt.Text = getSQLite.FillGrid(fulldatagrid_grd, queryfield_txt.Text);
        }

        private void clearQueryField(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "query...")
                textBox.Text = string.Empty;
            else
                textBox.SelectAll();
        }

        private void removeTable_Click(object sender, RoutedEventArgs e)
        {
            if (tables_lb.SelectedIndex > -1)
            {
                ListBoxItem lbi = (ListBoxItem)tables_lb.SelectedItem;
                if (lbi.Name != "NoTable" && MessageBox.Show("Are your sure you want to delete this table?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string query = "DROP TABLE '" + lbi.Content + "'";
                    getSQLite.ExecuteNonQuery_(query);
                    this.loadTables();
                }
            }
        }

        private void addTable_Click(object sender, RoutedEventArgs e)
        {
            addTable addTable_ = new addTable(getSQLite);
            addTable_.Show();
        }

        private void refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            loadTables();
        }

        private void editTable_btn_Click(object sender, RoutedEventArgs e)
        {
            if (tables_lb.SelectedIndex > -1)
            {
                string tableName_ = ((ListBoxItem)tables_lb.SelectedItem).Name;
                editTable editTable_ = new editTable(getSQLite, tableName_);
                editTable_.Show();
            }
        }

        private void addColumn_btn_Click(object sender, RoutedEventArgs e)
        {
            if (tables_lb.SelectedIndex > -1)
            {
                string tableName_ = ((ListBoxItem)tables_lb.SelectedItem).Name;
                addColumn addc = new addColumn(getSQLite, tableName_);
                addc.Show();
            }
        }

        private void removeColumn_btn_Click(object sender, RoutedEventArgs e)
        {
            if (columns_lb.SelectedIndex != -1 && tables_lb.SelectedIndex != -1 && MessageBox.Show("Are your sure you want to delete this column?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string tableName_ = ((ListBoxItem)tables_lb.SelectedItem).Name;
                string columnName_ = ((ListBoxItem)columns_lb.SelectedItem).Name;
                string query = "PRAGMA table_info('" + tableName_ + "');";

                DataTable tableInfo = getSQLite.getValueByQuery(query);
                query = "ALTER TABLE " + tableName_ + " RENAME TO temp_" + tableName_ + ";";
                getSQLite.ExecuteNonQuery_(query);

                query = "CREATE TABLE " + tableName_ + "(";
                foreach (DataRow anyColumn in tableInfo.Rows)
                {
                    if (anyColumn["name"].ToString() != columnName_)
                        query += anyColumn["name"].ToString() + " " + anyColumn["type"].ToString() + ", ";
                }
                query += ");";
                query = query.Remove(query.LastIndexOf(", "), 2);

                getSQLite.ExecuteNonQuery_(query);

                query = "INSERT INTO " + tableName_ + "(";
                foreach (DataRow anyColumn in tableInfo.Rows)
                {
                    if (anyColumn["name"].ToString() != columnName_)
                        query += anyColumn["name"].ToString() + ", ";
                }

                query = query.Remove(query.LastIndexOf(", "), 2);
                query += ") SELECT ";

                foreach (DataRow anyColumn in tableInfo.Rows)
                {
                    if (anyColumn["name"].ToString() != columnName_)
                        query += anyColumn["name"].ToString() + ", ";
                }
                query = query.Remove(query.LastIndexOf(", "), 2);
                query += " FROM temp_" + tableName_ + " ;";

                getSQLite.ExecuteNonQuery_(query);

                query = "DROP TABLE temp_" + tableName_ + ";";
                getSQLite.ExecuteNonQuery_(query);

                this.getTableColumns(tableName_);
            }
        }

        private void editColumn_btn_Click(object sender, RoutedEventArgs e)
        {
            if (columns_lb.SelectedIndex != -1)
            {
                string colname = ((ListBoxItem)columns_lb.SelectedItem).Name;
                string tblname = ((ListBoxItem)tables_lb.SelectedItem).Name;
                editColumn ecol = new editColumn(getSQLite, colname, tblname);
                ecol.Show();
            }
        }

        private void updateData_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void getChanges(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (fulldatagrid_grd.SelectedIndex != -1)
                {
                    bool addFlag = true;
                    if (changedRows == null)
                    {
                        changedRows = new List<string>();
                        updateTable_btn.Background.Opacity = 1;
                    }

                    DataRowView drv = (DataRowView)fulldatagrid_grd.SelectedItem;
                    string rowid = (drv.Row)["rowid"].ToString();
                    foreach (string item in changedRows)
                    {
                        if (item == rowid)
                        {
                            addFlag = false;
                            break;
                        }
                    }

                    if (addFlag)
                        changedRows.Add(rowid);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: "+ex.ToString());
            }
        }

        private void updateTable_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (changedRows != null)
                {
                    string tblName = ((ListBoxItem)tables_lb.SelectedItem).Name;
                    string query = "PRAGMA table_info('" + tblName + "');";

                    DataTable tableInfo = getSQLite.getValueByQuery(query);
                    DataTable data = new DataTable();

                    data.Columns.Add("ROWID", typeof(Int64));
                    foreach (DataRow tablesRow in tableInfo.Rows)
                    {
                        data.Columns.Add(tablesRow[1].ToString());
                    }
                    foreach (string item in changedRows)
                    {
                        DataRowView drv = (DataRowView)fulldatagrid_grd.Items.GetItemAt(int.Parse(item) - 1);
                        data.Rows.Add(drv.Row.ItemArray);
                    }

                    changedRows = null;
                    updateTable_btn.Background.Opacity = 0.4;
                    getSQLite.Update(tblName, data);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: "+ex.ToString());
            }
        }

        private void gettoexcute_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addtable_btn_Click(object sender, RoutedEventArgs e)
        {
            if (tables_lb.SelectedIndex != -1)
            {
                string tblName = ((ListBoxItem)tables_lb.SelectedItem).Name;
                tableObject to = new tableObject(getSQLite, tblName);
                tablespanel_wpp.Children.Add(to);
            }
            
        }

        private void exportDBToXML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "SELECT name, sql from sqlite_master WHERE type='table';";

                DataTable table;
                DataTable tables = getSQLite.getValueByQuery(query);
                DataSet allTables = new DataSet();
                allTables.DataSetName = "SQLiteDB";

                if (tables != null)
                    foreach (DataRow tablesRow in tables.Rows)
                    {
                        table = new DataTable();
                        table = getSQLite.getValueByQuery("select * from " + tablesRow[0].ToString());
                        table.TableName = tablesRow[0].ToString();
                        allTables.Tables.Add(table);
                    }
                if (allTables.Tables.Count > 0)
                {

                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "XML (*.xml)|*.xml";

                    if (saveDialog.ShowDialog().Value)
                        if (saveDialog.FileName != null)
                        {
                            allTables.WriteXml(saveDialog.FileName);
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }
        private void exportTableToXML_Click(object sender, RoutedEventArgs e)
        {
            if (tables_lb.SelectedIndex > -1)
            {
                ListBoxItem lbi = (ListBoxItem)tables_lb.SelectedItem;
                if (lbi.Name != "NoTable")
                {
                    DataTable table = getSQLite.getValueByQuery("select * from " + lbi.Content);
                    table.TableName = lbi.Content.ToString();
                    DataSet tableTo = new DataSet();
                    tableTo.DataSetName = "SQLiteDB";
                    tableTo.Tables.Add(table);

                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "XML (*.xml)|*.xml";

                    if (saveDialog.ShowDialog().Value)
                        if (saveDialog.FileName != null)
                        {
                            tableTo.WriteXml(saveDialog.FileName);
                        }
                }
            }
        }

        
    }
}
