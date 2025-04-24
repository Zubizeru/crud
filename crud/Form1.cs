using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace crud
{
    public partial class FrmCadastroDeCliente : Form
    {
        //Conexao com o banco de dados mySql
        MySqlConnection Conexao;
        string data_source = "datasource=localhost;username=root;password =; database=bd_cadastro";

        private int? codigo_cliente = null;

        public FrmCadastroDeCliente()
        {
            InitializeComponent();

            //Configurações inicial do listview
            lstCliente.View = View.Details;
            lstCliente.LabelEdit = true;
            lstCliente.AllowColumnReorder = true;
            lstCliente.FullRowSelect = true;
            lstCliente.GridLines = true;

            //Definindo as colunas do ListView
            lstCliente.Columns.Add("Codigo", 100, HorizontalAlignment.Left);
            lstCliente.Columns.Add("Nome Completo", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("Nome Social", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("E-mail", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("CPF", 200, HorizontalAlignment.Left);

            carregarClientes();
        }

        private void carregar_clientes_com_query(string query)
        {
            try
            {
                // Cria a conexao com o banco de dados
                Conexao = new MySqlConnection(data_source);
                Conexao.Open();

                // Executa a consulta SQL fornecida
                MySqlCommand cmd = new MySqlCommand(query, Conexao);

                // Se a consulta contém o parametro @q, adiciona o valor da caixa de pesquisa
                if (query.Contains("@q"))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + txtBuscar.Text + "%");
                }

                MySqlDataReader reader = cmd.ExecuteReader();

                lstCliente.Items.Clear();

                //Preenche o ListView com os dados dos clientes
                while (reader.Read())
                {
                    string[] row =
                    {
                        Convert.ToString(reader.GetInt32(0)),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4)
                    };
                    lstCliente.Items.Add(new ListViewItem(row));
                }
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
                if (Conexao != null && Conexao.State == ConnectionState.Open)
                {
                    Conexao.Close();
                    // MessageBox.Show("Conexão Fechada");
                }
            }
        }

        private void carregarClientes()
        {
            string query = "SELECT * FROM dadosdecliente ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
        }

        private bool isValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        private bool isValidCPF(string cpf)
        {
            cpf = cpf.Replace(",", "").Replace("-", "").Replace(".", "");

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

                string cpf = txtCPF.Text.Replace(".", "").Replace("-", "").Replace(",", "").Trim();
                if (!isValidCPF(cpf))
                {
                    MessageBox.Show("CPF inválido. Certifique-se de que o CPF tenha 11 dígitos numéricos", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string email = txtEmail.Text.Trim();
                if (!isValidEmail(email))
                {
                    MessageBox.Show("E-mail inválido. Certifique-se de que o e-mail está no formato correto", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Conexao = new MySqlConnection(data_source);
                Conexao.Open();
                // MessageBox.Show("Conexão Aberta");

                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = Conexao
                };

                cmd.Prepare();

                if (codigo_cliente == null)
                {
                    // insert CREATE
                    cmd.CommandText = "INSERT INTO dadosdecliente(nomecompleto, nomesocial, email, cpf)" +
                    "Values (@nomecompleto, @nomesocial, @email, @cpf)";

                    cmd.Parameters.AddWithValue(@"nomecompleto", txtNomeCompleto.Text.Trim());
                    cmd.Parameters.AddWithValue(@"nomesocial", txtNomeSocial.Text.Trim());
                    cmd.Parameters.AddWithValue(@"email", email);
                    cmd.Parameters.AddWithValue(@"cpf", cpf);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Contato inserido com sucesso: ", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // UPDATE 
                    cmd.CommandText = $"UPDATE `dadosdecliente` SET " +
                    $"nomecompleto = @nomecompleto, " +
                    $"nomesocial = @nomesocial, " +
                    $"email = @email, " +
                    $"cpf = @cpf " +   
                    $"WHERE codigo = @codigo ";

                    cmd.Parameters.AddWithValue("@nomecompleto", txtNomeCompleto.Text.Trim());
                    cmd.Parameters.AddWithValue("@nomesocial", txtNomeSocial.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@cpf", cpf);
                    cmd.Parameters.AddWithValue("@codigo", codigo_cliente);

                    //Executa o comando de alteração no banco
                    cmd.ExecuteNonQuery();

                    MessageBox.Show($"Os dados com o código {codigo_cliente} foram alterados com Sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

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
                if (Conexao != null && Conexao.State == ConnectionState.Open)
                {
                    Conexao.Close();
                    // MessageBox.Show("Conexão Fechada");
                }
            }

            carregarClientes();
            limparCampos();

            TabControl.SelectedIndex = 1;
        }

        private void limparCampos()
        {
            txtNomeCompleto.Text = "";
            txtNomeSocial.Text = "";
            txtEmail.Text = "";
            txtCPF.Text = "";
            codigo_cliente = null;

            btnExcluirCliente.Visible = false;
        }


        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM dadosdecliente WHERE nomecompleto LIKE @q OR nomesocial LIKE @q ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            string query = "SELECT * FROM dadosdecliente ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
        }

        private void btnNovoCliente_Click(object sender, EventArgs e)
        {
            limparCampos();

            txtNomeCompleto.Focus();
        }

        private void lstCliente_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView.SelectedListViewItemCollection clientedaselecao = lstCliente.SelectedItems;

            foreach (ListViewItem item in clientedaselecao)
            {
                codigo_cliente = Convert.ToInt32(item.SubItems[0].Text);

                MessageBox.Show($"Código do Cliente: {codigo_cliente.ToString()}", "Código Selecionado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtNomeCompleto.Text = item.SubItems[1].Text;
                txtNomeSocial.Text = item.SubItems[2].Text;
                txtEmail.Text = item.SubItems[3].Text;
                txtCPF.Text = item.SubItems[4].Text;

                btnExcluirCliente.Visible = true;
            }

            TabControl.SelectedIndex = 0;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            excluir_cliente();
            limparCampos();
        }

        private void btnExcluirCliente_Click(object sender, EventArgs e)
        {
            excluir_cliente();
            limparCampos();
        }

        private void excluir_cliente()
        {
            try
            {
                DialogResult opcaoDigitada = MessageBox.Show($"Tem Certeza que deseja excluir o registro de código {codigo_cliente}?", "Tem Certeza", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (opcaoDigitada == DialogResult.Yes)
                {
                    Conexao = new MySqlConnection(data_source);
                    Conexao.Open();

                    MySqlCommand cmd = new MySqlCommand();

                    cmd.Connection = Conexao;

                    cmd.Prepare();

                    cmd.CommandText = "DELETE FROM dadosdecliente WHERE codigo = @codigo";

                    cmd.Parameters.AddWithValue("@codigo", codigo_cliente);

                    cmd.ExecuteNonQuery();

                    //Excluir dados do banco
                    MessageBox.Show("Os dados do cliente foram EXCLUÍDOS", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    carregarClientes();

                }
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
                if (Conexao != null && Conexao.State == ConnectionState.Open)
                {
                    Conexao.Close();
                }
            }
        }
    }
}
