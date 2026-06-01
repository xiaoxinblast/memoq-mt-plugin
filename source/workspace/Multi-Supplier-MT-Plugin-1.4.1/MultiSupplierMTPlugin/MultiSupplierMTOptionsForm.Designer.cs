namespace MultiSupplierMTPlugin
{
    partial class MultiSupplierMTOptionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonGithub = new System.Windows.Forms.Button();
            this.comboBoxLanguages = new System.Windows.Forms.ComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);

            // === 主 TabControl ===
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageProvider = new System.Windows.Forms.TabPage();
            this.tabPageLimits = new System.Windows.Forms.TabPage();
            this.tabPageCacheStats = new System.Windows.Forms.TabPage();

            // === Provider 标签页控件 ===
            this.linkLabelProvider = new System.Windows.Forms.LinkLabel();
            this.comboBoxServiceProvider = new System.Windows.Forms.ComboBox();
            this.labelRequestType = new System.Windows.Forms.Label();
            this.comboBoxRequestType = new System.Windows.Forms.ComboBox();
            this.checkBoxShowSupportedOnly = new System.Windows.Forms.CheckBox();
            this.checkBoxTagsToEnd = new System.Windows.Forms.CheckBox();
            this.checkBoxNormalizeWhitespace = new System.Windows.Forms.CheckBox();
            this.checkBoxTagsToEndFake = new System.Windows.Forms.CheckBox();
            this.checkBoxNormalizeWhitespaceFake = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomRequestLimit = new System.Windows.Forms.CheckBox();
            this.linkLabelCustomRequestLimit = new System.Windows.Forms.LinkLabel();
            this.checkBoxCustomDisplayName = new System.Windows.Forms.CheckBox();
            this.labelCustomDisplayName = new System.Windows.Forms.Label();
            this.textBoxCustomDisplayName = new System.Windows.Forms.TextBox();
            this.checkBoxStatsAndLog = new System.Windows.Forms.CheckBox();
            this.linkLabelStatsAndLog = new System.Windows.Forms.LinkLabel();
            this.checkBoxTranslateCache = new System.Windows.Forms.CheckBox();
            this.linkLabelTranslateCache = new System.Windows.Forms.LinkLabel();

            // === Limits 标签页控件（从 CustomLimit 迁移）===
            this.buttonLoadProviderDefault = new System.Windows.Forms.Button();
            this.tabControlLimits = new System.Windows.Forms.TabControl();
            this.tabPageSizeLimit = new System.Windows.Forms.TabPage();
            this.tabPageRateLimit = new System.Windows.Forms.TabPage();
            this.tabPageConcurrencyLimit = new System.Windows.Forms.TabPage();
            this.tabPageRetryLimit = new System.Windows.Forms.TabPage();
            this.labelMaxSegmentsPerRequest = new System.Windows.Forms.Label();
            this.numericUpDownMaxSegmentsPerRequest = new System.Windows.Forms.NumericUpDown();
            this.labelMaxCharactersPerRequest = new System.Windows.Forms.Label();
            this.numericUpDownMaxCharactersPerRequest = new System.Windows.Forms.NumericUpDown();
            this.labelNoBathTip = new System.Windows.Forms.Label();
            this.labelMaxRequestsPerWindow = new System.Windows.Forms.Label();
            this.numericUpDownMaxRequestsPerWindow = new System.Windows.Forms.NumericUpDown();
            this.labelWindowSizeMs = new System.Windows.Forms.Label();
            this.numericUpDownWindowSizeMs = new System.Windows.Forms.NumericUpDown();
            this.labelRequestSmoothness = new System.Windows.Forms.Label();
            this.numericUpDownRequestSmoothness = new System.Windows.Forms.NumericUpDown();
            this.labelMaxRequestsHold = new System.Windows.Forms.Label();
            this.numericUpDownMaxRequestsHold = new System.Windows.Forms.NumericUpDown();
            this.labelNumberOfRetries = new System.Windows.Forms.Label();
            this.numericUpDownNumberOfRetries = new System.Windows.Forms.NumericUpDown();
            this.labelFailedTimeoutMs = new System.Windows.Forms.Label();
            this.numericUpDownFailedTimeoutMs = new System.Windows.Forms.NumericUpDown();
            this.labelRetryWaitingMs = new System.Windows.Forms.Label();
            this.numericUpDownRetryWaitingMs = new System.Windows.Forms.NumericUpDown();

            // === Cache & Stats 标签页控件（嵌套 TabControl）===
            this.tabControlCacheStats = new System.Windows.Forms.TabControl();
            this.tabPageStatistics = new System.Windows.Forms.TabPage();
            this.tabPageLogging = new System.Windows.Forms.TabPage();
            this.tabPageCache = new System.Windows.Forms.TabPage();
            this.labelSuccessRequests = new System.Windows.Forms.Label();
            this.labelSuccessCountValue = new System.Windows.Forms.Label();
            this.labelFailedRequest = new System.Windows.Forms.Label();
            this.labelFailedCountValue = new System.Windows.Forms.Label();
            this.linkLabelResetStats = new System.Windows.Forms.LinkLabel();
            this.linkLabelOpenLogFile = new System.Windows.Forms.LinkLabel();
            this.linkLabelOpenLogDir = new System.Windows.Forms.LinkLabel();
            this.checkBoxVerboseRuntimeLog = new System.Windows.Forms.CheckBox();
            this.checkBoxApiRequestResponseLog = new System.Windows.Forms.CheckBox();
            this.labelLoggingLevel = new System.Windows.Forms.Label();
            this.radioButtonDebug = new System.Windows.Forms.RadioButton();
            this.radioButtonInfo = new System.Windows.Forms.RadioButton();
            this.radioButtonWarn = new System.Windows.Forms.RadioButton();
            this.radioButtonError = new System.Windows.Forms.RadioButton();
            this.labelCacheCount = new System.Windows.Forms.Label();
            this.labelCacheCountValue = new System.Windows.Forms.Label();
            this.linkLabelCleanCache = new System.Windows.Forms.LinkLabel();

            // Suspend all layouts
            this.tabControlMain.SuspendLayout();
            this.tabPageProvider.SuspendLayout();
            this.tabPageLimits.SuspendLayout();
            this.tabPageCacheStats.SuspendLayout();
            this.tabControlLimits.SuspendLayout();
            this.tabPageSizeLimit.SuspendLayout();
            this.tabPageRateLimit.SuspendLayout();
            this.tabPageConcurrencyLimit.SuspendLayout();
            this.tabPageRetryLimit.SuspendLayout();
            this.tabControlCacheStats.SuspendLayout();
            this.tabPageStatistics.SuspendLayout();
            this.tabPageLogging.SuspendLayout();
            this.tabPageCache.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSegmentsPerRequest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharactersPerRequest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxRequestsPerWindow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWindowSizeMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRequestSmoothness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxRequestsHold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfRetries)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFailedTimeoutMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRetryWaitingMs)).BeginInit();
            this.SuspendLayout();

            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(286, 415);
            this.buttonOK.Size = new System.Drawing.Size(100, 27);
            this.buttonOK.TabIndex = 100;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = false;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(393, 415);
            this.buttonCancel.Size = new System.Drawing.Size(100, 27);
            this.buttonCancel.TabIndex = 101;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            //
            // buttonGithub
            //
            this.buttonGithub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGithub.Location = new System.Drawing.Point(500, 415);
            this.buttonGithub.Size = new System.Drawing.Size(100, 27);
            this.buttonGithub.TabIndex = 102;
            this.buttonGithub.Text = "&Github";
            this.buttonGithub.UseVisualStyleBackColor = false;
            this.buttonGithub.Click += new System.EventHandler(this.buttonGithub_Click);
            //
            // comboBoxLanguages
            //
            this.comboBoxLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguages.FormattingEnabled = true;
            this.comboBoxLanguages.Location = new System.Drawing.Point(12, 417);
            this.comboBoxLanguages.Size = new System.Drawing.Size(150, 23);
            this.comboBoxLanguages.TabIndex = 99;
            //
            // toolTip (tooltips kept for limits tab)
            //
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 100;

            // ============ TAB CONTROL MAIN ============
            //
            // tabControlMain
            //
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPageProvider);
            this.tabControlMain.Controls.Add(this.tabPageLimits);
            this.tabControlMain.Controls.Add(this.tabPageCacheStats);
            this.tabControlMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControlMain.ItemSize = new System.Drawing.Size(120, 32);
            this.tabControlMain.Location = new System.Drawing.Point(8, 8);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(594, 400);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 0;
            //
            // tabPageProvider
            //
            // tabPageProvider backcolor set by theme
            this.tabPageProvider.Controls.Add(this.linkLabelProvider);
            this.tabPageProvider.Controls.Add(this.comboBoxServiceProvider);
            this.tabPageProvider.Controls.Add(this.labelRequestType);
            this.tabPageProvider.Controls.Add(this.comboBoxRequestType);
            this.tabPageProvider.Controls.Add(this.checkBoxShowSupportedOnly);
            this.tabPageProvider.Controls.Add(this.checkBoxTagsToEnd);
            this.tabPageProvider.Controls.Add(this.checkBoxNormalizeWhitespace);
            this.tabPageProvider.Controls.Add(this.checkBoxTagsToEndFake);
            this.tabPageProvider.Controls.Add(this.checkBoxNormalizeWhitespaceFake);
            this.tabPageProvider.Controls.Add(this.checkBoxCustomRequestLimit);
            this.tabPageProvider.Controls.Add(this.linkLabelCustomRequestLimit);
            this.tabPageProvider.Controls.Add(this.checkBoxCustomDisplayName);
            this.tabPageProvider.Controls.Add(this.labelCustomDisplayName);
            this.tabPageProvider.Controls.Add(this.textBoxCustomDisplayName);
            this.tabPageProvider.Controls.Add(this.checkBoxStatsAndLog);
            this.tabPageProvider.Controls.Add(this.linkLabelStatsAndLog);
            this.tabPageProvider.Controls.Add(this.checkBoxTranslateCache);
            this.tabPageProvider.Controls.Add(this.linkLabelTranslateCache);
            this.tabPageProvider.Location = new System.Drawing.Point(4, 36);
            this.tabPageProvider.Name = "tabPageProvider";
            this.tabPageProvider.Size = new System.Drawing.Size(586, 360);
            this.tabPageProvider.TabIndex = 0;
            this.tabPageProvider.Text = "Provider";
            //
            // tabPageLimits
            //
            // tabPageLimits backcolor set by theme
            this.tabPageLimits.Controls.Add(this.buttonLoadProviderDefault);
            this.tabPageLimits.Controls.Add(this.tabControlLimits);
            this.tabPageLimits.Location = new System.Drawing.Point(4, 36);
            this.tabPageLimits.Name = "tabPageLimits";
            this.tabPageLimits.Size = new System.Drawing.Size(586, 360);
            this.tabPageLimits.TabIndex = 1;
            this.tabPageLimits.Text = "Limits";
            //
            // tabPageCacheStats
            //
            // tabPageCacheStats backcolor set by theme
            this.tabPageCacheStats.Controls.Add(this.tabControlCacheStats);
            this.tabPageCacheStats.Location = new System.Drawing.Point(4, 36);
            this.tabPageCacheStats.Name = "tabPageCacheStats";
            this.tabPageCacheStats.Size = new System.Drawing.Size(586, 360);
            this.tabPageCacheStats.TabIndex = 2;
            this.tabPageCacheStats.Text = "Cache && Stats";

            // ============ PROVIDER TAB CONTROLS ============

            // linkLabelProvider
            this.linkLabelProvider.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelProvider.Location = new System.Drawing.Point(12, 14);
            this.linkLabelProvider.Name = "linkLabelProvider";
            this.linkLabelProvider.Size = new System.Drawing.Size(194, 18);
            this.linkLabelProvider.TabIndex = 1;
            this.linkLabelProvider.TabStop = true;
            this.linkLabelProvider.Text = "Provider";
            this.linkLabelProvider.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelProvider_LinkClicked);

            // comboBoxServiceProvider
            this.comboBoxServiceProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxServiceProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxServiceProvider.FormattingEnabled = true;
            this.comboBoxServiceProvider.Location = new System.Drawing.Point(215, 12);
            this.comboBoxServiceProvider.Name = "comboBoxServiceProvider";
            this.comboBoxServiceProvider.Size = new System.Drawing.Size(356, 23);
            this.comboBoxServiceProvider.TabIndex = 2;

            // labelRequestType
            this.labelRequestType.AutoSize = true;
            this.labelRequestType.Location = new System.Drawing.Point(12, 50);
            this.labelRequestType.Name = "labelRequestType";
            this.labelRequestType.Size = new System.Drawing.Size(83, 15);
            this.labelRequestType.TabIndex = 3;
            this.labelRequestType.Text = "Request Type";

            // comboBoxRequestType
            this.comboBoxRequestType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRequestType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRequestType.FormattingEnabled = true;
            this.comboBoxRequestType.Location = new System.Drawing.Point(215, 47);
            this.comboBoxRequestType.Name = "comboBoxRequestType";
            this.comboBoxRequestType.Size = new System.Drawing.Size(356, 23);
            this.comboBoxRequestType.TabIndex = 4;

            // checkBoxShowSupportedOnly (inline from RequestTypeLimit)
            this.checkBoxShowSupportedOnly.AutoSize = true;
            this.checkBoxShowSupportedOnly.Location = new System.Drawing.Point(215, 78);
            this.checkBoxShowSupportedOnly.Name = "checkBoxShowSupportedOnly";
            this.checkBoxShowSupportedOnly.Size = new System.Drawing.Size(250, 19);
            this.checkBoxShowSupportedOnly.TabIndex = 5;
            this.checkBoxShowSupportedOnly.Text = "Show Provider Supported Request Type Only";
            this.checkBoxShowSupportedOnly.UseVisualStyleBackColor = true;

            // checkBoxTagsToEnd
            this.checkBoxTagsToEnd.AutoSize = true;
            this.checkBoxTagsToEnd.Location = new System.Drawing.Point(12, 112);
            this.checkBoxTagsToEnd.Name = "checkBoxTagsToEnd";
            this.checkBoxTagsToEnd.Size = new System.Drawing.Size(245, 19);
            this.checkBoxTagsToEnd.TabIndex = 6;
            this.checkBoxTagsToEnd.Text = "Insert Required Tags To End";
            this.checkBoxTagsToEnd.UseVisualStyleBackColor = true;

            // checkBoxNormalizeWhitespace
            this.checkBoxNormalizeWhitespace.AutoSize = true;
            this.checkBoxNormalizeWhitespace.Location = new System.Drawing.Point(312, 112);
            this.checkBoxNormalizeWhitespace.Name = "checkBoxNormalizeWhitespace";
            this.checkBoxNormalizeWhitespace.Size = new System.Drawing.Size(250, 19);
            this.checkBoxNormalizeWhitespace.TabIndex = 7;
            this.checkBoxNormalizeWhitespace.Text = "Normalize Whitespace Around Tags";
            this.checkBoxNormalizeWhitespace.UseVisualStyleBackColor = true;

            // checkBoxTagsToEndFake
            this.checkBoxTagsToEndFake.AutoSize = true;
            this.checkBoxTagsToEndFake.Enabled = false;
            this.checkBoxTagsToEndFake.Location = new System.Drawing.Point(12, 112);
            this.checkBoxTagsToEndFake.Name = "checkBoxTagsToEndFake";
            this.checkBoxTagsToEndFake.Size = new System.Drawing.Size(18, 17);
            this.checkBoxTagsToEndFake.TabIndex = 20;
            this.checkBoxTagsToEndFake.UseVisualStyleBackColor = true;
            this.checkBoxTagsToEndFake.Visible = false;

            // checkBoxNormalizeWhitespaceFake
            this.checkBoxNormalizeWhitespaceFake.AutoSize = true;
            this.checkBoxNormalizeWhitespaceFake.Enabled = false;
            this.checkBoxNormalizeWhitespaceFake.Location = new System.Drawing.Point(312, 113);
            this.checkBoxNormalizeWhitespaceFake.Name = "checkBoxNormalizeWhitespaceFake";
            this.checkBoxNormalizeWhitespaceFake.Size = new System.Drawing.Size(18, 17);
            this.checkBoxNormalizeWhitespaceFake.TabIndex = 21;
            this.checkBoxNormalizeWhitespaceFake.UseVisualStyleBackColor = true;
            this.checkBoxNormalizeWhitespaceFake.Visible = false;

            // checkBoxCustomRequestLimit
            this.checkBoxCustomRequestLimit.AutoSize = true;
            this.checkBoxCustomRequestLimit.Location = new System.Drawing.Point(12, 155);
            this.checkBoxCustomRequestLimit.Name = "checkBoxCustomRequestLimit";
            this.checkBoxCustomRequestLimit.Size = new System.Drawing.Size(18, 17);
            this.checkBoxCustomRequestLimit.TabIndex = 8;
            this.checkBoxCustomRequestLimit.UseVisualStyleBackColor = true;

            // linkLabelCustomRequestLimit
            this.linkLabelCustomRequestLimit.AutoSize = true;
            this.linkLabelCustomRequestLimit.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelCustomRequestLimit.Location = new System.Drawing.Point(33, 156);
            this.linkLabelCustomRequestLimit.Name = "linkLabelCustomRequestLimit";
            this.linkLabelCustomRequestLimit.Size = new System.Drawing.Size(160, 15);
            this.linkLabelCustomRequestLimit.TabIndex = 9;
            this.linkLabelCustomRequestLimit.TabStop = true;
            this.linkLabelCustomRequestLimit.Text = "Enable Custom Request Limit";
            this.linkLabelCustomRequestLimit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCustomRequestLimit_LinkClicked);

            // checkBoxCustomDisplayName
            this.checkBoxCustomDisplayName.AutoSize = true;
            this.checkBoxCustomDisplayName.Location = new System.Drawing.Point(312, 155);
            this.checkBoxCustomDisplayName.Name = "checkBoxCustomDisplayName";
            this.checkBoxCustomDisplayName.Size = new System.Drawing.Size(18, 17);
            this.checkBoxCustomDisplayName.TabIndex = 10;
            this.checkBoxCustomDisplayName.UseVisualStyleBackColor = true;

            // labelCustomDisplayName
            this.labelCustomDisplayName.AutoSize = true;
            this.labelCustomDisplayName.Location = new System.Drawing.Point(333, 156);
            this.labelCustomDisplayName.Name = "labelCustomDisplayName";
            this.labelCustomDisplayName.Size = new System.Drawing.Size(140, 15);
            this.labelCustomDisplayName.TabIndex = 11;
            this.labelCustomDisplayName.Text = "Enable Custom Display Name";

            // textBoxCustomDisplayName (inline from CustomDisplayName)
            this.textBoxCustomDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCustomDisplayName.Enabled = false;
            this.textBoxCustomDisplayName.Location = new System.Drawing.Point(333, 178);
            this.textBoxCustomDisplayName.Name = "textBoxCustomDisplayName";
            this.textBoxCustomDisplayName.Size = new System.Drawing.Size(238, 23);
            this.textBoxCustomDisplayName.TabIndex = 12;

            // checkBoxStatsAndLog
            this.checkBoxStatsAndLog.AutoSize = true;
            this.checkBoxStatsAndLog.Location = new System.Drawing.Point(12, 205);
            this.checkBoxStatsAndLog.Name = "checkBoxStatsAndLog";
            this.checkBoxStatsAndLog.Size = new System.Drawing.Size(18, 17);
            this.checkBoxStatsAndLog.TabIndex = 13;
            this.checkBoxStatsAndLog.UseVisualStyleBackColor = true;

            // linkLabelStatsAndLog
            this.linkLabelStatsAndLog.AutoSize = true;
            this.linkLabelStatsAndLog.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelStatsAndLog.Location = new System.Drawing.Point(33, 206);
            this.linkLabelStatsAndLog.Name = "linkLabelStatsAndLog";
            this.linkLabelStatsAndLog.Size = new System.Drawing.Size(126, 15);
            this.linkLabelStatsAndLog.TabIndex = 14;
            this.linkLabelStatsAndLog.TabStop = true;
            this.linkLabelStatsAndLog.Text = "Enable Stats And Log";
            this.linkLabelStatsAndLog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelStatsAndLog_LinkClicked);

            // checkBoxTranslateCache
            this.checkBoxTranslateCache.AutoSize = true;
            this.checkBoxTranslateCache.Location = new System.Drawing.Point(312, 205);
            this.checkBoxTranslateCache.Name = "checkBoxTranslateCache";
            this.checkBoxTranslateCache.Size = new System.Drawing.Size(18, 17);
            this.checkBoxTranslateCache.TabIndex = 15;
            this.checkBoxTranslateCache.UseVisualStyleBackColor = true;

            // linkLabelTranslateCache
            this.linkLabelTranslateCache.AutoSize = true;
            this.linkLabelTranslateCache.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelTranslateCache.Location = new System.Drawing.Point(333, 206);
            this.linkLabelTranslateCache.Name = "linkLabelTranslateCache";
            this.linkLabelTranslateCache.Size = new System.Drawing.Size(140, 15);
            this.linkLabelTranslateCache.TabIndex = 16;
            this.linkLabelTranslateCache.TabStop = true;
            this.linkLabelTranslateCache.Text = "Enable Translate Cache";
            this.linkLabelTranslateCache.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelTranslateCache_LinkClicked);

            // ============ LIMITS TAB CONTROLS (from CustomLimit) ============

            // buttonLoadProviderDefault
            this.buttonLoadProviderDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoadProviderDefault.Location = new System.Drawing.Point(12, 324);
            this.buttonLoadProviderDefault.Name = "buttonLoadProviderDefault";
            this.buttonLoadProviderDefault.Size = new System.Drawing.Size(560, 27);
            this.buttonLoadProviderDefault.TabIndex = 30;
            this.buttonLoadProviderDefault.Text = "&Load provider default";
            this.buttonLoadProviderDefault.UseVisualStyleBackColor = false;
            this.buttonLoadProviderDefault.Click += new System.EventHandler(this.buttonLoadProviderDefault_Click);

            // tabControlLimits
            this.tabControlLimits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlLimits.Controls.Add(this.tabPageSizeLimit);
            this.tabControlLimits.Controls.Add(this.tabPageRateLimit);
            this.tabControlLimits.Controls.Add(this.tabPageConcurrencyLimit);
            this.tabControlLimits.Controls.Add(this.tabPageRetryLimit);
            this.tabControlLimits.Location = new System.Drawing.Point(12, 10);
            this.tabControlLimits.Name = "tabControlLimits";
            this.tabControlLimits.SelectedIndex = 0;
            this.tabControlLimits.Size = new System.Drawing.Size(560, 308);
            this.tabControlLimits.TabIndex = 20;

            // tabPageSizeLimit
            this.tabPageSizeLimit.Controls.Add(this.labelMaxCharactersPerRequest);
            this.tabPageSizeLimit.Controls.Add(this.numericUpDownMaxCharactersPerRequest);
            this.tabPageSizeLimit.Controls.Add(this.labelNoBathTip);
            this.tabPageSizeLimit.Controls.Add(this.labelMaxSegmentsPerRequest);
            this.tabPageSizeLimit.Controls.Add(this.numericUpDownMaxSegmentsPerRequest);
            this.tabPageSizeLimit.Location = new System.Drawing.Point(4, 25);
            this.tabPageSizeLimit.Name = "tabPageSizeLimit";
            this.tabPageSizeLimit.Size = new System.Drawing.Size(552, 268);
            this.tabPageSizeLimit.TabIndex = 0;
            this.tabPageSizeLimit.Text = "Size Limit";
            this.tabPageSizeLimit.UseVisualStyleBackColor = true;

            // labelMaxSegmentsPerRequest
            this.labelMaxSegmentsPerRequest.AutoSize = true;
            this.labelMaxSegmentsPerRequest.Location = new System.Drawing.Point(15, 22);
            this.labelMaxSegmentsPerRequest.Name = "labelMaxSegmentsPerRequest";
            this.labelMaxSegmentsPerRequest.Size = new System.Drawing.Size(160, 15);
            this.labelMaxSegmentsPerRequest.TabIndex = 21;
            this.labelMaxSegmentsPerRequest.Text = "Max Segments Per Request";

            this.numericUpDownMaxSegmentsPerRequest.Location = new System.Drawing.Point(360, 18);
            this.numericUpDownMaxSegmentsPerRequest.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownMaxSegmentsPerRequest.Name = "numericUpDownMaxSegmentsPerRequest";
            this.numericUpDownMaxSegmentsPerRequest.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownMaxSegmentsPerRequest.TabIndex = 22;

            // labelMaxCharactersPerRequest
            this.labelMaxCharactersPerRequest.AutoSize = true;
            this.labelMaxCharactersPerRequest.Location = new System.Drawing.Point(15, 58);
            this.labelMaxCharactersPerRequest.Name = "labelMaxCharactersPerRequest";
            this.labelMaxCharactersPerRequest.Size = new System.Drawing.Size(176, 15);
            this.labelMaxCharactersPerRequest.TabIndex = 23;
            this.labelMaxCharactersPerRequest.Text = "Max Characters Per Request";

            this.numericUpDownMaxCharactersPerRequest.Increment = new decimal(new int[] { 500, 0, 0, 0 });
            this.numericUpDownMaxCharactersPerRequest.Location = new System.Drawing.Point(360, 54);
            this.numericUpDownMaxCharactersPerRequest.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownMaxCharactersPerRequest.Name = "numericUpDownMaxCharactersPerRequest";
            this.numericUpDownMaxCharactersPerRequest.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownMaxCharactersPerRequest.TabIndex = 24;

            // labelNoBathTip
            this.labelNoBathTip.AutoSize = true;
            this.labelNoBathTip.ForeColor = System.Drawing.Color.Red;
            this.labelNoBathTip.Location = new System.Drawing.Point(15, 250);
            this.labelNoBathTip.Name = "labelNoBathTip";
            this.labelNoBathTip.Size = new System.Drawing.Size(407, 15);
            this.labelNoBathTip.TabIndex = 25;
            this.labelNoBathTip.Text = "Selected provider does not support batch translation!";
            this.labelNoBathTip.Visible = false;

            // tabPageRateLimit
            this.tabPageRateLimit.Controls.Add(this.labelMaxRequestsPerWindow);
            this.tabPageRateLimit.Controls.Add(this.numericUpDownMaxRequestsPerWindow);
            this.tabPageRateLimit.Controls.Add(this.labelWindowSizeMs);
            this.tabPageRateLimit.Controls.Add(this.numericUpDownWindowSizeMs);
            this.tabPageRateLimit.Controls.Add(this.labelRequestSmoothness);
            this.tabPageRateLimit.Controls.Add(this.numericUpDownRequestSmoothness);
            this.tabPageRateLimit.Location = new System.Drawing.Point(4, 25);
            this.tabPageRateLimit.Name = "tabPageRateLimit";
            this.tabPageRateLimit.Size = new System.Drawing.Size(552, 268);
            this.tabPageRateLimit.TabIndex = 1;
            this.tabPageRateLimit.Text = "Rate Limit";
            this.tabPageRateLimit.UseVisualStyleBackColor = true;

            this.labelMaxRequestsPerWindow.AutoSize = true;
            this.labelMaxRequestsPerWindow.Location = new System.Drawing.Point(15, 22);
            this.labelMaxRequestsPerWindow.Name = "labelMaxRequestsPerWindow";
            this.labelMaxRequestsPerWindow.Size = new System.Drawing.Size(156, 15);
            this.labelMaxRequestsPerWindow.TabIndex = 31;
            this.labelMaxRequestsPerWindow.Text = "Max Requests Per Window";

            this.numericUpDownMaxRequestsPerWindow.Location = new System.Drawing.Point(360, 18);
            this.numericUpDownMaxRequestsPerWindow.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownMaxRequestsPerWindow.Name = "numericUpDownMaxRequestsPerWindow";
            this.numericUpDownMaxRequestsPerWindow.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownMaxRequestsPerWindow.TabIndex = 32;

            this.labelWindowSizeMs.AutoSize = true;
            this.labelWindowSizeMs.Location = new System.Drawing.Point(15, 58);
            this.labelWindowSizeMs.Name = "labelWindowSizeMs";
            this.labelWindowSizeMs.Size = new System.Drawing.Size(96, 15);
            this.labelWindowSizeMs.TabIndex = 33;
            this.labelWindowSizeMs.Text = "Window Size Ms";

            this.numericUpDownWindowSizeMs.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownWindowSizeMs.Location = new System.Drawing.Point(360, 54);
            this.numericUpDownWindowSizeMs.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownWindowSizeMs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDownWindowSizeMs.Name = "numericUpDownWindowSizeMs";
            this.numericUpDownWindowSizeMs.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownWindowSizeMs.TabIndex = 34;
            this.numericUpDownWindowSizeMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });

            this.labelRequestSmoothness.AutoSize = true;
            this.labelRequestSmoothness.Location = new System.Drawing.Point(15, 94);
            this.labelRequestSmoothness.Name = "labelRequestSmoothness";
            this.labelRequestSmoothness.Size = new System.Drawing.Size(124, 15);
            this.labelRequestSmoothness.TabIndex = 35;
            this.labelRequestSmoothness.Text = "Request Smoothness";

            this.numericUpDownRequestSmoothness.DecimalPlaces = 2;
            this.numericUpDownRequestSmoothness.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numericUpDownRequestSmoothness.Location = new System.Drawing.Point(360, 90);
            this.numericUpDownRequestSmoothness.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDownRequestSmoothness.Name = "numericUpDownRequestSmoothness";
            this.numericUpDownRequestSmoothness.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownRequestSmoothness.TabIndex = 36;
            this.numericUpDownRequestSmoothness.Value = new decimal(new int[] { 100, 0, 0, 131072 });

            // tabPageConcurrencyLimit
            this.tabPageConcurrencyLimit.Controls.Add(this.labelMaxRequestsHold);
            this.tabPageConcurrencyLimit.Controls.Add(this.numericUpDownMaxRequestsHold);
            this.tabPageConcurrencyLimit.Location = new System.Drawing.Point(4, 25);
            this.tabPageConcurrencyLimit.Name = "tabPageConcurrencyLimit";
            this.tabPageConcurrencyLimit.Size = new System.Drawing.Size(552, 268);
            this.tabPageConcurrencyLimit.TabIndex = 2;
            this.tabPageConcurrencyLimit.Text = "Concurrency Limit";
            this.tabPageConcurrencyLimit.UseVisualStyleBackColor = true;

            this.labelMaxRequestsHold.AutoSize = true;
            this.labelMaxRequestsHold.Location = new System.Drawing.Point(15, 22);
            this.labelMaxRequestsHold.Name = "labelMaxRequestsHold";
            this.labelMaxRequestsHold.Size = new System.Drawing.Size(117, 15);
            this.labelMaxRequestsHold.TabIndex = 41;
            this.labelMaxRequestsHold.Text = "Max Requests Hold";

            this.numericUpDownMaxRequestsHold.Location = new System.Drawing.Point(360, 18);
            this.numericUpDownMaxRequestsHold.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownMaxRequestsHold.Name = "numericUpDownMaxRequestsHold";
            this.numericUpDownMaxRequestsHold.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownMaxRequestsHold.TabIndex = 42;

            // tabPageRetryLimit
            this.tabPageRetryLimit.Controls.Add(this.labelNumberOfRetries);
            this.tabPageRetryLimit.Controls.Add(this.numericUpDownNumberOfRetries);
            this.tabPageRetryLimit.Controls.Add(this.labelFailedTimeoutMs);
            this.tabPageRetryLimit.Controls.Add(this.numericUpDownFailedTimeoutMs);
            this.tabPageRetryLimit.Controls.Add(this.labelRetryWaitingMs);
            this.tabPageRetryLimit.Controls.Add(this.numericUpDownRetryWaitingMs);
            this.tabPageRetryLimit.Location = new System.Drawing.Point(4, 25);
            this.tabPageRetryLimit.Name = "tabPageRetryLimit";
            this.tabPageRetryLimit.Size = new System.Drawing.Size(552, 268);
            this.tabPageRetryLimit.TabIndex = 3;
            this.tabPageRetryLimit.Text = "Retry Limit";
            this.tabPageRetryLimit.UseVisualStyleBackColor = true;

            this.labelNumberOfRetries.AutoSize = true;
            this.labelNumberOfRetries.Location = new System.Drawing.Point(15, 22);
            this.labelNumberOfRetries.Name = "labelNumberOfRetries";
            this.labelNumberOfRetries.Size = new System.Drawing.Size(109, 15);
            this.labelNumberOfRetries.TabIndex = 51;
            this.labelNumberOfRetries.Text = "Number Of Retries";

            this.numericUpDownNumberOfRetries.Location = new System.Drawing.Point(360, 18);
            this.numericUpDownNumberOfRetries.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownNumberOfRetries.Name = "numericUpDownNumberOfRetries";
            this.numericUpDownNumberOfRetries.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownNumberOfRetries.TabIndex = 52;

            this.labelFailedTimeoutMs.AutoSize = true;
            this.labelFailedTimeoutMs.Location = new System.Drawing.Point(15, 58);
            this.labelFailedTimeoutMs.Name = "labelFailedTimeoutMs";
            this.labelFailedTimeoutMs.Size = new System.Drawing.Size(117, 15);
            this.labelFailedTimeoutMs.TabIndex = 53;
            this.labelFailedTimeoutMs.Text = "Failed Timeout Ms";

            this.numericUpDownFailedTimeoutMs.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownFailedTimeoutMs.Location = new System.Drawing.Point(360, 54);
            this.numericUpDownFailedTimeoutMs.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownFailedTimeoutMs.Name = "numericUpDownFailedTimeoutMs";
            this.numericUpDownFailedTimeoutMs.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownFailedTimeoutMs.TabIndex = 54;

            this.labelRetryWaitingMs.AutoSize = true;
            this.labelRetryWaitingMs.Location = new System.Drawing.Point(15, 94);
            this.labelRetryWaitingMs.Name = "labelRetryWaitingMs";
            this.labelRetryWaitingMs.Size = new System.Drawing.Size(109, 15);
            this.labelRetryWaitingMs.TabIndex = 55;
            this.labelRetryWaitingMs.Text = "Retry Waiting Ms";

            this.numericUpDownRetryWaitingMs.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownRetryWaitingMs.Location = new System.Drawing.Point(360, 90);
            this.numericUpDownRetryWaitingMs.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            this.numericUpDownRetryWaitingMs.Name = "numericUpDownRetryWaitingMs";
            this.numericUpDownRetryWaitingMs.Size = new System.Drawing.Size(147, 25);
            this.numericUpDownRetryWaitingMs.TabIndex = 56;

            // ============ CACHE & STATS TAB CONTROLS ============

            // tabControlCacheStats
            this.tabControlCacheStats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlCacheStats.Controls.Add(this.tabPageStatistics);
            this.tabControlCacheStats.Controls.Add(this.tabPageLogging);
            this.tabControlCacheStats.Controls.Add(this.tabPageCache);
            this.tabControlCacheStats.Location = new System.Drawing.Point(12, 10);
            this.tabControlCacheStats.Name = "tabControlCacheStats";
            this.tabControlCacheStats.SelectedIndex = 0;
            this.tabControlCacheStats.Size = new System.Drawing.Size(560, 340);
            this.tabControlCacheStats.TabIndex = 60;

            // tabPageStatistics
            this.tabPageStatistics.Controls.Add(this.labelSuccessRequests);
            this.tabPageStatistics.Controls.Add(this.labelSuccessCountValue);
            this.tabPageStatistics.Controls.Add(this.labelFailedRequest);
            this.tabPageStatistics.Controls.Add(this.labelFailedCountValue);
            this.tabPageStatistics.Controls.Add(this.linkLabelResetStats);
            this.tabPageStatistics.Location = new System.Drawing.Point(4, 25);
            this.tabPageStatistics.Name = "tabPageStatistics";
            this.tabPageStatistics.Size = new System.Drawing.Size(552, 300);
            this.tabPageStatistics.TabIndex = 0;
            this.tabPageStatistics.Text = "Statistics";
            this.tabPageStatistics.UseVisualStyleBackColor = true;

            this.labelSuccessRequests.AutoSize = true;
            this.labelSuccessRequests.Location = new System.Drawing.Point(20, 30);
            this.labelSuccessRequests.Name = "labelSuccessRequests";
            this.labelSuccessRequests.Size = new System.Drawing.Size(103, 15);
            this.labelSuccessRequests.TabIndex = 61;
            this.labelSuccessRequests.Text = "Success Requests";

            this.labelSuccessCountValue.AutoSize = true;
            this.labelSuccessCountValue.Location = new System.Drawing.Point(200, 30);
            this.labelSuccessCountValue.Name = "labelSuccessCountValue";
            this.labelSuccessCountValue.Size = new System.Drawing.Size(15, 15);
            this.labelSuccessCountValue.TabIndex = 62;
            this.labelSuccessCountValue.Text = "0";

            this.labelFailedRequest.AutoSize = true;
            this.labelFailedRequest.Location = new System.Drawing.Point(20, 60);
            this.labelFailedRequest.Name = "labelFailedRequest";
            this.labelFailedRequest.Size = new System.Drawing.Size(96, 15);
            this.labelFailedRequest.TabIndex = 63;
            this.labelFailedRequest.Text = "Failed Requests";

            this.labelFailedCountValue.AutoSize = true;
            this.labelFailedCountValue.Location = new System.Drawing.Point(200, 60);
            this.labelFailedCountValue.Name = "labelFailedCountValue";
            this.labelFailedCountValue.Size = new System.Drawing.Size(15, 15);
            this.labelFailedCountValue.TabIndex = 64;
            this.labelFailedCountValue.Text = "0";

            this.linkLabelResetStats.AutoSize = true;
            this.linkLabelResetStats.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelResetStats.Location = new System.Drawing.Point(20, 90);
            this.linkLabelResetStats.Name = "linkLabelResetStats";
            this.linkLabelResetStats.Size = new System.Drawing.Size(64, 15);
            this.linkLabelResetStats.TabIndex = 65;
            this.linkLabelResetStats.TabStop = true;
            this.linkLabelResetStats.Text = "Reset Stats";
            this.linkLabelResetStats.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelResetStats_LinkClicked);

            // tabPageLogging
            this.tabPageLogging.Controls.Add(this.linkLabelOpenLogFile);
            this.tabPageLogging.Controls.Add(this.linkLabelOpenLogDir);
            this.tabPageLogging.Controls.Add(this.checkBoxVerboseRuntimeLog);
            this.tabPageLogging.Controls.Add(this.checkBoxApiRequestResponseLog);
            this.tabPageLogging.Controls.Add(this.labelLoggingLevel);
            this.tabPageLogging.Controls.Add(this.radioButtonDebug);
            this.tabPageLogging.Controls.Add(this.radioButtonInfo);
            this.tabPageLogging.Controls.Add(this.radioButtonWarn);
            this.tabPageLogging.Controls.Add(this.radioButtonError);
            this.tabPageLogging.Location = new System.Drawing.Point(4, 25);
            this.tabPageLogging.Name = "tabPageLogging";
            this.tabPageLogging.Size = new System.Drawing.Size(552, 300);
            this.tabPageLogging.TabIndex = 1;
            this.tabPageLogging.Text = "Logging";
            this.tabPageLogging.UseVisualStyleBackColor = true;

            this.linkLabelOpenLogFile.AutoSize = true;
            this.linkLabelOpenLogFile.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelOpenLogFile.Location = new System.Drawing.Point(20, 25);
            this.linkLabelOpenLogFile.Name = "linkLabelOpenLogFile";
            this.linkLabelOpenLogFile.Size = new System.Drawing.Size(82, 15);
            this.linkLabelOpenLogFile.TabIndex = 71;
            this.linkLabelOpenLogFile.TabStop = true;
            this.linkLabelOpenLogFile.Text = "Open Log File";
            this.linkLabelOpenLogFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenLogFile_LinkClicked);

            this.linkLabelOpenLogDir.AutoSize = true;
            this.linkLabelOpenLogDir.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelOpenLogDir.Location = new System.Drawing.Point(370, 25);
            this.linkLabelOpenLogDir.Name = "linkLabelOpenLogDir";
            this.linkLabelOpenLogDir.Size = new System.Drawing.Size(84, 15);
            this.linkLabelOpenLogDir.TabIndex = 72;
            this.linkLabelOpenLogDir.TabStop = true;
            this.linkLabelOpenLogDir.Text = "Open Log Dir";
            this.linkLabelOpenLogDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenLogDir_LinkClicked);

            this.checkBoxVerboseRuntimeLog.AutoSize = true;
            this.checkBoxVerboseRuntimeLog.Location = new System.Drawing.Point(20, 60);
            this.checkBoxVerboseRuntimeLog.Name = "checkBoxVerboseRuntimeLog";
            this.checkBoxVerboseRuntimeLog.Size = new System.Drawing.Size(137, 19);
            this.checkBoxVerboseRuntimeLog.TabIndex = 73;
            this.checkBoxVerboseRuntimeLog.Text = "Detailed runtime log";
            this.checkBoxVerboseRuntimeLog.UseVisualStyleBackColor = true;

            this.checkBoxApiRequestResponseLog.AutoSize = true;
            this.checkBoxApiRequestResponseLog.Location = new System.Drawing.Point(20, 90);
            this.checkBoxApiRequestResponseLog.Name = "checkBoxApiRequestResponseLog";
            this.checkBoxApiRequestResponseLog.Size = new System.Drawing.Size(163, 19);
            this.checkBoxApiRequestResponseLog.TabIndex = 74;
            this.checkBoxApiRequestResponseLog.Text = "API request/response log";
            this.checkBoxApiRequestResponseLog.UseVisualStyleBackColor = true;

            this.labelLoggingLevel.AutoSize = true;
            this.labelLoggingLevel.Location = new System.Drawing.Point(20, 135);
            this.labelLoggingLevel.Name = "labelLoggingLevel";
            this.labelLoggingLevel.Size = new System.Drawing.Size(84, 15);
            this.labelLoggingLevel.TabIndex = 75;
            this.labelLoggingLevel.Text = "Logging Level";

            this.radioButtonDebug.AutoSize = true;
            this.radioButtonDebug.Location = new System.Drawing.Point(130, 133);
            this.radioButtonDebug.Name = "radioButtonDebug";
            this.radioButtonDebug.Size = new System.Drawing.Size(58, 19);
            this.radioButtonDebug.TabIndex = 76;
            this.radioButtonDebug.Text = "Debug";
            this.radioButtonDebug.UseVisualStyleBackColor = true;

            this.radioButtonInfo.AutoSize = true;
            this.radioButtonInfo.Location = new System.Drawing.Point(210, 133);
            this.radioButtonInfo.Name = "radioButtonInfo";
            this.radioButtonInfo.Size = new System.Drawing.Size(45, 19);
            this.radioButtonInfo.TabIndex = 77;
            this.radioButtonInfo.Text = "Info";
            this.radioButtonInfo.UseVisualStyleBackColor = true;

            this.radioButtonWarn.AutoSize = true;
            this.radioButtonWarn.Location = new System.Drawing.Point(280, 133);
            this.radioButtonWarn.Name = "radioButtonWarn";
            this.radioButtonWarn.Size = new System.Drawing.Size(52, 19);
            this.radioButtonWarn.TabIndex = 78;
            this.radioButtonWarn.Text = "Warn";
            this.radioButtonWarn.UseVisualStyleBackColor = true;

            this.radioButtonError.AutoSize = true;
            this.radioButtonError.Location = new System.Drawing.Point(355, 133);
            this.radioButtonError.Name = "radioButtonError";
            this.radioButtonError.Size = new System.Drawing.Size(54, 19);
            this.radioButtonError.TabIndex = 79;
            this.radioButtonError.Text = "Error";
            this.radioButtonError.UseVisualStyleBackColor = true;

            // tabPageCache (new – from TranslateCache)
            this.tabPageCache.Controls.Add(this.labelCacheCount);
            this.tabPageCache.Controls.Add(this.labelCacheCountValue);
            this.tabPageCache.Controls.Add(this.linkLabelCleanCache);
            this.tabPageCache.Location = new System.Drawing.Point(4, 25);
            this.tabPageCache.Name = "tabPageCache";
            this.tabPageCache.Size = new System.Drawing.Size(552, 300);
            this.tabPageCache.TabIndex = 2;
            this.tabPageCache.Text = "Cache";
            this.tabPageCache.UseVisualStyleBackColor = true;

            this.labelCacheCount.AutoSize = true;
            this.labelCacheCount.Location = new System.Drawing.Point(20, 30);
            this.labelCacheCount.Name = "labelCacheCount";
            this.labelCacheCount.Size = new System.Drawing.Size(79, 15);
            this.labelCacheCount.TabIndex = 81;
            this.labelCacheCount.Text = "Cache Count:";

            this.labelCacheCountValue.AutoSize = true;
            this.labelCacheCountValue.Location = new System.Drawing.Point(200, 30);
            this.labelCacheCountValue.Name = "labelCacheCountValue";
            this.labelCacheCountValue.Size = new System.Drawing.Size(15, 15);
            this.labelCacheCountValue.TabIndex = 82;
            this.labelCacheCountValue.Text = "0";

            this.linkLabelCleanCache.AutoSize = true;
            this.linkLabelCleanCache.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabelCleanCache.Location = new System.Drawing.Point(20, 60);
            this.linkLabelCleanCache.Name = "linkLabelCleanCache";
            this.linkLabelCleanCache.Size = new System.Drawing.Size(75, 15);
            this.linkLabelCleanCache.TabIndex = 83;
            this.linkLabelCleanCache.TabStop = true;
            this.linkLabelCleanCache.Text = "Clean Cache";
            this.linkLabelCleanCache.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCleanCache_LinkClicked);

            // ============ FORM ============

            // MultiSupplierMTOptionsForm
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(609, 450);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.comboBoxLanguages);
            this.Controls.Add(this.buttonGithub);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(609, 450);
            this.MaximizeBox = false;
            this.Name = "MultiSupplierMTOptionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multi Supplier MT Plugin Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MultiSupplierMTOptionsForm_FormClosing);
            this.Load += new System.EventHandler(this.MultiSupplierMTOptionsForm_Load);

            // Resume layouts
            this.tabControlMain.ResumeLayout(false);
            this.tabPageProvider.ResumeLayout(false);
            this.tabPageProvider.PerformLayout();
            this.tabPageLimits.ResumeLayout(false);
            this.tabPageCacheStats.ResumeLayout(false);
            this.tabControlLimits.ResumeLayout(false);
            this.tabPageSizeLimit.ResumeLayout(false);
            this.tabPageSizeLimit.PerformLayout();
            this.tabPageRateLimit.ResumeLayout(false);
            this.tabPageRateLimit.PerformLayout();
            this.tabPageConcurrencyLimit.ResumeLayout(false);
            this.tabPageConcurrencyLimit.PerformLayout();
            this.tabPageRetryLimit.ResumeLayout(false);
            this.tabPageRetryLimit.PerformLayout();
            this.tabControlCacheStats.ResumeLayout(false);
            this.tabPageStatistics.ResumeLayout(false);
            this.tabPageStatistics.PerformLayout();
            this.tabPageLogging.ResumeLayout(false);
            this.tabPageLogging.PerformLayout();
            this.tabPageCache.ResumeLayout(false);
            this.tabPageCache.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSegmentsPerRequest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharactersPerRequest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxRequestsPerWindow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWindowSizeMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRequestSmoothness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxRequestsHold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfRetries)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFailedTimeoutMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRetryWaitingMs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // === 主控件 ===
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageProvider;
        private System.Windows.Forms.TabPage tabPageLimits;
        private System.Windows.Forms.TabPage tabPageCacheStats;

        // === Provider 标签页 ===
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGithub;
        private System.Windows.Forms.ComboBox comboBoxLanguages;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.LinkLabel linkLabelProvider;
        private System.Windows.Forms.ComboBox comboBoxServiceProvider;
        private System.Windows.Forms.Label labelRequestType;
        private System.Windows.Forms.ComboBox comboBoxRequestType;
        private System.Windows.Forms.CheckBox checkBoxShowSupportedOnly;
        private System.Windows.Forms.CheckBox checkBoxTagsToEnd;
        private System.Windows.Forms.CheckBox checkBoxNormalizeWhitespace;
        private System.Windows.Forms.CheckBox checkBoxTagsToEndFake;
        private System.Windows.Forms.CheckBox checkBoxNormalizeWhitespaceFake;
        private System.Windows.Forms.CheckBox checkBoxCustomRequestLimit;
        private System.Windows.Forms.LinkLabel linkLabelCustomRequestLimit;
        private System.Windows.Forms.CheckBox checkBoxCustomDisplayName;
        private System.Windows.Forms.Label labelCustomDisplayName;
        private System.Windows.Forms.TextBox textBoxCustomDisplayName;
        private System.Windows.Forms.CheckBox checkBoxStatsAndLog;
        private System.Windows.Forms.LinkLabel linkLabelStatsAndLog;
        private System.Windows.Forms.CheckBox checkBoxTranslateCache;
        private System.Windows.Forms.LinkLabel linkLabelTranslateCache;

        // === Limits 标签页 ===
        private System.Windows.Forms.Button buttonLoadProviderDefault;
        private System.Windows.Forms.TabControl tabControlLimits;
        private System.Windows.Forms.TabPage tabPageSizeLimit;
        private System.Windows.Forms.TabPage tabPageRateLimit;
        private System.Windows.Forms.TabPage tabPageConcurrencyLimit;
        private System.Windows.Forms.TabPage tabPageRetryLimit;
        private System.Windows.Forms.Label labelMaxSegmentsPerRequest;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxSegmentsPerRequest;
        private System.Windows.Forms.Label labelMaxCharactersPerRequest;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharactersPerRequest;
        private System.Windows.Forms.Label labelNoBathTip;
        private System.Windows.Forms.Label labelMaxRequestsPerWindow;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxRequestsPerWindow;
        private System.Windows.Forms.Label labelWindowSizeMs;
        private System.Windows.Forms.NumericUpDown numericUpDownWindowSizeMs;
        private System.Windows.Forms.Label labelRequestSmoothness;
        private System.Windows.Forms.NumericUpDown numericUpDownRequestSmoothness;
        private System.Windows.Forms.Label labelMaxRequestsHold;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxRequestsHold;
        private System.Windows.Forms.Label labelNumberOfRetries;
        private System.Windows.Forms.NumericUpDown numericUpDownNumberOfRetries;
        private System.Windows.Forms.Label labelFailedTimeoutMs;
        private System.Windows.Forms.NumericUpDown numericUpDownFailedTimeoutMs;
        private System.Windows.Forms.Label labelRetryWaitingMs;
        private System.Windows.Forms.NumericUpDown numericUpDownRetryWaitingMs;

        // === Cache & Stats 标签页 ===
        private System.Windows.Forms.TabControl tabControlCacheStats;
        private System.Windows.Forms.TabPage tabPageStatistics;
        private System.Windows.Forms.TabPage tabPageLogging;
        private System.Windows.Forms.TabPage tabPageCache;
        private System.Windows.Forms.Label labelSuccessRequests;
        private System.Windows.Forms.Label labelSuccessCountValue;
        private System.Windows.Forms.Label labelFailedRequest;
        private System.Windows.Forms.Label labelFailedCountValue;
        private System.Windows.Forms.LinkLabel linkLabelResetStats;
        private System.Windows.Forms.LinkLabel linkLabelOpenLogFile;
        private System.Windows.Forms.LinkLabel linkLabelOpenLogDir;
        private System.Windows.Forms.CheckBox checkBoxVerboseRuntimeLog;
        private System.Windows.Forms.CheckBox checkBoxApiRequestResponseLog;
        private System.Windows.Forms.Label labelLoggingLevel;
        private System.Windows.Forms.RadioButton radioButtonDebug;
        private System.Windows.Forms.RadioButton radioButtonInfo;
        private System.Windows.Forms.RadioButton radioButtonWarn;
        private System.Windows.Forms.RadioButton radioButtonError;
        private System.Windows.Forms.Label labelCacheCount;
        private System.Windows.Forms.Label labelCacheCountValue;
        private System.Windows.Forms.LinkLabel linkLabelCleanCache;
    }
}
