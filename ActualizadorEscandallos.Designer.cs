namespace RotoTools
{
    partial class ActualizadorEscandallos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActualizadorEscandallos));
            lbl_Filtrar = new Label();
            txt_Filter = new TextBox();
            listBox_Escandallos = new ListBox();
            txt_ContenidoEscandallo = new TextBox();
            SuspendLayout();
            // 
            // lbl_Filtrar
            // 
            lbl_Filtrar.AutoSize = true;
            lbl_Filtrar.BackColor = Color.Transparent;
            lbl_Filtrar.Location = new Point(16, 27);
            lbl_Filtrar.Name = "lbl_Filtrar";
            lbl_Filtrar.Size = new Size(37, 15);
            lbl_Filtrar.TabIndex = 9;
            lbl_Filtrar.Text = "Filtrar";
            // 
            // txt_Filter
            // 
            txt_Filter.Location = new Point(59, 24);
            txt_Filter.Name = "txt_Filter";
            txt_Filter.Size = new Size(150, 23);
            txt_Filter.TabIndex = 7;
            txt_Filter.TextChanged += txt_Filter_TextChanged;
            // 
            // listBox_Escandallos
            // 
            listBox_Escandallos.FormattingEnabled = true;
            listBox_Escandallos.ItemHeight = 15;
            listBox_Escandallos.Location = new Point(12, 54);
            listBox_Escandallos.Name = "listBox_Escandallos";
            listBox_Escandallos.Size = new Size(197, 664);
            listBox_Escandallos.TabIndex = 6;
            listBox_Escandallos.SelectedIndexChanged += listBox_Escandallos_SelectedIndexChanged;
            // 
            // txt_ContenidoEscandallo
            // 
            txt_ContenidoEscandallo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_ContenidoEscandallo.Location = new Point(226, 54);
            txt_ContenidoEscandallo.Multiline = true;
            txt_ContenidoEscandallo.Name = "txt_ContenidoEscandallo";
            txt_ContenidoEscandallo.ScrollBars = ScrollBars.Both;
            txt_ContenidoEscandallo.Size = new Size(901, 664);
            txt_ContenidoEscandallo.TabIndex = 10;
            txt_ContenidoEscandallo.WordWrap = false;
            // 
            // ActualizadorEscandallos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1384, 748);
            Controls.Add(txt_ContenidoEscandallo);
            Controls.Add(lbl_Filtrar);
            Controls.Add(txt_Filter);
            Controls.Add(listBox_Escandallos);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ActualizadorEscandallos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Escandallos";
            Load += ActualizadorEscandallos_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Filtrar;
        private TextBox txt_Filter;
        private ListBox listBox_Escandallos;
        private TextBox txt_ContenidoEscandallo;
    }
}