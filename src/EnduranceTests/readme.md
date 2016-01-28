### Connections.config

Connections.config has been excluded from Git, add a file with the following to the project directory and set the connection string in that to run.

```
  <connectionStrings configSource="Connections.config">    
    <add name="NServiceBus/Transport"
      connectionString="<Add Your ASB queue connection string here>"/>
  </connectionStrings>
```