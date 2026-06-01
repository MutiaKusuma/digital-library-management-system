using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DijitalKutuphane
    {
      // Menu sınıfı, ana formun kullanıcı arayüzünü temsil eder
      public partial class Menu : Form {
        // Menu sınıfının yapıcı metodu (constructor)
        public Menu()
        { InitializeComponent(); }
        // Kitap Veritabanina Yonlendirir
      private void button1_Click(object sender, EventArgs e)
      {Form1 kitap = new Form1();
        kitap.ShowDialog();}
      // Kullanici tablosuna Yonlendirir
      private void button2_Click(object sender, EventArgs e)
        { Form2 kullanici = new Form2();
          kullanici.ShowDialog();}}
    }
