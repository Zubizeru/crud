﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace crud
{
    public partial class FrmCadastroDeCliente : Form
    {
        //Conexao com o banco de dados mySql
        MySqlConnection Conexao;
        string data_source = "datasource=localhost;username=root;password =; database=bd_cadastro";

        public FrmCadastroDeCliente()
        {
            InitializeComponent();
        }

        private bool isValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        private bool isValidCPF(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            {
                return false;
            }
            return true;
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtNomeCompleto.Text.Trim()) || 
                    string.IsNullOrEmpty(txtEmail.Text.Trim()) || 
                    string.IsNullOrEmpty(txtCPF.Text.Trim()))
                {
                    MessageBox.Show("Todos os campos devem ser preenchidos", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string cpf = txtCPF.Text.Trim();
                if (!isValidCPF(cpf))
                {
                    MessageBox.Show("CPF inválido. Certifique-se de que o CPF tenha 11 digítos numéricos", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string email = txtEmail.Text.Trim();
                if (!isValidEmail(email))
                {
                    MessageBox.Show("E-mail inválido. Certifique-se de que o e-mail está no formato correto", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);


                Conexao = new MySqlConnection(data_source);
                Conexao.Open();
                // MessageBox.Show("Conexão Aberta");

                MySqlCommand cmd =new MySqlCommand
                {
                    Connection = Conexao
                };

                cmd.Prepare();

                cmd.CommandText = "INSERT INTO dadosdecliente(nomecompleto, nomesocial, email, cpf)" +
                    "Values (@nomecompleto, @nomesocial, @email, @cpf)";

                cmd.Parameters.AddWithValue(@"nomecompleto", txtNomeCompleto.Text.Trim());
                cmd.Parameters.AddWithValue(@"nomesocial", txtNomeSocial.Text.Trim());
                cmd.Parameters.AddWithValue(@"email", email);
                cmd.Parameters.AddWithValue(@"cpf",cpf);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Contato inserido com sucesso: ", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro " + ex.Number + " ocorreu: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if(Conexao != null && Conexao.State == ConnectionState.Open)
                {
                Conexao.Close();
                    // MessageBox.Show("Conexão Fechada");
                }
            }
        }
    }
}
