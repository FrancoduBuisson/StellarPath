resource "aws_ssm_parameter" "google_client_id" {
  name        = "/${var.app_name}/google-client-id"
  description = "Google OAuth Client ID"
  type        = "SecureString"
  value       = var.google_client_id
}

resource "aws_secretsmanager_secret" "jwt_secret" {
  name        = "${var.app_name}/jwt-secret-key"
  description = "Secret key used for JWT token signing"
}

resource "aws_secretsmanager_secret_version" "jwt_secret_version" {
  secret_id     = aws_secretsmanager_secret.jwt_secret.id
  secret_string = var.jwt_secret_key
}

resource "aws_secretsmanager_secret" "db_connection" {
  name = "${var.app_name}-db-connection"
}

resource "aws_secretsmanager_secret_version" "db_connection_version" {
  secret_id     = aws_secretsmanager_secret.db_connection.id
  secret_string = "Host=${aws_db_instance.postgres.address};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${var.db_password}"
}
