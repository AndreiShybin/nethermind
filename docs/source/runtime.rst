Runtime
*******

Directories
^^^^^^^^^^^

If you built the application locally then the entry point will be located in::

    src/Nethermind/Nethermind.Runner/bin/Release/netcoreapp2.2/

Log files by default are located in::

    src/Nethermind/Nethermind.Runner/bin/Release/netcoreapp2.2/logs

Database by default is located in::

    src/Nethermind/Nethermind.Runner/bin/Release/netcoreapp2.2/nethermind_db

Logs
^^^^

Logging level can be controlled in NLog.config file located in::

    src/Nethermind/Nethermind.Runner/bin/Debug/netcoreapp2.2/

You can change the level of logging in file and in the console by choosing one of the levels (Error, Warn, Info, Debug, Trace)::

    <logger name="*" minlevel="Info" writeTo="file-async"/>
    <logger name="*" minlevel="Info" writeTo="auto-colored-console-async"/>
