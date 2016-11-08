using MicrocolsaAccesoDatosNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace TestConnectionOracle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            MotorBD.Items.Clear();
            MotorBD.Items.Add("ORACLE");
            MotorBD.Items.Add("ORACLEMS");
            MotorBD.Items.Add("ORACLE10G");
        }


        private MicrocolsaDatosCorrespondencia.tipoMotorDb GetMotor()
        {
            MicrocolsaDatosCorrespondencia.tipoMotorDb mt = MicrocolsaDatosCorrespondencia.tipoMotorDb.ORACLEMS;
            string strMotor = MotorBD.SelectedItem.ToString();
            switch (strMotor)
            {
                case "ORACLEMS":
                    {
                        mt = MicrocolsaDatosCorrespondencia.tipoMotorDb.ORACLEMS;
                    }
                    break;
                case "ORACLE10G":
                    {
                        mt = MicrocolsaDatosCorrespondencia.tipoMotorDb.ORACLE10G;
                    }
                    break;
                case "ORACLE":
                    {
                        mt = MicrocolsaDatosCorrespondencia.tipoMotorDb.ORACLE;
                    }
                    break;
            }

            return mt;
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            try
            {
                MicrocolsaDatosCorrespondencia mc = new MicrocolsaDatosCorrespondencia();
                mc.ConfigurarDb(this.txtUsuario.Text,
                    this.txtContra.Text,
                    this.txtBaseDatos.Text,
                    "",
                    this.txtServidor.Text,
                    this.GetMotor(),
                    this.txtPuerto.Text,
                    this.txtTsname.Text,
                    ""
                    );
                mc.Conectar();
                MessageBox.Show("Conectó bien", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message + "\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
