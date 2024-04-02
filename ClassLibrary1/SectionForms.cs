using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using System.Data.SqlClient;

namespace RevitPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]



    public partial class SectionForms : System.Windows.Forms.Form
    {
        public SectionForms()
        {
            InitializeComponent();
            lblError.Visible = false;
            TxtDesfase.Text = "";
            TxtProfundidad.Text = "";
            TxtAltura.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (TxtDesfase.Text     != "" ||
                TxtProfundidad.Text != "" ||
                TxtAltura.Text      != "")
                {
                    if (Convert.ToDouble(TxtDesfase.Text) < Convert.ToDouble(TxtAltura.Text))
                    {
                    Configuracion.eliminarlineas = chkEliminarLineas.Checked;
                    Configuracion.altura = Convert.ToDouble(TxtAltura.Text);
                    Configuracion.desfase = Convert.ToDouble(TxtDesfase.Text);
                    Configuracion.profundidad = Convert.ToDouble(TxtProfundidad.Text);
                    DialogResult = DialogResult.OK;
                    this.Hide();
                    }
                    else
                    {
                    lblError.Visible = true;
                    lblError.Text = "La altura debe ser mayor al desfase";
                    }
                }
            else
            {
            lblError.Visible = true;
            lblError.Text = "Complete todos los campos";
            }
        }

        private void Cancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void TxtAltura_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir solo numeros 
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar))
                && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            // Permitir solo un punto decimal
            if (e.KeyChar == '.' && (sender as System.Windows.Forms.TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

            // Permitir negativos solo al principio
            if (e.KeyChar == '-' && (sender as System.Windows.Forms.TextBox).Text.Length > 0)
                e.Handled = true;
        }

        private void TxtDesfase_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar))
                && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            // Permitir solo un punto decimal
            if (e.KeyChar == '.' && (sender as System.Windows.Forms.TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

            // Permitir negativos solo al principio
            if (e.KeyChar == '-' && (sender as System.Windows.Forms.TextBox).Text.Length > 0)
                e.Handled = true;
        }

        private void TxtProfundidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir solo numeros
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar))
                && (e.KeyChar != '.'))
                e.Handled = true;

            // Permitir un punto decimal
            if (e.KeyChar == '.' && (sender as System.Windows.Forms.TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

        }
    }

}
