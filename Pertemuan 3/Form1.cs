using System;
using System.Windows.Forms;

namespace CalculatorApp
{
    public partial class Form1 : Form
    {
        private double firstNumber = 0;
        private double secondNumber = 0;
        private double result = 0;
        private string operation = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void NumberButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (txtDisplay.Text == "0")
            {
                txtDisplay.Text = button.Text;
            }
            else
            {
                txtDisplay.Text += button.Text;
            }
        }

        private void OperatorButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (double.TryParse(txtDisplay.Text, out firstNumber))
            {
                operation = button.Text;
                txtDisplay.Clear();
            }
        }
        private void btnEquals_Click(object sender, EventArgs e)
        {
            try
            {
                secondNumber = double.Parse(txtDisplay.Text);

                switch (operation)
                {
                    case "+":
                        result = firstNumber + secondNumber;
                        break;
                    case "-":
                        result = firstNumber - secondNumber;
                        break;
                    case "x":
                        result = firstNumber * secondNumber;
                        break;
                    case "÷":
                    case "/":
                        if (secondNumber == 0)
                        {
                            throw new DivideByZeroException("Tidak dapat membagi dengan angka nol!");
                        }
                        result = firstNumber / secondNumber;
                        break;
                    default:
                        return;
                }

                txtDisplay.Text = result.ToString();
            }
            catch (DivideByZeroException ex)
            {
                MessageBox.Show(ex.Message, "Error Pembagian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDisplay.Text = "0";
            }
            catch (FormatException)
            {
                MessageBox.Show("Format input tidak valid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            firstNumber = 0;
            secondNumber = 0;
            result = 0;
            operation = "";
            txtDisplay.Text = "0";
        }

        private void btnDecimal_Click(object sender, EventArgs e)
        {
            if (!txtDisplay.Text.Contains("."))
            {
                if (string.IsNullOrEmpty(txtDisplay.Text))
                {
                    txtDisplay.Text = "0.";
                }
                else
                {
                    txtDisplay.Text += ".";
                }
            }
        }
    }
}
