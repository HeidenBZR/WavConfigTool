
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WavConfigTool
{
    public partial class MainWindow
    {
        void ChangeVoicebank()
        {
            string old_vb = Reclist.VoicebankPath;
            SaveBackup();
            var vb = ProjectWindow.VoicebankDialog(Reclist.VoicebankPath);
            if (vb != null)
                if (Directory.Exists(vb))
                {
                    Reclist.SetVoicebank(vb);
                    if (Settings.IsUnsaved)
                        SaveBackup();
                    else
                        Save();
                    ClearWavcontrols();
                    loaded = (CheckSettings() && (Settings.IsUnsaved && ReadProject(TempProject)
                        || ReadProject(Settings.ProjectFile)));
                    if (!loaded)
                    {
                        ClearWavcontrols();
                        Reclist.SetVoicebank(old_vb);
                        if (Settings.IsUnsaved)
                            SaveBackup();
                        else
                            Save();
                        MessageBox.Show("Voicebank changing failed");
                        loaded = (CheckSettings() && (Settings.IsUnsaved && ReadProject(TempProject)
                        || ReadProject(Settings.ProjectFile)));
                    }
                }
        }

        void ChangeSettings()
        {
            string old_s = Settings.WavSettings;
            var s = ProjectWindow.SettingsDialog();
            if (s != null)
                if (File.Exists(s))
                {
                    Settings.WavSettings = s;
                    ClearWavcontrols();
                    loaded = (CheckSettings() && (Settings.IsUnsaved && OpenBackup()
                        || ReadProject(Settings.ProjectFile)));
                    if (!loaded)
                    {
                        Settings.WavSettings = old_s;
                        ClearWavcontrols();
                        loaded = (CheckSettings() && (Settings.IsUnsaved && OpenBackup()
                            || ReadProject(Settings.ProjectFile)));
                        MessageBox.Show("WavSettings changing failed");
                    }
                }
        }


        bool OpenBackup()
        {
            try
            {
                if (IsUnsaved && File.Exists(TempProject))
                {
                    if (CheckSettings() && ReadProject(TempProject))
                    {
                        MessageBox.Show("Some unsaved project was restored. Please resave it.");
                        loaded = true;
                        IsUnsaved = true;
                        return loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on openbackup",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        bool OpenLast()
        {
            try
            {
                if (CheckSettings() && CheckLast())
                {
                    loaded = true;
                    return loaded;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on open last",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        bool CheckSettings()
        {
            if (Settings.WavSettings != "" && File.Exists(Settings.WavSettings))
            {
                ReadSettings(Settings.WavSettings);
                return true;
            }
            return false;
        }

        bool CheckLast()
        {
            if (Settings.ProjectFile != "" && File.Exists(Settings.ProjectFile))
            {
                bool result = ReadProject(Settings.ProjectFile);
                return result;
            }
            return false;
        }

        void NewProject(string settings, string voicebank)
        {
            try
            {
                ReadSettings(settings);
                Reclist.SetVoicebank(voicebank);
                Settings.ProjectFile = TempProject;
                SetTitle();
                if (Settings.ProjectFile != TempProject && File.Exists(TempProject))
                    File.Delete(TempProject);
                ProjectLoaded();
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on new project");
            }
        }

        bool Open(string settings, string project)
        {
            ReadSettings(settings);
            if (File.Exists(project))
            {
                bool result = ReadProject(project);
                if (!result)
                    return false;
                if (Settings.ProjectFile != TempProject && File.Exists(TempProject))
                    File.Delete(TempProject);
                return true;
            }
            else MessageBox.Show("Ошибка при открытии файла проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        public void Save()
        {
            // TODO: Сохранять в файл проекта параметры, связанные с проектом
            try
            {

                if (Settings.ProjectFile == TempProject)
                {
                    SaveAs();
                    return;
                }
                string text = "";
                text += $"{Reclist.VoicebankPath}\r\n";

                foreach (WavControl control in WavControls)
                {
                    if (control.Ds.Count == 0 && control.Vs.Count == 0 && control.Cs.Count == 0)
                        continue;
                    text += $"{control.Recline.Filename}\r\n";
                    text += $"{String.Join(" ", control.Ds.Select(n => n.ToString("f0")))}\r\n";
                    text += $"{ String.Join(" ", control.Vs.Select(n => n.ToString("f0"))) }\r\n";
                    text += $"{String.Join(" ", control.Cs.Select(n => n.ToString("f0")))}\r\n";
                }
                File.WriteAllText(Settings.ProjectFile, text, Encoding.UTF8);
                IsUnsaved = false;
                SetTitle();
                if (File.Exists(TempProject))
                    File.Delete(TempProject);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on save project");
                return;
            }
        }

        public void SaveAs()
        {
            try
            {
                string lastpath = Settings.ProjectFile;
                SaveFileDialog openFileDialog = new SaveFileDialog();
                openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName == "") return;
                Settings.ProjectFile = openFileDialog.FileName;
                Save();
                File.Delete(lastpath);
                IsUnsaved = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on save as");
                return;
            }
        }

        public void SaveBackup()
        {
            string text = "";
            text += $"{Reclist.VoicebankPath}\r\n";

            foreach (WavControl control in WavControls)
            {
                text += $"{control.Recline.Filename}\r\n";
                text += $"{String.Join(" ", control.Ds.Select(n => n.ToString("f0")))}\r\n";
                text += $"{ String.Join(" ", control.Vs.Select(n => n.ToString("f0"))) }\r\n";
                text += $"{String.Join(" ", control.Cs.Select(n => n.ToString("f0")))}\r\n";
            }
            File.WriteAllText(TempProject, text, Encoding.UTF8);
            SetTitle(true);
            // IsUnsaved = true;
        }

        void ReadSettings(string settings)
        {
            try
            {

                string[] lines = File.ReadAllLines(settings);
                var vs = lines[0].Split(' ');
                var cs = lines[1].Split(' ');
                Reclist = new Reclist(vs, cs);
                WavControls = new List<WavControl>();
                var name = System.IO.Path.GetFileNameWithoutExtension(settings);
                Reclist.Name = name;
                for (int i = 2; i < lines.Length; i++)
                {
                    string[] items = lines[i].Split('\t');
                    if (items.Length != 3)
                        continue;
                    AddFile(items[0], items[1], items[2]);
                }
                Settings.WavSettings = settings;

                OtoGenerator.Init(Reclist.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on wavsettings reading");
            }
        }

        bool ReadProject(string project)
        {
            // TODO: Создать класс ПРОЕКТ и перенести туда все параметры и функции, связанные с проектом
            try
            {
                string missing = "";
                Settings.ProjectFile = project;
                string[] lines = File.ReadAllLines(project);
                Reclist.SetVoicebank(lines[0]);
                for (int i = 1; i + 3 < lines.Length; i += 4)
                {
                    string filename = lines[i];
                    string pds = lines[i + 1];
                    string pvs = lines[i + 2];
                    string pcs = lines[i + 3];
                    WavControl control = WavControls.Find(n => n.Recline.Filename == filename);
                    if (control != null)
                    {

                        if (pds.Length > 0) control.Ds = pds.Split(' ').Select(n => double.Parse(n)).ToList();
                        if (pvs.Length > 0) control.Vs = pvs.Split(' ').Select(n => double.Parse(n)).ToList();
                        if (pcs.Length > 0) control.Cs = pcs.Split(' ').Select(n => double.Parse(n)).ToList();
                    }
                    else
                    {
                        //missing += $"{control.Recline.Path}\n";
                    }
                }
                //if (missing != "")
                   // MessageBox.Show("Some sample is missing: \n\n" + missing, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                ProjectLoaded();
                return true;
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on project reading");
                return false;
            }
        }

        public void GenerateOto()
        {
            try
            {
                string text = "";
                Reclist.Aliases = new List<string>();
                foreach (WavControl control in WavControls)
                {
                    text += control.GenerateOto();
                }
                string filename = SaveOtoDialog();
                if (filename != "")
                    File.WriteAllText(System.IO.Path.Combine(Reclist.VoicebankPath, filename), text, Encoding.ASCII);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on oto.ini", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
