// notes from advanced analytics video
// how to add custom events, metrics and counters?

traces | project timestamp, message | take 10

traces | take 10

traces 
| search "Digitail Tag {@AALASInfo} scanned at AALAS" 
| where severityLevel > 0
| where customDimensions.Application == "Ai.Connect.UI"
| sort by timestamp desc 

// messages grouped by device
traces 
| search "Digitail Tag {@AALASInfo} scanned at AALAS" 
| where customDimensions.Application == "Ai.Connect.UI"
| summarize device_count = count() by tostring(customDimensions['AiConnectDeviceName'])

// efficiently fetch most recent 
traces
| where customDimensions.SourceContext =~ "Somark.datahub.devices.datahubdevicemonitor" // case insensitive
| where client_Type == "PC" // case sensitive
| where severityLevel == 0 or severityLevel == 1
| top 10 by timestamp 


traces
| take 10 

// search across all columns in one table
search in (traces) "connect"
| take 10 

// search across all columns in all tables
search "connect"
|take 10 

// column selection
traces
| top 10 by timestamp 
| project message, severityLevel, itemType, utc = timestamp , local = timestamp+630m

// summarise by group
traces
| where strlen(customDimensions.AiConnectDeviceName) > 0
| summarize count(severityLevel) by tostring(customDimensions['AiConnectDeviceName'])

// summarise by scalar by using bin
traces
| where isnotempty(customDimensions.AiConnectDeviceName)
| summarize count(severityLevel) by bin(timestamp, 1d), tostring(customDimensions['AiConnectDeviceName'])

// how many records across all tables
union * | count 

// how many records in traces tables
traces | count 

// search all columns
traces | where * has "error" | take 10 

// project, extend and split
traces 
| where cloud_RoleInstance startswith 'AiConnect'
| project cloud_RoleInstance
| extend machineId = split(cloud_RoleInstance, "-")[1]
| take 10

// error distribution
traces
| where timestamp > ago(1d) 
| extend ok = severityLevel <= 1
| summarize count() by ok

// ok / error by time
traces
| where timestamp > ago(7d) 
| summarize ok = count(severityLevel <= 2), error = count(severityLevel > 2) by bin(timestamp, 1h) 
| render timechart 

// % ok by time
traces
| where timestamp > ago(7d) 
| summarize ok = count(severityLevel <= 2), error = count(severityLevel > 2), totalCount = count() by bin(timestamp, 1h) 
| extend pctOk = ok * 100.00 / totalCount 
| project pctOk, timestamp 
| render timechart 

// percentiles
pageViews
| where timestamp > ago(7d) 
| summarize percentiles(duration, 50, 95) , count() by bin(timestamp, 1h)
| render timechart 

// most recent message per device
traces
| where timestamp > ago(3d) 
| where notempty(cloud_RoleInstance)
| where severityLevel >= 1
| summarize argmax(timestamp, message, severityLevel) by cloud_RoleInstance
| sort by cloud_RoleInstance asc

// count message
traces
| where timestamp > ago(1d) 
| where customDimensions['Application'] == "Ai.Connect.UI"
| summarize count() by message
| order by count_ desc 
| take 20

// count messages with reduce by to group by message type
traces
| where timestamp > ago(1d) 
| where customDimensions['Application'] == "Ai.Connect.UI"
| summarize count() by message
| reduce by message 
| order by Count desc 
| take 20

// autocluster heuristically finds a small number of clusters with similar rows
traces
| where timestamp > ago(1d) 
| project message 
| evaluate autocluster_v2()

// basket to find frequent patterns
traces
| where timestamp > ago(1d) 
| where severityLevel > 2
| evaluate basket()

// patterns of messages between Australia and RoTW
traces
| where timestamp > ago(2d) 
| extend isAustralia = iff(client_CountryOrRegion == "Australia", "T", "F")
| project message , isAustralia
| evaluate diffpatterns(isAustralia, "T", "F")

