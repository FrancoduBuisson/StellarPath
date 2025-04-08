
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  backend "s3" {
    region  = "af-south-1"
  }
}

provider "aws" {
  region =  "af-south-1"
}

resource "aws_default_vpc" "default_vpc" {
  tags = {
    Name = "default_vpc"
  }
}

data "aws_availability_zones" "available_zones" {
  
}

resource "aws_default_subnet" "subnet_az1" {
  availability_zone = data.aws_availability_zones.available_zones.names[0]
}

resource "aws_default_subnet" "subnet_az2" {
  availability_zone = data.aws_availability_zones.available_zones.names[1]
}

resource "aws_security_group" "allow_postgres" {
  name_prefix = "allow_postgress_"

  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_instance" "fupboarddb" {
  identifier             = "fupboarddb"
  engine                 = "postgres"
  engine_version         = "16.4"
  instance_class         = "db.t4g.micro"
  db_name                = "fupboarddb"
  allocated_storage      = 20
  storage_type           = "gp2"
  publicly_accessible    = true
  username               = var.db_username
  password               = var.db_password
  skip_final_snapshot    = true
  vpc_security_group_ids = [aws_security_group.allow_postgres.id]
  tags = {
    Name = "fupboarddb"
  }
}

output "db_host" {
  value = aws_db_instance.fupboarddb.endpoint
  description = "The endpoint of the SQL Server RDS instance"
}

resource "aws_security_group" "ec2_security_group" {
  name_prefix = "fupboard_api_sg"

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_instance" "fup_ec2_instance" {
  ami           = "ami-00d6d5db7a745ff3f"
  instance_type = "t3.micro"
  key_name = var.key_name
  tags = {
    Name = "fup_ec2_instance"
  }

  vpc_security_group_ids = [ aws_security_group.ec2_security_group.id ]

  user_data = <<-EOF
    #!/bin/bash
    # Install necessary packages
    dnf install -y java-23-amazon-corretto nginx

    # Setup Systemd Service
    file="/etc/systemd/system/fupboard.service"

    echo [Unit] > $file
    echo Description=fupboard >> $file
    echo [Service] >> $file
    echo ExecStart="java -jar /home/ec2-user/fupboard-api.jar" >> $file
    echo WorkingDirectory=/home/ec2-user >> $file

    systemctl enable fupboard.service

    # Setup nginx proxy
    mkdir -p /etc/nginx/conf.d
    file="/etc/nginx/conf.d/proxy.conf"

    echo "server {" > $file
    echo "  listen 80;" >> $file
    echo "  server_name *.amazonaws.com;" >> $file
    echo "  location / {" >> $file
    echo "    proxy_pass http://localhost:8080;" >> $file
    echo "    proxy_set_header Host \$host;" >> $file
    echo "    proxy_set_header X-Real-IP \$remote_addr;" >> $file
    echo "  }" >> $file
    echo "}" >> $file

    systemctl enable nginx
    systemctl start nginx

    EOF
}

resource "aws_eip" "fup_ec2_eip" {
  instance = aws_instance.fup_ec2_instance.id
  domain   = "vpc"
}

output "ec2_host" {
  value = aws_eip.fup_ec2_eip.public_dns
  description = "The endpoint of the EC2 instance"
}