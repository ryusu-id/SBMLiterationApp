package api

import (
	"log"
	"net/http"

	"github.com/gorilla/websocket"
	"github.com/ryusu777/sbmliterationapp/hub/internal/hub"
)

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
	CheckOrigin: func(r *http.Request) bool {
		// In production, you should validate the origin
		return true
	},
}

// WebSocketHandler handles WebSocket upgrade requests
type WebSocketHandler struct {
	hub *hub.Hub
}

// NewWebSocketHandler creates a new WebSocket handler
func NewWebSocketHandler(h *hub.Hub) *WebSocketHandler {
	return &WebSocketHandler{hub: h}
}

// ServeHTTP handles WebSocket connections
func (wsh *WebSocketHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	// Get userId from query parameters (required)
	userId := r.URL.Query().Get("userId")
	if userId == "" {
		http.Error(w, "userId parameter is required", http.StatusBadRequest)
		return
	}

	// Get sessionId from query parameters (optional)
	sessionId := r.URL.Query().Get("sessionId")

	// Upgrade the HTTP connection to a WebSocket connection
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("Failed to upgrade connection: %v", err)
		return
	}

	// Create and register the client
	client := hub.NewClient(wsh.hub, conn, userId, sessionId)
	wsh.hub.Register(client)

	// Start the client's read and write pumps
	client.Start()

	log.Printf("WebSocket connection established: userId=%s, sessionId=%s", userId, sessionId)
}
