﻿using LoveCoody.entity;
using LoveCoody.expression;
using LoveCoody.handle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LoveCoody
{
    public partial class AddExpForm : Form
    {
        ListViewItem lvi;
        int index=-1;
        MainForm parentMain;

        public AddExpForm(int index, ListViewItem lvi,MainForm parentMain)
        {
            InitializeComponent();
            this.index = index;
            this.lvi = lvi;
            this.parentMain = parentMain;
        }
        public AddExpForm(MainForm parentMain)
        {
            InitializeComponent();
            this.parentMain = parentMain;
        }

        public void initLvi()
        {
            if (lvi == null)
            {
                for (int i = 0; i < parentMain.HeaderListview.Items.Count; i++)
                {
                    addOrModifyHeader(parentMain.HeaderListview.Items[i].SubItems[0].Text, parentMain.HeaderListview.Items[i].SubItems[1].Text, -1);
                }
                return;
            }
            String expName = lvi.SubItems[1].Text;
            String language = lvi.SubItems[2].Text;
            String status = lvi.SubItems[5].Text;
            ExpNameTextBox.Text = expName;
            LanguafeComboBox.Text = language;
            ExpStatusComboBox.Text = status;
            //解析Header
            String json = lvi.SubItems[3].Text;
            ExpModule exp = (ExpModule)JsonHandle.toBean<ExpModule>(json);
            if (exp == null)
            {
                return;
            }
            if (exp.ExpContext != null)
            {
                //解析Header
                if (exp.ExpContext.Header != null) {
                foreach (String key in exp.ExpContext.Header.Keys)
                {
                    ListViewItem lvitmp = new ListViewItem();
                    lvitmp.SubItems[0].Text = key;
                    lvitmp.SubItems.Add(exp.ExpContext.Header[key]);
                    HeaderListview.Items.Add(lvitmp);
                }
                }
                BodyTextBox.Text = exp.ExpContext.Body;
                EncodeComBox.Text = exp.ExpContext.Encode;
                FormatUrlComboBox.Text = "否";
                RequestMethodComboBox.Text = exp.ExpContext.Method;
                if (exp.FormatUrl) {
                    FormatUrlComboBox.Text = "是";
                }
            }
            if (exp.Verification != null)
            {
                VerificationValueTextBox.Text = exp.Verification.Context;
                CalcComboBox.Text = exp.Verification.Calc;
                VerificationComboBox.Text = MainForm.verificationTypes[exp.Verification.Type];
            }

        }

       


        private ExpModule parseFormToExp()
        {
            ExpModule exp = new ExpModule();
            exp.Name = ExpNameTextBox.Text;
            exp.Language = LanguafeComboBox.Text;
            exp.Status = 1;
            if (ExpStatusComboBox.Text.Equals("禁用"))
            {
                exp.Status = 0;
            }
            exp.FormatUrl = false;
            if (FormatUrlComboBox.Text.Equals("是")) {
                exp.FormatUrl = true;
            }
            ExpVerification verification = new ExpVerification();
            verification.Context = VerificationValueTextBox.Text;
            foreach (Int32 key in MainForm.verificationTypes.Keys) {
                if (MainForm.verificationTypes[key].Equals(VerificationComboBox.Text))
                {
                    verification.Type = key;
                }
            }
            exp.Verification = verification;
            exp.Verification.Calc = CalcComboBox.Text;
            HttpModule expContext = new HttpModule();
            Dictionary<String, String> headers = new Dictionary<string, string>();
            for (int index = 0; index < HeaderListview.Items.Count;index++ ) {
                try {
                    headers.Add(HeaderListview.Items[index].SubItems[0].Text, HeaderListview.Items[index].SubItems[1].Text);
                }
                catch { }
                
            }
            expContext.Header = headers;
            expContext.Encode = EncodeComBox.Text;
            expContext.Body = BodyTextBox.Text;
            if (!String.IsNullOrEmpty(expContext.Body)) {
                if(ExpHandle.IsHexadecimal(expContext.Body)){
                    expContext.IsHex = true;
                }
            }
            expContext.Method = RequestMethodComboBox.Text;
            exp.ExpContext = expContext;
            return exp;
        }
      

        public void addOrModifyHeader(string fieldName, string fieldValue, Int32 modifyIndex)
        {
            if (modifyIndex > -1)
            {
                HeaderListview.Items[modifyIndex].SubItems[0].Text = fieldName;
                HeaderListview.Items[modifyIndex].SubItems[1].Text = fieldValue;
                return;
            }
            for (int i = 0; i < HeaderListview.Items.Count; i++)
            {
                if (HeaderListview.Items[i].SubItems[0].Text.Equals(fieldName))
                {
                    HeaderListview.Items[i].SubItems[1].Text = fieldValue;
                    return;
                }
            }
            ListViewItem lvi = new ListViewItem();
            lvi.SubItems[0].Text = fieldName;
            lvi.SubItems.Add(fieldValue);
            HeaderListview.Items.Add(lvi);
            return;
        }
        private void AddExpForm_Load(object sender, EventArgs e)
        {
            this.Icon = MainForm.icon;
            LanguafeComboBox.SelectedIndex = 0;
            ExpStatusComboBox.SelectedIndex = 0;
            VerificationComboBox.SelectedIndex = 0;
            EncodeComBox.SelectedIndex = 0;
            FormatUrlComboBox.SelectedIndex = 0;
            RequestMethodComboBox.SelectedIndex = 0;
            CalcComboBox.SelectedIndex = 0;
            initLvi();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExpModule exp = parseFormToExp();
            parentMain.modifyExpInfo(exp, index);
            this.Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AddHeaderForm addHeaderForm = new AddHeaderForm(this, null, null, -1);
            addHeaderForm.ShowDialog();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                String fieldName = HeaderListview.SelectedItems[0].SubItems[0].Text;
                String fieldValue = HeaderListview.SelectedItems[0].SubItems[1].Text;
                AddHeaderForm addHeaderForm = new AddHeaderForm(this, fieldName, fieldValue, HeaderListview.SelectedItems[0].Index);
                addHeaderForm.ShowDialog();
            }
            catch { }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                HeaderListview.Items.Remove(HeaderListview.SelectedItems[0]);
            }
            catch { }
        }

        private void 清空参数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HeaderListview.Items.Clear();
        }

        private void VerificationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalcComboBox.Enabled = true;
            if (!VerificationComboBox.Text.Equals("响应内容"))
            {
                CalcComboBox.Enabled = false;
            }
            
        }

        private void BodyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(BodyTextBox.Text)) {
                if ((RequestMethodComboBox.Text.Equals("GET") || RequestMethodComboBox.Text.Equals("HEAD"))) {
                    RequestMethodComboBox.Text = "POST";                    
                }
            }
        }
    }
}
