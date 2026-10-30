using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagement.DTO;
namespace HotelManagement.DAL;
public sealed class CatalogDAL
{
    private readonly CatalogDTO catalog;
    internal CatalogDAL(CatalogDTO catalog) { this.catalog = catalog; }
    public DataTable List(string term)
    {
        var searchable = catalog.Fields.Where(f => f.Kind is "text" or "key").Select(f => $"[{f.Column}] LIKE @term");
        return DatabaseHelper.Query($"SELECT * FROM dbo.[{catalog.Table}] WHERE {string.Join(" OR ", searchable)} ORDER BY [{catalog.Key}]", DatabaseHelper.P("@term",SqlDbType.NVarChar,"%"+term+"%",255));
    }
    public void Save(Dictionary<string,object?> values, object? key)
    {
        var fields=catalog.Fields.Where(f=>!(catalog.Identity && f.Column==catalog.Key) && (key is null || f.Column!=catalog.Key)).ToArray();
        var parameters=fields.Select(f=>DatabaseHelper.P("@"+f.Column, f.Kind is "money" or "quantity" ? SqlDbType.Decimal : f.Kind=="int" ? SqlDbType.Int : f.Kind=="key" ? SqlDbType.VarChar : SqlDbType.NVarChar,values[f.Column],f.Kind is "money" or "quantity" or "int" ? 0:f.Length)).ToList();
        string sql;
        if(key is null) sql=$"INSERT dbo.[{catalog.Table}] ({string.Join(",",fields.Select(f=>$"[{f.Column}]"))}) VALUES ({string.Join(",",fields.Select(f=>"@"+f.Column))})";
        else { sql=$"UPDATE dbo.[{catalog.Table}] SET {string.Join(",",fields.Select(f=>$"[{f.Column}]=@{f.Column}"))} WHERE [{catalog.Key}]=@key"; parameters.Add(DatabaseHelper.P("@key",catalog.Identity?SqlDbType.Int:SqlDbType.VarChar,key, catalog.Identity?0:20)); }
        if(DatabaseHelper.Execute(sql,parameters.ToArray())!=1) throw new InvalidOperationException("Dữ liệu đã thay đổi. Vui lòng tải lại.");
    }
    public void Delete(object key)
    {
        if(DatabaseHelper.Execute($"DELETE dbo.[{catalog.Table}] WHERE [{catalog.Key}]=@key",DatabaseHelper.P("@key",catalog.Identity?SqlDbType.Int:SqlDbType.VarChar,key,catalog.Identity?0:20))!=1) throw new InvalidOperationException("Không tìm thấy bản ghi.");
    }
}
