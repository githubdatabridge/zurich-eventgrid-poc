# PoC Demo - EventGrid
PoC for exploring solution with EventGrid and System Topic Event produced buy Azure Blob Storage.

![plot](/assets/Screenshot%202023-01-24%20161509.png)

## Resource requirements:
- Create Service Principal [AppLocal] for auth by setting env on machine:

![plot](/assets/Screenshot%202023-01-24%20150248.png)

- Azure Storage Account 
    - Blob Containers 
        - mdm-b-input
        - mdm-b-output
    - Queue Storage:
        - cla-queue
        - mdm-b-queue
- Add Roles to Azure Storage Account:
    - for AppLocal Service Principal add
        - Storage Blob Data Owner
        - Storage Queue Data Contributor
- EventGrid Create System topics example: `Container1`
- `Container1` System topics create subscriptions:
    - MDM-B-on-MDM-B-INPUT for Create Blob type and Endpoint type Storage Queues named `mdm-b-queue` and filter `/blobServices/default/containers/mdm-b-input`
    - CLA-B-on-MDM-B-OUTPUT for Create Blob type and Endpoint type Storage Queues named `cla-queue` and filter `/blobServices/default/containers/mdm-b-output`

## Settings requirements:
Change `AccoutStorageUri` in `appsettings.json` as
```
https://{Azure Storage Account Name}.__service__.core.windows.net/
```

And now both of services can be run.