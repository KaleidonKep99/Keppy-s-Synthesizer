﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeppySynthConfigurator
{
    class UpdateSystem
    {
        [DllImport("wininet.dll")]
        public extern static bool InternetGetConnectedState(out int connDescription, int ReservedValue);

        public static string ProductName = "Keppy's Synthesizer";
        public static string UpdateTextFile = "https://raw.githubusercontent.com/KaleidonKep99/Keppy-s-Driver/master/output/keppydriverupdate.txt";
        public static string UpdatePage = "https://github.com/KaleidonKep99/Keppy-s-Synthesizer/releases/tag/{0}";
        public static string UpdateFileVersion = String.Format("{0}\\keppysynth\\keppysynth.dll", Environment.GetFolderPath(Environment.SpecialFolder.System));

        public static bool IsInternetAvailable()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        public static void TriggerUpdateWindow(Version x, Version y, String newestversion, bool forced, bool startup)
        {
            if (forced && startup)
            {
                Forms.DLEngine frm = new Forms.DLEngine(newestversion, String.Format("Downloading update {0}...", newestversion, @"{0}"), null, 0, true);
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog();
            }
            else
            {
                UpdateYesNo upd = new UpdateYesNo(x, y, true, startup);
                if (startup)
                    upd.StartPosition = FormStartPosition.CenterScreen;
                else
                    upd.StartPosition = FormStartPosition.CenterParent;
                DialogResult dialogResult = upd.ShowDialog();
                upd.Dispose();
                if (dialogResult == DialogResult.Yes)
                {
                    Forms.DLEngine frm = new Forms.DLEngine(newestversion, String.Format("Downloading update {0}...", newestversion, @"{0}"), null, 0, false);
                    frm.StartPosition = FormStartPosition.CenterScreen;
                    frm.ShowDialog();
                }
            }
        }

        public static void NoUpdates(bool startup, bool internetok)
        {
            if (!startup)
            {
                UpdateYesNo upd = new UpdateYesNo(null, null, internetok, startup);
                upd.StartPosition = FormStartPosition.CenterParent;
                upd.ShowDialog();
                upd.Dispose();
            }
        }

        public static void CheckChangelog()
        {
            bool internetok = IsInternetAvailable();
            if (internetok == false)
            {
                MessageBox.Show("There's no Internet connection.\n\nYou can't see the changelog without one.", String.Format("{0} - No Internet connection available", ProductName), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead(UpdateTextFile);
                    StreamReader reader = new StreamReader(stream);
                    String newestversion = reader.ReadToEnd();
                    Process.Start(String.Format(UpdatePage, newestversion));
                }
                catch (Exception ex)
                {
                    Functions.ShowErrorDialog(Properties.Resources.wi, System.Media.SystemSounds.Exclamation, "Unknown error", "An error has occurred while trying to show you the latest changelog.\nPlease try again later.\n\nPress OK to continue.", true, ex);
                }
            }
        }

        public static string GetCurrentBranch()
        {
            if (Properties.Settings.Default.UpdateBranch == "canary")
                return "Canary branch";
            else if (Properties.Settings.Default.UpdateBranch == "normal")
                return "Normal branch";
            else if (Properties.Settings.Default.UpdateBranch == "delay")
                return "Delayed branch";
            else if (Properties.Settings.Default.UpdateBranch == "choose")
                return "No branch selected";
            else
                return "Normal branch";
        }

        public static Color GetCurrentBranchColor()
        {
            if (Properties.Settings.Default.UpdateBranch == "canary")
                return Color.FromArgb(230, 149, 0);
            else if (Properties.Settings.Default.UpdateBranch == "normal")
                return SystemColors.ControlText;
            else if (Properties.Settings.Default.UpdateBranch == "delay")
                return SystemColors.GrayText;
            else if (Properties.Settings.Default.UpdateBranch == "choose")
                return Color.FromArgb(182, 0, 0);
            else
                return Color.FromArgb(255, 255, 255);
        }

        public static string GetCurrentBranchToolTip()
        {
            if (Properties.Settings.Default.UpdateBranch == "canary")
                return "Receive all updates.\nYou may get broken updates that haven't been fully tested.\nDesigned for testers and early adopters.";
            else if (Properties.Settings.Default.UpdateBranch == "normal")
                return "Receive occasional updates and urgent bugfixes (Eg. from version x.x.1.x to x.x.2.x).\nRecommended.";
            else if (Properties.Settings.Default.UpdateBranch == "delay")
                return "You will only get major releases (Eg. from version x.1.x.x to x.2.x.x).\nFor those who do not wish to update often.\nNot recommended.";
            else if (Properties.Settings.Default.UpdateBranch == "choose")
                return "No information, since you didn't chose a branch.";
            else
                return "Receive occasional updates and urgent bugfixes (Eg. from version x.x.1.x to x.x.2.x).\nRecommended.";
        }

        public static void CheckForUpdates(bool forced, bool startup)
        {
            bool internetok = IsInternetAvailable();
            if (internetok == false)
            {
                NoUpdates(startup, false);
            }
            else
            {
                try
                {
                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead(UpdateTextFile);
                    StreamReader reader = new StreamReader(stream);
                    String newestversion = reader.ReadToEnd();
                    FileVersionInfo Driver = FileVersionInfo.GetVersionInfo(UpdateFileVersion);
                    Version x = null;
                    Version.TryParse(newestversion.ToString(), out x);
                    Version y = null;
                    Version.TryParse(Driver.FileVersion.ToString(), out y);
                    if (forced)
                    {
                        Program.DebugToConsole(false, String.Format("The user forced a reinstall/downgrade of the program. ({0})", newestversion), null);
                        TriggerUpdateWindow(x, y, newestversion, forced, startup);
                    }
                    else
                    {
                        if (x > y)
                        {
                            if (Properties.Settings.Default.UpdateBranch == "canary")
                            {
                                Program.DebugToConsole(false, String.Format("New version found. Requesting user to download it. ({0})", newestversion), null);
                                TriggerUpdateWindow(x, y, newestversion, forced, startup);
                            }
                            else if (Properties.Settings.Default.UpdateBranch == "normal")
                            {
                                if (x.Major != y.Major || x.MajorRevision != y.MajorRevision || x.Minor != y.Minor)
                                {
                                    Program.DebugToConsole(false, String.Format("New version found. Requesting user to download it. ({0})", newestversion), null);
                                    TriggerUpdateWindow(x, y, newestversion, forced, startup);
                                }
                                else
                                {
                                    Program.DebugToConsole(false, String.Format("No updates have been found. ({0})", newestversion), null);
                                    NoUpdates(startup, internetok);
                                }
                            }
                            else if (Properties.Settings.Default.UpdateBranch == "delay")
                            {
                                if (x.Major != y.Major || x.MajorRevision != y.MajorRevision)
                                {
                                    Program.DebugToConsole(false, String.Format("New version found. Requesting user to download it. ({0})", newestversion), null);
                                    TriggerUpdateWindow(x, y, newestversion, forced, startup);
                                }
                                else
                                {
                                    Program.DebugToConsole(false, String.Format("No late updates have been found. ({0})", newestversion), null);
                                    NoUpdates(startup, internetok);
                                }
                            }
                        }
                        else
                        {
                            Program.DebugToConsole(false, String.Format("No updates have been found. ({0})", newestversion), null);
                            NoUpdates(startup, internetok);
                        }
                    }
                }
                catch
                {
                    Program.DebugToConsole(false, "An error has occurred while checking for updates.", null);
                    NoUpdates(startup, internetok);
                }
            }
        }
    }
}