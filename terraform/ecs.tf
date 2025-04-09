resource "aws_ecs_cluster" "main" {
  name = "${var.app_name}-cluster"
}

resource "aws_ecs_task_definition" "api" {
  family                   = "${var.app_name}-task"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"
  network_mode             = "awsvpc"
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn
  
  container_definitions = jsonencode([
    {
      name  = var.app_name
      image = "${aws_ecr_repository.api.repository_url}:latest",
      portMappings = [
        {
          containerPort = 80,
          hostPort     = 80
        }
      ],
      environment = [
        {
          name  = "ConnectionStrings__DefaultConnection",
          value = "Host=${aws_db_instance.postgres.address};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${var.db_password}"
        },
        {
          name  = "ASPNETCORE_ENVIRONMENT",
          value = "Development"
        }
      ],
      essential = true
    }
  ])
}

resource "aws_ecs_service" "api" {
  name            = "${var.app_name}-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.api.arn
  launch_type     = "FARGATE"
  desired_count   = 1
  
  network_configuration {
    subnets          = [aws_subnet.public_1.id, aws_subnet.public_2.id]
    security_groups  = [aws_security_group.ecs_sg.id]
    assign_public_ip = true
  }
}