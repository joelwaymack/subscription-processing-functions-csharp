@description('A unique resource prefix.')
param prefix string

@description('Resource location. Defaults to resource group location.')
param location string = resourceGroup().location

// Storage
resource storage 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: replace('${prefix}st', '-', '')
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

// Database
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' = {
  name: '${prefix}-cosmos'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: true
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-10-15' = {
  parent: cosmosDbAccount
  name: 'Sales'
  properties: {
    resource: {
      id: 'Sales'
    }
    options: {
      throughput: 400
    }
  }
}

resource subscriptionContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-10-15' = {
  parent: cosmosDb
  name: 'Subscriptions'
  properties: {
    resource: {
      id: 'Subscriptions'
      partitionKey: {
        paths: [
          '/customerId'
        ]
      }
    }
  }
}

resource paymentContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-10-15' = {
  parent: cosmosDb
  name: 'Payments'
  properties: {
    resource: {
      id: 'Payments'
      partitionKey: {
        paths: [
          '/customerId'
        ]
      }
    }
  }
}

// Service Bus
resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: '${prefix}-svb'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource paymentQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'payment-queue'
  parent: serviceBus
  properties: {
    maxDeliveryCount: 5
  }
}

var serviceBusEndpoint = '${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey'

#disable-next-line outputs-should-not-contain-secrets
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'

#disable-next-line outputs-should-not-contain-secrets
output cosmosDbConnectionString string = cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString

#disable-next-line outputs-should-not-contain-secrets
output serviceBusConnectionString string = listKeys(serviceBusEndpoint, serviceBus.apiVersion).primaryConnectionString
