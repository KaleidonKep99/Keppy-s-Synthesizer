﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OmniMIDIConfigurator.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-9999, -9999")]
        public global::System.Drawing.Point LastWindowPos {
            get {
                return ((global::System.Drawing.Point)(this["LastWindowPos"]));
            }
            set {
                this["LastWindowPos"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-1, -1")]
        public global::System.Drawing.Size LastWindowSize {
            get {
                return ((global::System.Drawing.Size)(this["LastWindowSize"]));
            }
            set {
                this["LastWindowSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int LastListSelected {
            get {
                return ((int)(this["LastListSelected"]));
            }
            set {
                this["LastListSelected"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastImportExportPath {
            get {
                return ((string)(this["LastImportExportPath"]));
            }
            set {
                this["LastImportExportPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastBrowserPath {
            get {
                return ((string)(this["LastBrowserPath"]));
            }
            set {
                this["LastBrowserPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("choose")]
        public string UpdateBranch {
            get {
                return ((string)(this["UpdateBranch"]));
            }
            set {
                this["UpdateBranch"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool PreRelease {
            get {
                return ((bool)(this["PreRelease"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LiveChanges {
            get {
                return ((bool)(this["LiveChanges"]));
            }
            set {
                this["LiveChanges"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoLoadList {
            get {
                return ((bool)(this["AutoLoadList"]));
            }
            set {
                this["AutoLoadList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool PatchInfoShow {
            get {
                return ((bool)(this["PatchInfoShow"]));
            }
            set {
                this["PatchInfoShow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfInt xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <int>-1</int>
  <int>-1</int>
  <int>-1</int>
  <int>-1</int>
  <int>-1</int>
  <int>-1</int>
  <int>-1</int>
  <int>-1</int>
</ArrayOfInt>")]
        public int[] SFColumnsSize {
            get {
                return ((int[])(this["SFColumnsSize"]));
            }
            set {
                this["SFColumnsSize"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfInt xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <int>425</int>
  <int>30</int>
  <int>30</int>
  <int>30</int>
  <int>30</int>
  <int>30</int>
  <int>53</int>
  <int>62</int>
</ArrayOfInt>")]
        public int[] SFColumnsDefSize {
            get {
                return ((int[])(this["SFColumnsDefSize"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://github.com/KeppySoftware/OmniMIDI")]
        public string ProjectLink {
            get {
                return ((string)(this["ProjectLink"]));
            }
        }
    }
}
