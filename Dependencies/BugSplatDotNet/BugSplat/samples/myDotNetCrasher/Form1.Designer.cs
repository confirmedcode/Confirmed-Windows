namespace myDotNetCrasher
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
            this.InnerException = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.ManagedThreadException = new System.Windows.Forms.Button();
            this.IntDivideByZero = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.unwatchedException = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // InnerException
            // 
            this.InnerException.AllowDrop = true;
            this.InnerException.Location = new System.Drawing.Point(845, 231);
            this.InnerException.Margin = new System.Windows.Forms.Padding(6);
            this.InnerException.Name = "InnerException";
            this.InnerException.Size = new System.Drawing.Size(224, 86);
            this.InnerException.TabIndex = 15;
            this.InnerException.Text = "Inner Exception";
            this.InnerException.Click += new System.EventHandler(this.InnerExceptionButton_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(845, 124);
            this.button4.Margin = new System.Windows.Forms.Padding(6);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(224, 86);
            this.button4.TabIndex = 14;
            this.button4.Text = "Send Additional Files";
            this.button4.Click += new System.EventHandler(this.SendAdditionalFilesButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(844, 17);
            this.button3.Margin = new System.Windows.Forms.Padding(6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(224, 86);
            this.button3.TabIndex = 13;
            this.button3.Text = "Catch Exception";
            this.button3.Click += new System.EventHandler(this.CatchExceptionButton_Click);
            // 
            // ManagedThreadException
            // 
            this.ManagedThreadException.Location = new System.Drawing.Point(342, 231);
            this.ManagedThreadException.Margin = new System.Windows.Forms.Padding(6);
            this.ManagedThreadException.Name = "ManagedThreadException";
            this.ManagedThreadException.Size = new System.Drawing.Size(224, 86);
            this.ManagedThreadException.TabIndex = 12;
            this.ManagedThreadException.Text = "Exception In Managed Thread";
            this.ManagedThreadException.Click += new System.EventHandler(this.ManagedThreadExceptionButton_Click);
            // 
            // IntDivideByZero
            // 
            this.IntDivideByZero.Location = new System.Drawing.Point(342, 124);
            this.IntDivideByZero.Margin = new System.Windows.Forms.Padding(6);
            this.IntDivideByZero.Name = "IntDivideByZero";
            this.IntDivideByZero.Size = new System.Drawing.Size(224, 86);
            this.IntDivideByZero.TabIndex = 10;
            this.IntDivideByZero.Text = "Divide By Zero";
            this.IntDivideByZero.Click += new System.EventHandler(this.DivideByZeroButton_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(15, 337);
            this.button2.Margin = new System.Windows.Forms.Padding(6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(128, 62);
            this.button2.TabIndex = 9;
            this.button2.Text = "Exit";
            this.button2.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(342, 17);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(224, 86);
            this.button1.TabIndex = 8;
            this.button1.Text = "Memory Access Violation";
            this.button1.Click += new System.EventHandler(this.MemoryAccessViolationButton_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(593, 124);
            this.button5.Margin = new System.Windows.Forms.Padding(6);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(224, 86);
            this.button5.TabIndex = 16;
            this.button5.Text = "Exception In Native Dll";
            this.button5.Click += new System.EventHandler(this.NativeDllExceptionButton_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(593, 17);
            this.button6.Margin = new System.Windows.Forms.Padding(6);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(224, 86);
            this.button6.TabIndex = 17;
            this.button6.Text = "Exception In Managed Dll";
            this.button6.Click += new System.EventHandler(this.ManagedDllExceptionButton_Click);
            // 
            // unwatchedException
            // 
            this.unwatchedException.Location = new System.Drawing.Point(593, 231);
            this.unwatchedException.Name = "unwatchedException";
            this.unwatchedException.Size = new System.Drawing.Size(224, 86);
            this.unwatchedException.TabIndex = 18;
            this.unwatchedException.Text = "Unwatched Exception";
            this.unwatchedException.UseVisualStyleBackColor = true;
            this.unwatchedException.Click += new System.EventHandler(this.unwatchedExceptionButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::myDotNetCrasher.Properties.Resources.BugSplat_logo_square;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(304, 305);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 414);
            this.Controls.Add(this.unwatchedException);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.InnerException);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.ManagedThreadException);
            this.Controls.Add(this.IntDivideByZero);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "Form1";
            this.Text = "myDotNetCrasher";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button InnerException;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button ManagedThreadException;
        private System.Windows.Forms.Button IntDivideByZero;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button unwatchedException;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

