namespace RotoTools
{
    partial class ConfiguradorOpciones
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfiguradorOpciones));
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            btn_SaveConfig = new Button();
            listBox_Opciones = new ListBox();
            txt_Filter = new TextBox();
            datagrid_ContenidoOpciones = new DataGridView();
            lbl_Filtrar = new Label();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagrid_ContenidoOpciones).BeginInit();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 455);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(902, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // btn_SaveConfig
            // 
            btn_SaveConfig.Image = (Image)resources.GetObject("btn_SaveConfig.Image");
            btn_SaveConfig.ImageAlign = ContentAlignment.MiddleLeft;
            btn_SaveConfig.Location = new Point(650, 411);
            btn_SaveConfig.Name = "btn_SaveConfig";
            btn_SaveConfig.Size = new Size(82, 41);
            btn_SaveConfig.TabIndex = 1;
            btn_SaveConfig.Text = "Guardar";
            btn_SaveConfig.TextAlign = ContentAlignment.MiddleRight;
            btn_SaveConfig.UseVisualStyleBackColor = true;
            btn_SaveConfig.Click += btn_SaveConfig_Click;
            // 
            // listBox_Opciones
            // 
            listBox_Opciones.FormattingEnabled = true;
            listBox_Opciones.ItemHeight = 15;
            listBox_Opciones.Location = new Point(30, 82);
            listBox_Opciones.Name = "listBox_Opciones";
            listBox_Opciones.Size = new Size(197, 319);
            listBox_Opciones.TabIndex = 2;
            listBox_Opciones.SelectedIndexChanged += listBox_Opciones_SelectedIndexChanged;
            // 
            // txt_Filter
            // 
            txt_Filter.Location = new Point(77, 52);
            txt_Filter.Name = "txt_Filter";
            txt_Filter.Size = new Size(150, 23);
            txt_Filter.TabIndex = 3;
            txt_Filter.TextChanged += txt_Filter_TextChanged;
            // 
            // datagrid_ContenidoOpciones
            // 
            datagrid_ContenidoOpciones.BackgroundColor = Color.White;
            datagrid_ContenidoOpciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagrid_ContenidoOpciones.Location = new Point(256, 52);
            datagrid_ContenidoOpciones.Name = "datagrid_ContenidoOpciones";
            datagrid_ContenidoOpciones.Size = new Size(476, 349);
            datagrid_ContenidoOpciones.TabIndex = 4;
            // 
            // lbl_Filtrar
            // 
            lbl_Filtrar.AutoSize = true;
            lbl_Filtrar.BackColor = Color.Transparent;
            lbl_Filtrar.Location = new Point(34, 55);
            lbl_Filtrar.Name = "lbl_Filtrar";
            lbl_Filtrar.Size = new Size(37, 15);
            lbl_Filtrar.TabIndex = 5;
            lbl_Filtrar.Text = "Filtrar";
            // 
            // ConfiguradorOpciones
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(902, 477);
            Controls.Add(lbl_Filtrar);
            Controls.Add(datagrid_ContenidoOpciones);
            Controls.Add(txt_Filter);
            Controls.Add(listBox_Opciones);
            Controls.Add(btn_SaveConfig);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ConfiguradorOpciones";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Configurador de opciones";
            Load += ConfiguradorOpciones_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)datagrid_ContenidoOpciones).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Button btn_SaveConfig;
        private ListBox listBox_Opciones;
        private TextBox txt_Filter;
        private DataGridView datagrid_ContenidoOpciones;
        private Label lbl_Filtrar;
    }
}