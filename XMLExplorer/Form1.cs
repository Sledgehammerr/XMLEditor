using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using TextEditor;

namespace WindowsFormsApp3
{

    public partial class Form1 : Form
    {
        DataSet dataSet = new DataSet();
        List<Student> students;
        string filePath = "";
        bool needsSaving = false;

        public Form1()
        {
            InitializeComponent();
            dataSet.Clear();

            exitToolStripMenuItem.Click += (s,e) => Close();
        }

        private void InitDataGridPages()
        {
            foreach (var table in dataSet.Tables)
            {
                this.tabControl1.TabPages.Add(table.ToString());
                var page = tabControl1.TabPages[tabControl1.TabPages.Count - 1];
                var dataGrid = new DataGridView();
                page.Controls.Add(dataGrid);
                dataGrid.Dock = DockStyle.Fill;
                dataGrid.DataSource = table;

                var t = table as DataTable;
                foreach (var c in t.Columns)
                {
                    var col = c as DataColumn;
                    if (col.ColumnMapping == MappingType.Hidden)
                        col.ColumnMapping = MappingType.Attribute;
                }

                dataGrid.RowValidating += RowValidating;
                dataGrid.DataError += DataError;
            }
        }

        private void DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            var dataGrid = sender as DataGridView;
            dataGrid.Rows[e.RowIndex].ErrorText = e.Exception.Message;
            MessageBox.Show(e.Exception.Message, "DataError", MessageBoxButtons.OK, MessageBoxIcon.Error);            
        }

        private void RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            Debug.WriteLine($"{e.RowIndex}:{e.ColumnIndex}");
            var dataGrid = sender as DataGridView;
            dataGrid.Rows[e.RowIndex].ErrorText = "";

            string Text = dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            bool hasPunctuation = false;

            for (int i = 0; i < Text.Length; i++)
            {
                if (Char.IsPunctuation(Text, i))
                {
                    hasPunctuation = true;
                    break;
                }
            }
            if ((String.IsNullOrEmpty(Text)) || String.IsNullOrWhiteSpace(Text) || hasPunctuation)
            {
                e.Cancel = true;
                dataGrid.Rows[e.RowIndex].ErrorText = "Error";
                return;
            }

            string headerText = dataGrid.Columns[e.ColumnIndex].HeaderText;
            if (((new List<string> { "Course", "Group_Text", "SingleMark_Text" }).Contains(headerText)) && !(Text.All(char.IsDigit)))
            {
                e.Cancel = true;
                dataGrid.Rows[e.RowIndex].ErrorText = "Wrong Symbol";
            }
        }

        private void DeserializeDataSet()
        {
            XmlReader xml = XmlReader.Create(new StringReader(dataSet.GetXml()));
            var serializer = new XmlSerializer(typeof(List<Student>));
            students = serializer.Deserialize(xml) as List<Student>;
            foreach (var student in students as List<Student>)
            {
                Debug.WriteLine(student.Name);
            }
        }

        private void SerializeDataSet()
        {
            var serializer = new XmlSerializer(typeof(List<Student>));
            StringBuilder data = new StringBuilder();
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.Unicode;
            var xml = XmlWriter.Create(new StringWriter(data), settings);

            serializer.Serialize(xml, students);
            dataSet.ReadXml(XmlReader.Create(new StringReader(data.ToString())));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                dataSet.Clear();
                while (tabControl1.Controls.Count > 0) tabControl1.TabPages[0].Dispose();
                dataSet.ReadXml(filePath);
                needsSaving = false;
                InitDataGridPages();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (needsSaving)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filePath = saveFileDialog1.FileName;
                    dataSet.WriteXml(filePath);
                    needsSaving = false;
                }
            }
            else
            {
                dataSet.WriteXml(filePath);
                needsSaving = false;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog1.FileName;
                dataSet.WriteXml(filePath);
                needsSaving = false;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newFileDialog = new NewFileForm();
            if (newFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                filePath = newFileDialog.FileName;
                needsSaving = true;
                dataSet.Clear();
                while (tabControl1.Controls.Count > 0) tabControl1.TabPages[0].Dispose();
                dataSet.ReadXmlSchema("student_scheme.xsd");
                InitDataGridPages();
            }
        }
    }
}
