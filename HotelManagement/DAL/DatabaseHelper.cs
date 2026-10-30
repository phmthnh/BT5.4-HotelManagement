using System.Data;
using Microsoft.Data.SqlClient;
namespace HotelManagement.DAL;
public static class DatabaseHelper
{
    // Sửa chuỗi kết nối tại đây hoặc đặt biến môi trường BT54_CONNECTION_STRING.
    public static string ConnectionString => Environment.GetEnvironmentVariable("BT54_CONNECTION_STRING") is { Length: > 0 } value
        ? value : @"Server=(localdb)\LTNET_BT5_SQL;Database=HotelDB;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Connect Timeout=15";
    public static SqlConnection Open()
    {
        var connection = new SqlConnection(ConnectionString);
        try { connection.Open(); return connection; } catch { connection.Dispose(); throw; }
    }
    public static SqlParameter P(string name, SqlDbType type, object? value, int size = 0)
    {
        var parameter = size > 0 ? new SqlParameter(name, type, size) : new SqlParameter(name, type);
        if (type == SqlDbType.Decimal) { parameter.Precision = 18; parameter.Scale = 2; }
        parameter.Value = value ?? DBNull.Value; return parameter;
    }
    public static DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using var connection = Open(); using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters); using var adapter = new SqlDataAdapter(command);
        var data = new DataTable(); adapter.Fill(data); return data;
    }
    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using var connection = Open(); using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters); return command.ExecuteNonQuery();
    }
    public static decimal Scalar(string sql, params SqlParameter[] parameters)
    {
        using var connection = Open(); using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters); var result = command.ExecuteScalar();
        return result is null or DBNull ? 0 : Convert.ToDecimal(result);
    }
}
