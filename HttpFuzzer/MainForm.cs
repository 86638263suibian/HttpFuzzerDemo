﻿using LoveCoody.entity;
using LoveCoody.handle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace LoveCoody
{
    public partial class MainForm : Form
    {

        public static Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe");

        public static Dictionary<Int32, String> verificationTypes = new Dictionary<int, string>() { { 0, "响应码" }, { 1, "响应内容" }, { 2, "新文件" } };

        public static object base_lock = new object();

        public MainForm()
        {

            InitializeComponent();
        }

        private void CoodyMain_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            loadConfig();
            initStyle();

        }

        private void initStyle()
        {
            ReadMeTextBox.BackColor = Color.White;
            if (EncodeComboBox.Text.Equals(""))
            {
                EncodeComboBox.SelectedIndex = 0;

            }
            if (ExpsComboBox.Text.Equals(""))
            {
                ExpsComboBox.SelectedIndex = 0;

            }
            if (TimeOutComboBox.Text.Equals(""))
            {
                TimeOutComboBox.SelectedIndex = 3;

            }
            if (ThreadNumComboBox.Text.Equals(""))
            {
                ThreadNumComboBox.SelectedIndex = 6;
                Int32 threadNum = Convert.ToInt32(ThreadNumComboBox.Text);
                ThreadPool.SetMaxThreads(threadNum, threadNum + 10);
                ThreadPool.SetMinThreads(threadNum, threadNum - 10);
            }
            if (ScannLanguafeComboBox.Text.Equals(""))
            {
                ScannLanguafeComboBox.SelectedIndex = 0;

            }
            this.Icon = icon;
        }

        private void ThreadNumComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Int32 threadNum = Convert.ToInt32(ThreadNumComboBox.Text);
            ThreadPool.SetMaxThreads(threadNum, threadNum + 10);
            ThreadPool.SetMinThreads(threadNum, threadNum - 10);
            saveConfig();
        }

        private void CoodyMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            DialogResult r = MessageBox.Show("Really want to exit?", "Remind", MessageBoxButtons.YesNo);
            if (r == DialogResult.Yes)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private void 添加数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddExpForm addExpForm = new AddExpForm(this);
            addExpForm.ShowDialog();
        }

        private void 修改数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {


            try
            {
                AddExpForm addExpForm = new AddExpForm(ExpListView.SelectedItems[0].Index, ExpListView.SelectedItems[0], this);
                addExpForm.ShowDialog();
            }
            catch { }
        }

        public void modifyExpInfo(ExpModule exp, Int32 index)
        {
            try
            {
                String configJson = JsonHandle.toJson(exp);
                if (index == -1)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems[0].Text = Convert.ToString(ExpListView.Items.Count + 1);
                    lvi.SubItems.Add(exp.Name);
                    lvi.SubItems.Add(exp.Language);
                    lvi.SubItems.Add(configJson);
                    lvi.SubItems.Add(verificationTypes[exp.Verification.Type] + ":" + exp.Verification.Context);
                    lvi.SubItems.Add(exp.Status == 0 ? "禁用" : "启用");
                    for (int i = 0; i < ExpListView.Items.Count; i++)
                    {
                        if (ExpListView.Items[i].SubItems[1].Text.Equals(exp.Name))
                        {
                            ExpListView.Items[i] = lvi;
                            ExpListView.Update();
                            return;
                        }
                    }
                    ExpListView.Items.Add(lvi);
                    return;
                }
                ExpListView.Items[index].SubItems[0].Text = Convert.ToString(ExpListView.Items.Count + 1);
                ExpListView.Items[index].SubItems[1].Text = exp.Name;
                ExpListView.Items[index].SubItems[2].Text = exp.Language;
                ExpListView.Items[index].SubItems[3].Text = configJson;
                ExpListView.Items[index].SubItems[4].Text = (verificationTypes[exp.Verification.Type] + ":" + exp.Verification.Context);
                ExpListView.Items[index].SubItems[5].Text = (exp.Status == 0 ? "禁用" : "启用");
                return;
            }
            catch { }
            finally
            {
                resetListViewIndex(ExpListView);
                saveConfig();
            }

        }
        private void resetListViewIndex(ListView listView)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {

                if (Convert.ToInt32(listView.Items[i].SubItems[0].Text) == i + 1)
                {
                    continue;
                }
                listView.Items[i].SubItems[0].Text = Convert.ToString(i + 1);
            }
        }

        public void addOrModifyHeader(string fieldName, string fieldValue, Int32 modifyIndex)
        {
            try
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
            catch { }
            finally
            {
                saveConfig();
            }

        }
        private void loadConfig()
        {

            try
            {
                StreamReader sr = new StreamReader(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".conf", Encoding.Default);
                String line;
                StringBuilder configContext = new StringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    configContext.AppendLine(line);
                }
                sr.Close();
                CoodyConfig config = (CoodyConfig)JsonHandle.toBean<CoodyConfig>(configContext.ToString());
                ThreadNumComboBox.Text = config.ThreadNum;
                TimeOutComboBox.Text = config.TimeOut;
                if (config.ExpListViews != null)
                {
                    foreach (String[] lines in config.ExpListViews)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.SubItems[0].Text = Convert.ToString(ExpListView.Items.Count + 1);
                        lvi.SubItems.Add(lines[1]);
                        lvi.SubItems.Add(lines[2]);
                        lvi.SubItems.Add(lines[3]);
                        lvi.SubItems.Add(lines[4]);
                        lvi.SubItems.Add(lines[5]);
                        ExpListView.Items.Add(lvi);
                    }
                }
                if (config.HeaderListviews != null)
                {
                    foreach (String[] lines in config.HeaderListviews)
                    {
                        addOrModifyHeader(lines[0], lines[1], -1);
                    }
                }
            }
            catch { }
            finally
            {
                if (ExpListView.Items.Count == 0)
                {
                    String jsons = HttpFuzzer.Properties.Resources.Struts2_exp;
                    String[] lines = jsons.Split(Environment.NewLine.ToCharArray());
                    foreach (String line in lines)
                    {
                        ExpModule exp = (ExpModule)JsonHandle.toBean<ExpModule>(line);
                        modifyExpInfo(exp, -1);
                    }

                }
            }
        }
        private void saveConfig()
        {
            CoodyConfig config = new CoodyConfig();
            config.ThreadNum = ThreadNumComboBox.Text;
            config.TimeOut = TimeOutComboBox.Text;
            List<String[]> headerListviews = new List<string[]>();
            for (int i = 0; i < HeaderListview.Items.Count; i++)
            {
                String[] lines = new String[2];
                lines[0] = HeaderListview.Items[i].SubItems[0].Text;
                lines[1] = HeaderListview.Items[i].SubItems[1].Text;
                headerListviews.Add(lines);
            }
            config.HeaderListviews = headerListviews;
            List<String[]> expListViews = new List<string[]>();
            for (int i = 0; i < ExpListView.Items.Count; i++)
            {
                String[] lines = new String[6];
                lines[0] = ExpListView.Items[i].SubItems[0].Text;
                lines[1] = ExpListView.Items[i].SubItems[1].Text;
                lines[2] = ExpListView.Items[i].SubItems[2].Text;
                lines[3] = ExpListView.Items[i].SubItems[3].Text;
                lines[4] = ExpListView.Items[i].SubItems[4].Text;
                lines[5] = ExpListView.Items[i].SubItems[5].Text;
                expListViews.Add(lines);
            }
            config.ExpListViews = expListViews;
            String json = JsonHandle.toJson(config);
            FileStream fs = new FileStream(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".conf", FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }

        private void TimeOutComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpHandle.timeOut = Convert.ToInt32(TimeOutComboBox.Text);
        }

        private void CommEncodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            saveConfig();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AddHeaderForm addHeaderForm = new AddHeaderForm(this, null, null, -1);
            addHeaderForm.ShowDialog();
            saveConfig();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                String fieldName = HeaderListview.SelectedItems[0].SubItems[0].Text;
                String fieldValue = HeaderListview.SelectedItems[0].SubItems[1].Text;
                AddHeaderForm addHeaderForm = new AddHeaderForm(this, fieldName, fieldValue, HeaderListview.SelectedItems[0].Index);
                addHeaderForm.ShowDialog();
                saveConfig();
            }
            catch { }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                HeaderListview.Items.Remove(HeaderListview.SelectedItems[0]);
                saveConfig();
            }
            catch { }
        }

        private void 清空参数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HeaderListview.Items.Clear();
            saveConfig();
        }

        private void UrlCheckButton_Click(object sender, EventArgs e)
        {
            reloadExpress();
            Thread urlCheckThread = new Thread(checkExps);
            urlCheckThread.Start();
        }
        private void checkExps()
        {
            UrlCheckButton.Enabled = false;
            HttpHandle.deathHost.Clear();
            ExpListView.Enabled = false; HeaderListview.Enabled = false;
            TimeOutComboBox.Enabled = false;
            ThreadNumComboBox.Enabled = false;
            for (int i = 0; i < ScannerExpListView.Items.Count; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(checkExpForSign), i);
            }

            while (true)
            {
                Thread.Sleep(200);
                int workerThreads = 0;
                int maxWordThreads = 0;
                //int   
                int compleThreads = 0;
                ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                if (workerThreads == maxWordThreads)
                {
                    break;
                }
            }
            MessageBox.Show("检测完成!");
            UrlCheckButton.Enabled = true;
            ExpListView.Enabled = true; HeaderListview.Enabled = true;
            TimeOutComboBox.Enabled = true;
            ThreadNumComboBox.Enabled = true;
        }

        private void checkExpForSign(object indexObj)
        {
            int index = (int)indexObj;
            String url = UrlTextBox.Text;
            String expJson = ScannerExpListView.Items[index].SubItems[5].Text;
            ExpModule exp = (ExpModule)JsonHandle.toBean<ExpModule>(expJson);
            ScannerExpListView.Items[index].SubItems[3].Text = "检测中";
            try
            {
                ExpVerificationResult result = ExpHandle.Verification(url, exp);
                ScannerExpListView.Items[index].SubItems[6].Text = result.Html;
                if (!String.IsNullOrEmpty(result.Html))
                {
                    ResultTextBox.Text = result.Html;
                }
                if (result.Code == 0)
                {
                    ScannerExpListView.Items[index].SubItems[4].Text = result.Result;
                    ScannerExpListView.Items[index].ForeColor = Color.Red;

                    return;
                }
                if (result.Code == 1)
                {
                    ScannerExpListView.Items[index].SubItems[4].Text = result.Result;
                    ScannerExpListView.Items[index].ForeColor = Color.Green;
                }
                if (result.Code == 2)
                {
                    ScannerExpListView.Items[index].SubItems[4].Text = "连接失败";
                    ScannerExpListView.Items[index].ForeColor = Color.Red;
                }
            }
            catch { }
            finally
            {
                ScannerExpListView.Items[index].SubItems[3].Text = "检测完成";
            }

        }

        private void ExpsComboBox_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void ScannLanguafeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExpsComboBox.Items.Clear();
            ExpsComboBox.Items.Add("全部");
            ExpsComboBox.SelectedIndex = 0;
            if (ScannLanguafeComboBox.Text.Equals("全部"))
            {
                for (int i = 0; i < ExpListView.Items.Count; i++)
                {
                    ExpsComboBox.Items.Add(ExpListView.Items[i].SubItems[0].Text + "-" + ExpListView.Items[i].SubItems[1].Text);
                }
                return;
            }
            for (int i = 0; i < ExpListView.Items.Count; i++)
            {
                if (ExpListView.Items[i].SubItems[2].Text.Equals("全部") || ExpListView.Items[i].SubItems[2].Text.Equals(ScannLanguafeComboBox.Text))
                {
                    ExpsComboBox.Items.Add(ExpListView.Items[i].SubItems[0].Text + "-" + ExpListView.Items[i].SubItems[1].Text);
                }
            }
        }

        private void ExpsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void reloadExpress()
        {
            ScannerExpListView.Items.Clear();
            String[] tabs = Regex.Split(ExpsComboBox.Text, "-");
            String id = tabs[0];
            for (int m = 0; m < ExpListView.Items.Count; m++)
            {
                String expId = ExpListView.Items[m].SubItems[0].Text;
                if (id.Equals(expId) && ExpListView.Items[m].SubItems[5].Text.Equals("启用"))
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems[0].Text = expId;
                    lvi.SubItems.Add(ExpListView.Items[m].SubItems[1].Text);
                    lvi.SubItems.Add(ExpListView.Items[m].SubItems[2].Text);
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(ExpListView.Items[m].SubItems[3].Text);
                    lvi.SubItems.Add("");
                    ScannerExpListView.Items.Add(lvi);
                }
            }
            if (ExpsComboBox.Text.Equals("全部"))
            {
                Dictionary<String, ListViewItem> itemMap = new Dictionary<string, ListViewItem>();
                for (int i = 0; i < ExpsComboBox.Items.Count; i++)
                {
                    for (int m = 0; m < ExpListView.Items.Count; m++)
                    {
                        String language = ExpListView.Items[m].SubItems[2].Text;
                        if (!ScannLanguafeComboBox.Text.Equals("全部") && !language.Equals("全部"))
                        {
                            if (!ScannLanguafeComboBox.Text.Equals(language))
                            {
                                continue;
                            }
                        }
                        if (ExpListView.Items[m].SubItems[5].Text.Equals("启用"))
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems[0].Text = ExpListView.Items[m].SubItems[0].Text;
                            lvi.SubItems.Add(ExpListView.Items[m].SubItems[1].Text);
                            lvi.SubItems.Add(ExpListView.Items[m].SubItems[2].Text);
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add(ExpListView.Items[m].SubItems[3].Text);
                            lvi.SubItems.Add("");
                            try
                            {
                                itemMap.Add(ExpListView.Items[m].SubItems[0].Text, lvi);
                            }
                            catch { }
                        }
                    }
                }
                foreach (String key in itemMap.Keys)
                {
                    ScannerExpListView.Items.Add(itemMap[key]);
                } return;
            }

        }

        private void ScannerExpListView_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                String html = ScannerExpListView.SelectedItems[0].SubItems[6].Text;
                ResultTextBox.Text = html;
            }
            catch { }
        }

        private void 导入资源ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileName != "")
                {
                    StreamReader sr = new StreamReader(ofd.FileName);
                    List<ListViewItem> items = new List<ListViewItem>();
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        if (line.Equals(""))
                        {
                            continue;
                        }

                        String url = line.Trim();
                        ListViewItem lvi = new ListViewItem();
                        lvi.SubItems[0].Text = Convert.ToString(BatchCheckListView.Items.Count + 1);
                        lvi.SubItems.Add(url);
                        lvi.SubItems.Add("暂无");
                        lvi.SubItems.Add("待检测");
                        lvi.SubItems.Add("0");
                        items.Add(lvi);
                        if (items.Count > 100)
                        {
                            BatchCheckListView.Items.AddRange(items.ToArray());
                            items.Clear();
                        }
                    }
                    if (items.Count > 0)
                    {
                        BatchCheckListView.Items.AddRange(items.ToArray());
                    }
                    sr.Close();
                    resetListViewIndex(BatchCheckListView);
                }
            }
        }

        private void 导出内容ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".txt";
            if (sfd.FileName != "" && sfd.ShowDialog() == DialogResult.OK)
            {
                sfd.RestoreDirectory = true;
                sfd.CreatePrompt = true;
                StreamWriter sw = File.CreateText(sfd.FileName);
                for (int i = 0; i < BatchCheckListView.Items.Count; i++)
                {
                    try
                    {
                        String line = BatchCheckListView.Items[i].SubItems[1].Text;

                        if (String.IsNullOrEmpty(BatchCheckListView.Items[i].SubItems[2].Text))
                        {
                            line += ("|" + "null");
                        }
                        else
                        {
                            line += ("|" + BatchCheckListView.Items[i].SubItems[2].Text);
                        }
                        if (String.IsNullOrEmpty(BatchCheckListView.Items[i].SubItems[3].Text))
                        {
                            line += ("|" + "null");
                        }
                        else
                        {
                            line += ("|" + BatchCheckListView.Items[i].SubItems[3].Text);
                        }
                        sw.WriteLine(line);
                    }
                    catch { }
                }
                sw.Close();
                MessageBox.Show("Save Success!");
            }
        }

        private void 复制网址ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(BatchCheckListView.SelectedItems[0].SubItems[1].Text);
            }
            catch { }
        }

        private void ExpListView_DoubleClick(object sender, EventArgs e)
        {

        }

        private void CoodyTabControl_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(BatchCheckListView.SelectedItems[0].SubItems[1].Text);
            }
            catch { }
        }

        private void 删除本行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                BatchCheckListView.Items.Remove(BatchCheckListView.SelectedItems[0]);
                resetListViewIndex(BatchCheckListView);
            }
            catch { }
        }

        private void 全部删除ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show("确定要全部删除?", "Remind", MessageBoxButtons.YesNo);
            if (r == DialogResult.Yes)
            {
                BatchCheckListView.Items.Clear();
            }

        }

        private void 清理失败ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = BatchCheckListView.Items.Count - 1; i > -1; i--)
            {
                if (BatchCheckListView.Items[i].ForeColor != Color.Green)
                {
                    continue;
                }
                items.Add(BatchCheckListView.Items[i]);
            }
            BatchCheckListView.Items.Clear();
            BatchCheckListView.Items.AddRange(items.ToArray());
            resetListViewIndex(BatchCheckListView);
        }
        Dictionary<Int32, ExpModule> expDics = new Dictionary<int, ExpModule>();
        private ExpModule getExpByIndex(Int32 expIndex)
        {
            if (expDics.ContainsKey(expIndex))
            {
                return expDics[expIndex];
            }
            String expJson = ExpListView.Items[expIndex].SubItems[3].Text;
            ExpModule exp = (ExpModule)JsonHandle.toBean<ExpModule>(expJson);
            try { expDics.Add(expIndex, exp); }
            catch { }

            return exp;
        }
        private void 批量检测ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BatchCheckListView.Items.Count < 1)
            {
                return;
            }
            initUrlMapping();
            Thread batchPoolThread = new Thread(batchPool);
            batchPoolThread.Start();
        }

        private void initUrlMapping()
        {
            BatchStatusStrip.Enabled = false;
            try
            {
                urlMappings.Clear();
                expDics.Clear();
                checkNum.Clear();
                int remain = BatchCheckListView.Items.Count;
                for (int i = 0; i < remain; i++)
                {
                    try { checkNum.Add(i, 0); }
                    catch { }

                }
                for (int i = 0; i < BatchCheckListView.Items.Count; i++)
                {
                    if (BatchCheckListView.Items[i].ForeColor != Color.Black)
                    {
                        BatchCheckListView.Items[i].ForeColor = Color.Black;
                    }
                }
                List<Int32> ranges = new List<int>();
                Int32 threadNum = Convert.ToInt32(ThreadNumComboBox.Text);
                while (remain > 0)
                {
                    if (remain > threadNum)
                    {
                        remain = remain - threadNum;
                        ranges.Add(threadNum);
                        continue;
                    }
                    ranges.Add(remain);
                    remain = remain - remain;
                }
                int index = 0;
                foreach (Int32 range in ranges)
                {
                    for (int j = 0; j < ExpListView.Items.Count; j++)
                    {
                        for (int i = 0; i < range; i++)
                        {
                            try
                            {
                                if (ExpListView.Items[j].SubItems[5].Text.Equals("启用"))
                                {
                                    BatchExps batch = new BatchExps();
                                    batch.ExpIndex = j;
                                    batch.Index = index + i;
                                    urlMappings.Enqueue(batch);
                                }
                            }
                            catch { }
                        }
                    }
                    index += range;
                }
                expDics.Clear();
                for (int i = 0; i < ExpListView.Items.Count; i++)
                {
                    getExpByIndex(i);
                }
            }
            catch { }
            finally
            {
                BatchStatusStrip.Enabled = true;
            }

        }

        int successProsion = 0;
        Queue<BatchExps> urlMappings = new Queue<BatchExps>();
        private void batchPool()
        {
            if (urlMappings.Count == 0)
            {
                return;
            }
            successProsion = 0;
            HttpHandle.deathHost.Clear();
            expLength = getAvaExpSizi();
            TotalCheckToolStripStatusLabel.Text = "检测总数：" + Convert.ToString(urlMappings.Count);
            BatchCheckMenuStrip.Enabled = false;
            BatchProgressBar.Value = 0;
            int total = urlMappings.Count;
            for (int i = 0; i < Convert.ToInt32(ThreadNumComboBox.Text); i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(checkAExp), i);
            }
            long whileNumber = 0;
            while (true)
            {
                whileNumber++;
                Thread.Sleep(100);
                int workerThreads = 0;
                int maxWordThreads = 0;
                //int   
                int compleThreads = 0;
                ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);

                //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                BatchProgressBar.Value = (int)(((total - urlMappings.Count) * 100 / total));
                if (whileNumber % 10 == 0)
                {
                    RemainThreadToolStripStatusLabel.Text = "剩余线程：" + Convert.ToString(maxWordThreads - workerThreads);
                    RemainToolStripStatusLabel.Text = "剩余数量：" + Convert.ToString(urlMappings.Count);
                    SuccessToolStripStatusLabel.Text = "成功数量：" + Convert.ToString(successProsion);
                }
                if (workerThreads == maxWordThreads)
                {
                    RemainThreadToolStripStatusLabel.Text = "剩余线程：0";
                    RemainToolStripStatusLabel.Text = "剩余数量：0";
                    break;
                }
            }

            MessageBox.Show("执行完毕");
            BatchCheckMenuStrip.Enabled = true;
        }

        private void checkAExp(Object index)
        {
            while (urlMappings.Count > 0)
            {
                BatchExps batchExp = null;
                lock (base_lock)
                {
                    batchExp = urlMappings.Dequeue();
                }
                if (batchExp == null)
                {
                    continue;
                }
                try
                {
                    String url = "";
                    if (BatchCheckListView.Items[batchExp.Index].ForeColor == Color.Green)
                    {
                        continue;
                    }
                    ExpModule exp = getExpByIndex(batchExp.ExpIndex);
                    url = BatchCheckListView.Items[batchExp.Index].SubItems[1].Text;
                    BatchCheckListView.Items[batchExp.Index].SubItems[3].Text = "检测" + exp.Name;
                    ExpVerificationResult result = ExpHandle.Verification(url, exp);
                    result.Index = batchExp.Index;
                    if (result.Code == 1)
                    {
                        BatchCheckListView.Items[result.Index].SubItems[3].Text = result.Result;
                        BatchCheckListView.Items[result.Index].SubItems[2].Text = result.ExpName;
                        BatchCheckListView.Items[result.Index].ForeColor = Color.Green;
                        successProsion++;
                    }
                    if (result.Code == 2)
                    {
                        continue;
                    }

                }
                catch { }
                finally
                {
                    try
                    {
                        lock (base_lock)
                        {
                            checkNum[batchExp.Index]++;
                        }
                        if (BatchCheckListView.Items[batchExp.Index].ForeColor != Color.Green)
                        {
                            if (checkNum[batchExp.Index] >= expLength)
                            {
                                BatchCheckListView.Items[batchExp.Index].ForeColor = Color.DarkGray;
                                BatchCheckListView.Items[batchExp.Index].SubItems[3].Text = "不存在漏洞";
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(1);
                }
            }
        }


        private Dictionary<Int32, Int32> checkNum = new Dictionary<int, int>();
        private static Int32 expLength = 0;
        private Int32 getAvaExpSizi()
        {
            Int32 length = 0;
            for (int i = 0; i < ExpListView.Items.Count; i++)
            {
                if (ExpListView.Items[i].SubItems[5].Text.Equals("启用"))
                {
                    length++;
                }
            }
            return length;
        }
        private void 清空数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpListView.Items.Clear();
        }

        private void 导入数据ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileName != "")
                {
                    StreamReader sr = new StreamReader(ofd.FileName);
                    StringBuilder sb = new StringBuilder("");
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        if (line.Equals(""))
                        {
                            continue;
                        }
                        try
                        {
                            ExpModule exp = (ExpModule)JsonHandle.toBean<ExpModule>(line);
                            modifyExpInfo(exp, -1);
                        }
                        catch { }
                    }
                    sr.Close();
                }
            }
        }

        private void 禁用本条ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                ExpListView.SelectedItems[0].SubItems[5].Text = "禁用";
            }
            catch { }
        }

        private void 启用本条ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ExpListView.SelectedItems[0].SubItems[5].Text = "启用";
            }
            catch { }

        }

        private void 删除本行ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                ExpListView.Items.Remove(ExpListView.SelectedItems[0]);
                resetListViewIndex(ExpListView);
            }
            catch { }
        }

        private void 导出数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + "_exp.txt";
            if (sfd.FileName != "" && sfd.ShowDialog() == DialogResult.OK)
            {
                sfd.RestoreDirectory = true;
                sfd.CreatePrompt = true;
                StreamWriter sw = File.CreateText(sfd.FileName);
                for (int i = 0; i < ExpListView.Items.Count; i++)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(ExpListView.Items[i].SubItems[3].Text))
                        {
                            continue;
                        }
                        sw.WriteLine(ExpListView.Items[i].SubItems[3].Text);
                    }
                    catch { }
                }
                sw.Close();
                MessageBox.Show("Save Success!");
            }
        }

        private void BatchCheckListView_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(BatchCheckListView.SelectedItems[0].SubItems[1].Text);
            }
            catch { }
        }

    }

}
