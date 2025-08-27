# SQLAuditWatcher

SQLAudittoSIEM monitors SQL Server audit files and writes the translated message to a TXT file, to be read by Filebeat or other file agent.
It translates numeric codes like action_id and class_type to human-readable strings using an internal lookup dictionary.

## Building the Windows Services

The `SQLAuditWatcherFilestreamService` projects contain Windows Service implementations. Build the project using the .NET SDK:

```bash
dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained true -o .\<folder to files>
```

Install the service with `sc.exe` or `New-Service` and ensure the event source exists as shown above.

## Configuration

Set the input and output folders in `appsettings.json`. You can also specify the log file path where the service will write entries about detected changes, CSV exports, and errors:

```json
{
  "Watcher": {
    "InputPath": "C:\\SQL Audit Logs",
    "OutputPath": "C:\\SQL Audit Logs",
    "LogFile": "C:\\SQL Audit Logs\\SQLAuditWatcher.log",
    "PollIntervalSeconds": 5
  }
}
```

`InputPath` is the directory containing `.sqlaudit` files. `OutputPath` is where exported `.txt` files will be written. `LogFile` is the text file where the watcher records change notifications, CSV exports, and any processing failures. (use double "\\").

`PollIntervalSeconds` applies to the filestream service and controls how frequently it checks the audit files for new data. UNC paths like `\\\\SERVER\\Share` are supported for watching files on Windows network shares.

## Latest release setup

In the releases section you can download the project compiled and ready to run.

Also you will find the `install.bat` and `uninstall.bat` to install the service or eliminite it. Both files must be run from `powershell` as administrator.

Once the service is installed, you must check if completely running. If the user running the service doesn't have permissions over the directory containing the `.sql audit` files, can fail after start.
