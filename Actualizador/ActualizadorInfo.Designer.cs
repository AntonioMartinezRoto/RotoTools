namespace RotoTools.Actualizador
{
    partial class ActualizadorInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActualizadorInfo));
            group_InfoActualizacionPVC = new GroupBox();
            lbl_PVCData = new Label();
            lbl_PVCFile = new Label();
            lbl_FechaPVC = new Label();
            lbl_XmlPVC = new Label();
            btn_RefreshPVC = new Button();
            group_InfoActualizacionALU = new GroupBox();
            lbl_ALUData = new Label();
            lbl_ALUFile = new Label();
            lbl_FechaALU = new Label();
            lbl_XmlALU = new Label();
            btn_RefreshALU = new Button();
            group_InfoActualizacionPAX = new GroupBox();
            lbl_PAXData = new Label();
            lbl_PAXFile = new Label();
            lbl_FechaPAX = new Label();
            lbl_XmlPAX = new Label();
            btn_RefreshPAX = new Button();
            group_InfoActualizacionPVC.SuspendLayout();
            group_InfoActualizacionALU.SuspendLayout();
            group_InfoActualizacionPAX.SuspendLayout();
            SuspendLayout();
            // 
            // group_InfoActualizacionPVC
            // 
            group_InfoActualizacionPVC.BackColor = Color.Transparent;
            group_InfoActualizacionPVC.Controls.Add(lbl_PVCData);
            group_InfoActualizacionPVC.Controls.Add(lbl_PVCFile);
            group_InfoActualizacionPVC.Controls.Add(lbl_FechaPVC);
            group_InfoActualizacionPVC.Controls.Add(lbl_XmlPVC);
            group_InfoActualizacionPVC.Controls.Add(btn_RefreshPVC);
            group_InfoActualizacionPVC.Location = new Point(90, 33);
            group_InfoActualizacionPVC.Name = "group_InfoActualizacionPVC";
            group_InfoActualizacionPVC.Size = new Size(426, 73);
            group_InfoActualizacionPVC.TabIndex = 12;
            group_InfoActualizacionPVC.TabStop = false;
            group_InfoActualizacionPVC.Text = "Información última actualización";
            // 
            // lbl_PVCData
            // 
            lbl_PVCData.Location = new Point(119, 45);
            lbl_PVCData.Name = "lbl_PVCData";
            lbl_PVCData.Size = new Size(222, 15);
            lbl_PVCData.TabIndex = 14;
            // 
            // lbl_PVCFile
            // 
            lbl_PVCFile.Location = new Point(119, 21);
            lbl_PVCFile.Name = "lbl_PVCFile";
            lbl_PVCFile.Size = new Size(222, 16);
            lbl_PVCFile.TabIndex = 13;
            // 
            // lbl_FechaPVC
            // 
            lbl_FechaPVC.AutoSize = true;
            lbl_FechaPVC.Location = new Point(59, 45);
            lbl_FechaPVC.Name = "lbl_FechaPVC";
            lbl_FechaPVC.Size = new Size(41, 15);
            lbl_FechaPVC.TabIndex = 12;
            lbl_FechaPVC.Text = "Fecha:";
            // 
            // lbl_XmlPVC
            // 
            lbl_XmlPVC.AutoSize = true;
            lbl_XmlPVC.Location = new Point(63, 22);
            lbl_XmlPVC.Name = "lbl_XmlPVC";
            lbl_XmlPVC.Size = new Size(34, 15);
            lbl_XmlPVC.TabIndex = 11;
            lbl_XmlPVC.Text = "XML:";
            // 
            // btn_RefreshPVC
            // 
            btn_RefreshPVC.BackgroundImage = (Image)resources.GetObject("btn_RefreshPVC.BackgroundImage");
            btn_RefreshPVC.BackgroundImageLayout = ImageLayout.Stretch;
            btn_RefreshPVC.Location = new Point(363, 22);
            btn_RefreshPVC.Name = "btn_RefreshPVC";
            btn_RefreshPVC.Size = new Size(32, 32);
            btn_RefreshPVC.TabIndex = 9;
            btn_RefreshPVC.UseVisualStyleBackColor = true;
            btn_RefreshPVC.Click += btn_RefreshPVC_Click;
            // 
            // group_InfoActualizacionALU
            // 
            group_InfoActualizacionALU.BackColor = Color.Transparent;
            group_InfoActualizacionALU.Controls.Add(lbl_ALUData);
            group_InfoActualizacionALU.Controls.Add(lbl_ALUFile);
            group_InfoActualizacionALU.Controls.Add(lbl_FechaALU);
            group_InfoActualizacionALU.Controls.Add(lbl_XmlALU);
            group_InfoActualizacionALU.Controls.Add(btn_RefreshALU);
            group_InfoActualizacionALU.Location = new Point(90, 123);
            group_InfoActualizacionALU.Name = "group_InfoActualizacionALU";
            group_InfoActualizacionALU.Size = new Size(426, 73);
            group_InfoActualizacionALU.TabIndex = 13;
            group_InfoActualizacionALU.TabStop = false;
            group_InfoActualizacionALU.Text = "Información última actualización";
            // 
            // lbl_ALUData
            // 
            lbl_ALUData.Location = new Point(119, 45);
            lbl_ALUData.Name = "lbl_ALUData";
            lbl_ALUData.Size = new Size(222, 15);
            lbl_ALUData.TabIndex = 14;
            // 
            // lbl_ALUFile
            // 
            lbl_ALUFile.Location = new Point(119, 21);
            lbl_ALUFile.Name = "lbl_ALUFile";
            lbl_ALUFile.Size = new Size(222, 16);
            lbl_ALUFile.TabIndex = 13;
            // 
            // lbl_FechaALU
            // 
            lbl_FechaALU.AutoSize = true;
            lbl_FechaALU.Location = new Point(59, 45);
            lbl_FechaALU.Name = "lbl_FechaALU";
            lbl_FechaALU.Size = new Size(41, 15);
            lbl_FechaALU.TabIndex = 12;
            lbl_FechaALU.Text = "Fecha:";
            // 
            // lbl_XmlALU
            // 
            lbl_XmlALU.AutoSize = true;
            lbl_XmlALU.Location = new Point(63, 22);
            lbl_XmlALU.Name = "lbl_XmlALU";
            lbl_XmlALU.Size = new Size(34, 15);
            lbl_XmlALU.TabIndex = 11;
            lbl_XmlALU.Text = "XML:";
            // 
            // btn_RefreshALU
            // 
            btn_RefreshALU.BackgroundImage = (Image)resources.GetObject("btn_RefreshALU.BackgroundImage");
            btn_RefreshALU.BackgroundImageLayout = ImageLayout.Stretch;
            btn_RefreshALU.Location = new Point(363, 22);
            btn_RefreshALU.Name = "btn_RefreshALU";
            btn_RefreshALU.Size = new Size(32, 32);
            btn_RefreshALU.TabIndex = 9;
            btn_RefreshALU.UseVisualStyleBackColor = true;
            btn_RefreshALU.Click += btn_RefreshALU_Click;
            // 
            // group_InfoActualizacionPAX
            // 
            group_InfoActualizacionPAX.BackColor = Color.Transparent;
            group_InfoActualizacionPAX.Controls.Add(lbl_PAXData);
            group_InfoActualizacionPAX.Controls.Add(lbl_PAXFile);
            group_InfoActualizacionPAX.Controls.Add(lbl_FechaPAX);
            group_InfoActualizacionPAX.Controls.Add(lbl_XmlPAX);
            group_InfoActualizacionPAX.Controls.Add(btn_RefreshPAX);
            group_InfoActualizacionPAX.Location = new Point(90, 222);
            group_InfoActualizacionPAX.Name = "group_InfoActualizacionPAX";
            group_InfoActualizacionPAX.Size = new Size(426, 73);
            group_InfoActualizacionPAX.TabIndex = 14;
            group_InfoActualizacionPAX.TabStop = false;
            group_InfoActualizacionPAX.Text = "Información última actualización";
            // 
            // lbl_PAXData
            // 
            lbl_PAXData.Location = new Point(119, 45);
            lbl_PAXData.Name = "lbl_PAXData";
            lbl_PAXData.Size = new Size(222, 15);
            lbl_PAXData.TabIndex = 14;
            // 
            // lbl_PAXFile
            // 
            lbl_PAXFile.Location = new Point(119, 21);
            lbl_PAXFile.Name = "lbl_PAXFile";
            lbl_PAXFile.Size = new Size(222, 16);
            lbl_PAXFile.TabIndex = 13;
            // 
            // lbl_FechaPAX
            // 
            lbl_FechaPAX.AutoSize = true;
            lbl_FechaPAX.Location = new Point(59, 45);
            lbl_FechaPAX.Name = "lbl_FechaPAX";
            lbl_FechaPAX.Size = new Size(41, 15);
            lbl_FechaPAX.TabIndex = 12;
            lbl_FechaPAX.Text = "Fecha:";
            // 
            // lbl_XmlPAX
            // 
            lbl_XmlPAX.AutoSize = true;
            lbl_XmlPAX.Location = new Point(63, 22);
            lbl_XmlPAX.Name = "lbl_XmlPAX";
            lbl_XmlPAX.Size = new Size(34, 15);
            lbl_XmlPAX.TabIndex = 11;
            lbl_XmlPAX.Text = "XML:";
            // 
            // btn_RefreshPAX
            // 
            btn_RefreshPAX.BackgroundImage = (Image)resources.GetObject("btn_RefreshPAX.BackgroundImage");
            btn_RefreshPAX.BackgroundImageLayout = ImageLayout.Stretch;
            btn_RefreshPAX.Location = new Point(363, 22);
            btn_RefreshPAX.Name = "btn_RefreshPAX";
            btn_RefreshPAX.Size = new Size(32, 32);
            btn_RefreshPAX.TabIndex = 9;
            btn_RefreshPAX.UseVisualStyleBackColor = true;
            btn_RefreshPAX.Click += btn_RefreshPAX_Click;
            // 
            // ActualizadorInfo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(680, 337);
            Controls.Add(group_InfoActualizacionPAX);
            Controls.Add(group_InfoActualizacionALU);
            Controls.Add(group_InfoActualizacionPVC);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ActualizadorInfo";
            StartPosition = FormStartPosition.CenterScreen;
            Load += ActualizadorInfo_Load;
            group_InfoActualizacionPVC.ResumeLayout(false);
            group_InfoActualizacionPVC.PerformLayout();
            group_InfoActualizacionALU.ResumeLayout(false);
            group_InfoActualizacionALU.PerformLayout();
            group_InfoActualizacionPAX.ResumeLayout(false);
            group_InfoActualizacionPAX.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox group_InfoActualizacionPVC;
        private Label lbl_FechaPVC;
        private Label lbl_XmlPVC;
        private Button btn_RefreshPVC;
        private Label lbl_PVCData;
        private Label lbl_PVCFile;
        private GroupBox group_InfoActualizacionALU;
        private Label lbl_ALUData;
        private Label lbl_ALUFile;
        private Label lbl_FechaALU;
        private Label lbl_XmlALU;
        private Button btn_RefreshALU;
        private GroupBox group_InfoActualizacionPAX;
        private Label lbl_PAXData;
        private Label lbl_PAXFile;
        private Label lbl_FechaPAX;
        private Label lbl_XmlPAX;
        private Button btn_RefreshPAX;
    }
}