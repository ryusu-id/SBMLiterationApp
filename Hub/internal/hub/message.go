package hub

import (
	"encoding/json"
	"time"
)

// Message represents a message sent to WebSocket clients
type Message struct {
	Type      string          `json:"type"`
	Payload   json.RawMessage `json:"payload"`
	Timestamp time.Time       `json:"timestamp"`
}

// SendMessageRequest represents a request to send a message to a specific client
type SendMessageRequest struct {
	TargetUserId    string  `json:"targetUserId,omitempty"`
	TargetSessionId string  `json:"targetSessionId,omitempty"`
	Message         Message `json:"message"`
}
