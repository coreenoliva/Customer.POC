// Resource Group
resource "azurerm_resource_group" "rg"{
    name = var.resource_group_name
    location  = var.resource_group_location
}

// Storage Account
resource "azurerm_storage_account" "storage_account" {
  name                     = var.storage_account_name
  resource_group_name      = var.resource_group_name
  location                 = var.resource_group_location
  account_tier             = "Standard"
  account_replication_type = "GRS"
}

resource "azurerm_application_insights" "application_insights" {
  name                = var.application_insights_name
  location            = var.resource_group_location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}

// Cosmos DB
resource "azurerm_cosmosdb_account" "cosmos_acc" {
  name = var.cosmos_db_account_name
  location = var.resource_group_location
  resource_group_name = var.resource_group_name
  offer_type = "Standard"
  kind = "GlobalDocumentDB"
  enable_automatic_failover = true
consistency_policy {
    consistency_level = "Session"
  }
  
  geo_location {
    location = "${var.failover_location}"
    failover_priority = 1
  }
geo_location {
    location = "${var.resource_group_location}"
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name = var.cosmos_db_database_name
  resource_group_name = var.resource_group_name
  account_name =  var.cosmos_db_account_name
  depends_on = [
   azurerm_cosmosdb_account.cosmos_acc  
  ]
}

resource "azurerm_cosmosdb_sql_container" "container" {
  name                  = var.cosmos_db_container_name
  resource_group_name   = var.resource_group_name
  account_name          = var.cosmos_db_account_name
  database_name         = var.cosmos_db_database_name
  partition_key_path    = "/definition/id"
  partition_key_version = 1
  throughput            = 400
  depends_on = [
    azurerm_cosmosdb_sql_database.db
  ]
}

// App

resource "azurerm_service_plan" "playground" {
  name                = var.asp_name
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  sku_name = "F1"
  os_type = "Windows"
}

resource "azurerm_windows_function_app" "customer_poc" {
  name                = var.customer_poc_function_app_name
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name

  storage_account_name = var.storage_account_name
  service_plan_id      = azurerm_service_plan.playground.id
  storage_uses_managed_identity = true

  site_config {}
  app_settings = {
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.storage_account.primary_connection_string,
    "FUNCTIONS_WORKER_RUNTIME" = "dotnet",
    "AzureWebJobsSecretStorageType" = "files",
    "CustomerCreationAPIBaseUrl" = "https://0c38b091-4a67-45d2-a394-4867eda89137.mock.pstmn.io",
    "CosmosDbConnectionString" = "AccountEndpoint=https://sandbox-cosmos-db-co.documents.azure.com:443/;AccountKey=NDwPmPfTJlRFghIn9kEJXM4tGTY82wHrsJOMawRiX6lNdQGmzrRat524gnyvqx7vh60DPLtmuVDmEbLtEL66zA==;",
    "SendGridKey" = "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",
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.application_insights.instrumentation_key,
    "AzureWebJobsStorage" =  azurerm_storage_account.storage_account.primary_connection_string
  }
}