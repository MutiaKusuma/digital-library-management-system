
# Digital Library Management System (Dijital Kütüphane Uygulaması)

A comprehensive Digital Library Management System designed to optimize library operations, track book borrowing/returns, and manage user interactions. This project was developed as part of the Database Management Systems (VTYS) course at Sakarya University.

> **Note:** The original complete source code files were lost due to a local hardware/system migration. However, the complete system architecture, database schemas, core code snippets, and interface designs have been fully preserved and documented below.
---

## 👥 Project Contributors
* **Mutia Maharani Kusuma** (Student ID: B221210590) - Section 1B
* **Alviola Permatasari** (Student ID: B231210575) - Section 1B
* **Institution**: T.C. Sakarya Üniversitesi  
* **Department**: Bilgisayar Mühendisliği (Computer Engineering)

---

##  Features
* **User Management:** Registration, role tracking, and activity logs.
* **Book Inventory & Borrowing:** Seamless tracking of borrowed books, return deadlines, and review systems.
* **Advanced Search:** Dynamic querying using PostgreSQL backend and C# frontend.
---
##  Tech Stack
* **Frontend/Desktop App:** C# (.NET / Windows Forms)
* **Database:** PostgreSQL
* **Library/Driver:** Npgsql
---
##  Core Business Rules (İş Kuralları)
The system operates under strict relational rules to maintain data integrity:
* **Borrowing & Fine System:** Users can borrow books for a maximum of 14 days. Exceeding this period automatically triggers a fine of 15 TL per day. If a return is delayed for more than a year, the system automatically suspends the user's account.
* **Staff Categorization (Disjoint Specialization):** Staff members are strictly categorized into two non-overlapping roles: *Danisman* (Consultants with tracked working hours) and *Sistem Yonetici* (System Administrators mapped with specific expertise domains).
* **Book Relations:** Built to support complex entity mappings; a book can feature multiple authors and be linked to multiple categories (Many-to-Many relationships).
---
##  Relational Database Schema
The system maps its operational logic into a PostgreSQL database named `DijitalKutuphane` with the following entities:

1. **Kisi** (`KisiID` (SERIAL, PK), `Adi`, `Soyadi`, `Eposta` (UNIQUE), `DogumTarihi`, `Cinsiyet`, `KisiTuru`)
2. **Kullanici** (`KisiID` (PK, FK), `CihazAdi`)
3. **Personel** (`KisiID` (PK, FK), `PersonelTipi`)
4. **Danisman** (`KisiID` (PK, FK), `Mesai`)
5. **Sistem Yonetici** (`KisiID` (PK, FK), `Uzmanlik`)
6. **Yazar** (`KisiID` (PK, FK), `takmaAdi`)
7. **Cihaz** (`CihazID` (PK), `CihazAdi`, `KisiID` (FK))
8. **YayinEvi** (`YayinEviKodu` (PK), `Adi`, `TelefonNo`, `WebAdres`)
9. **Kategori** (`KategoriID` (PK), `KategoriAdi`)
10. **Dil** (`DilKodu` (PK), `DilAdi`)
11. **Kitap** (`ISBN` (PK), `Baslik`, `StokDurumu`, `BasimTarihi`, `YayinEviKodu` (FK), `DilKodu` (FK))
12. **KitapKategori** (`KategoriID` (PK, FK), `ISBN` (PK, FK))
13. **KitapYazar** (`ISBN` (PK, FK), `KisiID` (PK, FK))
14. **KitapKullaniciOdunc** (`OduncKodu` (PK), `KisiID` (FK), `ISBN` (FK), `OduncTarihi`, `IadeTarihi`)
15. **Borc** (`BorcID` (PK), `OduncKodu` (FK), `ToplamBorc`, `OdemeDurumu`)
16. **Yorum** (`YorumID` (PK), `ISBN` (FK), `KisiID` (FK), `Tarih`, `Icerik`)

---

##  Advanced Database Components (PostgreSQL)

### 1. Stored Functions & Procedures
* **`YasHesapla(KisiID)`**: Automatically calculates a user's exact age based on their registered birth date.
* **`toplam_kitap_sayisi()`**: Returns the global aggregate count of unique books available in the inventory.
* **`ceza_hesapla_by_oduncid(OduncKodu)`**: Dynamically processes fine balances by assessing overdue loan lengths past the standard 14-day threshold.

### 2. Triggers
* **`email_validation_trigger`**: Runs `BEFORE INSERT OR UPDATE` on the `Kisi` table, filtering records using Regex to block invalid email structures.
* **`yazar_ekleme_trigger`**: Automatically syncs roles by switching `KisiTuru` to 'Yazar' when a row lands in the Author table, logging changes into `YazarUpdate`.
* **`kitap_stok_trigger`**: Performs inventory checks `BEFORE INSERT` into `KitapKullaniciOdunc`. If stock is available, it decreases it by 1. If stock is 0, the transaction aborts with a user-facing exception.
---
##  Application Features (C# Windows Forms)
The desktop frontend communicates natively with PostgreSQL via the `Npgsql` driver, embedding features such as:
* **Dynamic Search & Filtering:** Implements parameterized multi-criteria filtering to safely query users and books without SQL injection risks.
* **Data-Binding ComboBoxes:** Automatically populates dropdown values (`YayinEviKodu`, `DilKodu`) directly from corresponding backend tables.

---

## 💻 Repository Code Structure
The application frontend is built with **C# (.NET Windows Forms)** and communicates with the **PostgreSQL** database using the `Npgsql` driver. The business logic files are organized as follows:

* 📄 **[Menu.cs](./src/Menu.cs):** Handles main application navigation and forms switching routing.
* 📄 **[Form1.cs](./src/Form1.cs):** Controls comprehensive Book Management (CRUD operations, safe parameterized dynamic search queries, and ComboBox data binding layouts).
* 📄 **[Form2.cs](./src/Form2.cs):** Governs User and Device Tracking (Multi-table synchronized insertions/updates using PostgreSQL sequences and lookup validation).

---

## 🖥️ Desktop Application Interface
Screenshots captured from the operational Windows Forms interface:
### Adding Books 
<img width="1666" height="976" alt="WPS Photo(1)" src="https://github.com/user-attachments/assets/6d0b7768-0459-43bf-9f16-4c7cdbf85ade" />

### Deleting Books 
<img width="1680" height="972" alt="WPS Photo(2)" src="https://github.com/user-attachments/assets/217a43f7-ae2e-4bc9-8316-8b3e348250fc" />

### Updating Books
<img width="1668" height="964" alt="WPS Photo(3)" src="https://github.com/user-attachments/assets/09778ad8-21e1-4a80-9acc-9e7932b0a4a8" />

### Searching Books
<img width="1683" height="967" alt="WPS Photo(4)" src="https://github.com/user-attachments/assets/8789f2c5-ba2f-4496-bca0-780041bfaed2" />

### Adding User
<img width="1207" height="661" alt="WPS Photo(1)" src="https://github.com/user-attachments/assets/f6a683ea-438f-45dd-867b-c5152fc5463b" />

### Deleting User
<img width="1207" height="669" alt="WPS Photo(1)" src="https://github.com/user-attachments/assets/9d849109-ee54-4d9d-9716-4a45b7541cf1" />

### Updating User
<img width="1200" height="657" alt="WPS Photo(1)" src="https://github.com/user-attachments/assets/070f28a1-ece5-465b-a89d-ee70a1e9ef92" />

### Searching User
<img width="1221" height="657" alt="WPS Photo(1)" src="https://github.com/user-attachments/assets/954786be-4c89-4c72-88b7-e249efe0684b" />














