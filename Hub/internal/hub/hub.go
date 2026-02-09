package hub

import (
	"log"
	"sync"
)

// Hub maintains the set of active clients and broadcasts messages to the clients
type Hub struct {
	// Registered clients by userId
	clientsByUserId map[string]map[*Client]bool

	// Registered clients by sessionId
	clientsBySessionId map[string]*Client

	// Register requests from the clients
	register chan *Client

	// Unregister requests from clients
	unregister chan *Client

	// Send message to specific client(s)
	sendMessage chan SendMessageRequest

	// Broadcast message to all clients
	broadcast chan Message

	// Mutex for thread-safe operations
	mu sync.RWMutex

	// Shutdown signal
	shutdown chan struct{}
}

// NewHub creates a new Hub instance
func NewHub() *Hub {
	return &Hub{
		clientsByUserId:    make(map[string]map[*Client]bool),
		clientsBySessionId: make(map[string]*Client),
		register:           make(chan *Client),
		unregister:         make(chan *Client),
		sendMessage:        make(chan SendMessageRequest),
		broadcast:          make(chan Message),
		shutdown:           make(chan struct{}),
	}
}

// Run starts the hub's main event loop
func (h *Hub) Run() {
	for {
		select {
		case client := <-h.register:
			h.registerClient(client)

		case client := <-h.unregister:
			h.unregisterClient(client)

		case request := <-h.sendMessage:
			h.sendToClient(request)

		case message := <-h.broadcast:
			h.broadcastMessage(message)

		case <-h.shutdown:
			log.Println("Hub shutting down")
			return
		}
	}
}

// registerClient registers a new client
func (h *Hub) registerClient(client *Client) {
	h.mu.Lock()
	defer h.mu.Unlock()

	// Register by userId
	if _, ok := h.clientsByUserId[client.userId]; !ok {
		h.clientsByUserId[client.userId] = make(map[*Client]bool)
	}
	h.clientsByUserId[client.userId][client] = true

	// Register by sessionId if provided
	if client.sessionId != "" {
		h.clientsBySessionId[client.sessionId] = client
	}

	log.Printf("Client registered: userId=%s, sessionId=%s, total_users=%d",
		client.userId, client.sessionId, len(h.clientsByUserId))
}

// unregisterClient unregisters a client
func (h *Hub) unregisterClient(client *Client) {
	h.mu.Lock()
	defer h.mu.Unlock()

	// Unregister from userId map
	if clients, ok := h.clientsByUserId[client.userId]; ok {
		if _, exists := clients[client]; exists {
			delete(clients, client)
			close(client.send)

			// Remove the userId entry if no more clients
			if len(clients) == 0 {
				delete(h.clientsByUserId, client.userId)
			}
		}
	}

	// Unregister from sessionId map
	if client.sessionId != "" {
		if _, ok := h.clientsBySessionId[client.sessionId]; ok {
			delete(h.clientsBySessionId, client.sessionId)
		}
	}

	log.Printf("Client unregistered: userId=%s, sessionId=%s, total_users=%d",
		client.userId, client.sessionId, len(h.clientsByUserId))
}

// sendToClient sends a message to a specific client
func (h *Hub) sendToClient(request SendMessageRequest) {
	h.mu.RLock()
	defer h.mu.RUnlock()

	sent := false

	// Try to send to specific sessionId first
	if request.TargetSessionId != "" {
		if client, ok := h.clientsBySessionId[request.TargetSessionId]; ok {
			select {
			case client.send <- request.Message:
				sent = true
				log.Printf("Message sent to sessionId: %s", request.TargetSessionId)
			default:
				log.Printf("Failed to send message to sessionId: %s (buffer full)", request.TargetSessionId)
			}
		} else {
			log.Printf("SessionId not found: %s", request.TargetSessionId)
		}
	}

	// Send to all clients with the userId
	if request.TargetUserId != "" {
		if clients, ok := h.clientsByUserId[request.TargetUserId]; ok {
			for client := range clients {
				select {
				case client.send <- request.Message:
					sent = true
				default:
					log.Printf("Failed to send message to userId: %s (buffer full)", request.TargetUserId)
				}
			}
			log.Printf("Message sent to userId: %s (%d clients)", request.TargetUserId, len(clients))
		} else {
			log.Printf("UserId not found: %s", request.TargetUserId)
		}
	}

	if !sent {
		log.Printf("Message not sent: no valid target found")
	}
}

// broadcastMessage sends a message to all connected clients
func (h *Hub) broadcastMessage(message Message) {
	h.mu.RLock()
	defer h.mu.RUnlock()

	count := 0
	for _, clients := range h.clientsByUserId {
		for client := range clients {
			select {
			case client.send <- message:
				count++
			default:
				log.Printf("Failed to broadcast to client: userId=%s (buffer full)", client.userId)
			}
		}
	}

	log.Printf("Broadcast message sent to %d clients", count)
}

// SendMessage queues a message to be sent to a specific client
func (h *Hub) SendMessage(request SendMessageRequest) {
	h.sendMessage <- request
}

// Broadcast queues a message to be broadcast to all clients
func (h *Hub) Broadcast(message Message) {
	h.broadcast <- message
}

// Register queues a client to be registered
func (h *Hub) Register(client *Client) {
	h.register <- client
}

// GetStats returns connection statistics
func (h *Hub) GetStats() map[string]interface{} {
	h.mu.RLock()
	defer h.mu.RUnlock()

	totalConnections := 0
	for _, clients := range h.clientsByUserId {
		totalConnections += len(clients)
	}

	return map[string]interface{}{
		"total_users":       len(h.clientsByUserId),
		"total_connections": totalConnections,
		"total_sessions":    len(h.clientsBySessionId),
	}
}

// Shutdown gracefully shuts down the hub
func (h *Hub) Shutdown() {
	close(h.shutdown)

	// Close all client connections
	h.mu.Lock()
	defer h.mu.Unlock()

	for _, clients := range h.clientsByUserId {
		for client := range clients {
			close(client.send)
		}
	}
}
