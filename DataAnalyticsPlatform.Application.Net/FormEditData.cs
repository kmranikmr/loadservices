using DataAnalyticsPlatform.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataAnalyticsPlatform.Application.Net
{
    public partial class FormEditData : Form
    {
        public FieldInfo SelectedData { get; set; }

        public bool HideMap { get; set; }

        public FormEditData()
        {
            InitializeComponent();
            this.Load += FormEditData_Load;
            btnUpdate.Click += BtnUpdate_Click;
            btnCancel.Click += BtnCancel_Click;

        }

        private void FormEditData_Load(object sender, EventArgs e)
        {
            if (HideMap)
            {
                txtMap.Enabled = false;
            }

            cmbDataType.DataSource = Enum.GetValues(typeof(DataType));

            txtColumnName.Text = SelectedData.Name;
            txtLength.Value = SelectedData.Length;
            cmbDataType.SelectedItem = SelectedData.DataType;
            txtMap.Text = SelectedData.Map;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            SelectedData.Name = txtColumnName.Text;
            SelectedData.Length = (int)txtLength.Value;
            SelectedData.DataType = (DataType) cmbDataType.SelectedValue;
            SelectedData.Map = txtMap.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
