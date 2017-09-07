﻿/*
 * CreateQueryForm.cs
 * Form for constructing search queries.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PVEAPIUtility
{
    public partial class CreateQueryForm : Form
    {
        private string entID;
        private string sessID;
        private string url;
        private int condCtr = 0;
        private List<ComboBox> condFields = new List<ComboBox>();
        private List<ComboBox> condOps = new List<ComboBox>();
        private List<TextBox> condVals = new List<TextBox>();
        private List<Label> labels = new List<Label>();
        private bool reset = false;

        public CreateQueryForm(string entityID, string sessionID, string hosturl)
        {
            InitializeComponent();
            entID = entityID;
            sessID = sessionID;
            url = hosturl;
        }

        public int ProjID { get; set; }
        public string ReturnFields { get; set; }
        public string[] CondFieldNames { get; set; }
        public string[] Ops { get; set; }
        public string[] Values { get; set; }
        public string SearchType { get; set; }
        public string SortFieldName { get; set; }
        public bool RCO { get; set; }

        private void Form2_Load(object sender, EventArgs e)
        {
            QueryFormInit();
            ToolTipFTR.SetToolTip(cmbFTR, "Comma separated list of indexes to return.");
        }

        /// <summary>
        /// Populates the query window with controls.
        /// </summary>
        private void QueryFormInit()
        {
            TrySetCBox(cmbFTR);
            TrySetCBox(cmbField0);
            condFields.Add(cmbField0);
            condOps.Add(new ComboBox());
            condOps[0] = cmbOp0;
            cmbOp0.SelectedIndex = 0;
            condVals.Add(new TextBox());
            condVals[0] = txtValue0;
            cmbSearchType.SelectedIndex = 0;
            ++condCtr;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            ProjID = Convert.ToInt32(nupProjID.Value);
            ReturnFields = cmbFTR.Text;
            CondFieldNames = new string[condFields.Count];
            Ops = new string[condFields.Count];
            Values = new string[condFields.Count];
            int i = 0;
            foreach (ComboBox cBox in condFields)
            {
                CondFieldNames[i] = cBox.Text;
                ++i;
            }

            i = 0;
            foreach (ComboBox cBox in condOps)
            {
                Ops[i] = cBox.Text;
                ++i;
            }

            i = 0;
            foreach (TextBox tBox in condVals)
            {
                Values[i] = tBox.Text;
                ++i;
            }

            SearchType = cmbSearchType.Text;
            SortFieldName = cmbSort.Text;
            RCO = chkRCO.Checked;
            this.Visible = false;
        }

        private void CmbSort_Click(object sender, EventArgs e)
        {
            string[] flds = cmbFTR.Text.Split(new string[] { ",", ", " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < flds.Length; ++i)
                flds[i] = flds[i].Trim();
            this.cmbSort.Items.Clear();
            this.cmbSort.Items.AddRange(flds);
        }

        /// <summary>
        /// Add condition to the condition panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddCond_Click(object sender, EventArgs e)
        {
            labels.Add(new Label());
            condPanel.Controls.Add(CreateLabel("field", labels[(3 * (condCtr - 1))]));
            condFields.Add(new ComboBox());
            TrySetCBox(condFields[condCtr]);
            condPanel.Controls.Add(condFields[condCtr]);
            labels.Add(new Label());
            condPanel.Controls.Add(CreateLabel("operator", labels[3 * (condCtr - 1) + 1]));
            condOps.Add(new ComboBox());
            condPanel.Controls.Add(WomboCombo("operator", condOps[condCtr]));
            labels.Add(new Label());
            condPanel.Controls.Add(CreateLabel("value", labels[3 * (condCtr - 1) + 2]));
            condVals.Add(new TextBox());
            condPanel.Controls.Add(condVals[condCtr]);
            ++condCtr;
        }

        /// <summary>
        /// Creates new operator combobox.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cBox"></param>
        /// <returns>Returns new Operator combobox.</returns>
        private ComboBox WomboCombo(string type, ComboBox cBox)
        {
            cBox.DropDownStyle = ComboBoxStyle.DropDownList;
            cBox.Items.AddRange(new object[]
            {
            "EQUAL",
            "NOTEQUAL",
            "GREATERTHAN",
            "GREATERTHANOREQUAL",
            "LESSTHAN",
            "LESSTHANOREQUAL",
            "ISNULL",
            "ISNOTNULL",
            "LIKE",
            "NOTLIKE",
            "IN",
            "NOTIN"
            });
            cBox.Size = new System.Drawing.Size(141, 21);
            cBox.SelectedIndex = 0;
            return cBox;
        }

        /// <summary>
        /// Set combobox properties to populate project fields.
        /// </summary>
        /// <param name="cBox"></param>
        private bool TrySetCBox(ComboBox cBox)
        {
            string[] projectFields = XMLHelper.BuildFieldList(entID, sessID, nupProjID.Value.ToString(), url, out bool success).ToArray();
            if (!success)
            {
                MessageBox.Show("Invalid Project ID.");
                return false;
            }

            cBox.Items.Clear();
            cBox.Items.AddRange(projectFields);
            AutoCompleteStringCollection autocomp = new AutoCompleteStringCollection();
            autocomp.AddRange(projectFields);
            cBox.AutoCompleteCustomSource = autocomp;
            cBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            return true;
        }

        /// <summary>
        /// Update comboboxes when changing project.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjChanged(object sender, EventArgs e)
        {
            if (TrySetCBox(cmbFTR))
            {
                foreach (ComboBox cmbField in condFields)
                {
                    TrySetCBox(cmbField);
                }
            }
        }

        private Label CreateLabel(string type, Label label)
        {
            switch (type)
            {
                case "field":
                    label.Padding = new System.Windows.Forms.Padding(6, 7, 21, 0);
                    label.Size = new System.Drawing.Size(87, 20);
                    label.Text = "Field " + Convert.ToInt32(condCtr + 1);
                    return label;

                case "operator":
                    label.Padding = new System.Windows.Forms.Padding(6, 7, 33, 0);
                    label.Size = new System.Drawing.Size(87, 20);
                    label.Text = "Operator";
                    return label;

                case "value":
                    label.Padding = new System.Windows.Forms.Padding(6, 7, 46, 0);
                    label.Size = new System.Drawing.Size(86, 20);
                    label.Text = "Value";
                    return label;

                default:
                    return label;
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            ResetControls();
            reset = true;
        }

        private void ResetControls()
        {
            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = null;
                }

                if (control is ComboBox comboBox)
                {
                    if (comboBox.Items.Count > 0 && comboBox != cmbField0)
                        comboBox.SelectedIndex = 0;
                }

                if (control is CheckBox checkBox)
                {
                    checkBox.Checked = false;
                }
            }

            List<Control> listControls = this.condPanel.Controls.Cast<Control>().ToList();

            foreach (Control flowControl in listControls)
            {
                if (flowControl != lblFName0 && flowControl != lblOp0 && flowControl != lblVal0 && flowControl != cmbField0 && flowControl != cmbOp0 && flowControl != txtValue0)
                {
                    condPanel.Controls.Remove(flowControl);
                    flowControl.Dispose();
                }

                if (flowControl is TextBox textBox)
                {
                    textBox.Text = null;
                }

                if (flowControl is ComboBox comboBox)
                {
                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = 0;
                }
            }

            labels.Clear();
            condFields.Clear();
            condOps.Clear();
            condVals.Clear();
            condCtr = 0;
            QueryFormInit();
        }
    }
}