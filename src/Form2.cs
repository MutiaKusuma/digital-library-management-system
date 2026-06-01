```csharp
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DijitalKutuphane
{
    public partial class Form2 : Form
    {
        // Constructor: Form2 başlatıldığında otomatik olarak çağrılır
        public Form2()
        {
            InitializeComponent(); // Formdaki bileşenleri başlatır
        }

        // PostgreSQL baglantisi icin metot
        private NpgsqlConnection GetConnection()
        {
            // Bağlantı dizesi: PostgreSQL'e bağlanmak için gerekli bilgiler
            string connectionString = "Host=localhost;Port=5433;Database=DijitalKutuphane;Username=postgres;Password=sukses";
            return new NpgsqlConnection(connectionString);
        }

        // LISTELEME (Kullanıcı listesini DataGridView'e yüklemek için metot)
        private void ListeleButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = GetConnection()) // Bağlantıyı almak
                {
                    conn.Open(); // Bağlantıyı açmak

                    string query = @"
SELECT
    ""Kisi"".""KisiID"",
    ""Kisi"".""Adi"",
    ""Kisi"".""Soyadi"",
    ""Kisi"".""Eposta"",
    ""Kisi"".""DogumTarihi"",
    ""Kisi"".""Cinsiyet"",
    ""Kullanici"".""CihazAdi""
FROM
    ""Kisi""
INNER JOIN
    ""Kullanici"" ON ""Kisi"".""KisiID"" = ""Kullanici"".""KisiID""";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            KullaniciTablosu.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // EKLEME (Kullanıcı ekleme işlemi)
        private void EkleButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    DateTime dogumTarihi;

                    try
                    {
                        dogumTarihi = DateTime.ParseExact(
                            DogumTarihi.Text,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Geçersiz tarih formatı. Lütfen 'dd.MM.yyyy' formatında bir tarih giriniz.");
                        return;
                    }

                    // Güncelleme işlemi
                    if (GuncelleKisiID.HasValue)
                    {
                        // Adım 1: Kisi tablosundaki veriyi güncelle
                        string kisiUpdateQuery = @"
UPDATE ""Kisi""
SET ""Adi"" = @Adi,
    ""Soyadi"" = @Soyadi,
    ""Eposta"" = @Eposta,
    ""DogumTarihi"" = @DogumTarihi,
    ""Cinsiyet"" = @Cinsiyet
WHERE ""KisiID"" = @KisiID";

                        using (var cmdKisi = new NpgsqlCommand(kisiUpdateQuery, conn))
                        {
                            cmdKisi.Parameters.AddWithValue("@Adi", Adi.Text);
                            cmdKisi.Parameters.AddWithValue("@Soyadi", Soyadi.Text);
                            cmdKisi.Parameters.AddWithValue("@Eposta", Eposta.Text);
                            cmdKisi.Parameters.AddWithValue("@DogumTarihi", dogumTarihi);
                            cmdKisi.Parameters.AddWithValue("@Cinsiyet", Cinsiyet.Text);
                            cmdKisi.Parameters.AddWithValue("@KisiID", GuncelleKisiID.Value);

                            cmdKisi.ExecuteNonQuery();
                        }

                        // Adım 2: Kullanici tablosundaki veriyi güncelle
                        string kullaniciUpdateQuery = @"
UPDATE ""Kullanici""
SET ""CihazAdi"" = @CihazAdi
WHERE ""KisiID"" = @KisiID";

                        using (var cmdKullanici = new NpgsqlCommand(kullaniciUpdateQuery, conn))
                        {
                            cmdKullanici.Parameters.AddWithValue("@CihazAdi", Cihaz.Text);
                            cmdKullanici.Parameters.AddWithValue("@KisiID", GuncelleKisiID.Value);

                            cmdKullanici.ExecuteNonQuery();
                        }

                        // Adım 3: Cihaz tablosundaki veriyi güncelle
                        string cihazUpdateQuery = @"
UPDATE ""Cihaz""
SET ""CihazAdi"" = @CihazAdi
WHERE ""KisiID"" = @KisiID";

                        using (var cmdCihaz = new NpgsqlCommand(cihazUpdateQuery, conn))
                        {
                            cmdCihaz.Parameters.AddWithValue("@CihazAdi", Cihaz.Text);
                            cmdCihaz.Parameters.AddWithValue("@KisiID", GuncelleKisiID.Value);

                            cmdCihaz.ExecuteNonQuery();
                        }

                        MessageBox.Show("Kullanıcı bilgileri başarıyla güncellendi.");
                    }
                    else // Yeni ekleme işlemi
                    {
                        // Adım 1: Kisi tablosuna veri ekle
                        string kisiQuery = @"
INSERT INTO ""Kisi""
(""Adi"", ""Soyadi"", ""Eposta"", ""DogumTarihi"", ""Cinsiyet"", ""KisiTuru"")
VALUES
(@Adi, @Soyadi, @Eposta, @DogumTarihi, @Cinsiyet, 'Kullanici')
RETURNING ""KisiID""";

                        using (var cmd = new NpgsqlCommand(kisiQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Adi", Adi.Text);
                            cmd.Parameters.AddWithValue("@Soyadi", Soyadi.Text);
                            cmd.Parameters.AddWithValue("@Eposta", Eposta.Text);
                            cmd.Parameters.AddWithValue("@DogumTarihi", dogumTarihi);
                            cmd.Parameters.AddWithValue("@Cinsiyet", Cinsiyet.Text);

                            int kisiID = (int)cmd.ExecuteScalar();

                            // Adım 2 : Kullanici tablosuna veri ekle
                            string kullaniciQuery = @"
INSERT INTO ""Kullanici"" (""KisiID"", ""CihazAdi"")
VALUES (@KisiID, @CihazAdi)";

                            using (var cmdKullanici = new NpgsqlCommand(kullaniciQuery, conn))
                            {
                                cmdKullanici.Parameters.AddWithValue("@KisiID", kisiID);
                                cmdKullanici.Parameters.AddWithValue("@CihazAdi", Cihaz.Text);

                                cmdKullanici.ExecuteNonQuery();
                            }

                            // Adım 3 : Cihaz tablosuna veri ekle
                            string cihazQuery = @"
INSERT INTO ""Cihaz"" (""CihazAdi"", ""KisiID"")
VALUES (@CihazAdi, @KisiID)";

                            using (var cmdCihaz = new NpgsqlCommand(cihazQuery, conn))
                            {
                                cmdCihaz.Parameters.AddWithValue("@CihazAdi", Cihaz.Text);
                                cmdCihaz.Parameters.AddWithValue("@KisiID", kisiID);

                                cmdCihaz.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Kullanıcı ve cihaz başarıyla eklendi.");
                    }

                    // Güncelleme işleminden sonra tabloyu yenile
                    GuncelleKisiID = null;
                    ListeleButton_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        // SILMEK (Kullanıcıyı ve ilişkili verilerini silmek için metot)
        private void SilButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (KullaniciTablosu.SelectedRows.Count > 0)
                {
                    int kisiID = Convert.ToInt32(
                        KullaniciTablosu.SelectedRows[0]
                        .Cells["KisiID"].Value);

                    using (var conn = GetConnection())
                    {
                        conn.Open();

                        string cihazQuery = @"
DELETE FROM ""Cihaz""
WHERE ""KisiID"" = @KisiID";

                        using (var cmdCihaz = new NpgsqlCommand(cihazQuery, conn))
                        {
                            cmdCihaz.Parameters.AddWithValue("@KisiID", kisiID);
                            cmdCihaz.ExecuteNonQuery();
                        }

                        string kullaniciQuery = @"
DELETE FROM ""Kullanici""
WHERE ""KisiID"" = @KisiID";

                        using (var cmdKullanici = new NpgsqlCommand(kullaniciQuery, conn))
                        {
                            cmdKullanici.Parameters.AddWithValue("@KisiID", kisiID);
                            cmdKullanici.ExecuteNonQuery();
                        }

                        string kisiQuery = @"
DELETE FROM ""Kisi""
WHERE ""KisiID"" = @KisiID";

                        using (var cmdKisi = new NpgsqlCommand(kisiQuery, conn))
                        {
                            cmdKisi.Parameters.AddWithValue("@KisiID", kisiID);
                            cmdKisi.ExecuteNonQuery();
                        }

                        MessageBox.Show("Seçilen kullanıcı ve ilgili veriler başarıyla silindi.");
                    }

                    ListeleButton_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Lütfen silmek istediğiniz veriyi seçin.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

// Güncellemek için seçilen kullanıcının bilgilerini DataGridView'den form alanlarına aktaran metot
private int? GuncelleKisiID = null;

// GUNCELLEME (Güncelleme butonuna tıklanma olayı)
private void GuncelleButton_Click(object sender, EventArgs e)
{
    if (KullaniciTablosu.SelectedRows.Count > 0)
    {
        // Güncelleme işlemi için kullanıcı onayı alınır
        var result = MessageBox.Show(
            "Seçilen kullanıcıyı güncellemek istediğinize emin misiniz?",
            "Onay",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            // DataGridView'deki seçili satırdan kullanıcı bilgileri ilgili giriş alanlarına aktarılır
            Adi.Text = KullaniciTablosu.SelectedRows[0].Cells["Adi"].Value.ToString();
            Soyadi.Text = KullaniciTablosu.SelectedRows[0].Cells["Soyadi"].Value.ToString();
            Eposta.Text = KullaniciTablosu.SelectedRows[0].Cells["Eposta"].Value.ToString();

            DogumTarihi.Text =
                DateTime.Parse(
                    KullaniciTablosu.SelectedRows[0].Cells["DogumTarihi"].Value.ToString()
                ).ToString("dd.MM.yyyy");

            Cinsiyet.Text = KullaniciTablosu.SelectedRows[0].Cells["Cinsiyet"].Value.ToString();
            Cihaz.Text = KullaniciTablosu.SelectedRows[0].Cells["CihazAdi"].Value.ToString();

            // Güncelleme işlemi sırasında kullanılacak KisiID değerini saklar
            GuncelleKisiID =
                Convert.ToInt32(
                    KullaniciTablosu.SelectedRows[0].Cells["KisiID"].Value
                );
        }
    }
    else
    {
        MessageBox.Show("Lütfen güncellemek istediğiniz kullanıcıyı seçin.");
    }
}

// ARAMA (Kullanıcı bilgileri üzerinde dinamik arama işlemlerini gerçekleştiren metot)
private void AramaButton_Click(object sender, EventArgs e)
{
    try
    {
        using (var conn = GetConnection()) // Veritabanı bağlantısı oluşturulur
        {
            conn.Open();

            // SQL sorgusu başlangıç hali (Kisi ve Kullanici tabloları birleştirilir)
            string query = @"
SELECT
    ""Kisi"".""KisiID"",
    ""Kisi"".""Adi"",
    ""Kisi"".""Soyadi"",
    ""Kisi"".""Eposta"",
    ""Kisi"".""DogumTarihi"",
    ""Kisi"".""Cinsiyet"",
    ""Kullanici"".""CihazAdi""
FROM
    ""Kisi""
INNER JOIN
    ""Kullanici"" ON ""Kisi"".""KisiID"" = ""Kullanici"".""KisiID""
WHERE 1=1";

            // Başlangıçta tüm kayıtlar getirilir
            // Kullanıcı tarafından girilen kriterlere göre sorguya dinamik filtreler eklenir

            if (!string.IsNullOrWhiteSpace(KisiID.Text))
            {
                query += " AND \"Kisi\".\"KisiID\" = @KisiID";
            }

            if (!string.IsNullOrWhiteSpace(Adi.Text))
            {
                query += " AND \"Kisi\".\"Adi\" ILIKE @Adi";
            }

            if (!string.IsNullOrWhiteSpace(Soyadi.Text))
            {
                query += " AND \"Kisi\".\"Soyadi\" ILIKE @Soyadi";
            }

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                // Girilen kriterlere bağlı olarak parametreler sorguya eklenir

                if (!string.IsNullOrWhiteSpace(KisiID.Text))
                {
                    cmd.Parameters.AddWithValue("@KisiID", int.Parse(KisiID.Text));
                }

                if (!string.IsNullOrWhiteSpace(Adi.Text))
                {
                    cmd.Parameters.AddWithValue("@Adi", $"%{Adi.Text}%");
                }

                if (!string.IsNullOrWhiteSpace(Soyadi.Text))
                {
                    cmd.Parameters.AddWithValue("@Soyadi", $"%{Soyadi.Text}%");
                }

                // Sorgu çalıştırılarak sonuçlar DataTable nesnesine aktarılır
                using (var reader = cmd.ExecuteReader())
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader); // Sorgudan dönen veriler yüklenir
                    KullaniciTablosu.DataSource = dt; // DataGridView'e aktarılır
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
