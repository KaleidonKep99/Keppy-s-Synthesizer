﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Midi;

namespace OmniMIDIConfigurator
{
    class SoundFontListExtension
    {
        public static Boolean StopCheck = false;

        static FileSystemWatcher CSFWatcher;
        public static String CSFCS = "";

        public static void OpenCSFWatcher(bool FSEH, FileSystemEventHandler CSFH)
        {
            if (!FSEH)
            {
                // Spawn listener for shared list
                CSFWatcher = new FileSystemWatcher();
                CSFWatcher.Path = Path.GetDirectoryName(Program.ListsPath[0]);
                CSFWatcher.Filter = Path.GetFileName(Program.ListsPath[0]);
                CSFWatcher.NotifyFilter = NotifyFilters.LastWrite;
                return;
            }

            // Create handlers
            if (CSFH != null)
            {
                CSFWatcher.Changed += CSFH;
                CSFWatcher.Created += CSFH;
            }
        }

        public static void StartCSFWatcher()
        {
            CSFWatcher.EnableRaisingEvents = true;
        }

        public static void CloseCSFWatcher()
        {
            CSFWatcher.EnableRaisingEvents = false;
            CSFWatcher.Dispose();
        }

        public static bool CheckSupportedFormat(string FE)
        {
            foreach (String FF in Properties.Settings.Default.SupportedFormats)
            {
                if (FE.ToLowerInvariant() == FF)
                {
                    return true;
                }
            }

            return false;
        }

        public static string ReturnSoundFontFormat(string fileext)
        {
            if (fileext.ToLowerInvariant() == ".sf1")
                return "SF1";
            else if (fileext.ToLowerInvariant() == ".sf2")
                return "SF2";
            else if (fileext.ToLowerInvariant() == ".sfz")
                return "SFZ";
            else if (fileext.ToLowerInvariant() == ".sf2pack")
                return "SF2 Pack";
            else
                return "N/A";
        }

        public static string ReturnSoundFontFormatMore(string fileext)
        {
            if (fileext.ToLowerInvariant() == ".sf1")
                return "SoundFont 1.x by E-mu Systems";
            else if (fileext.ToLowerInvariant() == ".sf2")
                return "SoundFont 2.x by Creative Labs";
            else if (fileext.ToLowerInvariant() == ".sfz")
                return "SoundFontZ by Cakewalk™";
            else if (fileext.ToLowerInvariant() == ".sf2pack")
                return "SoundFont 2 compressed package";
            else
                return "Unknown format";
        }

        public static string ReturnSoundFontSize(string preset, string ext, long length)
        {
            if (ext.ToLowerInvariant() != ".sfz")
            {
                string size = Functions.ReturnLength(length, false);
                return size;
            }
            else
            {
                long size = SFZ.GetSoundFontZSize(preset);
                if (size > 0) return Functions.ReturnLength(size, true);
                else return "N/A";
            }
        }

        public static ListViewItem[] AddSFToList(String[] SFs, Boolean BPO, Boolean NoErrs)
        {
            List<ListViewItem> iSFs = new List<ListViewItem>();
            List<String> SFErrs = new List<String>();

            int BV = -1, PV = -1, DBV = 0, DPV = -1;
            bool XGMode = false;

            foreach (String SF in SFs)
            {
                if (CheckSupportedFormat(Path.GetExtension(SF)))
                {
                    // Check if it's valid
                    int SFH = BassMidi.BASS_MIDI_FontInit(SF, BASSFlag.BASS_DEFAULT);
                    BASSError Err = Bass.BASS_ErrorGetCode();

                    // SoundFont is valid, continue
                    if (Err == 0)
                    {
                        BassMidi.BASS_MIDI_FontFree(SFH);

                        if (BPO | Path.GetExtension(SF).ToLowerInvariant() == ".sfz")
                        {
                            using (var BPOW = new BankNPresetSel(Path.GetFileName(SF), false, (BPO && Path.GetExtension(SF).ToLowerInvariant() != ".sfz"), null))
                            {
                                var RES = BPOW.ShowDialog();

                                if (RES == DialogResult.OK)
                                {
                                    BV = BPOW.BankValueReturn;
                                    PV = BPOW.PresetValueReturn;
                                    DBV = BPOW.DesBankValueReturn;
                                    DPV = BPOW.DesPresetValueReturn;
                                    XGMode = BPOW.XGModeC;
                                }
                                else continue;
                            }
                        }

                        FileInfo file = new FileInfo(SF);
                        ListViewItem iSF = new ListViewItem(new[]
                        {
                            SF,
                            BV.ToString(), PV.ToString(), DBV.ToString(), DPV.ToString(), XGMode ? "Yes" : "No", "Yes",
                            ReturnSoundFontFormat(Path.GetExtension(SF)),
                            ReturnSoundFontSize(SF, Path.GetExtension(SF), file.Length)
                        });

                        iSF.Checked = true;

                        iSFs.Add(iSF);
                    }
                    else SFErrs.Add(SF);
                }
                else SFErrs.Add(SF);
            }

            if (SFErrs.Count > 0)
            {
                if (!NoErrs)
                {
                    Program.ShowError(
                        4,
                        "Invalid SoundFont(s) detected",
                        String.Format(
                            "The following SoundFont(s) were not valid, and have not been added to the list.\n\n{0}\n\nPress OK to continue",
                            string.Join(Environment.NewLine, SFErrs.ToArray())),
                        null);
                }

                return new ListViewItem[] { };
            }

            return iSFs.ToArray();
        }

        public static bool WaitForFile(string File)
        {
            int Retries = 0;
            while (true)
            {
                ++Retries;
                try
                {
                    using (FileStream FS = new FileStream(File, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 256))
                    {
                        FS.ReadByte();

                        // File is accessible, continue
                        return true;
                    }
                }
                catch
                {
                    if (Retries > 10)
                        return false;

                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public static string StripSFZValues(string SFToStrip)
        {
            if (SFToStrip.ToLower().IndexOf('=') != -1)
                return SFToStrip.Substring(SFToStrip.LastIndexOf('|') + 1);
            else
                return SFToStrip;
        }

        public static string GetName(string value)
        {
            int A = value.IndexOf(" = ");
            if (A == -1) return value;
            return value.Substring(0, A);
        }

        public static string GetValue(string value)
        {
            int A = value.LastIndexOf(" = ");
            if (A == -1) return "";
            int A2 = A + (" = ").Length;
            if (A2 >= value.Length) return "";
            return value.Substring(A2);
        }

        public static void ChangeList(int SelectedList, String LoadPath, bool ImportMode, bool SuppressErrors)
        {
            List<ListViewItem> iSFs = new List<ListViewItem>();
            String WhichList = (SelectedList == -1) ? LoadPath : Program.ListsPath[SelectedList];

            try
            {
                if (!Directory.Exists(Program.CSFFixedPath))
                {
                    // The "Common SoundFonts" standard directory doesn't exist, create it
                    Directory.CreateDirectory(Program.CSFFixedPath);

                    File.Create(Program.ListsPath[0]);

                    if (!ImportMode) SoundFontListEditor.Delegate.Lis.Items.Clear();
                }

                if (!Directory.Exists(Program.OMFixedPath))
                {
                    // OM's directory doesn't exist, create it
                    Directory.CreateDirectory(Program.OMFixedPath);
                    Directory.CreateDirectory(Program.OMSFPath);
                    Directory.CreateDirectory(Program.DebugDataPath);

                    for (int i = 1; i < Program.ListsPath.Count(); i++)
                        File.Create(Program.ListsPath[i]).Dispose();

                    if (!ImportMode) SoundFontListEditor.Delegate.Lis.Items.Clear();
                }

                if (!File.Exists(WhichList))
                {
                    // List doesn't exist, create it
                    File.Create(WhichList).Dispose();

                    if (!ImportMode) SoundFontListEditor.Delegate.Lis.Items.Clear();
                }

                // Read the list
                try
                {
                    if (!WaitForFile(WhichList))
                    {
                        Program.ShowError(4, "Error while accessing list", "The list might be locked or currently not available for access by OmniMIDI.", null);
                    }

                    MemoryStream MS = new MemoryStream(File.ReadAllBytes(WhichList));
                    using (StreamReader SFR = new StreamReader(MS))
                    {
                        string SF = null;
                        bool AI = false, ES = false, XG = false, PL = true;
                        int BV = -1, PV = -1, DBV = 0, DPV = -1;

                        ListViewItem iSF;

                        if (!ImportMode) SoundFontListEditor.Delegate.Lis.Items.Clear();

                        string L;
                        while ((L = SFR.ReadLine()) != null)
                        {
                            try
                            {
                                if (L.Equals("sf.start")) {
                                    if (AI) continue;

                                    // Start of SoundFont item detected
                                    AI = true;
                                }
                                else if (L.Equals("sf.end")) {
                                    if (!AI) continue;

                                    // Add to the list
                                    FileInfo File = new FileInfo(SF);
                                    iSF = new ListViewItem(new[] {
                                            SF,
                                            BV.ToString(), PV.ToString(), DBV.ToString(), DPV.ToString(), XG ? "Yes" : "No", PL ? "Yes" : "No",
                                             ReturnSoundFontFormat(Path.GetExtension(SF)),
                                             ReturnSoundFontSize(SF, Path.GetExtension(SF), File.Length)
                                        });

                                    iSF.Checked = ES;

                                    iSFs.Add(iSF);

                                    SF = null;
                                    iSF = null;
                                }
                                else if (GetName(L).Equals("sf.path")) {
                                    if (!AI | SF != null) continue;

                                    SF = GetValue(L);
                                }
                                else if (GetName(L).Equals("sf.enabled")) {
                                    if (!AI) continue;

                                    ES = Convert.ToBoolean(Convert.ToInt32(GetValue(L)));
                                }
                                else if (GetName(L).Equals("sf.preload")) {
                                    if (!AI) continue;

                                    PL = Convert.ToBoolean(Convert.ToInt32(GetValue(L)));
                                }
                                else if (GetName(L).Equals("sf.xgdrums")) {
                                    if (!AI) continue;

                                    XG = Convert.ToBoolean(Convert.ToInt32(GetValue(L)));
                                }
                                else if (GetName(L).Equals("sf.srcb")) {
                                    if (!AI) continue;

                                    BV = Convert.ToInt32(GetValue(L));
                                }
                                else if (GetName(L).Equals("sf.srcp")) {
                                    if (!AI) continue;

                                    PV = Convert.ToInt32(GetValue(L));
                                }
                                else if (GetName(L).Equals("sf.desb")) {
                                    if (!AI) continue;

                                    DBV = Convert.ToInt32(GetValue(L));
                                }
                                else if (GetName(L).Equals("sf.desp")) {
                                    if (!AI) continue;

                                    DPV = Convert.ToInt32(GetValue(L));
                                }
                                else if (L.Contains("//") || L.Contains('#') || String.IsNullOrWhiteSpace(L)) continue;
                                else
                                {
                                    try
                                    {
                                        FileInfo file = new FileInfo(StripSFZValues(L));
                                        String IsSFZ = (Path.GetExtension(StripSFZValues(L)) == ".sfz") ? "0" : "-1";

                                        iSF = new ListViewItem(new[] {
                                                StripSFZValues(L),
                                                IsSFZ, IsSFZ, "0", IsSFZ, "No", "Yes",
                                                ReturnSoundFontFormat(Path.GetExtension(StripSFZValues(L))),
                                                ReturnSoundFontSize(StripSFZValues(L), Path.GetExtension(StripSFZValues(L)), file.Length)
                                            });

                                        iSF.Checked = true;

                                        if (!ImportMode) SoundFontListEditor.Delegate.Lis.Items.Add(iSF);
                                    }
                                    catch { }
                                }
                            }
                            catch
                            {
                                iSF = new ListViewItem(new[] {
                                        "Unrecognizable SoundFont",
                                        "0", "0", "0", "0", "No", "No",
                                        "Missing",
                                        "N/A"
                                    });

                                iSF.ForeColor = Color.Red;

                                iSFs.Add(iSF);
                            }
                        }

                        foreach (ListViewItem uiSF in iSFs)
                        {
                            if (CheckSupportedFormat(Path.GetExtension(uiSF.Text)))
                                SoundFontListEditor.Delegate.Lis.Items.Add(uiSF);
                        }                           
                    }
                    MS.Dispose();
                }
                catch (Exception ex1)
                {
                    if (!ImportMode && !SuppressErrors)
                    {
                        DialogResult RES = Program.ShowError(
                            3,
                            "Missing or inaccessible list",
                            "The soundfont list doesn't exist, or OmniMIDI is unable to access it. If you believe the SoundFont is actually missing, press \"Yes\" to let OmniMIDI recreate it, otherwise press \"No\" to retry to load.",
                            ex1
                        );

                        if (RES == DialogResult.Yes)
                        {
                            SoundFontListEditor.Delegate.Lis.Items.Clear();
                            File.Create(WhichList).Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.ShowError(
                    4,
                    "Error while loading list",
                    String.Format("The configurator has encountered an error while trying to parse the following list:\n{0}", WhichList),
                    ex
                );
            }
        }

        public static void SaveList(ref ListViewEx List, int SelectedList, String SavePath)
        {
            UInt32 SFCount = 1;
            String ToPrint = "";
            String WhichList = (SelectedList == -1) ? SavePath : Program.ListsPath[SelectedList];

            ListViewItem[] Items = new ListViewItem[List.Items.Count];
            List.Items.CopyTo(Items, 0);

            StopCheck = true;
            ToPrint += "// SoundFont list\n";
            ToPrint += String.Format("// Last edit date: {0}\n\n", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fffffff tt"));
            foreach (ListViewItem item in Items)
            {
                ToPrint += String.Format("// SoundFont n°{0}\n", SFCount);
                ToPrint += "sf.start\n";
                ToPrint += String.Format("sf.path = {0}\n", item.Text);
                ToPrint += String.Format("sf.enabled = {0}\n", item.Checked ? 1 : 0);
                ToPrint += String.Format("sf.preload = {0}\n", item.SubItems[6].Text.ToLowerInvariant().Equals("yes") ? 1 : 0);
                ToPrint += String.Format("sf.srcb = {0}\n", item.SubItems[1].Text);
                ToPrint += String.Format("sf.srcp = {0}\n", item.SubItems[2].Text);
                ToPrint += String.Format("sf.desb = {0}\n", item.SubItems[3].Text);
                ToPrint += String.Format("sf.desp = {0}\n", item.SubItems[4].Text);
                ToPrint += String.Format("sf.xgdrums = {0}\n", item.SubItems[5].Text.ToLowerInvariant().Equals("yes") ? 1 : 0);
                ToPrint += "sf.end\n\n";
                SFCount++;
            }
            ToPrint += "// Generated by: OmniMIDI";

            CSFCS = ToPrint;
            File.WriteAllText(WhichList, ToPrint, Encoding.UTF8);
            StopCheck = false;

            if (SelectedList != -1)
            {
                int CurList = SelectedList + 1;

                if (Convert.ToInt32(Program.Watchdog.GetValue("currentsflist", 1)) == CurList)
                {
                    if (CurList == 1 | Properties.Settings.Default.AutoLoadList)
                        Program.Watchdog.SetValue(String.Format("rel{0}", CurList), 1, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
        }
    }
}