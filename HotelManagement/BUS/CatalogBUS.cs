using System.Data;
using HotelManagement.DTO;
using HotelManagement.DAL;
namespace HotelManagement.BUS;
public sealed class CatalogBUS
{
    public CatalogDTO Catalog { get; }
    private readonly CatalogDAL dal;
    public CatalogBUS(string table) { Catalog=Definitions().Single(x=>x.Table==table); dal=new CatalogDAL(Catalog); }
    public static CatalogDTO[] Definitions() => new[] {new CatalogDTO("DichVu","Dịch vụ","MaDV",true,new[]{new FieldDTO("MaDV","Mã","int"),new FieldDTO("TenDV","Tên dịch vụ","text",100),new FieldDTO("DonGia","Đơn giá","money")})};
    public DataTable List(string term="") => dal.List(term.Trim());
    public void Save(Dictionary<string,object?> values,object? key)
    {
        foreach(var field in Catalog.Fields)
        {
            if(Catalog.Identity && field.Column==Catalog.Key) continue;
            var text=Convert.ToString(values.GetValueOrDefault(field.Column))?.Trim()??"";
            if(field.Required && text.Length==0) throw new ArgumentException($"Vui lòng nhập {field.Caption}.");
            if(field.Kind is "money" or "quantity") { if(!decimal.TryParse(text,out var amount)||amount<0||amount>9999999999999999.99m||decimal.Round(amount,2)!=amount) throw new ArgumentException($"{field.Caption} phải >= 0, tối đa 2 số thập phân."); values[field.Column]=amount; }
            else if(field.Kind=="int") { if(!int.TryParse(text,out var id)||id<=0) throw new ArgumentException($"{field.Caption} phải là mã nguyên dương."); values[field.Column]=id; }
            else { if(text.Length>field.Length) throw new ArgumentException($"{field.Caption} tối đa {field.Length} ký tự."); values[field.Column]=text.Length==0?null:text; }
        }
        ValidateSpecial(values,key);
        dal.Save(values,key);
    }
    private void ValidateSpecial(Dictionary<string,object?> values,object? key) {  }
    public void Delete(object key) {  dal.Delete(key); }
}
