using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace XsdDocumentation.PlugIn
{
    internal class HelpAwareForm : Form
    {
        public HelpAwareForm()
        {
            HelpButtonClicked += HelpAwareForm_HelpButtonClicked;
            HelpRequested += HelpAwareForm_HelpRequested;
        }

        public string HelpKeyword { get; set; }

        private void ShowHelp()
        {
            Help.ShowHelp(this, XsdPlugIn.GetHelpFilePath(), HelpKeyword);
        }

        private void HelpAwareForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            ShowHelp();
            e.Cancel = true;
        }

        private void HelpAwareForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            ShowHelp();
        }
    }
}