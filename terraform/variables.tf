variable "app_name" {
  type        = string
  description = "Name of the application"
  default     = "stellar-path-api"
}

variable "db_name" {
  type        = string
  description = "The name for the database"
  default     = "stellarpath_db"
}

variable "db_username" {
  type        = string
  description = "The username for the database"
}

variable "db_password" {
  type        = string
  description = "The password for the database"
  sensitive   = true
}

variable "db_port" {
  type        = number
  description = "The port for the database"
}

variable "subnet_cidrs" {
  type        = string
  description = "Base CIDR block for VPC"
}

variable "region" {
  type        = string
  description = "The region where the resources will be deployed."
}

variable "enable_dns_hostnames" {
  type        = bool
  description = "Enable DNS hostnames in VPC"
}

variable "google_client_id" {
  description = "Google OAuth Client ID"
  type        = string
  sensitive   = true
}

variable "jwt_secret_key" {
  description = "Secret key used for JWT token signing"
  type        = string
  sensitive   = true
}