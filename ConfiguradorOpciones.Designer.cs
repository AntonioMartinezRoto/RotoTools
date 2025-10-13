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
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 610);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(1031, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(151, 20);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // btn_SaveConfig
            // 
            btn_SaveConfig.Image = (Image)resources.GetObject("btn_SaveConfig.Image");
            btn_SaveConfig.ImageAlign = ContentAlignment.MiddleLeft;
            btn_SaveConfig.Location = new Point(738, 551);
            btn_SaveConfig.Margin = new Padding(10, 4, 3, 4);
            btn_SaveConfig.Name = "btn_SaveConfig";
            btn_SaveConfig.Size = new Size(99, 55);
            btn_SaveConfig.TabIndex = 1;
            btn_SaveConfig.Text = "Guardar";
            btn_SaveConfig.TextAlign = ContentAlignment.MiddleRight;
            btn_SaveConfig.UseVisualStyleBackColor = true;
            btn_SaveConfig.Click += btn_SaveConfig_Click;
            // 
            // listBox_Opciones
            // 
            listBox_Opciones.FormattingEnabled = true;
            listBox_Opciones.Location = new Point(34, 109);
            listBox_Opciones.Margin = new Padding(3, 4, 3, 4);
            listBox_Opciones.Name = "listBox_Opciones";
            listBox_Opciones.Size = new Size(225, 424);
            listBox_Opciones.TabIndex = 2;
            listBox_Opciones.SelectedIndexChanged += listBox_Opciones_SelectedIndexChanged;
            // 
            // txt_Filter
            // 
            txt_Filter.Location = new Point(88, 69);
            txt_Filter.Margin = new Padding(3, 4, 3, 4);
            txt_Filter.Name = "txt_Filter";
            txt_Filter.Size = new Size(171, 27);
            txt_Filter.TabIndex = 3;
            txt_Filter.TextChanged += txt_Filter_TextChanged;
            // 
            // datagrid_ContenidoOpciones
            // 
            datagrid_ContenidoOpciones.BackgroundColor = Color.White;
            datagrid_ContenidoOpciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagrid_ContenidoOpciones.Location = new Point(293, 69);
            datagrid_ContenidoOpciones.Margin = new Padding(3, 4, 3, 4);
            datagrid_ContenidoOpciones.Name = "datagrid_ContenidoOpciones";
            datagrid_ContenidoOpciones.RowHeadersWidth = 51;
            datagrid_ContenidoOpciones.Size = new Size(544, 465);
            datagrid_ContenidoOpciones.TabIndex = 4;
            // 
            // lbl_Filtrar
            // 
            lbl_Filtrar.AutoSize = true;
            lbl_Filtrar.BackColor = Color.Transparent;
            lbl_Filtrar.Location = new Point(39, 73);
            lbl_Filtrar.Name = "lbl_Filtrar";
            lbl_Filtrar.Size = new Size(47, 20);
            lbl_Filtrar.TabIndex = 5;
            lbl_Filtrar.Text = "Filtrar";
            // 
            // ConfiguradorOpciones
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1031, 636);
            Controls.Add(lbl_Filtrar);
            Controls.Add(datagrid_ContenidoOpciones);
            Controls.Add(txt_Filter);
            Controls.Add(listBox_Opciones);
            Controls.Add(btn_SaveConfig);
            Controls.Add(statusStrip1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
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