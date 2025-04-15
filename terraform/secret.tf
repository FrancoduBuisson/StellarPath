resource "aws_ssm_parameter" "google_client_id" {
  name        = "/${var.app_name}/google-client-id-v3"
  description = "Google OAuth Client ID"
  type        = "SecureString"
  value       = var.google_client_id
}

resource "aws_secretsmanager_secret" "jwt_secret" {
  name        = "${var.app_name}/jwt-secret-key-v3"
  description = "Secret key used for JWT token signing"
}

resource "aws_secretsmanager_secret_version" "jwt_secret_version" {
  secret_id     = aws_secretsmanager_secret.jwt_secret.id
  secret_string = var.jwt_secret_key
}

resource "aws_secretsmanager_secret" "db_connection" {
  name = "${var.app_name}-db-connection-v3"
}

resource "aws_secretsmanager_secret_version" "db_connection_version" {
  secret_id     = aws_secretsmanager_secret.db_connection.id
  secret_string = "Host=${aws_db_instance.postgres.address};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${var.db_password}"
}

resource "aws_secretsmanager_secret" "api_key_1" {
  name        = "${var.app_name}/api-key-1-v3"
  description = "First API key for external service"
}

resource "aws_secretsmanager_secret_version" "api_key_1_version" {
  secret_id     = aws_secretsmanager_secret.api_key_1.id
  secret_string = var.api_key_1
}

resource "aws_secretsmanager_secret" "api_key_2" {
  name        = "${var.app_name}/api-key-2-v3"
  description = "Second API key for external service"
}

resource "aws_secretsmanager_secret_version" "api_key_2_version" {
  secret_id     = aws_secretsmanager_secret.api_key_2.id
  secret_string = var.api_key_2
}