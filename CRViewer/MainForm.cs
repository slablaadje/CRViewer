using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CRViewer
{
    public partial class MainForm : Form
    {
        public string Path { get; set; }
        public List<string> Files { get; set; }

        public int CurrentIndex { get; set; }
        public Image CurrentImage { get; set; }
        public Orientation CurrentOrientation { get; private set; }

        public static NotifyIcon NotifyIcon = new NotifyIcon();
        System.Threading.Timer Timer;

        public MainForm()
        {
            InitializeComponent();
        }

        public void SetPath(string path)
        {
            try
            {
                if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    path = PathHelper.GetParentPath(path);
                }
                    
                if (Directory.Exists(path))
                {
                    Path = path;
                    LoadDirectory();
                }
            }
            catch(Exception)
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateImage()
        {
            var oldImage = CurrentImage;

            if (Files == null || !Files.Any())
            {
                CurrentImage = null;
            }
            else if (!File.Exists(Files[CurrentIndex]))
            {
                ShowBalloonTip("CRViewer", "Could not find file " + Files[CurrentIndex] + "; reloading directory", ToolTipIcon.Info);
                LoadDirectoryAndResetIndex();
            }
            else
            {
                var fileName = Files[CurrentIndex];
                try
                {
                    using (var Reader = ReaderFactory.GetReader(fileName))
                    {
                        CurrentImage = Reader.GetImage();

                        if (CurrentImage == null)
                            ShowBalloonTip("CRViewer", "Could not load file " + fileName, ToolTipIcon.Warning);

                        CurrentOrientation = Reader.Orientation();
                    }
                }
                catch (Exception)
                {
                    ShowBalloonTip("CRViewer", "Could not extract jpeg from " + Files[CurrentIndex], ToolTipIcon.Info);
                }
            }

            DrawImage(true);
        }

        private void DrawImage(bool invalidate = false)
        {
            if (CurrentImage != null)
            {
                // https://magnushoff.com/jpeg-orientation.html
                using (var g = this.CreateGraphics())
                {
                    Matrix matrix = new Matrix();
                    Point offset = new Point(0, 40);
                    if (CurrentOrientation != null)
                    {
                        matrix = CurrentOrientation.Matrix;
                        offset = CurrentOrientation.CalculateOffset(offset, CurrentImage.Width, CurrentImage.Height);
                    }
                    if (invalidate)
                        this.Invalidate();

                    g.MultiplyTransform(matrix);
                    g.DrawImage(CurrentImage, offset);

                    g.ResetTransform();
                }
            }
        }

        internal void LoadDirectory()
        {
            LoadDirectory(i => i);
        }

        internal void LoadDirectoryAndResetIndex()
        {
            LoadDirectory(i => 0);
        }

        internal void LoadDirectory(Func<int, int> indexFunc)
        {
            try
            {
                Files = Directory.GetFiles(Path, "*.cr*")
                    .Where(f => ReaderFactory.SupportedExtensions.Contains(System.IO.Path.GetExtension(f).ToLower())).ToList();

                if (!Files.Any())
                {
                    ShowBalloonTip("CRViewer", "Path " + Path + " contains no files that are supported. Supported extensions: " + string.Join(", ", ReaderFactory.SupportedExtensions), ToolTipIcon.Info);
                }
            }
            catch (Exception)
            {
                Files = new List<string>();
                ShowBalloonTip("CRViewer", "Could not get files from path " + Path, ToolTipIcon.Info);
            }
            finally
            {
                CurrentIndex = indexFunc(CurrentIndex);
                    
                if (CurrentIndex >= Files.Count)
                {
                    CurrentIndex = Files.Count - 1;
                }

                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            lblImageIndex.Text = "Index: " + (Files != null && Files.Any() ? (CurrentIndex+1) + "/" + Files.Count : "-/-");
            lblImage.Text = "Filename: " + (Files != null && Files.Any() ? Files[CurrentIndex] : "-");
            lblLocation.Text = "Path: " + Path;
            UpdateImage();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.O)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    Path = dialog.SelectedPath;
                    LoadDirectoryAndResetIndex();
                }
            }

            if (e.Modifiers.HasFlag(Keys.Alt))
            {
                if (e.KeyCode == Keys.Up)
                {
                    Path = PathHelper.GetParentPath(Path);
                    LoadDirectoryAndResetIndex();
                }

                if (e.KeyCode == Keys.Down)
                {
                    var dirs = Directory.GetDirectories(Path);
                    if (dirs.Any())
                    {
                        Path = System.IO.Path.Combine(Path, dirs.First());
                        LoadDirectoryAndResetIndex();
                    }
                    else
                    {
                        ShowBalloonTip("CRViewer", Path + " contains no subdirectories", ToolTipIcon.Info);
                    }
                }

                if (e.KeyCode == Keys.Left)
                {
                    var currentDir = new DirectoryInfo(Path).Name;
                    var dirs = Directory.GetDirectories(PathHelper.GetParentPath(Path)).ToList();
                    var nextDir = dirs?.OrderByDescending(d => d).FirstOrDefault(d => new DirectoryInfo(d).Name.CompareTo(currentDir) == -1);

                    if (nextDir != null)
                    {
                        Path = nextDir;
                        LoadDirectoryAndResetIndex();
                    }
                    else
                    {
                        ShowBalloonTip("CRViewer", currentDir + " is the last directory within " + PathHelper.GetParentPath(Path), ToolTipIcon.Info);
                    }
                }

                if (e.KeyCode == Keys.Right)
                {
                    var currentDir = new DirectoryInfo(Path).Name;
                    var dirs = Directory.GetDirectories(PathHelper.GetParentPath(Path)).ToList();
                    var nextDir = dirs?.FirstOrDefault(d => new DirectoryInfo(d).Name.CompareTo(currentDir) == 1);

                    if (nextDir != null)
                    {
                        Path = nextDir;
                        LoadDirectoryAndResetIndex();
                    }
                    else
                    {
                        ShowBalloonTip("CRViewer", currentDir + " is the first directory within " + PathHelper.GetParentPath(Path), ToolTipIcon.Info);
                    }
                }
            }

            if (e.KeyCode == Keys.F5)
            {
                LoadDirectory();
            }

            if (e.KeyData == Keys.Space)
            {
                CurrentIndex++;

                if (CurrentIndex == Files.Count)
                {
                    CurrentIndex = 0;
                }
                UpdateUI();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Back)
            {
                CurrentIndex--;

                if (CurrentIndex < 0)
                {
                    CurrentIndex = Files.Count - 1;
                }
                UpdateUI();
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == Keys.Y) MoveTo("_Yes", e);
            if (e.KeyData == Keys.N) MoveTo("_No", e);
            if (e.KeyData == Keys.M) MoveTo("_Maybe", e);

            if (e.KeyCode == Keys.Delete)
            {
                if (!e.Modifiers.HasFlag(Keys.Shift))
                {
                    var result = MessageBox.Show("Are you sure you want to permanently delete this file?", "Delete File", MessageBoxButtons.OKCancel);
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }

                File.Delete(Files[CurrentIndex]);
                LoadDirectory();
            }
        }

        public void MoveTo(string directory, KeyEventArgs e)
        {
            var filepath = Files[CurrentIndex];
            var filename = System.IO.Path.GetFileName(filepath);

            var path = System.IO.Path.Combine(Path, directory);
            Directory.CreateDirectory(path);
            var newFilepath = System.IO.Path.Combine(path, filename);
            File.Move(filepath, newFilepath);

            e.SuppressKeyPress = true;

            LoadDirectory();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawImage();
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Keyboard shortcuts:\r\n" +
                    "\r\n" +
                    "O\t\tOpen directory\r\n" +
                    "space\t\tGo to next file in directory\r\n" +
                    "backspace\tGo to previous file in directory\r\n" +
                    "F5\t\tReload directory\r\n" +
                    "\r\n" +
                    "alt+up\t\tNavigate to parent directory\r\n" +
                    "alt+down\t\tNavigate to first child directory\r\n" +
                    "alt+right\t\tNavigate to next directory within parent\r\n" +
                    "alt+left\t\tNavigate to previous directory within parent\r\n" +
                    "\r\n" +
                    "delete\t\tDelete file\r\n" +
                    "shift+delete\tDelete file without confirmation\r\n" +
                    "\r\n" +
                    "Y\t\tMove file to subdirectory _Yes\r\n" +
                    "N\t\tMove file to subdirectory _No\r\n" +
                    "M\t\tMove file to subdirectory _Maybe",
                    "CR3 Viewer - Help"
                );
        }

        private void ShowBalloonTip(string tipTitle, string tipText, ToolTipIcon icon)
        {
            NotifyIcon.Icon = SystemIcons.Information;
            NotifyIcon.Visible = true;
            NotifyIcon.ShowBalloonTip(0, tipTitle, tipText, icon);
            Timer = new System.Threading.Timer(state => NotifyIcon.Visible = false, null, 7000, Timeout.Infinite);
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Path = System.IO.Path.GetDirectoryName(files.First());
                LoadDirectory(i => { int j = Files.IndexOf(files.First()); return j == -1 ? i : j; });
            }
            catch(Exception)
            {

            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
    }
}
