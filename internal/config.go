package config

import (
	"github.com/joho/godotenv"
)

func Load() {
	godotenv.Load(".env.local")
	godotenv.Load()
}
