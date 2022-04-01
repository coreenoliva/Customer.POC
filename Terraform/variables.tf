variable "resource_group_location" {
  default = "Australia East"
  description   = "Location of the resource group."
}

variable "resource_group_name" {
  default = "customer-poc-rg"
  description   = "Location of the resource group."
}

variable "cosmos_db_account_name" {
  default = "sandbox-cosmos-db-co"
}

variable "failover_location" {
  default = "australiacentral"
}

variable "cosmos_db_container_name"{
    default = "customer"
}

variable "cosmos_db_database_name"{
    default ="playground"
}

variable "storage_account_name"{
    default = "playgroundco"
}

variable "asp_name"{
    default = "playground_asp_co"
}

variable "customer_poc_function_app_name"{
    default = "customer-poc-func-01"
}