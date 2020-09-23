namespace ColorimeterDiagnosticApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.firmwareVersionTextBox = new System.Windows.Forms.TextBox();
            this.testFileVersionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SaveUserTestsFileButton = new System.Windows.Forms.Button();
            this.SaveUserTestsPathTextBox = new System.Windows.Forms.TextBox();
            this.SaveUserTestsFileBrowseButton = new System.Windows.Forms.Button();
            this.SaveTestResultsPathTextBox = new System.Windows.Forms.TextBox();
            this.SaveTestResultsBrowseButton = new System.Windows.Forms.Button();
            this.SaveTestResultsButton = new System.Windows.Forms.Button();
            this.UpdateUserTestsFileBrowseButton = new System.Windows.Forms.Button();
            this.UpdateUserTestsFileButton = new System.Windows.Forms.Button();
            this.UpdateUserTestsPathTextBox = new System.Windows.Forms.TextBox();
            this.UpdateTaylorTestsFileButton = new System.Windows.Forms.Button();
            this.UpdateTaylorTestsPathTextBox = new System.Windows.Forms.TextBox();
            this.UpdateTaylorTestsFileBrowseButton = new System.Windows.Forms.Button();
            this.UpdateFirmwareButton = new System.Windows.Forms.Button();
            this.UpdateFirmwarePathTextBox = new System.Windows.Forms.TextBox();
            this.UpdateFirmwareBrowseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(62, 357);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(620, 212);
            this.listBox1.TabIndex = 1;
            // 
            // firmwareVersionTextBox
            // 
            this.firmwareVersionTextBox.Location = new System.Drawing.Point(144, 33);
            this.firmwareVersionTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.firmwareVersionTextBox.Name = "firmwareVersionTextBox";
            this.firmwareVersionTextBox.ReadOnly = true;
            this.firmwareVersionTextBox.Size = new System.Drawing.Size(76, 20);
            this.firmwareVersionTextBox.TabIndex = 4;
            // 
            // testFileVersionTextBox
            // 
            this.testFileVersionTextBox.Location = new System.Drawing.Point(144, 84);
            this.testFileVersionTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.testFileVersionTextBox.Name = "testFileVersionTextBox";
            this.testFileVersionTextBox.ReadOnly = true;
            this.testFileVersionTextBox.Size = new System.Drawing.Size(76, 20);
            this.testFileVersionTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Firmware";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(61, 87);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Taylor Test File";
            // 
            // SaveUserTestsFileButton
            // 
            this.SaveUserTestsFileButton.Location = new System.Drawing.Point(62, 208);
            this.SaveUserTestsFileButton.Name = "SaveUserTestsFileButton";
            this.SaveUserTestsFileButton.Size = new System.Drawing.Size(297, 45);
            this.SaveUserTestsFileButton.TabIndex = 9;
            this.SaveUserTestsFileButton.Text = "Get User Tests File";
            this.SaveUserTestsFileButton.UseVisualStyleBackColor = true;
            this.SaveUserTestsFileButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // SaveUserTestsPathTextBox
            // 
            this.SaveUserTestsPathTextBox.Location = new System.Drawing.Point(174, 182);
            this.SaveUserTestsPathTextBox.Name = "SaveUserTestsPathTextBox";
            this.SaveUserTestsPathTextBox.ReadOnly = true;
            this.SaveUserTestsPathTextBox.Size = new System.Drawing.Size(185, 20);
            this.SaveUserTestsPathTextBox.TabIndex = 10;
            // 
            // SaveUserTestsFileBrowseButton
            // 
            this.SaveUserTestsFileBrowseButton.Location = new System.Drawing.Point(62, 182);
            this.SaveUserTestsFileBrowseButton.Name = "SaveUserTestsFileBrowseButton";
            this.SaveUserTestsFileBrowseButton.Size = new System.Drawing.Size(106, 20);
            this.SaveUserTestsFileBrowseButton.TabIndex = 11;
            this.SaveUserTestsFileBrowseButton.Text = "Browse";
            this.SaveUserTestsFileBrowseButton.UseVisualStyleBackColor = true;
            this.SaveUserTestsFileBrowseButton.Click += new System.EventHandler(this.browseSaveFile_Click);
            // 
            // SaveTestResultsPathTextBox
            // 
            this.SaveTestResultsPathTextBox.Location = new System.Drawing.Point(174, 270);
            this.SaveTestResultsPathTextBox.Name = "SaveTestResultsPathTextBox";
            this.SaveTestResultsPathTextBox.ReadOnly = true;
            this.SaveTestResultsPathTextBox.Size = new System.Drawing.Size(185, 20);
            this.SaveTestResultsPathTextBox.TabIndex = 12;
            // 
            // SaveTestResultsBrowseButton
            // 
            this.SaveTestResultsBrowseButton.Location = new System.Drawing.Point(62, 270);
            this.SaveTestResultsBrowseButton.Name = "SaveTestResultsBrowseButton";
            this.SaveTestResultsBrowseButton.Size = new System.Drawing.Size(106, 20);
            this.SaveTestResultsBrowseButton.TabIndex = 13;
            this.SaveTestResultsBrowseButton.Text = "Browse";
            this.SaveTestResultsBrowseButton.UseVisualStyleBackColor = true;
            this.SaveTestResultsBrowseButton.Click += new System.EventHandler(this.browseSaveFile_Click);
            // 
            // SaveTestResultsButton
            // 
            this.SaveTestResultsButton.Location = new System.Drawing.Point(62, 296);
            this.SaveTestResultsButton.Name = "SaveTestResultsButton";
            this.SaveTestResultsButton.Size = new System.Drawing.Size(297, 45);
            this.SaveTestResultsButton.TabIndex = 14;
            this.SaveTestResultsButton.Text = "Get Test Results";
            this.SaveTestResultsButton.UseVisualStyleBackColor = true;
            this.SaveTestResultsButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // UpdateUserTestsFileBrowseButton
            // 
            this.UpdateUserTestsFileBrowseButton.Location = new System.Drawing.Point(385, 181);
            this.UpdateUserTestsFileBrowseButton.Name = "UpdateUserTestsFileBrowseButton";
            this.UpdateUserTestsFileBrowseButton.Size = new System.Drawing.Size(106, 20);
            this.UpdateUserTestsFileBrowseButton.TabIndex = 15;
            this.UpdateUserTestsFileBrowseButton.Text = "Browse";
            this.UpdateUserTestsFileBrowseButton.UseVisualStyleBackColor = true;
            this.UpdateUserTestsFileBrowseButton.Click += new System.EventHandler(this.browseOpenFile_Click);
            // 
            // UpdateUserTestsFileButton
            // 
            this.UpdateUserTestsFileButton.Location = new System.Drawing.Point(385, 208);
            this.UpdateUserTestsFileButton.Name = "UpdateUserTestsFileButton";
            this.UpdateUserTestsFileButton.Size = new System.Drawing.Size(297, 45);
            this.UpdateUserTestsFileButton.TabIndex = 16;
            this.UpdateUserTestsFileButton.Text = "Update User Tests";
            this.UpdateUserTestsFileButton.UseVisualStyleBackColor = true;
            this.UpdateUserTestsFileButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // UpdateUserTestsPathTextBox
            // 
            this.UpdateUserTestsPathTextBox.Location = new System.Drawing.Point(497, 181);
            this.UpdateUserTestsPathTextBox.Name = "UpdateUserTestsPathTextBox";
            this.UpdateUserTestsPathTextBox.ReadOnly = true;
            this.UpdateUserTestsPathTextBox.Size = new System.Drawing.Size(185, 20);
            this.UpdateUserTestsPathTextBox.TabIndex = 17;
            // 
            // UpdateTaylorTestsFileButton
            // 
            this.UpdateTaylorTestsFileButton.Location = new System.Drawing.Point(385, 296);
            this.UpdateTaylorTestsFileButton.Name = "UpdateTaylorTestsFileButton";
            this.UpdateTaylorTestsFileButton.Size = new System.Drawing.Size(297, 45);
            this.UpdateTaylorTestsFileButton.TabIndex = 18;
            this.UpdateTaylorTestsFileButton.Text = "Update Taylor Tests";
            this.UpdateTaylorTestsFileButton.UseVisualStyleBackColor = true;
            this.UpdateTaylorTestsFileButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // UpdateTaylorTestsPathTextBox
            // 
            this.UpdateTaylorTestsPathTextBox.Location = new System.Drawing.Point(497, 271);
            this.UpdateTaylorTestsPathTextBox.Name = "UpdateTaylorTestsPathTextBox";
            this.UpdateTaylorTestsPathTextBox.ReadOnly = true;
            this.UpdateTaylorTestsPathTextBox.Size = new System.Drawing.Size(185, 20);
            this.UpdateTaylorTestsPathTextBox.TabIndex = 19;
            // 
            // UpdateTaylorTestsFileBrowseButton
            // 
            this.UpdateTaylorTestsFileBrowseButton.Location = new System.Drawing.Point(385, 271);
            this.UpdateTaylorTestsFileBrowseButton.Name = "UpdateTaylorTestsFileBrowseButton";
            this.UpdateTaylorTestsFileBrowseButton.Size = new System.Drawing.Size(106, 20);
            this.UpdateTaylorTestsFileBrowseButton.TabIndex = 20;
            this.UpdateTaylorTestsFileBrowseButton.Text = "Browse";
            this.UpdateTaylorTestsFileBrowseButton.UseVisualStyleBackColor = true;
            this.UpdateTaylorTestsFileBrowseButton.Click += new System.EventHandler(this.browseOpenFile_Click);
            // 
            // UpdateFirmwareButton
            // 
            this.UpdateFirmwareButton.Location = new System.Drawing.Point(385, 118);
            this.UpdateFirmwareButton.Name = "UpdateFirmwareButton";
            this.UpdateFirmwareButton.Size = new System.Drawing.Size(297, 45);
            this.UpdateFirmwareButton.TabIndex = 21;
            this.UpdateFirmwareButton.Text = "Update Firmware";
            this.UpdateFirmwareButton.UseVisualStyleBackColor = true;
            this.UpdateFirmwareButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // UpdateFirmwarePathTextBox
            // 
            this.UpdateFirmwarePathTextBox.Location = new System.Drawing.Point(497, 92);
            this.UpdateFirmwarePathTextBox.Name = "UpdateFirmwarePathTextBox";
            this.UpdateFirmwarePathTextBox.ReadOnly = true;
            this.UpdateFirmwarePathTextBox.Size = new System.Drawing.Size(185, 20);
            this.UpdateFirmwarePathTextBox.TabIndex = 22;
            // 
            // UpdateFirmwareBrowseButton
            // 
            this.UpdateFirmwareBrowseButton.Location = new System.Drawing.Point(385, 91);
            this.UpdateFirmwareBrowseButton.Name = "UpdateFirmwareBrowseButton";
            this.UpdateFirmwareBrowseButton.Size = new System.Drawing.Size(106, 20);
            this.UpdateFirmwareBrowseButton.TabIndex = 23;
            this.UpdateFirmwareBrowseButton.Text = "Browse";
            this.UpdateFirmwareBrowseButton.UseVisualStyleBackColor = true;
            this.UpdateFirmwareBrowseButton.Click += new System.EventHandler(this.browseOpenFile_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 581);
            this.Controls.Add(this.UpdateFirmwareBrowseButton);
            this.Controls.Add(this.UpdateFirmwarePathTextBox);
            this.Controls.Add(this.UpdateFirmwareButton);
            this.Controls.Add(this.UpdateTaylorTestsFileBrowseButton);
            this.Controls.Add(this.UpdateTaylorTestsPathTextBox);
            this.Controls.Add(this.UpdateTaylorTestsFileButton);
            this.Controls.Add(this.UpdateUserTestsPathTextBox);
            this.Controls.Add(this.UpdateUserTestsFileButton);
            this.Controls.Add(this.UpdateUserTestsFileBrowseButton);
            this.Controls.Add(this.SaveTestResultsButton);
            this.Controls.Add(this.SaveTestResultsBrowseButton);
            this.Controls.Add(this.SaveTestResultsPathTextBox);
            this.Controls.Add(this.SaveUserTestsFileBrowseButton);
            this.Controls.Add(this.SaveUserTestsPathTextBox);
            this.Controls.Add(this.SaveUserTestsFileButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.testFileVersionTextBox);
            this.Controls.Add(this.firmwareVersionTextBox);
            this.Controls.Add(this.listBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox firmwareVersionTextBox;
        private System.Windows.Forms.TextBox testFileVersionTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button SaveUserTestsFileButton;
        private System.Windows.Forms.TextBox SaveUserTestsPathTextBox;
        private System.Windows.Forms.Button SaveUserTestsFileBrowseButton;
        private System.Windows.Forms.TextBox SaveTestResultsPathTextBox;
        private System.Windows.Forms.Button SaveTestResultsBrowseButton;
        private System.Windows.Forms.Button SaveTestResultsButton;
        private System.Windows.Forms.Button UpdateUserTestsFileBrowseButton;
        private System.Windows.Forms.Button UpdateUserTestsFileButton;
        private System.Windows.Forms.TextBox UpdateUserTestsPathTextBox;
        private System.Windows.Forms.Button UpdateTaylorTestsFileButton;
        private System.Windows.Forms.TextBox UpdateTaylorTestsPathTextBox;
        private System.Windows.Forms.Button UpdateTaylorTestsFileBrowseButton;
        private System.Windows.Forms.Button UpdateFirmwareButton;
        private System.Windows.Forms.TextBox UpdateFirmwarePathTextBox;
        private System.Windows.Forms.Button UpdateFirmwareBrowseButton;
    }
}

