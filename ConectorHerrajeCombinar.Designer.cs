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
            bnt_Guardar = new Button();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            label1 = new Label();
            label2 = new Label();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // chk_Predefinido
            // 
            chk_Predefinido.AutoSize = true;
            chk_Predefinido.BackColor = Color.Transparent;
            chk_Predefinido.Location = new Point(437, 87);
            chk_Predefinido.Margin = new Padding(3, 4, 3, 4);
            chk_Predefinido.Name = "chk_Predefinido";
            chk_Predefinido.Size = new Size(192, 24);
            chk_Predefinido.TabIndex = 21;
            chk_Predefinido.Text = "Poner como predefinido";
            chk_Predefinido.UseVisualStyleBackColor = false;
            // 
            // txt_ConectorName
            // 
            txt_ConectorName.Location = new Point(214, 84);
            txt_ConectorName.Margin = new Padding(3, 4, 3, 4);
            txt_ConectorName.Name = "txt_ConectorName";
            txt_ConectorName.Size = new Size(198, 27);
            txt_ConectorName.TabIndex = 20;
            // 
            // lbl_NombreConector
            // 
            lbl_NombreConector.AutoSize = true;
            lbl_NombreConector.BackColor = Color.Transparent;
            lbl_NombreConector.Location = new Point(80, 88);
            lbl_NombreConector.Name = "lbl_NombreConector";
            lbl_NombreConector.Size = new Size(128, 20);
            lbl_NombreConector.TabIndex = 19;
            lbl_NombreConector.Text = "Nombre Conector";
            // 
            // listBox_AllConectores
            // 
            listBox_AllConectores.FormattingEnabled = true;
            listBox_AllConectores.Location = new Point(90, 197);
            listBox_AllConectores.Name = "listBox_AllConectores";
            listBox_AllConectores.Size = new Size(212, 164);
            listBox_AllConectores.Sorted = true;
            listBox_AllConectores.TabIndex = 22;
            // 
            // btn_Add
            // 
            btn_Add.BackColor = Color.White;
            btn_Add.Location = new Point(310, 197);
            btn_Add.Name = "btn_Add";
            btn_Add.Size = new Size(40, 27);
            btn_Add.TabIndex = 23;
            btn_Add.Text = ">";
            btn_Add.UseVisualStyleBackColor = false;
            btn_Add.Click += btn_Add_Click;
            // 
            // btn_Add_All
            // 
            btn_Add_All.BackColor = Color.White;
            btn_Add_All.Location = new Point(310, 231);
            btn_Add_All.Name = "btn_Add_All";
            btn_Add_All.Size = new Size(40, 31);
            btn_Add_All.TabIndex = 24;
            btn_Add_All.Text = ">>";
            btn_Add_All.UseVisualStyleBackColor = false;
            btn_Add_All.Click += btn_Add_All_Click;
            // 
            // btn_DeleteAll
            // 
            btn_DeleteAll.BackColor = Color.White;
            btn_DeleteAll.Location = new Point(310, 328);
            btn_DeleteAll.Name = "btn_DeleteAll";
            btn_DeleteAll.Size = new Size(40, 31);
            btn_DeleteAll.TabIndex = 26;
            btn_DeleteAll.Text = "<<";
            btn_DeleteAll.UseVisualStyleBackColor = false;
            btn_DeleteAll.Click += btn_DeleteAll_Click;
            // 
            // btn_Delete
            // 
            btn_Delete.BackColor = Color.White;
            btn_Delete.Location = new Point(310, 295);
            btn_Delete.Name = "btn_Delete";
            btn_Delete.Size = new Size(40, 27);
            btn_Delete.TabIndex = 25;
            btn_Delete.Text = "<";
            btn_Delete.UseVisualStyleBackColor = false;
            btn_Delete.Click += btn_Delete_Click;
            // 
            // listBox_Combinar
            // 
            listBox_Combinar.FormattingEnabled = true;
            listBox_Combinar.Location = new Point(357, 197);
            listBox_Combinar.Name = "listBox_Combinar";
            listBox_Combinar.Size = new Size(212, 164);
            listBox_Combinar.TabIndex = 27;
            // 
            // btn_up
            // 
            btn_up.BackColor = Color.White;
            btn_up.Location = new Point(576, 197);
            btn_up.Name = "btn_up";
            btn_up.Size = new Size(40, 27);
            btn_up.TabIndex = 29;
            btn_up.Text = "^";
            btn_up.UseVisualStyleBackColor = false;
            btn_up.Click += btn_up_Click;
            // 
            // btn_down
            // 
            btn_down.BackColor = Color.White;
            btn_down.Location = new Point(576, 235);
            btn_down.Name = "btn_down";
            btn_down.Size = new Size(40, 27);
            btn_down.TabIndex = 30;
            btn_down.Text = "v";
            btn_down.UseVisualStyleBackColor = false;
            btn_down.Click += btn_down_Click;
            // 
            // bnt_Guardar
            // 
            bnt_Guardar.BackColor = Color.White;
            bnt_Guardar.Image = (Image)resources.GetObject("bnt_Guardar.Image");
            bnt_Guardar.ImageAlign = ContentAlignment.MiddleLeft;
            bnt_Guardar.Location = new Point(514, 417);
            bnt_Guardar.Name = "bnt_Guardar";
            bnt_Guardar.Size = new Size(99, 53);
            bnt_Guardar.TabIndex = 35;
            bnt_Guardar.Text = "Guardar";
            bnt_Guardar.TextAlign = ContentAlignment.MiddleRight;
            bnt_Guardar.UseVisualStyleBackColor = false;
            bnt_Guardar.Click += bnt_Guardar_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 497);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(793, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 36;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(151, 20);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(90, 169);
            label1.Name = "label1";
            label1.Size = new Size(216, 20);
            label1.TabIndex = 37;
            label1.Text = "Conectores en la base de datos";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(357, 169);
            label2.Name = "label2";
            label2.Size = new Size(162, 20);
            label2.TabIndex = 38;
            label2.Text = "Conectores a combinar";
            // 
            // ConectorHerrajeCombinar
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(793, 523);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(statusStrip1);
            Controls.Add(bnt_Guardar);
            Controls.Add(btn_down);
            Controls.Add(btn_up);
            Controls.Add(listBox_Combinar);
            Controls.Add(btn_DeleteAll);
            Controls.Add(btn_Delete);
            Controls.Add(btn_Add_All);
            Controls.Add(btn_Add);
            Controls.Add(listBox_AllConectores);
            Controls.Add(chk_Predefinido);
            Controls.Add(txt_ConectorName);
            Controls.Add(lbl_NombreConector);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ConectorHerrajeCombinar";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Combinar Conectores";
            Load += CombinarConectores_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
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
        private Button bnt_Guardar;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Label label1;
        private Label label2;
    }
}