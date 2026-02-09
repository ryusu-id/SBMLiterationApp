package config

import (
	"log"
	"os"
	"strconv"
	"time"
)

// Config holds the application configuration
type Config struct {
	Port           string
	ApiKey         string
	ReadTimeout    time.Duration
	WriteTimeout   time.Duration
	MaxMessageSize int64
}

// Load loads configuration from environment variables
func Load() *Config {
	config := &Config{
		Port:           getEnv("PORT", "8081"),
		ApiKey:         getEnv("HUB_API_KEY", ""),
		ReadTimeout:    getDurationEnv("READ_TIMEOUT", 60*time.Second),
		WriteTimeout:   getDurationEnv("WRITE_TIMEOUT", 10*time.Second),
		MaxMessageSize: getInt64Env("MAX_MESSAGE_SIZE", 512*1024), // 512KB
	}

	if config.ApiKey == "" {
		log.Fatal("HUB_API_KEY environment variable is required")
	}

	return config
}

// getEnv gets an environment variable or returns a default value
func getEnv(key, defaultValue string) string {
	if value := os.Getenv(key); value != "" {
		return value
	}
	return defaultValue
}

// getDurationEnv gets a duration environment variable or returns a default value
func getDurationEnv(key string, defaultValue time.Duration) time.Duration {
	if value := os.Getenv(key); value != "" {
		if duration, err := time.ParseDuration(value); err == nil {
			return duration
		}
		log.Printf("Invalid duration for %s, using default: %v", key, defaultValue)
	}
	return defaultValue
}

// getInt64Env gets an int64 environment variable or returns a default value
func getInt64Env(key string, defaultValue int64) int64 {
	if value := os.Getenv(key); value != "" {
		if intValue, err := strconv.ParseInt(value, 10, 64); err == nil {
			return intValue
		}
		log.Printf("Invalid int64 for %s, using default: %d", key, defaultValue)
	}
	return defaultValue
}
