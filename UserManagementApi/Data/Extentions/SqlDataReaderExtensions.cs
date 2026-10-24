using Microsoft.Data.SqlClient;

namespace UserManagementApi.Data.Extentions
{
    public static class SqlDataReaderExtensions
    {
        public static string? GetStringOrNull(this SqlDataReader reader, int index)
           => reader.IsDBNull(index) ? null : reader.GetString(index);

        public static bool? GetBooleanOrNull(this SqlDataReader reader, int index)
            => reader.IsDBNull(index) ? (bool?)null : reader.GetBoolean(index);

        public static int? GetInt32OrNull(this SqlDataReader reader, int index)
            => reader.IsDBNull(index) ? (int?)null : reader.GetInt32(index);

        public static DateTime? GetDateTimeOrNull(this SqlDataReader reader, int index)
            => reader.IsDBNull(index) ? (DateTime?)null : reader.GetDateTime(index);

        public static Guid? GetGuidOrNull(this SqlDataReader reader, int index)
            => reader.IsDBNull(index) ? (Guid?)null : reader.GetGuid(index);
    }
}
