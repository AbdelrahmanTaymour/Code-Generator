using CodeGenerator_BusinessLayer;
using Generator.GenerateAppConfig;
using Generator.GenerateBusiness;
using Generator.GenerateDataAccess;
using Generator.GenerateStoredProcedure;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeGenerator_PresentationLayer
{
    public partial class frmCodeGeneratorUI : Form
    {
        string _TableName = "";
        string _AppName = "";
        public frmCodeGeneratorUI()
        {
            InitializeComponent();

        }

        void _LoadDatabasesNameInComboBox()
        {
            DataTable dtDatabasesName = clsCodeGenerator.GetDatabasesList();

            foreach (DataRow row in dtDatabasesName.Rows)
            {
                cbDatabasesName.Items.Add(row["Databases"]);
            }
        }
        void _LoadTabels()
        {
            dgvTablesList.DataSource =  clsCodeGenerator.GetTablesList(cbDatabasesName.Text.Trim());
        }
        void _LoadColumns()
        {
            DataTable dt = clsCodeGenerator.GetColumnsOfTableList(cbDatabasesName.Text.Trim(), _TableName);
            dgvColumnsList.DataSource = dt;
        }

        private void frmCodeGeneratorUI_Load(object sender, EventArgs e)
        {
            _LoadDatabasesNameInComboBox();
            cbDatabasesName.SelectedIndex = 0;

        }
        private void cbDatabasesName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _AppName = cbDatabasesName.Text.Trim();
            _LoadTabels();

        }
        private void dgvTablesList_SelectionChanged(object sender, EventArgs e)
        {
            _TableName = dgvTablesList.CurrentRow.Cells[0].Value.ToString();
            _LoadColumns();
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(txtGeneratedCode.Text))
                Clipboard.SetText(txtGeneratedCode.Text);
        }
        private void imgSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implementing yet...");
        }
        private void txtPaths_Validating(object sender, CancelEventArgs e)
        {
            Guna2TextBox txtBox = (Guna2TextBox)sender;

            if (string.IsNullOrEmpty(txtBox.Text))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtBox, "Please provide the path of the data access layer folder");
                return;
            }

            if (!Directory.Exists(txtBox.Text))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtBox, "This path does not exist");
                return;
            }

            e.Cancel = false;
            errorProvider1.SetError(txtBox, null);
            return;
        }

        private void btnDataAccessLayer_Click(object sender, EventArgs e)
        {
            txtGeneratedCode.Text = clsGenerateDataAccessLayer.GenerateDataAccessLayerClass(_AppName, _TableName, (DataTable)dgvColumnsList.DataSource);
        }
        private void btnBusinessLayer_Click(object sender, EventArgs e)
        {
            txtGeneratedCode.Text = clsGenerateBusinessLayer.GenerateBusinessLayerClass(_AppName, _TableName, (DataTable)dgvColumnsList.DataSource);
        }
        private void btnStoredProcedure_Click(object sender, EventArgs e)
        {
            DataTable tableColumnsPrecision = clsCodeGenerator.GetColumnsOfTableWithPrecisionList(cbDatabasesName.Text.Trim(), _TableName);
            txtGeneratedCode.Text = clsGenerateStoredProcedure.GenerateStoredProcedure(_AppName, _TableName, tableColumnsPrecision);
        
        }
        private void btnShowAllStoredProcedures_Click(object sender, EventArgs e)
        {
            txtGeneratedCode.Text = clsGenerateStoredProcedure.ShowSPForAllTables(_AppName, (DataTable)dgvTablesList.DataSource);
        }

        private void btnGenerateAllDataAccess_Click(object sender, EventArgs e)
        {
            if (dgvTablesList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a table from the list ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folderPath = txtDataAccessPath.Text.Trim();
            if(clsGenerateDataAccessLayer.GenerateAllDataAccessClasses(folderPath, _AppName, (DataTable)dgvTablesList.DataSource))
            {
                MessageBox.Show("The data access layer have been generated successfully.", "Done",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The generation is failed ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnGenerateAllBusiness_Click(object sender, EventArgs e)
        {
            if (dgvTablesList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a table from the list ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folderPath = txtBusinessPath.Text.Trim();
            if (clsGenerateBusinessLayer.GenerateAllBusinessClasses(folderPath, _AppName, (DataTable)dgvTablesList.DataSource))
            {
                MessageBox.Show("The business layer have been generated successfully.", "Done",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The generation is failed ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnGenerateAppConfig_Click(object sender, EventArgs e)
        {
            if (dgvTablesList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a table from the list ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folderPath = txtAppConfigPath.Text.Trim();
            if (clsGenerateAppConfigFile.CreateAppConfigFile(folderPath, _AppName))
            {
                MessageBox.Show("The App config file have been generated successfully.", "Done",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The generation is failed ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnGenerateSPselectedTable_Click(object sender, EventArgs e)
        {
            if (dgvTablesList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a table from the list ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataTable tableColumnsPrecision = clsCodeGenerator.GetColumnsOfTableWithPrecisionList(cbDatabasesName.Text.Trim(), _TableName);
            if (clsGenerateStoredProcedure.GenerateSPForSelectedTable(_AppName,_TableName, tableColumnsPrecision))
            {
                MessageBox.Show($"The stored procedures for table {_TableName} have been generated successfully.", "Done",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"The generation {_TableName} is failed ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnGenerateSPforAllTables_Click(object sender, EventArgs e)
        {
            if (dgvTablesList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a table from the list ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (clsGenerateStoredProcedure.GenerateSPForAllTables(_AppName, (DataTable)dgvTablesList.DataSource))
            {
                MessageBox.Show($"The stored procedures for table {_TableName} have been generated successfully.", "Done",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"The generation is failed ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDataAccessSetting_Click(object sender, EventArgs e)
        {
            txtGeneratedCode.Text = clsGenerateDataAccessLayer.GenerateDataAccessSettingsClass(_AppName);
        }
        private void btnLogHandler_Click(object sender, EventArgs e)
        {
            txtGeneratedCode.Text = clsGenerateDataAccessLayer.GenerateDataAccessUtilitiesClass(_AppName);
        }
    }
}
