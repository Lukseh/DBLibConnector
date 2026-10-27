using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using DBLib.Contracts;
using LinqToDB.Internal.Common;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using CommandInfo = CounterStrikeSharp.API.Modules.Commands.CommandInfo;

namespace DBLibConnector;

public class DBLibPlugin : BasePlugin, IPluginConfig<DBLibPluginConfig>
{
  public override string ModuleName => "DBLib Connector";
  public override string ModuleVersion => "0.0.1-alpha";
  public override string ModuleAuthor => "Lukseh74";
  public override string ModuleDescription =>
    "Connector plugin for DBLib -> database library with multiplugin support";

  public DBLibPluginConfig Config { get; set; } = new();
  private DatabaseService? _service;

  public void OnConfigParsed(DBLibPluginConfig config)
  {
    Config = config;
  }

  public override void Load(bool hotReload)
  {
    try
    {
      _service = new DatabaseService(Config);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "[DBLib] Invalid database configuration. Capability NOT registered.");
      return;
    }

    Capabilities.RegisterPluginCapability(DatabaseCapability.Service, () => _service);
    Logger.LogInformation("[DBLib] Database service ready ({Provider}).", _service.Provider);
  }

  public override void Unload(bool hotReload)
  {
    _service = null;
    // Don't leak pooled connections across plugin reloads.
    NpgsqlConnection.ClearAllPools();
    MySqlConnection.ClearAllPools();
  }

  [ConsoleCommand("db_refresh", "Refresh Config")]
  [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
  [RequiresPermissions("@css/root")]
  public void OnDatabaseRefreshCommand(CCSPlayerController? player, CommandInfo commandInfo)
  {
    Config.Reload();
  }

  [ConsoleCommand("db_info", "Information about database connection")]
  [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
  [RequiresPermissions("@css/root")]
  public void OnDatabaseInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
  {
    if (_service.IsNullValue)
    {
      commandInfo.ReplyToCommand("Database not connected");
      return;
    }
    commandInfo.ReplyToCommand(_service.GetDatabaseConnection().ConnectionString ?? "CANT USE SERVICE");
    commandInfo.ReplyToCommand("TODO");
    // TODO: Add proper ping / connection status check
  }
}