namespace RotoTools
{
    partial class ConectorHerrajeCombinar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConectorHerrajeCombinar));
            chk_Predefinido = new CheckBox();
            txt_ConectorName = new TextBox();
            lbl_NombreConector = new Label();
            listBox_AllConectores = new ListBox();
            btn_Add = new Button();
            btn_Add_All = new Button();
            btn_DeleteAll = new Button();
            btn_Delete = new Button();
            listBox_Combinar = new ListBox();
            btn_up = new Button();
            btn_down = new Button();
            btn_Guardar = new Button();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            lbl_ConectorActivo = new ToolStripStatusLabel();
            lbl_ConectoresBD = new Label();
            lbl_ConectoresSeleccionados = new Label();
            groupCrearNuevo = new GroupBox();
            rb_NuevoConector = new RadioButton();
            groupExistente = new GroupBox();
            cmb_Conectores = new ComboBox();
            lbl_SeleccionarConector = new Label();
            rb_InsertarEnConector = new RadioButton();
            statusStrip1.SuspendLayout();
            groupCrearNuevo.SuspendLayout();
            groupExistente.SuspendLayout();
            SuspendLayout();
            // 
            // chk_Predefinido
            // 
            chk_Predefinido.AutoSize = true;
            chk_Predefinido.BackColor = Color.Transparent;
            chk_Predefinido.Location = new Point(323, 24);
            chk_Predefinido.Name = "chk_Predefinido";
            chk_Predefinido.Size = new Size(155, 19);
            chk_Predefinido.TabIndex = 21;
            chk_Predefinido.Text = "Poner como predefinido";
            chk_Predefinido.UseVisualStyleBackColor = false;
            // 
            // txt_ConectorName
            // 
            txt_ConectorName.Location = new Point(133, 20);
            txt_ConectorName.MaxLength = 25;
            txt_ConectorName.Name = "txt_ConectorName";
            txt_ConectorName.Size = new Size(174, 23);
            txt_ConectorName.TabIndex = 20;
            // 
            // lbl_NombreConector
            // 
            lbl_NombreConector.AutoSize = true;
            lbl_NombreConector.BackColor = Color.Transparent;
            lbl_NombreConector.Location = new Point(13, 25);
            lbl_NombreConector.Name = "lbl_NombreConector";
            lbl_NombreConector.Size = new Size(103, 15);
            lbl_NombreConector.TabIndex = 19;
            lbl_NombreConector.Text = "Nombre Conector";
            // 
            // listBox_AllConectores
            // 
            listBox_AllConectores.FormattingEnabled = true;
            listBox_AllConectores.ItemHeight = 15;
            listBox_AllConectores.Location = new Point(89, 199);
            listBox_AllConectores.Margin = new Padding(3, 2, 3, 2);
            listBox_AllConectores.Name = "listBox_AllConectores";
            listBox_AllConectores.Size = new Size(186, 124);
            listBox_AllConectores.Sorted = true;
            listBox_AllConectores.TabIndex = 22;
            // 
            // btn_Add
            // 
            btn_Add.BackColor = Color.White;
            btn_Add.Location = new Point(281, 199);
            btn_Add.Margin = new Padding(3, 2, 3, 2);
            btn_Add.Name = "btn_Add";
            btn_Add.Size = new Size(35, 20);
            btn_Add.TabIndex = 23;
            btn_Add.Text = ">";
            btn_Add.UseVisualStyleBackColor = false;
            btn_Add.Click += btn_Add_Click;
            // 
            // btn_Add_All
            // 
            btn_Add_All.BackColor = Color.White;
            btn_Add_All.Location = new Point(281, 224);
            btn_Add_All.Margin = new Padding(3, 2, 3, 2);
            btn_Add_All.Name = "btn_Add_All";
            btn_Add_All.Size = new Size(35, 23);
            btn_Add_All.TabIndex = 24;
            btn_Add_All.Text = ">>";
            btn_Add_All.UseVisualStyleBackColor = false;
            btn_Add_All.Click += btn_Add_All_Click;
            // 
            // btn_DeleteAll
            // 
            btn_DeleteAll.BackColor = Color.White;
            btn_DeleteAll.Location = new Point(281, 296);
            btn_DeleteAll.Margin = new Padding(3, 2, 3, 2);
            btn_DeleteAll.Name = "btn_DeleteAll";
            btn_DeleteAll.Size = new Size(35, 23);
            btn_DeleteAll.TabIndex = 26;
            btn_DeleteAll.Text = "<<";
            btn_DeleteAll.UseVisualStyleBackColor = false;
            btn_DeleteAll.Click += btn_DeleteAll_Click;
            // 
            // btn_Delete
            // 
            btn_Delete.BackColor = Color.White;
            btn_Delete.Location = new Point(281, 272);
            btn_Delete.Margin = new Padding(3, 2, 3, 2);
            btn_Delete.Name = "btn_Delete";
            btn_Delete.Size = new Size(35, 20);
            btn_Delete.TabIndex = 25;
            btn_Delete.Text = "<";
            btn_Delete.UseVisualStyleBackColor = false;
            btn_Delete.Click += btn_Delete_Click;
            // 
            // listBox_Combinar
            // 
            listBox_Combinar.FormattingEnabled = true;
            listBox_Combinar.ItemHeight = 15;
            listBox_Combinar.Location = new Point(322, 199);
            listBox_Combinar.Margin = new Padding(3, 2, 3, 2);
            listBox_Combinar.Name = "listBox_Combinar";
            listBox_Combinar.Size = new Size(186, 124);
            listBox_Combinar.TabIndex = 27;
            // 
            // btn_up
            // 
            btn_up.BackColor = Color.White;
            btn_up.Location = new Point(514, 199);
            btn_up.Margin = new Padding(3, 2, 3, 2);
            btn_up.Name = "btn_up";
            btn_up.Size = new Size(35, 20);
            btn_up.TabIndex = 29;
            btn_up.Text = "^";
            btn_up.UseVisualStyleBackColor = false;
            btn_up.Click += btn_up_Click;
            // 
            // btn_down
            // 
            btn_down.BackColor = Color.White;
            btn_down.Location = new Point(514, 220);
            btn_down.Margin = new Padding(3, 2, 3, 2);
            btn_down.Name = "btn_down";
            btn_down.Size = new Size(35, 20);
            btn_down.TabIndex = 30;
            btn_down.Text = "v";
            btn_down.UseVisualStyleBackColor = false;
            btn_down.Click += btn_down_Click;
            // 
            // btn_Guardar
            // 
            btn_Guardar.BackColor = Color.White;
            btn_Guardar.Image = (Image)resources.GetObject("btn_Guardar.Image");
            btn_Guardar.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Guardar.Location = new Point(472, 335);
            btn_Guardar.Margin = new Padding(3, 2, 3, 2);
            btn_Guardar.Name = "btn_Guardar";
            btn_Guardar.Padding = new Padding(2, 0, 0, 0);
            btn_Guardar.Size = new Size(87, 41);
            btn_Guardar.TabIndex = 35;
            btn_Guardar.Text = "Guardar";
            btn_Guardar.TextAlign = ContentAlignment.MiddleRight;
            btn_Guardar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_Guardar.UseVisualStyleBackColor = false;
            btn_Guardar.Click += bnt_Guardar_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion, lbl_ConectorActivo });
            statusStrip1.Location = new Point(0, 378);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(765, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 36;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(632, 17);
            lbl_Conexion.Spring = true;
            lbl_Conexion.Text = "toolStripStatusLabel1";
            lbl_Conexion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbl_ConectorActivo
            // 
            lbl_ConectorActivo.ForeColor = Color.White;
            lbl_ConectorActivo.Name = "lbl_ConectorActivo";
            lbl_ConectorActivo.Size = new Size(118, 17);
            lbl_ConectorActivo.Text = "toolStripStatusLabel1";
            lbl_ConectorActivo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lbl_ConectoresBD
            // 
            lbl_ConectoresBD.AutoSize = true;
            lbl_ConectoresBD.BackColor = Color.Transparent;
            lbl_ConectoresBD.Location = new Point(89, 178);
            lbl_ConectoresBD.Name = "lbl_ConectoresBD";
            lbl_ConectoresBD.Size = new Size(170, 15);
            lbl_ConectoresBD.TabIndex = 37;
            lbl_ConectoresBD.Text = "Conectores en la base de datos";
            // 
            // lbl_ConectoresSeleccionados
            // 
            lbl_ConectoresSeleccionados.AutoSize = true;
            lbl_ConectoresSeleccionados.BackColor = Color.Transparent;
            lbl_ConectoresSeleccionados.Location = new Point(322, 178);
            lbl_ConectoresSeleccionados.Name = "lbl_ConectoresSeleccionados";
            lbl_ConectoresSeleccionados.Size = new Size(130, 15);
            lbl_ConectoresSeleccionados.TabIndex = 38;
            lbl_ConectoresSeleccionados.Text = "Conectores a combinar";
            // 
            // groupCrearNuevo
            // 
            groupCrearNuevo.BackColor = Color.Transparent;
            groupCrearNuevo.Controls.Add(txt_ConectorName);
            groupCrearNuevo.Controls.Add(lbl_NombreConector);
            groupCrearNuevo.Controls.Add(chk_Predefinido);
            groupCrearNuevo.Location = new Point(61, 29);
            groupCrearNuevo.Name = "groupCrearNuevo";
            groupCrearNuevo.Size = new Size(498, 58);
            groupCrearNuevo.TabIndex = 39;
            groupCrearNuevo.TabStop = false;
            // 
            // rb_NuevoConector
            // 
            rb_NuevoConector.AutoSize = true;
            rb_NuevoConector.BackColor = Color.Transparent;
            rb_NuevoConector.Location = new Point(61, 25);
            rb_NuevoConector.Name = "rb_NuevoConector";
            rb_NuevoConector.Size = new Size(141, 19);
            rb_NuevoConector.TabIndex = 22;
            rb_NuevoConector.TabStop = true;
            rb_NuevoConector.Text = "Crear nuevo Conector";
            rb_NuevoConector.UseVisualStyleBackColor = false;
            rb_NuevoConector.CheckedChanged += rb_NuevoConector_CheckedChanged;
            // 
            // groupExistente
            // 
            groupExistente.BackColor = Color.Transparent;
            groupExistente.Controls.Add(cmb_Conectores);
            groupExistente.Controls.Add(lbl_SeleccionarConector);
            groupExistente.Location = new Point(60, 110);
            groupExistente.Name = "groupExistente";
            groupExistente.Size = new Size(499, 51);
            groupExistente.TabIndex = 40;
            groupExistente.TabStop = false;
            // 
            // cmb_Conectores
            // 
            cmb_Conectores.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Conectores.FormattingEnabled = true;
            cmb_Conectores.Location = new Point(134, 17);
            cmb_Conectores.Name = "cmb_Conectores";
            cmb_Conectores.Size = new Size(174, 23);
            cmb_Conectores.TabIndex = 13;
            cmb_Conectores.SelectedIndexChanged += cmb_Conectores_SelectedIndexChanged;
            // 
            // lbl_SeleccionarConector
            // 
            lbl_SeleccionarConector.AutoSize = true;
            lbl_SeleccionarConector.Location = new Point(14, 20);
            lbl_SeleccionarConector.Name = "lbl_SeleccionarConector";
            lbl_SeleccionarConector.Size = new Size(56, 15);
            lbl_SeleccionarConector.TabIndex = 0;
            lbl_SeleccionarConector.Text = "Conector";
            // 
            // rb_InsertarEnConector
            // 
            rb_InsertarEnConector.AutoSize = true;
            rb_InsertarEnConector.BackColor = Color.Transparent;
            rb_InsertarEnConector.Location = new Point(61, 103);
            rb_InsertarEnConector.Name = "rb_InsertarEnConector";
            rb_InsertarEnConector.Size = new Size(182, 19);
            rb_InsertarEnConector.TabIndex = 41;
            rb_InsertarEnConector.TabStop = true;
            rb_InsertarEnConector.Text = "Insertar en Conector existente";
            rb_InsertarEnConector.UseVisualStyleBackColor = false;
            rb_InsertarEnConector.CheckedChanged += rb_InsertarEnConector_CheckedChanged;
            // 
            // ConectorHerrajeCombinar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(765, 400);
            Controls.Add(rb_InsertarEnConector);
            Controls.Add(groupExistente);
            Controls.Add(rb_NuevoConector);
            Controls.Add(groupCrearNuevo);
            Controls.Add(lbl_ConectoresSeleccionados);
            Controls.Add(lbl_ConectoresBD);
            Controls.Add(statusStrip1);
            Controls.Add(btn_Guardar);
            Controls.Add(btn_down);
            Controls.Add(btn_up);
            Controls.Add(listBox_Combinar);
            Controls.Add(btn_DeleteAll);
            Controls.Add(btn_Delete);
            Controls.Add(btn_Add_All);
            Controls.Add(btn_Add);
            Controls.Add(listBox_AllConectores);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "ConectorHerrajeCombinar";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Combinar Conectores";
            Load += CombinarConectores_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupCrearNuevo.ResumeLayout(false);
            groupCrearNuevo.PerformLayout();
            groupExistente.ResumeLayout(false);
            groupExistente.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chk_Predefinido;
        private TextBox txt_ConectorName;
        private Label lbl_NombreConector;
        private ListBox listBox_AllConectores;
        private Button btn_Add;
        private Button btn_Add_All;
        private Button btn_DeleteAll;
        private Button btn_Delete;
        private ListBox listBox_Combinar;
        private Button btn_up;
        private Button btn_down;
        private Button btn_Guardar;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Label lbl_ConectoresBD;
        private Label lbl_ConectoresSeleccionados;
        private ToolStripStatusLabel lbl_ConectorActivo;
        private GroupBox groupCrearNuevo;
        private RadioButton rb_NuevoConector;
        private GroupBox groupExistente;
        private RadioButton rb_InsertarEnConector;
        private Label lbl_SeleccionarConector;
        private ComboBox cmb_Conectores;
    }
}