# .Net Core example - Azure Service Bus 'Request-Response' pattern 

## Steps to get working two way service bus messaging
* Within Azure create two service bus queues. These queues must support sessions. (I have named them sessiontest and sessiontestresponse in the configs)
* Ensure these queue names must match the appsettings.json in both projects. 
* Update the servicebus connection string to your Azure service either directly in config or using user secrets.
* Run up both projects using your tools of choice

## Whats going on?
* the API project creates a sessionid (GUID)
* Creates a listener on the response queue for that session
* Sends a request to the request queue
* The Worker project picks up the request and then sends a cloned message to the response queue with the same sessionid
* The API then receives the reponse from the worker. 

## Caveats
* This is an example of non-production code
* You will need to create objects in Azure so you must have an account