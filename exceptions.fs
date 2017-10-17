exceptions
| where customDimensions.MachineName == "AICONNECT-120" 
| where innermostType <> "System.ServiceModel.CommunicationObjectAbortedException" 
| sort by timestamp desc 
| limit 100 
