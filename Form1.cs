using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsToolkit
{
    public class Form1 : Form
    {
        // =====================================================
        // VERSION
        // =====================================================

        private const string CURRENT_VERSION = "2.3.7";

        // =====================================================
        // OTA UPDATE
        // =====================================================

        private const string UPDATE_INFO_URL =
            "https://raw.githubusercontent.com/linhngo1256/WindowsToolkit/main/version.json";

        private static readonly HttpClient httpClient =
            new HttpClient();

        // =====================================================
        // PATH
        // =====================================================

        private readonly string rootPath;
        private readonly string appToolPath;
        private readonly string configPath;

        // =====================================================
        // UI
        // =====================================================

        private readonly Panel contentPanel;
        private readonly MenuStrip mainMenu;

        private FlowLayoutPanel appPanel;
        private Label titleLabel;
        private Label emptyLabel;
        private Label footerLabel;
        private TextBox searchBox;

        private Form aboutForm;

        // =====================================================
        // STATE
        // =====================================================

        private readonly string buildDate;

        private AppConfig config;

        private readonly Dictionary<string, CheckBox>
            selectedApplications =
                new Dictionary<string, CheckBox>();

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public Form1()
        {
            rootPath =
                AppContext.BaseDirectory;

            appToolPath =
                Path.Combine(
                    rootPath,
                    "appTool"
                );

            configPath =
                Path.Combine(
                    rootPath,
                    "Config"
                );

            Directory.CreateDirectory(rootPath);
            Directory.CreateDirectory(appToolPath);
            Directory.CreateDirectory(configPath);

            config = LoadConfig();

            buildDate = GetBuildDate();

            Text =
                "WINDOWS TOOLKIT v" +
                CURRENT_VERSION;

            Width = 1100;
            Height = 700;

            MinimumSize =
                new Size(800, 500);

            StartPosition =
                FormStartPosition.CenterScreen;

            ApplyTheme();

            AllowDrop = true;

            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;

            mainMenu =
                new MenuStrip();

            mainMenu.Dock =
                DockStyle.Top;

            CreateMenu();

            contentPanel =
                new Panel();

            contentPanel.Dock =
                DockStyle.Fill;

            Controls.Add(contentPanel);
            Controls.Add(mainMenu);

            MainMenuStrip =
                mainMenu;

            ShowHomeScreen();
        }

        // =====================================================
        // BUILD DATE
        // =====================================================

        private string GetBuildDate()
        {
            try
            {
                return File
                    .GetLastWriteTime(
                        Application.ExecutablePath
                    )
                    .ToString(
                        "dd/MM/yyyy HH:mm"
                    );
            }
            catch
            {
                return DateTime.Now
                    .ToString(
                        "dd/MM/yyyy HH:mm"
                    );
            }
        }

        // =====================================================
        // CONFIG
        // =====================================================

        public class AppConfig
        {
            public bool DarkMode
            {
                get;
                set;
            }

            public List<string>
                FavoriteApplications
            {
                get;
                set;
            }

            public AppConfig()
            {
                FavoriteApplications =
                    new List<string>();
            }
        }

        // =====================================================
        // UPDATE INFO
        // =====================================================

        public class UpdateInfo
        {
            public string Version
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public string DownloadUrl
            {
                get;
                set;
            }

            public string Url
            {
                get;
                set;
            }

            public string Download
            {
                get;
                set;
            }

            public UpdateInfo()
            {
                Version = "";
                Description = "";
                DownloadUrl = "";
                Url = "";
                Download = "";
            }
        }

        // =====================================================
        // LOAD CONFIG
        // =====================================================

        private AppConfig LoadConfig()
        {
            string file =
                Path.Combine(
                    configPath,
                    "config.json"
                );

            try
            {
                if (!File.Exists(file))
                {
                    return new AppConfig();
                }

                string json =
                    File.ReadAllText(file);

                AppConfig result =
                    JsonSerializer
                    .Deserialize<AppConfig>(
                        json
                    );

                if (result == null)
                {
                    return new AppConfig();
                }

                if (
                    result.FavoriteApplications ==
                    null
                )
                {
                    result.FavoriteApplications =
                        new List<string>();
                }

                return result;
            }
            catch
            {
                return new AppConfig();
            }
        }

        // =====================================================
        // SAVE CONFIG
        // =====================================================

        private void SaveConfig()
        {
            try
            {
                string file =
                    Path.Combine(
                        configPath,
                        "config.json"
                    );

                string json =
                    JsonSerializer.Serialize(
                        config,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }
                    );

                File.WriteAllText(
                    file,
                    json
                );
            }
            catch
            {
            }
        }

        // =====================================================
        // MENU
        // =====================================================

        private void CreateMenu()
        {
            ToolStripMenuItem menu =
                new ToolStripMenuItem(
                    "MENU"
                );

            ToolStripMenuItem about =
                new ToolStripMenuItem(
                    "ABOUT"
                );

            menu.DropDownItems.Add(
                "THÊM ỨNG DỤNG",
                null,
                delegate
                {
                    AddApplication();
                }
            );

            menu.DropDownItems.Add(
                "XÓA ỨNG DỤNG ĐÃ CHỌN",
                null,
                delegate
                {
                    DeleteSelectedApplications();
                }
            );

            menu.DropDownItems.Add(
                new ToolStripSeparator()
            );

            menu.DropDownItems.Add(
                "MỞ THƯ MỤC ỨNG DỤNG",
                null,
                delegate
                {
                    OpenFolder(appToolPath);
                }
            );

            menu.DropDownItems.Add(
                new ToolStripSeparator()
            );

            ToolStripMenuItem darkModeItem =
                new ToolStripMenuItem(
                    "DARK MODE"
                );

            darkModeItem.CheckOnClick =
                true;

            darkModeItem.Checked =
                config.DarkMode;

            darkModeItem.CheckedChanged +=
                delegate
                {
                    config.DarkMode =
                        darkModeItem.Checked;

                    SaveConfig();

                    ApplyTheme();

                    ShowHomeScreen();
                };

            menu.DropDownItems.Add(
                darkModeItem
            );

            about.Click +=
                delegate
                {
                    ShowAbout();
                };

            mainMenu.Items.Add(menu);
            mainMenu.Items.Add(about);
        }

        // =====================================================
        // THEME
        // =====================================================

        private void ApplyTheme()
        {
            if (config.DarkMode)
            {
                BackColor =
                    Color.FromArgb(
                        32,
                        32,
                        32
                    );

                ForeColor =
                    Color.White;
            }
            else
            {
                BackColor =
                    Color.WhiteSmoke;

                ForeColor =
                    Color.Black;
            }
        }

        // =====================================================
        // HOME
        // =====================================================

        private void ShowHomeScreen()
        {
            contentPanel.Controls.Clear();

            selectedApplications.Clear();

            Color backgroundColor =
                config.DarkMode
                    ? Color.FromArgb(
                        32,
                        32,
                        32
                    )
                    : Color.WhiteSmoke;

            Color textColor =
                config.DarkMode
                    ? Color.White
                    : Color.Black;

            contentPanel.BackColor =
                backgroundColor;

            footerLabel =
                new Label();

            footerLabel.Text =
                "DEV: tronglinh1997";

            footerLabel.Dock =
                DockStyle.Bottom;

            footerLabel.Height =
                28;

            footerLabel.TextAlign =
                ContentAlignment.MiddleRight;

            footerLabel.Padding =
                new Padding(
                    0,
                    0,
                    15,
                    0
                );

            footerLabel.ForeColor =
                config.DarkMode
                    ? Color.LightGray
                    : Color.Gray;

            footerLabel.BackColor =
                backgroundColor;

            footerLabel.Font =
                new Font(
                    "Segoe UI",
                    8
                );

            titleLabel =
                new Label();

            titleLabel.Text =
                "DANH SÁCH ỨNG DỤNG";

            titleLabel.Dock =
                DockStyle.Top;

            titleLabel.Height =
                70;

            titleLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            titleLabel.Font =
                new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                );

            titleLabel.ForeColor =
                textColor;

            titleLabel.BackColor =
                backgroundColor;

            Panel toolbar =
                new Panel();

            toolbar.Dock =
                DockStyle.Top;

            toolbar.Height =
                60;

            toolbar.BackColor =
                backgroundColor;

            Button addButton =
                new Button();

            addButton.Text =
                "THÊM ỨNG DỤNG";

            addButton.Width =
                145;

            addButton.Height =
                38;

            addButton.Left =
                15;

            addButton.Top =
                8;

            Button deleteButton =
                new Button();

            deleteButton.Text =
                "XÓA ĐÃ CHỌN";

            deleteButton.Width =
                135;

            deleteButton.Height =
                38;

            deleteButton.Left =
                170;

            deleteButton.Top =
                8;

            Button refreshButton =
                new Button();

            refreshButton.Text =
                "LÀM MỚI";

            refreshButton.Width =
                105;

            refreshButton.Height =
                38;

            refreshButton.Left =
                315;

            refreshButton.Top =
                8;

            searchBox =
                new TextBox();

            searchBox.Width =
                260;

            searchBox.Left =
                440;

            searchBox.Top =
                12;

            searchBox.TextChanged +=
                delegate
                {
                    LoadApplications();
                };

            addButton.Click +=
                delegate
                {
                    AddApplication();
                };

            deleteButton.Click +=
                delegate
                {
                    DeleteSelectedApplications();
                };

            refreshButton.Click +=
                delegate
                {
                    LoadApplications();
                };

            toolbar.Controls.Add(addButton);
            toolbar.Controls.Add(deleteButton);
            toolbar.Controls.Add(refreshButton);
            toolbar.Controls.Add(searchBox);

            appPanel =
                new FlowLayoutPanel();

            appPanel.Dock =
                DockStyle.Fill;

            appPanel.AutoScroll =
                true;

            appPanel.Padding =
                new Padding(25);

            appPanel.BackColor =
                backgroundColor;

            emptyLabel =
                new Label();

            emptyLabel.Text =
                "CHƯA CÓ ỨNG DỤNG\r\n\r\n" +
                "Đưa file .EXE vào thư mục appTool\r\n" +
                "hoặc chọn MENU → THÊM ỨNG DỤNG.";

            emptyLabel.Dock =
                DockStyle.Fill;

            emptyLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            emptyLabel.Font =
                new Font(
                    "Segoe UI",
                    12
                );

            emptyLabel.ForeColor =
                config.DarkMode
                    ? Color.LightGray
                    : Color.Gray;

            emptyLabel.BackColor =
                backgroundColor;

            emptyLabel.Visible =
                false;

            contentPanel.Controls.Add(appPanel);
            contentPanel.Controls.Add(emptyLabel);
            contentPanel.Controls.Add(toolbar);
            contentPanel.Controls.Add(titleLabel);
            contentPanel.Controls.Add(footerLabel);

            LoadApplications();
        }

        // =====================================================
        // LOAD APPLICATIONS
        // =====================================================

        private void LoadApplications()
        {
            if (appPanel == null)
            {
                return;
            }

            appPanel.Controls.Clear();

            selectedApplications.Clear();

            Directory.CreateDirectory(
                appToolPath
            );

            string[] files =
                Directory.GetFiles(
                    appToolPath,
                    "*.exe"
                );

            files =
                files
                .OrderBy(
                    file =>
                    Path.GetFileName(file)
                )
                .ToArray();

            string search =
                searchBox != null
                    ? searchBox.Text
                        .Trim()
                        .ToLower()
                    : "";

            if (
                !string.IsNullOrWhiteSpace(
                    search
                )
            )
            {
                files =
                    files
                    .Where(
                        file =>
                        Path
                        .GetFileNameWithoutExtension(
                            file
                        )
                        .ToLower()
                        .Contains(
                            search
                        )
                    )
                    .ToArray();
            }

            emptyLabel.Visible =
                files.Length == 0;

            foreach (
                string file
                in files
            )
            {
                appPanel.Controls.Add(
                    CreateApplicationItem(
                        file
                    )
                );
            }
        }

        // =====================================================
        // APPLICATION ITEM
        // =====================================================

        private Control CreateApplicationItem(
            string file
        )
        {
            Panel borderPanel =
                new Panel();

            borderPanel.Width =
                140;

            borderPanel.Height =
                155;

            borderPanel.Margin =
                new Padding(10);

            borderPanel.Padding =
                new Padding(1);

            borderPanel.BackColor =
                Color.Gray;

            Panel panel =
                new Panel();

            panel.Dock =
                DockStyle.Fill;

            panel.BackColor =
                config.DarkMode
                    ? Color.FromArgb(
                        50,
                        50,
                        50
                    )
                    : Color.White;

            CheckBox selectBox =
                new CheckBox();

            selectBox.Size =
                new Size(28, 28);

            selectBox.Location =
                new Point(5, 5);

            PictureBox iconBox =
                new PictureBox();

            iconBox.Size =
                new Size(48, 48);

            iconBox.SizeMode =
                PictureBoxSizeMode.Zoom;

            iconBox.Top =
                35;

            try
            {
                Icon icon =
                    Icon.ExtractAssociatedIcon(
                        file
                    );

                iconBox.Image =
                    icon != null
                        ? icon.ToBitmap()
                        : SystemIcons.Application.ToBitmap();
            }
            catch
            {
                iconBox.Image =
                    SystemIcons.Application.ToBitmap();
            }

            string appName =
                Path.GetFileNameWithoutExtension(
                    file
                );

            Label nameLabel =
                new Label();

            nameLabel.Text =
                appName;

            nameLabel.Dock =
                DockStyle.Bottom;

            nameLabel.Height =
                38;

            nameLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            nameLabel.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold
                );

            nameLabel.ForeColor =
                config.DarkMode
                    ? Color.White
                    : Color.Black;

            panel.Resize +=
                delegate
                {
                    iconBox.Left =
                        (
                            panel.Width -
                            iconBox.Width
                        ) / 2;
                };

            selectBox.CheckedChanged +=
                delegate
                {
                    if (selectBox.Checked)
                    {
                        borderPanel.BackColor =
                            Color.DodgerBlue;

                        selectedApplications[file] =
                            selectBox;
                    }
                    else
                    {
                        borderPanel.BackColor =
                            Color.Gray;

                        selectedApplications.Remove(
                            file
                        );
                    }
                };

            Action runApplication =
                delegate
                {
                    if (!selectBox.Checked)
                    {
                        RunApplication(file);
                    }
                };

            panel.Click +=
                delegate
                {
                    runApplication();
                };

            iconBox.Click +=
                delegate
                {
                    runApplication();
                };

            nameLabel.Click +=
                delegate
                {
                    runApplication();
                };

            panel.Controls.Add(selectBox);
            panel.Controls.Add(iconBox);
            panel.Controls.Add(nameLabel);

            borderPanel.Controls.Add(panel);

            return borderPanel;
        }

        // =====================================================
        // RUN APPLICATION
        // =====================================================

        private void RunApplication(
            string file
        )
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = file,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =====================================================
        // ADD APPLICATION
        // =====================================================

        private void AddApplication()
        {
            using (
                OpenFileDialog dialog =
                    new OpenFileDialog()
            )
            {
                dialog.Filter =
                    "Ứng dụng (*.exe)|*.exe";

                dialog.Multiselect =
                    true;

                if (
                    dialog.ShowDialog()
                    != DialogResult.OK
                )
                {
                    return;
                }

                CopyApplications(
                    dialog.FileNames
                );

                LoadApplications();
            }
        }

        // =====================================================
        // COPY APPLICATIONS
        // =====================================================

        private void CopyApplications(
            string[] files
        )
        {
            foreach (
                string file
                in files
            )
            {
                if (
                    !File.Exists(file)
                )
                {
                    continue;
                }

                try
                {
                    string destination =
                        Path.Combine(
                            appToolPath,
                            Path.GetFileName(file)
                        );

                    if (
                        string.Equals(
                            Path.GetFullPath(file),
                            Path.GetFullPath(destination),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        continue;
                    }

                    File.Copy(
                        file,
                        destination,
                        true
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Lỗi"
                    );
                }
            }
        }

        // =====================================================
        // DELETE APPLICATIONS
        // =====================================================

        private void DeleteSelectedApplications()
        {
            if (
                selectedApplications.Count == 0
            )
            {
                MessageBox.Show(
                    "Hãy chọn ít nhất một ứng dụng để xóa."
                );

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Bạn có chắc muốn xóa " +
                    selectedApplications.Count +
                    " ứng dụng?",

                    "XÁC NHẬN",

                    MessageBoxButtons.YesNo,

                    MessageBoxIcon.Warning
                );

            if (
                result != DialogResult.Yes
            )
            {
                return;
            }

            List<string> files =
                selectedApplications.Keys.ToList();

            foreach (
                string file
                in files
            )
            {
                try
                {
                    if (
                        File.Exists(file)
                    )
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Lỗi xóa"
                    );
                }
            }

            selectedApplications.Clear();

            LoadApplications();
        }

        // =====================================================
        // DRAG DROP
        // =====================================================

        private void Form1_DragEnter(
            object sender,
            DragEventArgs e
        )
        {
            if (
                e.Data != null &&
                e.Data.GetDataPresent(
                    DataFormats.FileDrop
                )
            )
            {
                e.Effect =
                    DragDropEffects.Copy;
            }
            else
            {
                e.Effect =
                    DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(
            object sender,
            DragEventArgs e
        )
        {
            if (e.Data == null)
            {
                return;
            }

            string[] files =
                e.Data.GetData(
                    DataFormats.FileDrop
                )
                as string[];

            if (files == null)
            {
                return;
            }

            CopyApplications(files);

            LoadApplications();
        }

        // =====================================================
        // ABOUT
        // =====================================================

        private void ShowAbout()
        {
            if (
                aboutForm != null &&
                !aboutForm.IsDisposed
            )
            {
                aboutForm.Activate();
                return;
            }

            aboutForm =
                new Form();

            aboutForm.Text =
                "ABOUT";

            aboutForm.Width =
                430;

            aboutForm.Height =
                380;

            aboutForm.StartPosition =
                FormStartPosition.CenterParent;

            aboutForm.FormBorderStyle =
                FormBorderStyle.FixedDialog;

            aboutForm.MaximizeBox =
                false;

            Label info =
                new Label();

            info.Text =
                "WINDOWS TOOLKIT\r\n\r\n" +
                "VERSION: " +
                CURRENT_VERSION +

                "\r\n\r\nBUILD DATE: " +
                buildDate +

                "\r\n\r\nDEV: tronglinh1997";

            info.Dock =
                DockStyle.Top;

            info.Height =
                220;

            info.TextAlign =
                ContentAlignment.MiddleCenter;

            info.Font =
                new Font(
                    "Segoe UI",
                    11
                );

            Button updateButton =
                new Button();

            updateButton.Text =
                "KIỂM TRA CẬP NHẬT";

            updateButton.Width =
                220;

            updateButton.Height =
                40;

            updateButton.Top =
                250;

            updateButton.Left =
                100;

            updateButton.Click +=
                async delegate
                {
                    updateButton.Enabled =
                        false;

                    updateButton.Text =
                        "ĐANG KIỂM TRA...";

                    await CheckForUpdateManualAsync();

                    if (
                        !aboutForm.IsDisposed
                    )
                    {
                        updateButton.Enabled =
                            true;

                        updateButton.Text =
                            "KIỂM TRA CẬP NHẬT";
                    }
                };

            aboutForm.FormClosed +=
                delegate
                {
                    aboutForm =
                        null;
                };

            aboutForm.Controls.Add(info);
            aboutForm.Controls.Add(updateButton);

            aboutForm.Show();
        }

        // =====================================================
        // CHECK UPDATE
        // =====================================================

        private async Task CheckForUpdateManualAsync()
        {
            try
            {
                string json =
                    await httpClient.GetStringAsync(
                        UPDATE_INFO_URL
                    );

                UpdateInfo update =
                    JsonSerializer.Deserialize<UpdateInfo>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive =
                                true
                        }
                    );

                if (
                    update == null ||
                    string.IsNullOrWhiteSpace(
                        update.Version
                    )
                )
                {
                    MessageBox.Show(
                        "KHÔNG ĐỌC ĐƯỢC THÔNG TIN CẬP NHẬT."
                    );

                    return;
                }

                if (
                    CompareVersion(
                        update.Version,
                        CURRENT_VERSION
                    ) > 0
                )
                {
                    string message =
                        "CÓ BẢN CẬP NHẬT MỚI!\r\n\r\n" +

                        "Phiên bản hiện tại: " +
                        CURRENT_VERSION +

                        "\r\n\r\nPhiên bản mới: " +
                        update.Version;

                    if (
                        !string.IsNullOrWhiteSpace(
                            update.Description
                        )
                    )
                    {
                        message +=
                            "\r\n\r\nNỘI DUNG:\r\n" +
                            update.Description;
                    }

                    message +=
                        "\r\n\r\nBạn có muốn cập nhật ngay không?";

                    if (
                        MessageBox.Show(
                            message,
                            "WINDOWS TOOLKIT UPDATE",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information
                        )
                        == DialogResult.Yes
                    )
                    {
                        await DownloadAndInstallUpdateAsync(
                            update
                        );
                    }
                }
                else
                {
                    MessageBox.Show(
                        "BẠN ĐANG SỬ DỤNG PHIÊN BẢN MỚI NHẤT.\r\n\r\n" +
                        "VERSION: " +
                        CURRENT_VERSION,

                        "WINDOWS TOOLKIT UPDATE"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "KHÔNG THỂ KIỂM TRA CẬP NHẬT.\r\n\r\n" +
                    ex.Message,

                    "WINDOWS TOOLKIT UPDATE"
                );
            }
        }

        // =====================================================
        // DOWNLOAD UPDATE
        // =====================================================

        private async Task DownloadAndInstallUpdateAsync(
            UpdateInfo update
        )
        {
            string downloadUrl =
                update.DownloadUrl;

            if (
                string.IsNullOrWhiteSpace(
                    downloadUrl
                )
            )
            {
                downloadUrl =
                    update.Url;
            }

            if (
                string.IsNullOrWhiteSpace(
                    downloadUrl
                )
            )
            {
                downloadUrl =
                    update.Download;
            }

            if (
                string.IsNullOrWhiteSpace(
                    downloadUrl
                )
            )
            {
                MessageBox.Show(
                    "KHÔNG TÌM THẤY LINK TẢI."
                );

                return;
            }

            Form downloadForm =
                new Form();

            downloadForm.Text =
                "WINDOWS TOOLKIT UPDATE";

            downloadForm.Width =
                450;

            downloadForm.Height =
                190;

            downloadForm.StartPosition =
                FormStartPosition.CenterParent;

            downloadForm.FormBorderStyle =
                FormBorderStyle.FixedDialog;

            downloadForm.ControlBox =
                false;

            Label statusLabel =
                new Label();

            statusLabel.Text =
                "ĐANG TẢI BẢN CẬP NHẬT...";

            statusLabel.Dock =
                DockStyle.Top;

            statusLabel.Height =
                55;

            statusLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            ProgressBar progressBar =
                new ProgressBar();

            progressBar.Width =
                360;

            progressBar.Height =
                28;

            progressBar.Left =
                35;

            progressBar.Top =
                65;

            Label percentLabel =
                new Label();

            percentLabel.Text =
                "0%";

            percentLabel.Width =
                420;

            percentLabel.Top =
                105;

            percentLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            downloadForm.Controls.Add(statusLabel);
            downloadForm.Controls.Add(progressBar);
            downloadForm.Controls.Add(percentLabel);

            downloadForm.Show();

            try
            {
                string tempFolder =
                    Path.Combine(
                        Path.GetTempPath(),
                        "WindowsToolkitUpdate"
                    );

                Directory.CreateDirectory(
                    tempFolder
                );

                string newExePath =
                    Path.Combine(
                        tempFolder,
                        "WindowsToolkit_New.exe"
                    );

                using (
                    HttpResponseMessage response =
                        await httpClient.GetAsync(
                            downloadUrl,
                            HttpCompletionOption.ResponseHeadersRead
                        )
                )
                {
                    response.EnsureSuccessStatusCode();

                    long totalBytes =
                        response.Content.Headers.ContentLength
                        ?? -1;

                    using (
                        Stream source =
                            await response.Content
                            .ReadAsStreamAsync()
                    )
                    using (
                        FileStream destination =
                            new FileStream(
                                newExePath,
                                FileMode.Create,
                                FileAccess.Write,
                                FileShare.None
                            )
                    )
                    {
                        byte[] buffer =
                            new byte[81920];

                        long downloaded =
                            0;

                        int bytesRead;

                        while (
                            (
                                bytesRead =
                                    await source.ReadAsync(
                                        buffer,
                                        0,
                                        buffer.Length
                                    )
                            ) > 0
                        )
                        {
                            await destination.WriteAsync(
                                buffer,
                                0,
                                bytesRead
                            );

                            downloaded +=
                                bytesRead;

                            if (
                                totalBytes > 0
                            )
                            {
                                int percent =
                                    (int)(
                                        downloaded *
                                        100 /
                                        totalBytes
                                    );

                                percent =
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            100,
                                            percent
                                        )
                                    );

                                progressBar.Value =
                                    percent;

                                percentLabel.Text =
                                    percent +
                                    "%";
                            }
                        }

                        await destination.FlushAsync();
                    }
                }

                if (
                    !File.Exists(
                        newExePath
                    )
                )
                {
                    throw new Exception(
                        "KHÔNG TÌM THẤY FILE SAU KHI TẢI."
                    );
                }

                FileInfo fileInfo =
                    new FileInfo(
                        newExePath
                    );

                if (
                    fileInfo.Length <= 0
                )
                {
                    throw new Exception(
                        "FILE CẬP NHẬT KHÔNG HỢP LỆ."
                    );
                }

                progressBar.Value =
                    100;

                percentLabel.Text =
                    "100%";

                statusLabel.Text =
                    "ĐANG CÀI ĐẶT BẢN CẬP NHẬT...";

                await Task.Delay(500);

                downloadForm.Close();

                InstallUpdate(
                    newExePath
                );
            }
            catch (Exception ex)
            {
                if (
                    !downloadForm.IsDisposed
                )
                {
                    downloadForm.Close();
                }

                MessageBox.Show(
                    "KHÔNG THỂ TẢI BẢN CẬP NHẬT.\r\n\r\n" +
                    ex.Message,

                    "WINDOWS TOOLKIT UPDATE",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Error
                );
            }
        }

        // =====================================================
        // INSTALL UPDATE
        // =====================================================

        private void InstallUpdate(
            string newExePath
        )
        {
            try
            {
                string currentExe =
                    Application.ExecutablePath;

                int processId =
                    Process.GetCurrentProcess().Id;

                string tempFolder =
                    Path.Combine(
                        Path.GetTempPath(),
                        "WindowsToolkitUpdate"
                    );

                string updaterPath =
                    Path.Combine(
                        tempFolder,
                        "UpdateWindowsToolkit.cmd"
                    );

                string backupExe =
                    currentExe +
                    ".old";

                string script =
                    "@echo off\r\n" +

                    ":WAIT\r\n" +

                    "tasklist /FI \"PID eq " +
                    processId +
                    "\" 2>NUL | find \"" +
                    processId +
                    "\" >NUL\r\n" +

                    "if not errorlevel 1 (\r\n" +
                    "timeout /t 1 /nobreak >nul\r\n" +
                    "goto WAIT\r\n" +
                    ")\r\n" +

                    "if exist \"" +
                    backupExe +
                    "\" del /F /Q \"" +
                    backupExe +
                    "\"\r\n" +

                    "if exist \"" +
                    currentExe +
                    "\" move /Y \"" +
                    currentExe +
                    "\" \"" +
                    backupExe +
                    "\"\r\n" +

                    "copy /Y \"" +
                    newExePath +
                    "\" \"" +
                    currentExe +
                    "\"\r\n" +

                    "start \"\" \"" +
                    currentExe +
                    "\"\r\n" +

                    "del /F /Q \"" +
                    newExePath +
                    "\"\r\n" +

                    "del /F /Q \"%~f0\"\r\n";

                File.WriteAllText(
                    updaterPath,
                    script
                );

                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            "cmd.exe",

                        Arguments =
                            "/c \"" +
                            updaterPath +
                            "\"",

                        UseShellExecute =
                            true,

                        CreateNoWindow =
                            true
                    }
                );

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "KHÔNG THỂ CÀI ĐẶT CẬP NHẬT.\r\n\r\n" +
                    ex.Message,

                    "WINDOWS TOOLKIT UPDATE"
                );
            }
        }

        // =====================================================
        // COMPARE VERSION
        // =====================================================

        private int CompareVersion(
            string latest,
            string current
        )
        {
            try
            {
                Version latestVersion =
                    new Version(latest);

                Version currentVersion =
                    new Version(current);

                return latestVersion.CompareTo(
                    currentVersion
                );
            }
            catch
            {
                return 0;
            }
        }

        // =====================================================
        // OPEN FOLDER
        // =====================================================

        private void OpenFolder(
            string folder
        )
        {
            try
            {
                Directory.CreateDirectory(
                    folder
                );

                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            folder,

                        UseShellExecute =
                            true
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi"
                );
            }
        }
    }
}