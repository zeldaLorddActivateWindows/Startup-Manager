using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace StartupManager
{
    public partial class MainWindow : Window
    {
        public List<BitmapSource> GlobalIcons { get; } = new List<BitmapSource>();
        public List<string> AvailableCommands { get; } = new List<string> { "/background", "-silent", "--minimize", "--hidden", "--no-startup-window" };

        public MainWindow()
        {
            InitializeComponent();
            BtnGetData_Click(null, null);
        }

        private void BtnGetData_Click(object sender, RoutedEventArgs e)
        {
            var startupPrograms = GetStartupProgramsFromRegistry();

            StartupProgramsListView.ItemsSource = startupPrograms.Select((program, index) => new StartupProgram
            {
                Icon = GlobalIcons[index],
                Name = program.Split(':')[0],
                Path = ExtractProgramPath(program),
                Command = ExtractCommand(program),
                AvailableCommands = AvailableCommands
            })
            .ToList();
        }

        public List<string> GetStartupProgramsFromRegistry()
        {
            List<string> startupPrograms = new List<string>();

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        object value = key.GetValue(valueName);
                        if (value != null)
                        {
                            string programPath = value.ToString();
                            startupPrograms.Add($"{valueName}: {programPath}");

                            try
                            {
                                BitmapSource icon = IconHelper.GetFileIcon(programPath);
                                GlobalIcons.Add(icon);
                            }
                            catch
                            {
                                GlobalIcons.Add(null);
                            }
                        }
                    }
                }
            }

            return startupPrograms;
        }

        private string ExtractProgramPath(string program)
        {
            string fullPath = program.Substring(program.Split(':')[0].Length + 2).Trim();
            if (fullPath.StartsWith("\""))
            {
                int endQuoteIndex = fullPath.IndexOf('"', 1);
                return endQuoteIndex > 0 ? fullPath.Substring(1, endQuoteIndex - 1) : fullPath.Trim('"');
            }
            else
            {
                int firstSpaceIndex = fullPath.IndexOf(' ');
                return firstSpaceIndex > 0 ? fullPath.Substring(0, firstSpaceIndex) : fullPath;
            }
        }

        private string ExtractCommand(string program)
        {
            string fullPath = program.Substring(program.Split(':')[0].Length + 2).Trim();
            if (fullPath.StartsWith("\""))
            {
                int endQuoteIndex = fullPath.IndexOf('"', 1);
                return endQuoteIndex > 0 ? fullPath.Substring(endQuoteIndex + 1).Trim() : string.Empty;
            }
            else
            {
                int firstSpaceIndex = fullPath.IndexOf(' ');
                return firstSpaceIndex > 0 ? fullPath.Substring(firstSpaceIndex + 1).Trim() : string.Empty;
            }
        }

        private void CommandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.DataContext is StartupProgram program)
            {
                string newCommand = comboBox.SelectedItem as string;
                UpdateStartupCommand(program.Name, newCommand);
            }
        }

        private void UpdateStartupCommand(string programName, string newCommand)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key != null)
                {
                    string currentValue = key.GetValue(programName)?.ToString();
                    if (currentValue != null)
                    {
                        string programPath = ExtractProgramPath($"{programName}: {currentValue}");
                        string updatedValue = $"\"{programPath}\" {newCommand}";
                        key.SetValue(programName, updatedValue);
                    }
                }
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        key.DeleteValue(valueName);
                    }
                }
            }

            BtnGetData_Click(sender, e);
        }

        private void BtnAddProgram_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "Select a Program to Add to Startup"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string programPath = openFileDialog.FileName;
                string programName = System.IO.Path.GetFileNameWithoutExtension(programPath);
                string programExe = System.IO.Path.GetFileName(programPath);

                Dictionary<string, string> supportedCommands = new Dictionary<string, string>
                {
                    { "Discord.exe", "--minimize" },
                    { "Steam.exe", "-silent" },
                    { "msedge.exe", "--no-startup-window" }
                };

                string command = supportedCommands.ContainsKey(programExe) ? supportedCommands[programExe] : string.Empty;

                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue(programName, $"\"{programPath}\" {command}");
                    }
                }
                BtnGetData_Click(sender, e);
            }
        }
    }
}