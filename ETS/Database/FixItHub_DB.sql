-- SSMS (instance .\SQLEXPRESS)
IF DB_ID('FixItHub_DB') IS NULL CREATE DATABASE FixItHub_DB;
GO
USE FixItHub_DB;
GO

IF OBJECT_ID('dbo.TransaksiServis','U') IS NOT NULL DROP TABLE dbo.TransaksiServis;
IF OBJECT_ID('dbo.Pelanggan','U') IS NOT NULL DROP TABLE dbo.Pelanggan;
GO

CREATE TABLE dbo.Pelanggan (
    PelangganId INT IDENTITY(1,1) CONSTRAINT PK_Pelanggan PRIMARY KEY,
    Nama NVARCHAR(100) NOT NULL,
    NoTelepon NVARCHAR(20)  NOT NULL,
    Alamat NVARCHAR(255) NULL
);
GO

CREATE TABLE dbo.TransaksiServis (
    ServisId INT IDENTITY(1,1) CONSTRAINT PK_TransaksiServis PRIMARY KEY,
    NoNota NVARCHAR(30)  NOT NULL CONSTRAINT UQ_TransaksiServis_NoNota UNIQUE,
    PelangganId INT NOT NULL CONSTRAINT FK_Servis_Pelanggan REFERENCES dbo.Pelanggan(PelangganId),
    Perangkat NVARCHAR(100) NOT NULL,
    Keluhan NVARCHAR(500) NOT NULL,
    EstimasiBiaya DECIMAL(18,2) NOT NULL CONSTRAINT DF_Servis_Biaya DEFAULT 0,
    StatusServis NVARCHAR(20) NOT NULL CONSTRAINT DF_Servis_Status DEFAULT 'Antrean',
    TanggalMasuk DATETIME NOT NULL CONSTRAINT DF_Servis_Tgl DEFAULT GETDATE(),
    CONSTRAINT CK_Servis_Status CHECK (StatusServis IN ('Antrean','Proses','Siap Diambil','Selesai','Batal'))
);
GO

INSERT INTO dbo.Pelanggan (Nama, NoTelepon, Alamat) VALUES
(N'Budi Santoso','081234567890',N'Jl. Darmo No. 12, Surabaya'),
(N'Siti Aminah','085712345678',N'Jl. Rungkut Asri 5, Surabaya'),
(N'Rudi Hartono','082198765432',N'Jl. Gubeng Kertajaya 8, Surabaya');

INSERT INTO dbo.TransaksiServis (NoNota, PelangganId, Perangkat, Keluhan, EstimasiBiaya, StatusServis, TanggalMasuk) VALUES
('FIX-20261015-001',1,N'Laptop ASUS VivoBook',N'Layar bergaris',850000,'Selesai',DATEADD(DAY,-4,GETDATE())),
('FIX-20261016-001',2,N'iPhone 11',N'Baterai cepat habis',450000,'Proses',DATEADD(DAY,-3,GETDATE())),
('FIX-20261017-001',3,N'Samsung TV 43"',N'Tidak ada gambar',600000,'Siap Diambil',DATEADD(DAY,-2,GETDATE())),
('FIX-20261018-001',1,N'Printer Canon G2010',N'Tinta tidak keluar',150000,'Antrean',DATEADD(DAY,-1,GETDATE())),
('FIX-20261019-001',2,N'Kulkas Sharp 2 Pintu',N'Tidak dingin',700000,'Antrean',GETDATE());
GO
