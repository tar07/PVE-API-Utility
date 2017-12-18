﻿/*
 * Simple form for submitting manual API queries.
 */

using System;
using System.Windows.Forms;
using System.Xml.Linq;
using PVEAPIUtility.CustomExtensions;
using static System.String;

namespace PVEAPIUtility
{
    public partial class CustomQueryForm : Form
    {
        private readonly PVEAPIForm mainForm;

        public CustomQueryForm(PVEAPIForm form, string url)
        {
            if (form == null || IsNullOrEmpty(url))
            {
                throw new ArgumentNullException();
            }

            InitializeComponent();
            mainForm = form;
            RootURL = url;
        }

        public string RootURL { get; set; }

        private void BtnClose_Click(object sender, EventArgs e) => Hide();

        /// <summary>
        /// Sends the XML query and prints it as an XDocument (with encoded characters replaced for readability).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            btnSubmit.Enabled = false;
            btnSubmit.Text = "Sending...";
            string response;
            try
            {
                response = await txtXMLQuery.Text.SendXml(RootURL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Query Error");
                return;
            }
            finally
            {
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Submit Query";
            }

            Hide();
            mainForm.txtResponse.Focus();
            response = XDocument.Parse(response).ToString();
            response = response
                .Replace("&gt;", ">")
                .Replace("&lt;", "<");
            mainForm.txtResponse.AppendText($"{response}\n", mainForm.GetQueryColor());
        }

        private void BtnXML_Click(object sender, EventArgs e)
        {
            try
            {
                txtXMLQuery.Text = XDocument.Parse(txtXMLQuery.Text).ToString();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
    }
}