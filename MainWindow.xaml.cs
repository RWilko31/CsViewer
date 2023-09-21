using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace CSViewer
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int MaxColumns = 0;
        DataSet sheetData = new DataSet("userData");
        public List<string> col = new List<string> { "", "", "", "", "", "", "", "", "", "", "" };
        public List<List<string>> tableData = new List<List<string>>();

        private DataTable defaultTable()
        {
            tableData = new List<List<string>> { col, col, col, col, col };
            DataTable table = new DataTable();

            //find number of columns
            int columns = 0;
            foreach(List<string> line in tableData)
            { if (line.Count > columns) columns = line.Count; }

            //add columns to table
            for (int i = 0; i < columns; i++) 
            { table.Columns.Add(GetColumnName(i)); }

            //add data to table
            foreach (List<string> line in tableData) 
            { table.Rows.Add(line.ToArray()); }

            MaxColumns = columns;
            return table;
        }
        private DataTable loadData(string filePath)
        {
            DataTable table = new DataTable();
            List<string[]> lines = new List<string[]>();
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(filePath))
                {
                    // Read the stream as a string, and write the string to the console.
                    while (!sr.EndOfStream)
                    {
                        string Line = sr.ReadLine();
                        string[] LineSplit = Line.Split(',');
                        lines.Add(LineSplit);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            //Get number of columns
            int column = 0;
            foreach(string[] line in lines)
            { if (line.Length > column) column = line.Length; }

            //add columns to table
            for (int i = 0; i < column; i++)
            { table.Columns.Add(GetColumnName(i)); }

            //add data to table
            foreach (string[] line in lines)
            { table.Rows.Add(line); }

            MaxColumns = column;
            return table;
        }
        public MainWindow()
        {
            InitializeComponent();
            ResetTable();
        }
        void ResetTable()
        {
            defaultTable();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            dt.DataContext = defaultTable();
        }

        static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void New_Click(object sender, RoutedEventArgs e)
        { ResetTable(); }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string path;
            if (openFileDialog.ShowDialog() == true) dt.DataContext = loadData(openFileDialog.FileName);
            else return;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true) saveData(saveFileDialog.FileName + ".CSV");
            else return;
        }
        public static DataTable DataGridtoDataTable(DataGrid dg)
        {
            dg.SelectAllCells();
            dg.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dg);
            dg.UnselectAllCells();
            String result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            string[] Lines = result.Split(new string[] { "\r\n", "\n" },
            StringSplitOptions.None);
            string[] Fields;
            Fields = Lines[0].Split(new char[] { ',' });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable();
            //1st row must be column names; force lower case to ensure matching later on.  
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToUpper(), typeof(string));
            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0) - 1; i++)
            {
                Fields = Lines[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                {
                    Row[f] = Fields[f];
                }
                dt.Rows.Add(Row);
            }
            return dt;
        }
        void saveData(string filePath)
        {
            List<string> data = new List<string>();
            DataTable table = DataGridtoDataTable(dt);

            try
            {
                // Open the text file using a stream reader.
                using (var sw = new StreamWriter(filePath))
                {
                    List<string> lines = new List<string>();

                    int rows = table.Rows.Count;
                    foreach (DataRow row in table.Rows)
                    {
                        string fullLine = "";
                        for (int i = 0; i < MaxColumns; i++)
                        {
                            if (fullLine == "") fullLine = row[i].ToString();
                            else fullLine += "," + row[i].ToString();
                        }
                        if(rows > 1) sw.WriteLine(fullLine);
                        rows --;
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
