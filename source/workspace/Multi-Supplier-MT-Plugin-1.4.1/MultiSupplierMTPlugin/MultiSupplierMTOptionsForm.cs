using MultiSupplierMTPlugin.Forms;
using MultiSupplierMTPlugin.Helpers;
using MultiSupplierMTPlugin.Localized;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using LLH = MultiSupplierMTPlugin.Localized.LocalizedHelper;
using LLK = MultiSupplierMTPlugin.MultiSupplierMTOptionsFormLocalizedKey;
using LLKC = MultiSupplierMTPlugin.Localized.LocalizedKeyCommon;

namespace MultiSupplierMTPlugin
{
    partial class MultiSupplierMTOptionsForm : Form
    {
        private class ComboBoxItem
        {
            public string DisplayText { get; set; }
            public object ValueObj { get; set; }

            public ComboBoxItem(string displayText, object valueObj)
            {
                DisplayText = displayText;
                ValueObj = valueObj;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }


        private MultiSupplierMTOptions _mtOptions;

        private MultiSupplierMTGeneralSettings _mtGeneralSettings;

        private MultiSupplierMTSecureSettings _mtSecureSettings;

        private string _lastProvider;
        private bool _canUpdateLastProvider = true;

        private RequestType _lastRequestType;
        private bool _canUpdateLastRequestType = true;


        public MultiSupplierMTOptionsForm(MultiSupplierMTOptions mtOptions)
        {
            InitializeComponent();

            this._mtOptions = mtOptions;

            this._mtGeneralSettings = mtOptions.GeneralSettings;
            this._mtSecureSettings = mtOptions.SecureSettings;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyTheme();

            Localized();

            LoadOptions();
        }

        private void MultiSupplierMTOptionsForm_Load(object sender, EventArgs e)
        {
            // 主题已在 OnLoad 中应用，这里处理 Load 事件的额外逻辑
        }

        /// <summary>
        /// 应用统一主题样式
        /// </summary>
        private void ApplyTheme()
        {
            ThemeHelper.ApplyFormTheme(this);
            ThemeHelper.ApplyRoundedCorners(this, 8);
            ThemeHelper.ApplyFlatTabStyle(tabControlMain);
            ThemeHelper.ApplyFlatTabStyle(tabControlLimits);
            ThemeHelper.ApplyFlatTabStyle(tabControlCacheStats);
            ThemeHelper.ApplyFlatButtonStyle(buttonOK, isPrimary: true);
            ThemeHelper.ApplyFlatButtonStyle(buttonCancel, isPrimary: false);
            ThemeHelper.ApplyFlatButtonStyle(buttonGithub, isPrimary: false);
            ThemeHelper.ApplyFlatButtonStyle(buttonLoadProviderDefault, isPrimary: false);

            // 覆盖 Designer.cs 硬编码的 White 背景
            foreach (TabPage tp in tabControlMain.TabPages) tp.BackColor = ThemeHelper.CardBg;
            foreach (TabPage tp in tabControlLimits.TabPages) tp.BackColor = ThemeHelper.CardBg;
            foreach (TabPage tp in tabControlCacheStats.TabPages) tp.BackColor = ThemeHelper.CardBg;
        }


        private void Localized()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Text = LLH.G(LLK.Form) + $" v{version}";

            // Tab 标签页
            tabPageProvider.Text = LLH.G(LLK.TabPageProvider);
            tabPageLimits.Text = LLH.G(LLK.TabPageLimits);
            tabPageCacheStats.Text = LLH.G(LLK.TabPageCacheAndStats);

            // Provider 标签页
            linkLabelProvider.Text = LLH.G(LLK.LinkLabelProvider);
            labelRequestType.Text = LLH.G(LLK.LinkLabelRequestType);
            checkBoxShowSupportedOnly.Text = LLH.G(LLK.CheckBoxShowSupportedOnly);

            checkBoxTagsToEnd.Text = LLH.G(LLK.CheckBoxTagsToEnd);
            checkBoxNormalizeWhitespace.Text = LLH.G(LLK.CheckBoxNormalizeWhitespace);

            linkLabelCustomRequestLimit.Text = LLH.G(LLK.LinkLabelCustomRequestLimit);
            labelCustomDisplayName.Text = LLH.G(LLK.LinkLabelCustomDisplayName);
            linkLabelStatsAndLog.Text = LLH.G(LLK.LinkLabelStatsAndLog);
            linkLabelTranslateCache.Text = LLH.G(LLK.LinkLabelTranslateCache);

            // Limits 标签页
            tabPageSizeLimit.Text = LLH.G(LLK.TabPageSizeLimit);
            labelMaxSegmentsPerRequest.Text = LLH.G(LLK.LabelMaxSegmentsPerRequest);
            labelMaxCharactersPerRequest.Text = LLH.G(LLK.LabelMaxCharactersPerRequest);
            toolTip.SetToolTip(numericUpDownMaxSegmentsPerRequest, LLH.G(LLKC.ZeroIndicatesNoLimit));
            toolTip.SetToolTip(numericUpDownMaxCharactersPerRequest, LLH.G(LLKC.ZeroIndicatesNoLimit));

            tabPageRateLimit.Text = LLH.G(LLK.TabPageRateLimit);
            labelMaxRequestsPerWindow.Text = LLH.G(LLK.LabelMaxRequestsPerWindow);
            labelWindowSizeMs.Text = LLH.G(LLK.LabelWindowSizeMs);
            labelRequestSmoothness.Text = LLH.G(LLK.LabelRequestSmoothness);
            toolTip.SetToolTip(numericUpDownMaxRequestsPerWindow, LLH.G(LLKC.ZeroIndicatesNoLimit));
            toolTip.SetToolTip(numericUpDownWindowSizeMs, LLH.G(LLK.WindowSizeMsTip));
            toolTip.SetToolTip(numericUpDownRequestSmoothness, LLH.G(LLK.RequestSmoothnessTip));

            tabPageConcurrencyLimit.Text = LLH.G(LLK.TabPageConcurrencyLimit);
            labelMaxRequestsHold.Text = LLH.G(LLK.LabelMaxRequestsHold);
            toolTip.SetToolTip(numericUpDownMaxRequestsHold, LLH.G(LLKC.ZeroIndicatesNoLimit));

            tabPageRetryLimit.Text = LLH.G(LLK.TabPageRetryLimit);
            labelNumberOfRetries.Text = LLH.G(LLK.LabelNumberOfRetries);
            labelFailedTimeoutMs.Text = LLH.G(LLK.LabelFailedTimeoutMs);
            labelRetryWaitingMs.Text = LLH.G(LLK.LabelRetryWaitingMs);
            toolTip.SetToolTip(numericUpDownNumberOfRetries, LLH.G(LLK.NumberOfRetriesTip));
            toolTip.SetToolTip(numericUpDownFailedTimeoutMs, LLH.G(LLK.FailedTimeoutMsTip));
            toolTip.SetToolTip(numericUpDownRetryWaitingMs, LLH.G(LLK.RetryWaitingMsTip));

            buttonLoadProviderDefault.Text = LLH.G(LLK.ButtonLoadProviderDefault);

            // Cache & Stats 标签页
            tabPageStatistics.Text = LLH.G(LLK.TabPageStatistics);
            labelSuccessRequests.Text = LLH.G(LLK.LabelSuccessRequests);
            labelFailedRequest.Text = LLH.G(LLK.LabelFailedRequest);
            linkLabelResetStats.Text = LLH.G(LLK.LinkLabelResetStats);

            tabPageLogging.Text = LLH.G(LLK.TabPageLogging);
            linkLabelOpenLogFile.Text = LLH.G(LLK.LinkLabelOpenLogFile);
            linkLabelOpenLogDir.Text = LLH.G(LLK.LinkLabelOpenLogDir);
            checkBoxVerboseRuntimeLog.Text = LLH.G(LLK.CheckBoxVerboseRuntimeLog);
            checkBoxApiRequestResponseLog.Text = LLH.G(LLK.CheckBoxApiRequestResponseLog);
            labelLoggingLevel.Text = LLH.G(LLK.LabelLoggingLevel);
            radioButtonDebug.Text = LLH.G(LLK.RadioButtonDebug);
            radioButtonInfo.Text = LLH.G(LLK.RadioButtonInfo);
            radioButtonWarn.Text = LLH.G(LLK.RadioButtonWarn);
            radioButtonError.Text = LLH.G(LLK.RadioButtonError);
            toolTip.SetToolTip(checkBoxVerboseRuntimeLog, LLH.G(LLK.CheckBoxVerboseRuntimeLogTip));
            toolTip.SetToolTip(checkBoxApiRequestResponseLog, LLH.G(LLK.CheckBoxApiRequestResponseLogTip));
            toolTip.SetToolTip(radioButtonDebug, LLH.G(LLK.RadioButtonDebugTip));
            toolTip.SetToolTip(radioButtonInfo, LLH.G(LLK.RadioButtonInfoTip));
            toolTip.SetToolTip(radioButtonWarn, LLH.G(LLK.RadioButtonWarnTip));
            toolTip.SetToolTip(radioButtonError, LLH.G(LLK.RadioButtonErrorTip));

            tabPageCache.Text = LLH.G(LLK.TabPageCache);
            labelCacheCount.Text = LLH.G(LLK.LabelCacheCount);
            linkLabelCleanCache.Text = LLH.G(LLK.LinkLabelCleanCache);

            labelNoBathTip.Text = LLH.G(LLK.LabelNoBathTip);

            buttonOK.Text = LLH.G(LLKC.ButtonOK);
            buttonCancel.Text = LLH.G(LLKC.ButtonCancel);
            buttonGithub.Text = LLH.G(LLKC.ButtonGithub);
        }

        private void LoadOptions()
        {
            // === Provider 标签页 ===
            comboBoxServiceProvider.DisplayMember = "DisplayText";
            comboBoxServiceProvider.ValueMember = "ValueObj";
            var services = GetEnableServices(_mtGeneralSettings.EnableProviders);
            comboBoxServiceProvider.DataSource = new BindingList<ComboBoxItem>(services);
            SelectComboBoxServiceProvider(_mtGeneralSettings.CurrentServiceProvider);
            _lastProvider = (string)comboBoxServiceProvider.SelectedValue;

            comboBoxRequestType.DisplayMember = "DisplayText";
            comboBoxRequestType.ValueMember = "ValueObj";

            var service = ServiceHelper.GetServiceOrFallback(_lastProvider);
            var requestTypes = GetRequestTypes(service.IsXmlSupported, service.IsHtmlSupported);
            comboBoxRequestType.DataSource = new BindingList<ComboBoxItem>(requestTypes);
            SelectComboBoxRequestType(_mtGeneralSettings.RequestType);
            _lastRequestType = (RequestType)comboBoxRequestType.SelectedValue;

            checkBoxTagsToEnd.Checked = _mtGeneralSettings.InsertRequiredTagsToEnd;
            checkBoxNormalizeWhitespace.Checked = _mtGeneralSettings.NormalizeWhitespaceAroundTags;
            SetCheckBoxState(_lastRequestType);

            // Inline: ShowSupportedOnly (from RequestTypeLimit)
            checkBoxShowSupportedOnly.Checked = _mtGeneralSettings.ShowSupportedRequestTypeOnly;

            // Inline: CustomDisplayName (from CustomDisplayName)
            checkBoxCustomDisplayName.Checked = _mtGeneralSettings.EnableCustomDisplayName;
            textBoxCustomDisplayName.Text = _mtGeneralSettings.CustomDisplayName;
            textBoxCustomDisplayName.Enabled = _mtGeneralSettings.EnableCustomDisplayName;

            checkBoxCustomRequestLimit.Checked = _mtGeneralSettings.EnableCustomRequestLimit;
            checkBoxStatsAndLog.Checked = _mtGeneralSettings.EnableStatsAndLog;
            checkBoxTranslateCache.Checked = _mtGeneralSettings.EnableCache;

            // === Limits 标签页（数据从 CustomLimit 迁移）===
            LoadLimitsData();

            // === Cache & Stats 标签页 ===
            LoadStatsAndLogData();

            // 语言选择
            var languages = LLH.GetAvailableLanguages();
            comboBoxLanguages.Items.AddRange(languages);
            comboBoxLanguages.SelectedItem = languages.Contains(LLH.UILanguage) ? LLH.UILanguage : "en-US";

            // 事件绑定
            this.comboBoxServiceProvider.SelectedIndexChanged += new System.EventHandler(this.comboBoxServiceProvider_SelectedIndexChanged);
            this.comboBoxRequestType.SelectedIndexChanged += new System.EventHandler(this.comboBoxRequestType_SelectedIndexChanged);
            this.comboBoxLanguages.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguages_SelectedIndexChanged);
            this.checkBoxShowSupportedOnly.CheckedChanged += new System.EventHandler(this.checkBoxShowSupportedOnly_CheckedChanged);
            this.checkBoxCustomDisplayName.CheckedChanged += new System.EventHandler(this.checkBoxCustomDisplayName_CheckedChanged);

            // 首次加载时刷新 Limits 数据
            RefreshLimitsForProvider(_lastProvider);
        }

        // === Limits 数据加载 ===
        private void LoadLimitsData()
        {
            numericUpDownMaxSegmentsPerRequest.Value = _mtGeneralSettings.MaxSegmentsPerRequest;
            numericUpDownMaxCharactersPerRequest.Value = _mtGeneralSettings.MaxCharactersPerRequest;

            numericUpDownMaxRequestsPerWindow.Value = _mtGeneralSettings.MaxRequestsPerWindow;
            numericUpDownWindowSizeMs.Value = _mtGeneralSettings.WindowSizeMs;
            numericUpDownRequestSmoothness.Value = (decimal)_mtGeneralSettings.RequestSmoothness;

            numericUpDownMaxRequestsHold.Value = _mtGeneralSettings.MaxRequestsHold;

            numericUpDownNumberOfRetries.Value = _mtGeneralSettings.NumberOfRetries;
            numericUpDownFailedTimeoutMs.Value = _mtGeneralSettings.FailedTimeoutMs;
            numericUpDownRetryWaitingMs.Value = _mtGeneralSettings.RetryWaitingMs;
        }

        private void RefreshLimitsForProvider(string providerName)
        {
            var svc = ServiceHelper.GetServiceOrFallback(providerName);
            if (svc.IsBatchSupported)
            {
                numericUpDownMaxSegmentsPerRequest.Enabled = true;
                numericUpDownMaxCharactersPerRequest.Enabled = true;
                labelNoBathTip.Visible = false;
            }
            else
            {
                numericUpDownMaxSegmentsPerRequest.Value = 1;
                numericUpDownMaxCharactersPerRequest.Value = 0;
                numericUpDownMaxSegmentsPerRequest.Enabled = false;
                numericUpDownMaxCharactersPerRequest.Enabled = false;
                labelNoBathTip.Visible = true;
            }
        }

        // === Stats & Log & Cache 数据加载 ===
        private void LoadStatsAndLogData()
        {
            // Statistics
            labelSuccessCountValue.Text = StatsHelper.GetRequestSuccess().ToString();
            labelFailedCountValue.Text = StatsHelper.GetRequestFailed().ToString();

            // Logging
            checkBoxVerboseRuntimeLog.Checked = _mtGeneralSettings.EnableVerboseRuntimeLog;
            checkBoxApiRequestResponseLog.Checked = _mtGeneralSettings.EnableApiRequestResponseLog;

            switch (_mtGeneralSettings.LogLevel)
            {
                case LogLevel.Debug: radioButtonDebug.Checked = true; break;
                case LogLevel.Info: radioButtonInfo.Checked = true; break;
                case LogLevel.Warn: radioButtonWarn.Checked = true; break;
                case LogLevel.Error: radioButtonError.Checked = true; break;
                default: radioButtonInfo.Checked = true; break;
            }

            // Cache
            labelCacheCountValue.Text = CacheHelper.Count().ToString();
        }


        // === Provider Selection ===
        private void comboBoxServiceProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxServiceProvider.SelectedValue is string name)
            {
                var service = ServiceHelper.GetServiceOrFallback(name);
                var requestTypes = GetRequestTypes(service.IsXmlSupported, service.IsHtmlSupported);

                this.comboBoxRequestType.SelectedIndexChanged -= new System.EventHandler(this.comboBoxRequestType_SelectedIndexChanged);
                comboBoxRequestType.DataSource = new BindingList<ComboBoxItem>(requestTypes);
                this.comboBoxRequestType.SelectedIndexChanged += new System.EventHandler(this.comboBoxRequestType_SelectedIndexChanged);

                _canUpdateLastRequestType = false;
                SelectComboBoxRequestType(_lastRequestType);
                _canUpdateLastRequestType = true;

                // Refresh limits tab for the new provider
                RefreshLimitsForProvider(name);

                if (_canUpdateLastProvider)
                {
                    _lastProvider = name;
                    var option = service.ShowConfig();
                    OptionsHelper.SetProviderOptions(name, option);
                }

                buttonOK.Enabled = service.IsAvailable;
            }
        }

        private void comboBoxRequestType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRequestType.SelectedValue is RequestType selectedRequestType)
            {
                if (_canUpdateLastRequestType)
                    _lastRequestType = selectedRequestType;

                SetCheckBoxState(selectedRequestType);
            }
        }

        private void checkBoxShowSupportedOnly_CheckedChanged(object sender, EventArgs e)
        {
            // 实时刷新 RequestType 列表
            _mtGeneralSettings.ShowSupportedRequestTypeOnly = checkBoxShowSupportedOnly.Checked;
            _canUpdateLastProvider = false;
            SelectComboBoxServiceProvider(_lastProvider);
            _canUpdateLastProvider = true;
        }

        private void checkBoxCustomDisplayName_CheckedChanged(object sender, EventArgs e)
        {
            textBoxCustomDisplayName.Enabled = checkBoxCustomDisplayName.Checked;
        }

        private void SetCheckBoxState(RequestType selectedRequestType)
        {
            bool checkBoxTagsToEndEnabled =
                        selectedRequestType == RequestType.Plaintext ||
                        selectedRequestType == RequestType.OnlyFormattingWithXml ||
                        selectedRequestType == RequestType.OnlyFormattingWithHtml;

            checkBoxTagsToEnd.Enabled = checkBoxTagsToEndEnabled;
            checkBoxNormalizeWhitespace.Enabled = !checkBoxTagsToEndEnabled;

            checkBoxTagsToEndFake.Visible = !checkBoxTagsToEndEnabled;
            checkBoxNormalizeWhitespaceFake.Visible = checkBoxTagsToEndEnabled;
        }


        private ComboBoxItem[] GetEnableServices(string[] enableProviders)
        {
            var services = enableProviders
                .Select(name => ServiceHelper.TryGetService(name, out var s) ? s : null)
                .Where(s => s != null)
                .OrderBy(s => !s.IsBuiltIn)
                .ThenBy(s => s.IsLLM)
                .ThenBy(s => ServiceLocalizedNameHelper.GetWithSuffix(s.UniqueName, s.IsLLM, s.IsBuiltIn), new NaturalSortComparer())
                .Select(s => new ComboBoxItem(ServiceLocalizedNameHelper.GetWithSuffix(s.UniqueName, s.IsLLM, s.IsBuiltIn), s.UniqueName))
                .ToArray();

            if (services.Length == 0)
            {
                var service = ServiceHelper.GetServiceOrFallback(ServiceNames.Microsoft_BuiltIn);
                var item = new ComboBoxItem(ServiceLocalizedNameHelper.GetWithSuffix(service.UniqueName, service.IsLLM, service.IsBuiltIn), service.UniqueName);
                services = new ComboBoxItem[] { item };
            }

            return services;
        }

        private void ReloadComboBoxServiceProvider()
        {
            var services = GetEnableServices(_mtGeneralSettings.EnableProviders);

            this.comboBoxServiceProvider.SelectedIndexChanged -= new System.EventHandler(this.comboBoxServiceProvider_SelectedIndexChanged);
            comboBoxServiceProvider.DataSource = new BindingList<ComboBoxItem>(services);
            this.comboBoxServiceProvider.SelectedIndexChanged += new System.EventHandler(this.comboBoxServiceProvider_SelectedIndexChanged);

            _canUpdateLastProvider = false;
            SelectComboBoxServiceProvider(_lastProvider);
            _canUpdateLastProvider = true;
        }

        private ComboBoxItem[] GetRequestTypes(bool xmlSupported, bool htmlSupported)
        {
            List<ComboBoxItem> requstsTypes = new List<ComboBoxItem>();

            requstsTypes.Add(new ComboBoxItem(LLH.G(LLK.ComboBoxRequestType_Plaintext), RequestType.Plaintext));

            if (xmlSupported || !_mtGeneralSettings.ShowSupportedRequestTypeOnly)
            {
                requstsTypes.Add(new ComboBoxItem(LLH.G(LLK.ComboBoxRequestType_OnlyFormattingWithXml), RequestType.OnlyFormattingWithXml));
            }
            if (htmlSupported || !_mtGeneralSettings.ShowSupportedRequestTypeOnly)
            {
                requstsTypes.Add(new ComboBoxItem(LLH.G(LLK.ComboBoxRequestType_OnlyFormattingWithHtml), RequestType.OnlyFormattingWithHtml));
            }

            if (xmlSupported || !_mtGeneralSettings.ShowSupportedRequestTypeOnly)
            {
                requstsTypes.Add(new ComboBoxItem(LLH.G(LLK.ComboBoxRequestType_BothFormattingAndTagsWithXml), RequestType.BothFormattingAndTagsWithXml));
            }
            if (htmlSupported || !_mtGeneralSettings.ShowSupportedRequestTypeOnly)
            {
                requstsTypes.Add(new ComboBoxItem(LLH.G(LLK.ComboBoxRequestType_BothFormattingAndTagsWithHtml), RequestType.BothFormattingAndTagsWithHtml));
            }

            return requstsTypes.ToArray();
        }

        private void SelectComboBoxServiceProvider(string name)
        {
            foreach (ComboBoxItem item in comboBoxServiceProvider.Items)
            {
                if ((string)item.ValueObj == name)
                {
                    comboBoxServiceProvider.SelectedIndex = -1;
                    comboBoxServiceProvider.SelectedItem = item;
                    return;
                }
            }

            if (comboBoxServiceProvider.Items.Count > 0)
            {
                comboBoxServiceProvider.SelectedIndex = -1;
                comboBoxServiceProvider.SelectedIndex = 0;
            }
        }

        private void SelectComboBoxRequestType(RequestType requestType)
        {
            foreach (ComboBoxItem item in comboBoxRequestType.Items)
            {
                if ((RequestType)item.ValueObj == requestType)
                {
                    comboBoxRequestType.SelectedIndex = -1;
                    comboBoxRequestType.SelectedItem = item;
                    return;
                }
            }

            if (comboBoxRequestType.Items.Count > 0)
            {
                comboBoxRequestType.SelectedIndex = -1;
                comboBoxRequestType.SelectedIndex = 0;
            }
        }

        // === Limits Tab Events ===
        private void buttonLoadProviderDefault_Click(object sender, EventArgs e)
        {
            var currentService = ServiceHelper.GetServiceOrFallback(_lastProvider);

            numericUpDownMaxSegmentsPerRequest.Value = currentService.MaxSegments;
            numericUpDownMaxCharactersPerRequest.Value = currentService.MaxCharacters;

            numericUpDownMaxRequestsPerWindow.Value = currentService.MaxQueriesPerWindow;
            numericUpDownWindowSizeMs.Value = currentService.WindowSizeMs;
            numericUpDownRequestSmoothness.Value = (decimal)currentService.Smoothness;

            numericUpDownMaxRequestsHold.Value = currentService.MaxThreadHold;

            numericUpDownNumberOfRetries.Value = currentService.NumberOfRetries;
            numericUpDownFailedTimeoutMs.Value = currentService.FailedTimeoutMs;
            numericUpDownRetryWaitingMs.Value = currentService.RetryWaitingMs;
        }

        // === Stats events ===
        private void linkLabelResetStats_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StatsHelper.Reset();
            labelSuccessCountValue.Text = "0";
            labelFailedCountValue.Text = "0";
        }

        // === Logging events ===
        private void linkLabelOpenLogFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (LoggingHelper.TryGetLogFilePath(out var logfile))
                    Process.Start(Path.GetFullPath(logfile));
                else
                    throw new Exception("logger no init or init fail");
            }
            catch
            {
                MessageBox.Show(LLH.G(LLK.OpenLogDirFailMsg), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkLabelOpenLogDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string logDir = System.IO.Path.Combine(_mtGeneralSettings.DataDir, "Log");
                if (!System.IO.Directory.Exists(logDir))
                    System.IO.Directory.CreateDirectory(logDir);
                Process.Start(logDir);
            }
            catch
            {
                MessageBox.Show(LLH.G(LLK.OpenLogDirFailMsg), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // === Cache events ===
        private void linkLabelCleanCache_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var confirm = MessageBox.Show(
                LLH.G(LLK.MessageBoxConfirmCleanTip), "",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.OK)
            {
                CacheHelper.Clear();
                labelCacheCountValue.Text = "0";
            }
        }

        // === CustomRequestLimit link — jumps to Limits tab ===
        private void linkLabelCustomRequestLimit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tabControlMain.SelectedTab = tabPageLimits;
        }

        // === StatsAndLog link — switches to inline Cache & Stats tab ===
        private void linkLabelStatsAndLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tabControlMain.SelectedTab = tabPageCacheStats;
            tabControlCacheStats.SelectedTab = tabPageStatistics;
        }

        // === TranslateCache link — switches to Cache tab ===
        private void linkLabelTranslateCache_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tabControlMain.SelectedTab = tabPageCacheStats;
            tabControlCacheStats.SelectedTab = tabPageCache;
        }

        // === Provider management ===
        private void linkLabelProvider_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var form = new ProvidersManage(_mtGeneralSettings, _mtSecureSettings))
            {
                form.ShowDialog();

                if (form.DialogResult == DialogResult.OK)
                {
                    ServiceHelper.Init(_mtGeneralSettings.CustomOpenAICompatibleServiceInfos);
                    ReloadComboBoxServiceProvider();
                }
            }
        }

        // === Language ===
        private void comboBoxLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            LLH.Init((string)comboBoxLanguages.SelectedItem);
            Localized();
            ReloadComboBoxServiceProvider();
        }

        private void buttonGithub_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin");
            }
            catch
            {
                // do nothing
            }
        }


        private void MultiSupplierMTOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            try
            {
                // Provider tab
                _mtGeneralSettings.CurrentServiceProvider = (string)comboBoxServiceProvider.SelectedValue;

                _mtGeneralSettings.RequestType = (RequestType)comboBoxRequestType.SelectedValue;

                _mtGeneralSettings.InsertRequiredTagsToEnd = checkBoxTagsToEnd.Checked;
                _mtGeneralSettings.NormalizeWhitespaceAroundTags = checkBoxNormalizeWhitespace.Checked;

                _mtGeneralSettings.ShowSupportedRequestTypeOnly = checkBoxShowSupportedOnly.Checked;

                _mtGeneralSettings.EnableCustomRequestLimit = checkBoxCustomRequestLimit.Checked;
                _mtGeneralSettings.EnableCustomDisplayName = checkBoxCustomDisplayName.Checked;
                _mtGeneralSettings.CustomDisplayName = textBoxCustomDisplayName.Text;
                _mtGeneralSettings.EnableStatsAndLog = checkBoxStatsAndLog.Checked;
                _mtGeneralSettings.EnableCache = checkBoxTranslateCache.Checked;

                // Limits tab
                _mtGeneralSettings.MaxSegmentsPerRequest = (int)numericUpDownMaxSegmentsPerRequest.Value;
                _mtGeneralSettings.MaxCharactersPerRequest = (int)numericUpDownMaxCharactersPerRequest.Value;

                _mtGeneralSettings.MaxRequestsPerWindow = (int)numericUpDownMaxRequestsPerWindow.Value;
                _mtGeneralSettings.WindowSizeMs = (int)numericUpDownWindowSizeMs.Value;
                _mtGeneralSettings.RequestSmoothness = (double)numericUpDownRequestSmoothness.Value;

                _mtGeneralSettings.MaxRequestsHold = (int)numericUpDownMaxRequestsHold.Value;

                _mtGeneralSettings.NumberOfRetries = (int)numericUpDownNumberOfRetries.Value;
                _mtGeneralSettings.FailedTimeoutMs = (int)numericUpDownFailedTimeoutMs.Value;
                _mtGeneralSettings.RetryWaitingMs = (int)numericUpDownRetryWaitingMs.Value;

                // Logging
                _mtGeneralSettings.EnableVerboseRuntimeLog = checkBoxVerboseRuntimeLog.Checked;
                _mtGeneralSettings.EnableApiRequestResponseLog = checkBoxApiRequestResponseLog.Checked;

                if (radioButtonDebug.Checked) _mtGeneralSettings.LogLevel = LogLevel.Debug;
                else if (radioButtonInfo.Checked) _mtGeneralSettings.LogLevel = LogLevel.Info;
                else if (radioButtonWarn.Checked) _mtGeneralSettings.LogLevel = LogLevel.Warn;
                else if (radioButtonError.Checked) _mtGeneralSettings.LogLevel = LogLevel.Error;

                LoggingHelper.MinLogLevel = _mtGeneralSettings.LogLevel;
                LoggingHelper.EnableVerboseRuntimeLog = _mtGeneralSettings.EnableVerboseRuntimeLog;
                LoggingHelper.EnableApiRequestResponseLog = _mtGeneralSettings.EnableApiRequestResponseLog;

                _mtGeneralSettings.UILanguage = (string)comboBoxLanguages.SelectedItem;

                LoggingHelper.Enable = _mtGeneralSettings.EnableStatsAndLog;

                // FreeOpenSourceTip
                if (!_mtGeneralSettings.NeverShowTip && Math.Abs(_mtGeneralSettings.RuningTimes) % 10 == 0)
                {
                    using (var form = new FreeOpenSourceTip(_mtGeneralSettings, _mtSecureSettings))
                    {
                        form.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                // 防止 LoadOptions 失败后 FormClosing 空引用崩溃
                System.Diagnostics.Debug.WriteLine("FormClosing error: " + ex.Message);
            }
        }
    }

    class MultiSupplierMTOptionsFormLocalizedKey : LocalizedKeyBase
    {
        public MultiSupplierMTOptionsFormLocalizedKey(string name) : base(name)
        {
        }

        static MultiSupplierMTOptionsFormLocalizedKey()
        {
            AutoInit<MultiSupplierMTOptionsFormLocalizedKey>();
        }

        // Form
        [LocalizedValue("4ec208c3-410c-4daa-8cb7-8a1dbc8d9b13", "Multi Supplier MT Plugin", "多提供商机器翻译插件")]
        public static MultiSupplierMTOptionsFormLocalizedKey Form { get; private set; }

        // Tab pages (new)
        [LocalizedValue("a1000001-0000-0000-0000-000000000001", "Provider", "提供商")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageProvider { get; private set; }

        [LocalizedValue("a1000001-0000-0000-0000-000000000002", "Limits", "限制")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageLimits { get; private set; }

        [LocalizedValue("a1000001-0000-0000-0000-000000000003", "Cache && Stats", "缓存与统计")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageCacheAndStats { get; private set; }

        // Provider tab
        [LocalizedValue("d5b68680-860a-43b6-a34f-f9b06672361c", "Provider", "提供商")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelProvider { get; private set; }

        [LocalizedValue("98f52dda-407e-4558-bb5b-c5d1be9bae2a", "Request Type", "请求类型")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelRequestType { get; private set; }

        // New: ShowSupportedOnly inline (from RequestTypeLimit)
        [LocalizedValue("a1000002-0000-0000-0000-000000000001", "Only show provider supported request type", "仅显示提供商支持的请求类型")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxShowSupportedOnly { get; private set; }

        [LocalizedValue("4f5424da-0e9b-4248-9a34-68846494ba2a", "Insert Required Tags To End", "将原文中的内联标签追加到译文后")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxTagsToEnd { get; private set; }

        [LocalizedValue("c2c08303-5d5d-4341-84cf-0b4c7eb61a7f", "Normalize Whitespace Around Tags", "归一化译文中内联标签旁边的空格")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxNormalizeWhitespace { get; private set; }

        [LocalizedValue("f2a12541-8aef-4ef2-8544-87762cb08c36", "Enable Custom Request Limit", "启用自定义请求限制")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelCustomRequestLimit { get; private set; }

        [LocalizedValue("bac7187e-1367-4ffb-a8e9-439d30267790", "Enable Custom Display Name", "启用自定义显示名称")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelCustomDisplayName { get; private set; }

        [LocalizedValue("63604532-cd5c-4ef8-af3d-3540dc6e3acc", "Enable Stats And Log", "启用统计和日志")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelStatsAndLog { get; private set; }

        [LocalizedValue("73f9781d-d68f-45fa-bcc4-032e077895ed", "Enable Translate Cache", "启用翻译缓存")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelTranslateCache { get; private set; }

        // Request type combobox items
        [LocalizedValue("eb2b3011-77f5-498c-b3eb-15719ec439be", "Plaintext", "仅纯文本")]
        public static MultiSupplierMTOptionsFormLocalizedKey ComboBoxRequestType_Plaintext { get; private set; }

        [LocalizedValue("f926c81f-7e8c-4d93-819a-90d67f61e8f9", "Include Formatting With Xml", "包括格式标记，（用 Xml 表示）")]
        public static MultiSupplierMTOptionsFormLocalizedKey ComboBoxRequestType_OnlyFormattingWithXml { get; private set; }

        [LocalizedValue("ed3b6ee6-f020-4f97-ae01-b5e3f139cd60", "Include Formatting With Html", "包括格式标记，（用 Html 表示）")]
        public static MultiSupplierMTOptionsFormLocalizedKey ComboBoxRequestType_OnlyFormattingWithHtml { get; private set; }

        [LocalizedValue("095b951d-6052-4a60-a235-7ef4c08a31ef", "Include Formatting And Tags With Xml", "包括格式标记和内联标签，（用 Xml 表示）")]
        public static MultiSupplierMTOptionsFormLocalizedKey ComboBoxRequestType_BothFormattingAndTagsWithXml { get; private set; }

        [LocalizedValue("7699f7c8-f881-4fc1-b3d6-26f6fb3886ad", "Include Formatting And Tags With Html", "包括格式标记和内联标签，（用 Html 表示）")]
        public static MultiSupplierMTOptionsFormLocalizedKey ComboBoxRequestType_BothFormattingAndTagsWithHtml { get; private set; }

        // Limits tab (from CustomLimit)
        [LocalizedValue("22031b6b-eeb0-4599-b0d9-1e3641668875", "Size Limit", "大小限制")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageSizeLimit { get; private set; }

        [LocalizedValue("c72a9c7b-dceb-4c54-b62c-64738f86033f", "Max Segments Per Request", "每请求最大句段数")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelMaxSegmentsPerRequest { get; private set; }

        [LocalizedValue("a1000003-0001-0000-0000-000000000001", "Max Characters Per Request", "每请求最大字符数")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelMaxCharactersPerRequest { get; private set; }

        [LocalizedValue("6cbdf74a-8412-4c20-9d5b-f3eda4fc7f26", "Selected provider does not support batch translation!", "选择的提供商不支持批量翻译！")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelNoBathTip { get; private set; }

        [LocalizedValue("8d3c7ac2-b063-4de9-9d17-233d4a4f46ae", "Rate Limit", "速率限制")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageRateLimit { get; private set; }

        [LocalizedValue("849ae12e-3897-4247-afb0-e3419ec9bbd9", "Max Requests Per Window", "每窗口最大请求数")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelMaxRequestsPerWindow { get; private set; }

        [LocalizedValue("d1cb71bd-5def-4dae-99d1-697cfe21aaf7", "Window Size Ms", "窗口大小（毫秒）")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelWindowSizeMs { get; private set; }

        [LocalizedValue("9314b2e9-8bbb-43cb-96c9-205da152ee77", "Request Smoothness", "请求平滑度")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelRequestSmoothness { get; private set; }

        [LocalizedValue("fbfb46fd-f5f3-41f9-ac20-8d182feeeec0", "Concurrency Limit", "并发限制")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageConcurrencyLimit { get; private set; }

        [LocalizedValue("b12804ea-fc85-4a47-a2ea-a19c1bb69474", "Max Requests Hold", "请求最大保持数")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelMaxRequestsHold { get; private set; }

        [LocalizedValue("0c339fcb-14b1-44a5-81b7-74c57492d7ac", "Retry Limit", "重试限制")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageRetryLimit { get; private set; }

        [LocalizedValue("fcc7de38-9e72-4994-b329-0314c318fd82", "Number Of Retries", "重试最大次数")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelNumberOfRetries { get; private set; }

        [LocalizedValue("33beee65-d86f-4943-ab77-6b83d2a6e480", "Failed Timeout Ms", "超时失败（毫秒）")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelFailedTimeoutMs { get; private set; }

        [LocalizedValue("98d13160-5171-43ee-9d99-606f9c349985", "Retry Waiting Ms", "重试等待（毫秒）")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelRetryWaitingMs { get; private set; }

        [LocalizedValue("16aba4ce-e67e-46c2-82f0-fd4edd64ca1a", "Load Provider Default", "加载提供商默认值")]
        public static MultiSupplierMTOptionsFormLocalizedKey ButtonLoadProviderDefault { get; private set; }

        // Tooltips
        [LocalizedValue("bf5b7c53-32ce-4dd2-b735-efb04ef49ef4", "The value must be greater than zero", "值必需大于零")]
        public static MultiSupplierMTOptionsFormLocalizedKey WindowSizeMsTip { get; private set; }

        [LocalizedValue("84dc7869-305f-4d84-b180-17fee65546f9", "The larger the value, the smoother the request", "值越大越平滑")]
        public static MultiSupplierMTOptionsFormLocalizedKey RequestSmoothnessTip { get; private set; }

        [LocalizedValue("24177281-20be-4233-9ec0-9d9e7f9eba23", "Zero means no retry", "零代表不重试")]
        public static MultiSupplierMTOptionsFormLocalizedKey NumberOfRetriesTip { get; private set; }

        [LocalizedValue("8c03604e-5df7-4e9c-9eac-fa10397f38e5", "Zero means no timeout", "零代表不超时")]
        public static MultiSupplierMTOptionsFormLocalizedKey FailedTimeoutMsTip { get; private set; }

        [LocalizedValue("f8386f74-5805-4462-936c-8cdcb8e0fd51", "Zero means no waiting", "零代表不等待")]
        public static MultiSupplierMTOptionsFormLocalizedKey RetryWaitingMsTip { get; private set; }

        // Statistics (from StatsAndLog)
        [LocalizedValue("a2000001-0000-0000-0000-000000000001", "Statistics", "统计")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageStatistics { get; private set; }

        [LocalizedValue("a2000001-0000-0000-0000-000000000002", "Success Requests", "成功请求")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelSuccessRequests { get; private set; }

        [LocalizedValue("a2000001-0000-0000-0000-000000000003", "Failed Requests", "失败请求")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelFailedRequest { get; private set; }

        [LocalizedValue("a2000001-0000-0000-0000-000000000004", "Reset Stats", "重置统计")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelResetStats { get; private set; }

        // Logging (from StatsAndLog)
        [LocalizedValue("a2000002-0000-0000-0000-000000000001", "Logging", "日志")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageLogging { get; private set; }

        [LocalizedValue("a2000002-0000-0000-0000-000000000002", "Open Log File", "打开日志文件")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelOpenLogFile { get; private set; }

        [LocalizedValue("a2000002-0000-0000-0000-000000000003", "Open Log Dir", "打开日志目录")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelOpenLogDir { get; private set; }

        [LocalizedValue("a2000002-0000-0000-0000-000000000004", "Detailed runtime log", "详细运行时日志")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxVerboseRuntimeLog { get; private set; }

        [LocalizedValue("a2000002-0000-0000-0000-000000000005", "API request/response log", "API请求/响应日志")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxApiRequestResponseLog { get; private set; }

        [LocalizedValue("a2000002-0000-0000-0000-000000000006", "Logging Level", "日志级别")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelLoggingLevel { get; private set; }

        // Cache (from TranslateCache)
        [LocalizedValue("a2000003-0000-0000-0000-000000000001", "Cache", "缓存")]
        public static MultiSupplierMTOptionsFormLocalizedKey TabPageCache { get; private set; }

        [LocalizedValue("a2000003-0000-0000-0000-000000000002", "Cache Count:", "缓存条数：")]
        public static MultiSupplierMTOptionsFormLocalizedKey LabelCacheCount { get; private set; }

        [LocalizedValue("a2000003-0000-0000-0000-000000000003", "Clean Cache", "清空缓存")]
        public static MultiSupplierMTOptionsFormLocalizedKey LinkLabelCleanCache { get; private set; }

        [LocalizedValue("a2000003-0000-0000-0000-000000000004", "It cannot be restored after clearing. Continue?", "清空后将无法恢复，确定要清空吗？")]
        public static MultiSupplierMTOptionsFormLocalizedKey MessageBoxConfirmCleanTip { get; private set; }

        // Logging tooltips (from StatsAndLog)
        [LocalizedValue("effbf444-64cf-4773-a3f4-0e02d35e0ddd", "Record the most information", "记录最多的信息")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonDebugTip { get; private set; }

        [LocalizedValue("6c1aa3db-b379-43f1-ab06-c9cfad73d6fd", "Record more information", "记录较多的信息")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonInfoTip { get; private set; }

        [LocalizedValue("b6e5af4a-e8dc-4e35-998a-9de5b5d6b41d", "Record less information", "记录较少的信息")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonWarnTip { get; private set; }

        [LocalizedValue("1c58ed5f-8053-4065-9762-a2d3ecc09a6f", "Record the least information", "记录最少的信息")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonErrorTip { get; private set; }

        [LocalizedValue("2ad9f754-e163-4db7-9084-0406e5ef2872", "Record richer execution details such as cache, batching, waiting, retry, and prompt summaries.", "记录更详细的运行信息，例如缓存、分批、等待、重试和提示词摘要。")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxVerboseRuntimeLogTip { get; private set; }

        [LocalizedValue("1af8403d-bf63-40c2-9b52-a8545d6332a3", "Record raw AI API requests and responses, including source text and AI output. Sensitive headers such as API keys are redacted.", "记录发送到 AI API 的原始请求与响应，包括原文和 AI 输出。API Key 等敏感请求头会自动脱敏。")]
        public static MultiSupplierMTOptionsFormLocalizedKey CheckBoxApiRequestResponseLogTip { get; private set; }

        [LocalizedValue("500bd562-bc5a-46ca-aa41-f61a62c6aaf7", "Debug", "调试")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonDebug { get; private set; }

        [LocalizedValue("4e23b8ef-f70a-4fbd-9f1a-635b00e224db", "Info", "信息")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonInfo { get; private set; }

        [LocalizedValue("670e883c-1f15-4185-bda5-ce9d532f1f1a", "Warn", "警告")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonWarn { get; private set; }

        [LocalizedValue("049c3db8-4bb0-40b2-88e2-31d938ef90e9", "Error", "错误")]
        public static MultiSupplierMTOptionsFormLocalizedKey RadioButtonError { get; private set; }

        [LocalizedValue("9d5ea46e-76d8-4ef4-b0fd-b4494bbf9ac1", "Dir create or open fail", "目录创建或打开失败")]
        public static MultiSupplierMTOptionsFormLocalizedKey OpenLogDirFailMsg { get; private set; }
    }
}
