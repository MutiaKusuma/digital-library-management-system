```csharp
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DijitalKutuphane
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // PostgreSQL baglantisi icin metot tanimladik
        private NpgsqlConnection GetConnection()
        {
            string connectionString = "Host=localhost;Port=5433;Database=DijitalKutuphane;Username=postgres;Password=sukses";
            return new NpgsqlConnection(connectionString); // Yeni bir bağlantı nesnesi döndürür
        }

        // LISTELEME (Kitapları listelemek için butonun tıklanma olayı)
        private void ListeleButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open(); // Veritabanı bağlantısını açar
                    string query = "SELECT * FROM \"Kitap\""; // Kitap tablosundaki tüm verileri seçer

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader()) // Verileri okur
                        {
                            DataTable dt = new DataTable(); // Yeni bir DataTable nesnesi oluşturur
                            dt.Load(reader); // Verileri DataTable'a yükler
                            KitabTablosu.DataSource = dt; // DataGridView'e veri kaynağını ayarlar
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // EKLEME (Kitap eklemek için butonun tıklanma olayı)
        private void EkleButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open(); // Veritabanı bağlantısını açar

                    // Basım tarihi kontrolü ve dönüştürme
                    DateTime basimTarihi;
                    try
                    {
                        basimTarihi = DateTime.ParseExact(BasimTarihi.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Geçersiz tarih formatı. Lütfen 'dd.MM.yyyy' formatında bir tarih giriniz.");
                        return;
                    }

                    // ISBN mevcut ise eski bilgileri silmek (Guncellemek için kullanılır)
                    string deleteQuery = "DELETE FROM \"Kitap\" WHERE \"ISBN\" = @isbn";

                    using (var deleteCmd = new NpgsqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@isbn", ISBN.Text);
                        deleteCmd.ExecuteNonQuery(); // Silme işlemini gerçekleştirir
                    }

                    // Yeni kitap kaydını ekler
                    NpgsqlCommand komut = new NpgsqlCommand(
                        "INSERT INTO \"Kitap\" (\"ISBN\", \"Baslik\", \"StokDurumu\", " +
                        "\"BasimTarihi\", \"YayinEviKodu\", \"DilKodu\") VALUES " +
                        "(@isbn, @baslik, @stokdurumu, @basimtarihi, @yayinevikodu, @dilkodu)",
                        conn);

                    komut.Parameters.AddWithValue("@isbn", ISBN.Text);
                    komut.Parameters.AddWithValue("@baslik", Baslik.Text);
                    komut.Parameters.AddWithValue("@stokdurumu", int.Parse(StokDurumu.Value.ToString()));
                    komut.Parameters.AddWithValue("@basimtarihi", basimTarihi);
                    komut.Parameters.AddWithValue("@yayinevikodu", int.Parse(YayinEviKodu.Text)); // Yayınevi kodunu integer'a çevirir
                    komut.Parameters.AddWithValue("@dilkodu", int.Parse(DilKodu.SelectedValue.ToString())); // Dil kodunu integer'a çevirir

                    komut.ExecuteNonQuery(); // Yeni kitap kaydını ekler
                    conn.Close();

                    MessageBox.Show(
                        "Yeni kitap başarıyla eklendi ve kaydedildi",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ListeleButton_Click(null, null); // Kitap listesini günceller

                    // Textboxlari temizlemek icin fonksiyon
                    Temizle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // Dil ComboBox ile Dil tablosunun baglantisi icin metot tanimladik
        private void DilKoduDoldur()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(
                        "SELECT \"DilKodu\", \"DilAdi\" FROM \"Dil\"",
                        conn); // Dil tablosundaki verileri alır

                    DataTable dt = new DataTable();
                    da.Fill(dt); // Verileri DataTable'a doldurur

                    DilKodu.DisplayMember = "DilAdi"; // ComboBox'ta görüntülenecek alan
                    DilKodu.ValueMember = "DilKodu"; // ComboBox'ın değerine karşılık gelen alan
                    DilKodu.DataSource = dt; // ComboBox'ın veri kaynağını ayarlar

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // YayinEviKodu ComboBox ile YayinEvi tablosunun baglantisi icin metot tanimladik
        private void YayinEviKoduDoldur()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(
                        "SELECT \"YayinEviKodu\", \"Adi\" FROM \"YayinEvi\"",
                        conn); // Yayınevi tablosundaki verileri alır

                    DataTable dt = new DataTable();
                    da.Fill(dt); // Verileri DataTable'a doldurur

                    YayinEviKodu.DisplayMember = "YayinEviKodu"; // ComboBox'ta görüntülenecek alan
                    YayinEviKodu.ValueMember = "Adi"; // ComboBox'ın değerine karşılık gelen alan
                    YayinEviKodu.DataSource = dt; // ComboBox'ın veri kaynağını ayarlar

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // Ekleme ve Guncelleme islemini gerceklestirdikten sonra ilgili textboxlari temizlemek
        private void Temizle()
        {
            // TextBoxları temizle
            ISBN.Clear();
            Baslik.Clear();
            BasimTarihi.Clear();

            // NumericUpDown sıfırla
            StokDurumu.Value = 0;

            // ComboBox seçimlerini sıfırla
            YayinEviKodu.SelectedIndex = -1;
            DilKodu.SelectedIndex = -1;
        }

        // Form yüklendiğinde çalışacak metot
        private void Form1_Load(object sender, EventArgs e)
        {
            YayinEviKoduDoldur(); // YayinEvi ComboBox
            DilKoduDoldur(); // Dil ComboBox
        }

        // SILME (Kitap silme işlemi için butonun tıklanma olayı)
        private void SilButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Silinecek kayiti secildi mi ?
                if (KitabTablosu.SelectedRows.Count > 0)
                {
                    // Secilen ISBN , kayitlarin tablosunda bilgilerini getir
                    string selectedISBN = KitabTablosu.SelectedRows[0].Cells["ISBN"].Value.ToString();

                    using (var conn = GetConnection())
                    {
                        conn.Open();

                        string query = "DELETE FROM \"Kitap\" WHERE \"ISBN\" = @isbn"; // Kitap silme sorgusu

                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@isbn", selectedISBN);
                            cmd.ExecuteNonQuery(); // Kitap kaydını siler
                        }

                        conn.Close();
                        MessageBox.Show("Kitap başarıyla silindi.", "Bilgi");
                    }

                    // Silme islemini btirdikten sonra Kitab tablosunu yenilemek
                    ListeleButton_Click(null, null);
                }
                else
                {
                    MessageBox.Show("Lütfen silmek için bir kitap seçin.", "Uyarı");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // GUNCELLE (Güncelleme butonuna tıklanma olayı)
        private void GuncelleButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Eğer seçili bir satır varsa
                if (KitabTablosu.SelectedRows.Count > 0)
                {
                    // Kullanıcıdan güncelleme için onay alınır
                    DialogResult result = MessageBox.Show(
                        "Seçilen veriyi güncellemek istediğinizden emin misiniz?",
                        "Onay",
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        DataGridViewRow selectedRow = KitabTablosu.SelectedRows[0]; // Seçilen satır alınır

                        // TextBox'lara seçilen satırın değerleri yüklenir
                        ISBN.Text = selectedRow.Cells["ISBN"].Value.ToString();
                        Baslik.Text = selectedRow.Cells["Baslik"].Value.ToString();
                        StokDurumu.Value = Convert.ToInt32(selectedRow.Cells["StokDurumu"].Value);

                        // Tarihi sadece 'dd.MM.yyyy' formatında çekmek
                        DateTime basimTarihi = Convert.ToDateTime(selectedRow.Cells["BasimTarihi"].Value);
                        BasimTarihi.Text = basimTarihi.ToString("dd.MM.yyyy");

                        // Yayınevi ve dil kodları ComboBox'larına seçilen değerler yüklenir
                        YayinEviKodu.SelectedValue = selectedRow.Cells["YayinEviKodu"].Value.ToString();
                        DilKodu.SelectedValue = selectedRow.Cells["DilKodu"].Value.ToString();

                        // Basım tarihi formatını DataGridView'de ayarlama
                        if (KitabTablosu.Columns["BasimTarihi"] != null)
                        {
                            KitabTablosu.Columns["BasimTarihi"].DefaultCellStyle.Format = "dd.MM.yyyy";
                        }

                        MessageBox.Show(
                            "Lütfen gerekli düzenlemeleri yapın ve ardından 'Ekle' tuşuna basın.",
                            "Bilgi");
                    }
                }
                else
                {
                    MessageBox.Show("Lütfen güncellenecek bir kitap seçin.", "Uyarı");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // ARAMA (Arama butonuna tıklanma olayı)
        private void AramaButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = GetConnection()) // Veritabanı bağlantısı oluşturulur
                {
                    conn.Open(); // Bağlantı açılır

                    string query = "SELECT * FROM \"Kitap\" WHERE 1=1"; // Kitap tablosundaki tüm veriler sorgulanır

                    // ISBN gore arama yapar
                    if (!string.IsNullOrEmpty(ISBN.Text))
                    {
                        query += " AND \"ISBN\" = @isbn";
                    }

                    if (!string.IsNullOrEmpty(Baslik.Text))
                    {
                        query += " AND \"Baslik\" ILIKE @baslik"; // ILIKE 'case-insensitive' aramalar icin
                    }

                    using (var cmd = new NpgsqlCommand(query, conn)) // SQL komutunu oluşturur
                    {
                        // ISBN araması yapılacaksa parametre eklenir
                        if (!string.IsNullOrEmpty(ISBN.Text))
                        {
                            cmd.Parameters.AddWithValue("@isbn", ISBN.Text);
                        }

                        // Başlık araması yapılacaksa parametre eklenir
                        if (!string.IsNullOrEmpty(Baslik.Text))
                        {
                            cmd.Parameters.AddWithValue("@baslik", "%" + Baslik.Text + "%");
                        }

                        using (var reader = cmd.ExecuteReader()) // Sorguyu çalıştırır ve verileri okur
                        {
                            DataTable dt = new DataTable(); // Veriler DataTable'a yüklenir
                            dt.Load(reader); // Veriler DataTable'a yüklenir
                            KitabTablosu.DataSource = dt; // DataGridView'e veri kaynağını atar
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }
    }
}
```
