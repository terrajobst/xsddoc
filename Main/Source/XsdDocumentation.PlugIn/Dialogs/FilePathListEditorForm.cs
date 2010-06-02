using System;
using System.Linq;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace XsdDocumentation.PlugIn
{
    internal sealed partial class FilePathListEditorForm : HelpAwareForm
    {
        public FilePathListEditorForm()
        {
            InitializeComponent();
        }

        public IBasePathProvider BasePathProvider { get; set; }
        public FilePathCollection FilePathsList { get; set; }

        public string Filter
        {
            get { return _openFileDialog.Filter; }
            set { _openFileDialog.Filter = value; }
        }

        private void FilePathListEditor_Load(object sender, EventArgs e)
        {
            foreach (var path in FilePathsList)
            {
                var clonedPath = (FilePath)path.Clone();
                clonedPath.PersistablePathChanged += FilePath_OnPersistablePathChanged;
                _filesListBox.Items.Add(clonedPath);
            }

            if (_filesListBox.Items.Count > 0)
                _filesListBox.SelectedIndex = 0;
        }

        private void FilePath_OnPersistablePathChanged(object sender, EventArgs e)
        {
            _filesListBox.BeginUpdate();
            try
            {
                var temp = new object[_filesListBox.Items.Count];
                var selectedIndex = _filesListBox.SelectedIndex;
                _filesListBox.Items.CopyTo(temp, 0);
                _filesListBox.Items.Clear();
                _filesListBox.Items.AddRange(temp);
                _filesListBox.SelectedIndex = selectedIndex;
            }
            finally
            {
                _filesListBox.EndUpdate();
            }
        }

        private void FilePathListEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
                return;

            FilePathsList.Clear();
            foreach (FilePath filePath in _filesListBox.Items)
            {
                filePath.PersistablePathChanged -= FilePath_OnPersistablePathChanged;
                FilePathsList.Add(filePath);
            }
        }

        private void filesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _propertyGrid.SelectedObject = _filesListBox.SelectedItem;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (_openFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            foreach (var fileName in _openFileDialog.FileNames)
            {
                var filePath = new FilePath(fileName, BasePathProvider);
                _filesListBox.Items.Add(filePath);
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            var indexes = (from int idx in _filesListBox.SelectedIndices
                           orderby idx descending
                           select idx).ToList();

            foreach (var i in indexes)
                _filesListBox.Items.RemoveAt(i);
        }
    }
}