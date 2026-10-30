namespace HotelManagement.DTO;
public sealed record ServiceDTO(int MaPhieu,int MaDV,int SoLuong);
public sealed record GuestDTO(string HoTen,string CCCD,string SoDienThoai);
public sealed record InvoiceDTO(int MaPhieu,string SoPhong,DateTime NgayNhanPhong,DateTime NgayTraPhong,decimal TienPhong,decimal TienDichVu){public decimal TongTien=>TienPhong+TienDichVu;}
