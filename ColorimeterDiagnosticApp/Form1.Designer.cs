﻿namespace ColorimeterDiagnosticApp
{
    partial class Form1
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.GetUserTestsFileButton = new System.Windows.Forms.Button();
            this.saveUserTestPathTextBox = new System.Windows.Forms.TextBox();
            this.getUserTestFileButton = new System.Windows.Forms.Button();
            this.testResultFileTextBox = new System.Windows.Forms.TextBox();
            this.getTestResultFileButton = new System.Windows.Forms.Button();
            this.GetTestResultsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(62, 357);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(426, 212);
            this.listBox1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(395, 34);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(76, 20);
            this.textBox1.TabIndex = 4;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(395, 87);
            this.textBox2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(76, 20);
            this.textBox2.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(339, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Firmware";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(339, 87);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Test File";
            // 
            // GetUserTestsFileButton
            // 
            this.GetUserTestsFileButton.Location = new System.Drawing.Point(355, 197);
            this.GetUserTestsFileButton.Name = "GetUserTestsFileButton";
            this.GetUserTestsFileButton.Size = new System.Drawing.Size(133, 45);
            this.GetUserTestsFileButton.TabIndex = 9;
            this.GetUserTestsFileButton.Text = "Get User Tests File";
            this.GetUserTestsFileButton.UseVisualStyleBackColor = true;
            this.GetUserTestsFileButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // saveUserTestPathTextBox
            // 
            this.saveUserTestPathTextBox.Location = new System.Drawing.Point(74, 162);
            this.saveUserTestPathTextBox.Name = "saveUserTestPathTextBox";
            this.saveUserTestPathTextBox.ReadOnly = true;
            this.saveUserTestPathTextBox.Size = new System.Drawing.Size(285, 20);
            this.saveUserTestPathTextBox.TabIndex = 10;
            // 
            // getUserTestFileButton
            // 
            this.getUserTestFileButton.Location = new System.Drawing.Point(382, 162);
            this.getUserTestFileButton.Name = "getUserTestFileButton";
            this.getUserTestFileButton.Size = new System.Drawing.Size(106, 20);
            this.getUserTestFileButton.TabIndex = 11;
            this.getUserTestFileButton.Text = "Browse";
            this.getUserTestFileButton.UseVisualStyleBackColor = true;
            this.getUserTestFileButton.Click += new System.EventHandler(this.browseUserTestSaveButton_Click);
            // 
            // testResultFileTextBox
            // 
            this.testResultFileTextBox.Location = new System.Drawing.Point(74, 258);
            this.testResultFileTextBox.Name = "testResultFileTextBox";
            this.testResultFileTextBox.ReadOnly = true;
            this.testResultFileTextBox.Size = new System.Drawing.Size(285, 20);
            this.testResultFileTextBox.TabIndex = 12;
            // 
            // getTestResultFileButton
            // 
            this.getTestResultFileButton.Location = new System.Drawing.Point(382, 258);
            this.getTestResultFileButton.Name = "getTestResultFileButton";
            this.getTestResultFileButton.Size = new System.Drawing.Size(106, 20);
            this.getTestResultFileButton.TabIndex = 13;
            this.getTestResultFileButton.Text = "Browse";
            this.getTestResultFileButton.UseVisualStyleBackColor = true;
            this.getTestResultFileButton.Click += new System.EventHandler(this.browseTestResultsFileButton_Click);
            // 
            // GetTestResultsButton
            // 
            this.GetTestResultsButton.Location = new System.Drawing.Point(355, 284);
            this.GetTestResultsButton.Name = "GetTestResultsButton";
            this.GetTestResultsButton.Size = new System.Drawing.Size(133, 45);
            this.GetTestResultsButton.TabIndex = 14;
            this.GetTestResultsButton.Text = "Get Test Results";
            this.GetTestResultsButton.UseVisualStyleBackColor = true;
            this.GetTestResultsButton.Click += new System.EventHandler(this.buttonClickHandler);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 581);
            this.Controls.Add(this.GetTestResultsButton);
            this.Controls.Add(this.getTestResultFileButton);
            this.Controls.Add(this.testResultFileTextBox);
            this.Controls.Add(this.getUserTestFileButton);
            this.Controls.Add(this.saveUserTestPathTextBox);
            this.Controls.Add(this.GetUserTestsFileButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button GetUserTestsFileButton;
        private System.Windows.Forms.TextBox saveUserTestPathTextBox;
        private System.Windows.Forms.Button getUserTestFileButton;
        private System.Windows.Forms.TextBox testResultFileTextBox;
        private System.Windows.Forms.Button getTestResultFileButton;
        private System.Windows.Forms.Button GetTestResultsButton;
    }
}

