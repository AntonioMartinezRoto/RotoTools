namespace RotoTools
{
    partial class ExportacionOpciones
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportacionOpciones));
            btn_SaveOptions = new Button();
            groupBoxEscandallos = new GroupBox();
            chk_FittingLength = new CheckBox();
            chk_FormatoTabla = new CheckBox();
            chk_SDId = new CheckBox();
            chk_SetId = new CheckBox();
            chk_FittingId = new CheckBox();
            chk_Position = new CheckBox();
            groupBoxEscandallos.SuspendLayout();
            SuspendLayout();
            // 
            // btn_SaveOptions
            // 
            btn_SaveOptions.Image = (Image)resources.GetObject("btn_SaveOptions.Image");
            btn_SaveOptions.Location = new Point(313, 161);
            btn_SaveOptions.Margin = new Padding(0);
            btn_SaveOptions.Name = "btn_SaveOptions";
            btn_SaveOptions.Size = new Size(87, 41);
            btn_SaveOptions.TabIndex = 8;
            btn_SaveOptions.Text = "Guardar";
            btn_SaveOptions.TextAlign = ContentAlignment.MiddleRight;
            btn_SaveOptions.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_SaveOptions.UseVisualStyleBackColor = true;
            btn_SaveOptions.Click += btn_SaveOptions_Click;
            // 
            // groupBoxEscandallos
            // 
            groupBoxEscandallos.BackColor = Color.Transparent;
            groupBoxEscandallos.Controls.Add(chk_FittingLength);
            groupBoxEscandallos.Controls.Add(chk_FormatoTabla);
            groupBoxEscandallos.Controls.Add(chk_SDId);
            groupBoxEscandallos.Controls.Add(chk_SetId);
            groupBoxEscandallos.Controls.Add(chk_FittingId);
            groupBoxEscandallos.Controls.Add(chk_Position);
            groupBoxEscandallos.Location = new Point(14, 28);
            groupBoxEscandallos.Name = "groupBoxEscandallos";
            groupBoxEscandallos.Size = new Size(386, 113);
            groupBoxEscandallos.TabIndex = 18;
            groupBoxEscandallos.TabStop = false;
            groupBoxEscandallos.Text = "Opciones exportación";
            // 
            // chk_FittingLength
            // 
            chk_FittingLength.AutoSize = true;
            chk_FittingLength.BackColor = Color.Transparent;
            chk_FittingLength.Location = new Point(217, 81);
            chk_FittingLength.Name = "chk_FittingLength";
            chk_FittingLength.Size = new Size(130, 19);
            chk_FittingLength.TabIndex = 15;
            chk_FittingLength.Text = "Ver Fitting Longitud";
            chk_FittingLength.UseVisualStyleBackColor = false;
            // 
            // chk_FormatoTabla
            // 
            chk_FormatoTabla.AutoSize = true;
            chk_FormatoTabla.BackColor = Color.Transparent;
            chk_FormatoTabla.Location = new Point(26, 31);
            chk_FormatoTabla.Name = "chk_FormatoTabla";
            chk_FormatoTabla.Size = new Size(119, 19);
            chk_FormatoTabla.TabIndex = 16;
            chk_FormatoTabla.Text = "Dar formato tabla";
            chk_FormatoTabla.UseVisualStyleBackColor = false;
            // 
            // chk_SDId
            // 
            chk_SDId.AutoSize = true;
            chk_SDId.BackColor = Color.Transparent;
            chk_SDId.Location = new Point(26, 81);
            chk_SDId.Name = "chk_SDId";
            chk_SDId.Size = new Size(134, 19);
            chk_SDId.TabIndex = 11;
            chk_SDId.Text = "Ver SetDescription Id";
            chk_SDId.UseVisualStyleBackColor = false;
            // 
            // chk_SetId
            // 
            chk_SetId.AutoSize = true;
            chk_SetId.BackColor = Color.Transparent;
            chk_SetId.Location = new Point(26, 56);
            chk_SetId.Name = "chk_SetId";
            chk_SetId.Size = new Size(74, 19);
            chk_SetId.TabIndex = 12;
            chk_SetId.Text = "Ver Set Id";
            chk_SetId.UseVisualStyleBackColor = false;
            // 
            // chk_FittingId
            // 
            chk_FittingId.AutoSize = true;
            chk_FittingId.BackColor = Color.Transparent;
            chk_FittingId.Location = new Point(217, 56);
            chk_FittingId.Name = "chk_FittingId";
            chk_FittingId.Size = new Size(92, 19);
            chk_FittingId.TabIndex = 14;
            chk_FittingId.Text = "Ver Fitting Id";
            chk_FittingId.UseVisualStyleBackColor = false;
            // 
            // chk_Position
            // 
            chk_Position.AutoSize = true;
            chk_Position.BackColor = Color.Transparent;
            chk_Position.Location = new Point(217, 31);
            chk_Position.Name = "chk_Position";
            chk_Position.Size = new Size(167, 19);
            chk_Position.TabIndex = 13;
            chk_Position.Text = "Ver SetDescription Position";
            chk_Position.UseVisualStyleBackColor = false;
            // 
            // ExportacionOpciones
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(498, 223);
            Controls.Add(groupBoxEscandallos);
            Controls.Add(btn_SaveOptions);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ExportacionOpciones";
            StartPosition = FormStartPosition.CenterScreen;
            Load += ExportacionOpciones_Load;
            groupBoxEscandallos.ResumeLayout(false);
            groupBoxEscandallos.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_SaveOptions;
        private GroupBox groupBoxEscandallos;
        private CheckBox chk_FittingLength;
        private CheckBox chk_FormatoTabla;
        private CheckBox chk_SDId;
        private CheckBox chk_SetId;
        private CheckBox chk_FittingId;
        private CheckBox chk_Position;
    }
}