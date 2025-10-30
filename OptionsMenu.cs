using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoTools
{
    public partial class OptionsMenu : Form
    {
        public OptionsMenu()
        {
            InitializeComponent();
        }

        private void OptionsMenu_Load(object sender, EventArgs e)
        {
            CargarTextos();
            var idiomas = new List<LanguageItem>
                {
                    new LanguageItem { Text = "Español", Value = "es" },
                    new LanguageItem { Text = "Português", Value = "pt" },
                    new LanguageItem { Text = "English", Value = "en" },
                    new LanguageItem { Text = "Italiano", Value = "it" }
                };

            cmb_Idioma.DataSource = idiomas;
            cmb_Idioma.DisplayMember = "Text";
            cmb_Idioma.ValueMember = "Value";

            // Selecciona el idioma actual
            cmb_Idioma.SelectedValue = LocalizationManager.CurrentCulture.TwoLetterISOLanguageName;
        }

        private void btn_SaveOptions_Click(object sender, EventArgs e)
        {
            if (cmb_Idioma.SelectedValue != null)
            {
                string selectedCulture = cmb_Idioma.SelectedValue.ToString();

                LocalizationManager.SetLanguage(selectedCulture);

                Properties.Settings.Default["Language"] = selectedCulture;
                Properties.Settings.Default.Save();

                Close();
            }
        }
        private void CargarTextos()
        {
            lbl_Idioma.Text = LocalizationManager.GetString("L_SeleccionarIdioma");
            this.Text = LocalizationManager.GetString("L_Opciones");
            btn_SaveOptions.Text = LocalizationManager.GetString("L_Guardar");
        }
    }
    public class LanguageItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
