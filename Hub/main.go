package main

import (
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/ryusu777/sbmliterationapp/hub/internal/api"
	"github.com/ryusu777/sbmliterationapp/hub/internal/config"
	"github.com/ryusu777/sbmliterationapp/hub/internal/hub"
)

func main() {
	// Load configuration
	cfg := config.Load()
	log.Printf("Starting Hub service on port %s", cfg.Port)

	// Create and start the hub
	h := hub.NewHub()
	go h.Run()

	// Setup HTTP handlers
	handler := api.NewHandler(h, cfg)
	router := handler.SetupRoutes()

	// Create HTTP server
	server := &http.Server{
		Addr:         fmt.Sprintf(":%s", cfg.Port),
		Handler:      router,
		ReadTimeout:  cfg.ReadTimeout,
		WriteTimeout: cfg.WriteTimeout,
	}

	// Start server in a goroutine
	go func() {
		log.Printf("Hub service listening on :%s", cfg.Port)
		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatalf("Failed to start server: %v", err)
		}
	}()

	// Wait for interrupt signal to gracefully shut down the server
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	log.Println("Shutting down server...")

	// Graceful shutdown with timeout
	ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancel()

	// Shutdown the hub
	h.Shutdown()

	// Shutdown the HTTP server
	if err := server.Shutdown(ctx); err != nil {
		log.Fatalf("Server forced to shutdown: %v", err)
	}

	log.Println("Server exited")
}
