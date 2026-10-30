SET NOCOUNT ON;
IF DB_ID(N'HotelDB') IS NULL EXEC(N'CREATE DATABASE [HotelDB]');
GO
USE [HotelDB];
GO
IF OBJECT_ID(N'dbo.Phong',N'U') IS NULL
CREATE TABLE dbo.Phong(SoPhong varchar(10) PRIMARY KEY,LoaiPhong nvarchar(50) NOT NULL,GiaTheoGio decimal(18,2) NOT NULL CHECK(GiaTheoGio>=0),TrangThai nvarchar(30) NOT NULL DEFAULT N'Trống' CHECK(TrangThai IN(N'Trống',N'Đang ở',N'Đang dọn dẹp')));
IF OBJECT_ID(N'dbo.KhachHang',N'U') IS NULL
CREATE TABLE dbo.KhachHang(MaKH int IDENTITY PRIMARY KEY,HoTen nvarchar(100) NOT NULL CHECK(LEN(LTRIM(RTRIM(HoTen)))>0),CCCD varchar(20) NOT NULL UNIQUE,SoDienThoai varchar(15));
IF OBJECT_ID(N'dbo.DichVu',N'U') IS NULL
CREATE TABLE dbo.DichVu(MaDV int IDENTITY PRIMARY KEY,TenDV nvarchar(100) NOT NULL,DonGia decimal(18,2) NOT NULL CHECK(DonGia>=0));
IF OBJECT_ID(N'dbo.PhieuDatPhong',N'U') IS NULL
CREATE TABLE dbo.PhieuDatPhong(MaPhieu int IDENTITY PRIMARY KEY,SoPhong varchar(10) NOT NULL REFERENCES dbo.Phong(SoPhong),MaKH int NOT NULL REFERENCES dbo.KhachHang(MaKH),NgayNhanPhong datetime NOT NULL DEFAULT GETDATE(),NgayTraPhong datetime NULL,TongTienThanhToan decimal(18,2) NOT NULL DEFAULT 0 CHECK(TongTienThanhToan>=0));
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'UX_PhieuDatPhong_DangO' AND object_id=OBJECT_ID(N'dbo.PhieuDatPhong'))
CREATE UNIQUE INDEX UX_PhieuDatPhong_DangO ON dbo.PhieuDatPhong(SoPhong) WHERE NgayTraPhong IS NULL;
IF OBJECT_ID(N'dbo.SuDungDichVu',N'U') IS NULL
CREATE TABLE dbo.SuDungDichVu(MaPhieu int NOT NULL REFERENCES dbo.PhieuDatPhong(MaPhieu),MaDV int NOT NULL REFERENCES dbo.DichVu(MaDV),SoLuong int NOT NULL CHECK(SoLuong>0),ThoiGian datetime NOT NULL DEFAULT GETDATE());
IF NOT EXISTS(SELECT 1 FROM dbo.Phong) INSERT dbo.Phong(SoPhong,LoaiPhong,GiaTheoGio) VALUES('P101',N'Đơn',80000),('P102',N'Đơn',80000),('P201',N'Đôi',120000),('P202',N'Đôi',120000),('P301',N'VIP',200000);
IF NOT EXISTS(SELECT 1 FROM dbo.DichVu) INSERT dbo.DichVu(TenDV,DonGia) VALUES(N'Nước uống',15000),(N'Giặt là',50000),(N'Đồ ăn',60000);
GO
