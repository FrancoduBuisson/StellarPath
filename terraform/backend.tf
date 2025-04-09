# State bucket
 terraform {
   backend "s3" {
     bucket  = "stellar-path-s3-bucket"
     key     = "terraform.tfstate"
     region  = "af-south-1"
     encrypt = true
   }
 }
