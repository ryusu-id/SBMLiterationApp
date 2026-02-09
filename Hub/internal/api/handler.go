package api

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/mux"
	"github.com/ryusu777/sbmliterationapp/hub/internal/config"
	"github.com/ryusu777/sbmliterationapp/hub/internal/hub"
)

// Handler contains all HTTP handlers
type Handler struct {
	hub    *hub.Hub
	config *config.Config
}

// NewHandler creates a new Handler instance
func NewHandler(h *hub.Hub, cfg *config.Config) *Handler {
	return &Handler{
		hub:    h,
		config: cfg,
	}
}

// SetupRoutes sets up all HTTP routes
func (h *Handler) SetupRoutes() *mux.Router {
	router := mux.NewRouter()

	// WebSocket endpoint
	wsHandler := NewWebSocketHandler(h.hub)
	router.HandleFunc("/ws", wsHandler.ServeHTTP)

	// API endpoints (with API key authentication)
	api := router.PathPrefix("/api").Subrouter()
	api.Use(h.apiKeyMiddleware)
	api.HandleFunc("/send", h.sendMessage).Methods("POST")
	api.HandleFunc("/broadcast", h.broadcast).Methods("POST")

	// Health check endpoint (no authentication required)
	router.HandleFunc("/health", h.health).Methods("GET")

	return router
}

// apiKeyMiddleware validates the API key
func (h *Handler) apiKeyMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		apiKey := r.Header.Get("X-API-Key")
		if apiKey != h.config.ApiKey {
			http.Error(w, "Unauthorized", http.StatusUnauthorized)
			return
		}
		next.ServeHTTP(w, r)
	})
}

// sendMessage handles sending a message to a specific client
func (h *Handler) sendMessage(w http.ResponseWriter, r *http.Request) {
	var request hub.SendMessageRequest
	if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
		log.Printf("Failed to decode request: %v", err)
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	// Validate that at least one target is specified
	if request.TargetUserId == "" && request.TargetSessionId == "" {
		http.Error(w, "Either targetUserId or targetSessionId must be specified", http.StatusBadRequest)
		return
	}

	// Send the message
	h.hub.SendMessage(request)

	// Return success response
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{
		"status":  "success",
		"message": "Message sent successfully",
	})
}

// broadcast handles broadcasting a message to all clients
func (h *Handler) broadcast(w http.ResponseWriter, r *http.Request) {
	var message hub.Message
	if err := json.NewDecoder(r.Body).Decode(&message); err != nil {
		log.Printf("Failed to decode message: %v", err)
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	// Broadcast the message
	h.hub.Broadcast(message)

	// Return success response
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{
		"status":  "success",
		"message": "Broadcast sent successfully",
	})
}

// health handles health check requests
func (h *Handler) health(w http.ResponseWriter, r *http.Request) {
	stats := h.hub.GetStats()

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]interface{}{
		"status": "healthy",
		"stats":  stats,
	})
}
