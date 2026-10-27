using DBLib.Contracts;
using LinqToDB;
using LinqToDB.Data;
using MySqlConnector;
using Npgsql;

namespace DBLibConnector;

public sealed class DatabaseService : IDatabaseService
{
  private readonly DataOptions _options;
  public DbProviderKind Provider { get; }

  public DatabaseService(DBLibPluginConfig cfg)
  {
    var maxPool = Math.Max(1, cfg.MaxPoolSize);
    var timeout = Math.Max(1, cfg.CommandTimeoutSeconds);

    switch (cfg.DatabaseType.Trim().ToLowerInvariant())
    {
      case "postgres" or "postgresql" or "pgsql":
        {
          Provider = DbProviderKind.Postgres;
          var csb = new NpgsqlConnectionStringBuilder
          {
            Host = cfg.Host,
            Port = cfg.Port,
            Username = cfg.User,
            Password = cfg.Password,
            Database = cfg.Db,
            MaxPoolSize = maxPool,
            CommandTimeout = timeout,
          };
          if (ParseNpgsqlSsl(cfg.SSLMode) is { } ssl)
            csb.SslMode = ssl;

          _options = new DataOptions().UsePostgreSQL(csb.ConnectionString);
          break;
        }

      case "mysql" or "mariadb":
        {
          Provider = DbProviderKind.MySql;
          var csb = new MySqlConnectionStringBuilder
          {
            Server = cfg.Host,
            Port = (uint)cfg.Port,
            UserID = cfg.User,
            Password = cfg.Password,
            Database = cfg.Db,
            CharacterSet = cfg.Charset,
            MaximumPoolSize = (uint)maxPool,
            DefaultCommandTimeout = (uint)timeout,
          };
          if (ParseMySqlSsl(cfg.SSLMode) is { } ssl)
            csb.SslMode = ssl;

          _options = new DataOptions().UseMySql(csb.ConnectionString);
          break;
        }

      default:
        throw new InvalidOperationException(
          $"Unknown DatabaseType '{cfg.DatabaseType}'. Use 'postgres', 'mysql' or 'mariadb'.");
    }
  }

  public DataConnection GetDatabaseConnection() => new(_options);

  public async Task<T> UseAsync<T>(Func<DataConnection, Task<T>> work, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    await using var db = new DataConnection(_options);
    return await work(db).ConfigureAwait(false);
  }

  public async Task UseAsync(Func<DataConnection, Task> work, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    await using var db = new DataConnection(_options);
    await work(db).ConfigureAwait(false);
  }

  // --- SSL mode mapping (the two drivers use different enums) ---

  private static SslMode? ParseNpgsqlSsl(string value) =>
    Normalize(value) switch
    {
      "" => null,
      "disable" or "none" => SslMode.Disable,
      "allow" => SslMode.Allow,
      "prefer" or "preferred" => SslMode.Prefer,
      "require" or "required" => SslMode.Require,
      "verifyca" => SslMode.VerifyCA,
      "verifyfull" => SslMode.VerifyFull,
      var other => throw new InvalidOperationException($"Unknown DatabaseSSLMode '{other}'.")
    };

  private static MySqlSslMode? ParseMySqlSsl(string value) =>
    Normalize(value) switch
    {
      "" => null,
      "disable" or "none" => MySqlSslMode.None,
      "prefer" or "preferred" => MySqlSslMode.Preferred,
      "require" or "required" => MySqlSslMode.Required,
      "verifyca" => MySqlSslMode.VerifyCA,
      "verifyfull" => MySqlSslMode.VerifyFull,
      var other => throw new InvalidOperationException($"Unknown DatabaseSSLMode '{other}'.")
    };

  private static string Normalize(string s) =>
    s.Trim().ToLowerInvariant().Replace("-", "").Replace("_", "");
}