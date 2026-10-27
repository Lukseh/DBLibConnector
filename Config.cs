using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace DBLibConnector;

public class DBLibPluginConfig : BasePluginConfig
{
  [JsonPropertyName("DatabaseType")] public string DatabaseType { get; set; } = "postgres"; // postgres | mysql | mariadb
  [JsonPropertyName("DatabaseHost")] public string Host { get; set; } = "localhost";
  [JsonPropertyName("DatabasePort")] public int Port { get; set; } = 5432;
  [JsonPropertyName("DatabaseUser")] public string User { get; set; } = "postgres";
  [JsonPropertyName("DatabasePassword")] public string Password { get; set; } = "";
  [JsonPropertyName("DatabaseName")] public string Db { get; set; } = "postgres";
  // disable | prefer | require | verify-ca | verify-full  (empty = driver default)
  [JsonPropertyName("DatabaseSSLMode")] public string SSLMode { get; set; } = "";
  // MySQL/MariaDB only. Postgres ignores it.
  [JsonPropertyName("DatabaseCharset")] public string Charset { get; set; } = "utf8mb4";
  [JsonPropertyName("MaxPoolSize")] public int MaxPoolSize { get; set; } = 20;
  [JsonPropertyName("CommandTimeoutSeconds")] public int CommandTimeoutSeconds { get; set; } = 30;
}