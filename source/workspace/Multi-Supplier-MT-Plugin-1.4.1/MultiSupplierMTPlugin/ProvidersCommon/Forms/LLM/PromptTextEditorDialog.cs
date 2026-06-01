using MultiSupplierMTPlugin.Helpers;
using MultiSupplierMTPlugin.Localized;
using System;
using System.Drawing;
using System.Windows.Forms;
using LLH = MultiSupplierMTPlugin.Localized.LocalizedHelper;
using LLKC = MultiSupplierMTPlugin.Localized.LocalizedKeyCommon;

namespace MultiSupplierMTPlugin.ProvidersCommon.Forms.LLM
{
    internal sealed class PromptTextEditorDialog : Form
    {
        private readonly TextBox _textBoxPrompt;
        private readonly bool _readOnly;
        private bool _discardChangesRequested;

        public PromptTextEditorDialog(string title, string promptText, Font editorFont, bool readOnly, bool enablePlaceholderMenu)
        {
            _readOnly = readOnly;
            Text = string.IsNullOrWhiteSpace(title) ? "Prompt" : title;
            AutoScaleMode = AutoScaleMode.Font;
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MinimumSize = new Size(720, 520);
            Size = new Size(960, 720);
            FormBorderStyle = FormBorderStyle.Sizable;

            _textBoxPrompt = new TextBox()
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                Dock = DockStyle.Fill,
                Font = editorFont,
                Multiline = true,
                ReadOnly = readOnly,
                ScrollBars = ScrollBars.Vertical,
                Text = promptText ?? string.Empty,
                WordWrap = true,
            };

            if (enablePlaceholderMenu && !readOnly)
                _textBoxPrompt.ContextMenuStrip = PromptHelper.CreateTextBoxContextMenu();

            var buttonOk = new Button()
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                DialogResult = DialogResult.OK,
                Location = new Point(594, 10),
                Size = new Size(110, 30),
                Text = LLH.G(LLKC.ButtonOK),
                UseVisualStyleBackColor = true,
            };

            var buttonCancel = new Button()
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                DialogResult = DialogResult.Cancel,
                Location = new Point(714, 10),
                Size = new Size(110, 30),
                Text = LLH.G(LLKC.ButtonCancel),
                UseVisualStyleBackColor = true,
            };
            buttonCancel.Click += buttonCancel_Click;

            var bottomPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                Padding = new Padding(0, 8, 0, 0),
            };

            var editorPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 12, 12, 6),
            };

            bottomPanel.Controls.Add(buttonOk);
            bottomPanel.Controls.Add(buttonCancel);
            editorPanel.Controls.Add(_textBoxPrompt);

            Controls.Add(editorPanel);
            Controls.Add(bottomPanel);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
            FormClosing += PromptTextEditorDialog_FormClosing;
        }

        public string PromptText => _textBoxPrompt.Text;

        public static bool TryEdit(
            IWin32Window owner,
            string title,
            string promptText,
            Font editorFont,
            bool readOnly,
            bool enablePlaceholderMenu,
            Action<string> applyText)
        {
            if (applyText == null)
                throw new ArgumentNullException(nameof(applyText));

            using (var dialog = new PromptTextEditorDialog(
                title,
                promptText,
                editorFont,
                readOnly,
                enablePlaceholderMenu))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return false;

                if (readOnly)
                    return false;

                if (string.Equals(promptText ?? string.Empty, dialog.PromptText ?? string.Empty, StringComparison.Ordinal))
                    return false;

                applyText(dialog.PromptText);
                return true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _discardChangesRequested = true;
        }

        private void PromptTextEditorDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_readOnly || _discardChangesRequested || e.CloseReason != CloseReason.UserClosing)
                return;

            DialogResult = DialogResult.OK;
        }
    }
}
