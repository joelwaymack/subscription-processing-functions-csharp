// az deployment sub create --location eastus --name subscription-processing --template-file .\main-local.bicep --parameters unique=way
@description('A lowercase letter and number combination that is used to make resource names unique.')
@maxLength(13)
param unique string

@description('Region for deployment. Defaults to deployment location.')
param location string = deployment().location

var prefix = 'sub-proc-${unique}'

targetScope = 'subscription'

resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: '${prefix}-rg'
  location: location
}

module local './local.bicep' = {
  name: prefix
  scope: rg
  params: {
    prefix: prefix
    location: location
  }
}

// output storageConnectionString string = local.outputs.storageConnectionString
// output cosmosDbConnectionString string = local.outputs.cosmosDbConnectionString
// output serviceBusConnectionString string = local.outputs.serviceBusConnectionString
