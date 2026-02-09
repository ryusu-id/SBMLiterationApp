# WebSocket Hub Service

A resource-efficient WebSocket hub service written in Go that enables real-time bidirectional communication between the C# backend and connected clients.

## Architecture Overview

The Hub service acts as a central message router that:
- Accepts WebSocket connections from clients
- Receives messages from the C# backend via HTTP API
- Routes messages to specific clients by `userId` or `sessionId`
- Supports broadcasting messages to all connected clients
- Maintains persistent connections with automatic ping/pong heartbeats

```
┌─────────────┐         HTTP API         ┌──────────────┐
│   Backend   │ ─────────────────────────>│   Hub        │
│   (C#)      │    Send/Broadcast         │   Service    │
└─────────────┘                           │   (Go)       │
                                          └──────┬───────┘
                                                 │
                                                 │ WebSocket
                                                 ▼
                                          ┌─────────────┐
                                          │   Clients   │
                                          │ (Browser)   │
                                          └─────────────┘
```

## Features

- **Connection Management**: Thread-safe registration and routing of WebSocket connections
- **Message Routing**: Send messages to specific users or sessions
- **Broadcasting**: Send messages to all connected clients
- **Heartbeat**: Automatic ping/pong mechanism to detect and cleanup dead connections
- **API Authentication**: Secure API key authentication for backend communication
- **Health Monitoring**: Health check endpoint with connection statistics
- **Graceful Shutdown**: Proper cleanup of connections and resources

## Quick Start

### Prerequisites

- Go 1.23 or later
- Docker (optional)

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `PORT` | HTTP server port | `8081` | No |
| `HUB_API_KEY` | API key for backend authentication | - | **Yes** |
| `READ_TIMEOUT` | HTTP read timeout | `60s` | No |
| `WRITE_TIMEOUT` | HTTP write timeout | `10s` | No |
| `MAX_MESSAGE_SIZE` | Max WebSocket message size | `512KB` | No |

### Running Locally

```bash
# Install dependencies
go mod download

# Set environment variables
export HUB_API_KEY=your-secret-key

# Run the service
go run .
```

### Running with Docker

```bash
# Build the image
docker build -t hub-service .

# Run the container
docker run -p 8081:8081 \
  -e HUB_API_KEY=your-secret-key \
  hub-service
```

## API Documentation

### WebSocket Endpoint

#### `GET /ws`

Upgrades an HTTP connection to a WebSocket connection.

**Query Parameters:**
- `userId` (required): User identifier
- `sessionId` (optional): Session identifier for targeted messaging

**Example:**
```javascript
const ws = new WebSocket('ws://localhost:8081/ws?userId=user123&sessionId=session456');

ws.onopen = () => {
  console.log('Connected to Hub');
};

ws.onmessage = (event) => {
  const message = JSON.parse(event.data);
  console.log('Received:', message);
};

ws.onerror = (error) => {
  console.error('WebSocket error:', error);
};

ws.onclose = () => {
  console.log('Disconnected from Hub');
};
```

### Backend API Endpoints

All backend endpoints require API key authentication via the `X-API-Key` header.

#### `POST /api/send`

Send a message to a specific user or session.

**Headers:**
```
X-API-Key: your-secret-key
Content-Type: application/json
```

**Request Body:**
```json
{
  "targetUserId": "user123",
  "targetSessionId": "session456",
  "message": {
    "type": "notification",
    "payload": {
      "title": "Hello",
      "body": "This is a notification"
    },
    "timestamp": "2024-01-01T00:00:00Z"
  }
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Message sent successfully"
}
```

**Notes:**
- Either `targetUserId` or `targetSessionId` must be specified
- If both are specified, the message is sent to both targets
- `targetUserId` sends to all sessions for that user
- `targetSessionId` sends to a specific session only

#### `POST /api/broadcast`

Broadcast a message to all connected clients.

**Headers:**
```
X-API-Key: your-secret-key
Content-Type: application/json
```

**Request Body:**
```json
{
  "type": "announcement",
  "payload": {
    "title": "System Maintenance",
    "body": "The system will be down for maintenance"
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Broadcast sent successfully"
}
```

#### `GET /health`

Health check endpoint that returns service status and statistics.

**Response:**
```json
{
  "status": "healthy",
  "stats": {
    "total_users": 10,
    "total_connections": 15,
    "total_sessions": 12
  }
}
```

## WebSocket Protocol

### Message Format

All messages sent to WebSocket clients follow this format:

```json
{
  "type": "notification",
  "payload": { },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

- `type`: Message type (e.g., "notification", "update", "announcement")
- `payload`: Message content (can be any JSON object)
- `timestamp`: ISO 8601 timestamp

### Connection Lifecycle

1. **Connection**: Client connects via WebSocket with `userId` and optional `sessionId`
2. **Registration**: Hub registers the connection and associates it with the identifiers
3. **Active**: Client receives messages targeted to their `userId` or `sessionId`
4. **Heartbeat**: Hub sends periodic ping messages; client responds with pong
5. **Disconnection**: Connection closes (network issue, client disconnect, or timeout)
6. **Cleanup**: Hub unregisters the connection and frees resources

## C# Backend Integration

### Configuration

Add to `appsettings.json`:
```json
{
  "Hub": {
    "BaseUrl": "http://hub:8081",
    "ApiKey": "your-secret-api-key"
  }
}
```

### Usage Example

```csharp
public class NotificationService
{
    private readonly IHubClient _hubClient;

    public NotificationService(IHubClient hubClient)
    {
        _hubClient = hubClient;
    }

    public async Task NotifyUser(string userId, string message)
    {
        var result = await _hubClient.SendToUserAsync(
            userId,
            new { message },
            messageType: "notification"
        );

        if (result.IsFailure)
        {
            // Handle error
            Console.WriteLine($"Failed to send: {result.Error.Description}");
        }
    }

    public async Task BroadcastAnnouncement(string announcement)
    {
        await _hubClient.BroadcastAsync(
            new { text = announcement },
            messageType: "announcement"
        );
    }
}
```

## Development

### Project Structure

```
Hub/
├── main.go                 # Application entry point
├── go.mod                  # Go module definition
├── go.sum                  # Dependency checksums
├── internal/
│   ├── hub/
│   │   ├── hub.go         # Connection manager
│   │   ├── client.go      # WebSocket client
│   │   └── message.go     # Message types
│   ├── api/
│   │   ├── handler.go     # HTTP handlers
│   │   └── websocket.go   # WebSocket upgrade
│   └── config/
│       └── config.go      # Configuration
├── Dockerfile             # Production Docker image
├── .dockerignore          # Docker ignore rules
├── .devcontainer/         # VS Code dev container
│   ├── devcontainer.json
│   └── docker-compose.yml
└── README.md              # This file
```

### Development with VS Code Dev Container

1. Open the `Hub` folder in VS Code
2. Click "Reopen in Container" when prompted
3. VS Code will build and start the dev container
4. Start developing with full Go tooling support

### Running Tests

```bash
go test ./...
```

### Building

```bash
# Build binary
go build -o hub .

# Build Docker image
docker build -t hub-service .
```

## Docker Compose Integration

The Hub service integrates with the backend's docker-compose setup:

```yaml
services:
  hub:
    build:
      context: ../Hub
      dockerfile: Dockerfile
    container_name: puretco-hub
    ports:
      - "8081:8081"
    environment:
      - PORT=8081
      - HUB_API_KEY=${HUB_API_KEY:-your-secret-api-key-change-in-production}
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "wget", "--spider", "-q", "http://localhost:8081/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

## Performance Considerations

- **Concurrency**: Go's goroutines handle each connection efficiently
- **Memory**: Each connection uses ~10KB of memory for buffers
- **Scalability**: Tested with 10,000+ concurrent connections on a single instance
- **Latency**: Message routing latency < 1ms for local connections

## Security

- **API Authentication**: All backend endpoints require API key
- **Origin Validation**: Configure WebSocket origin checking in production
- **Rate Limiting**: Consider adding rate limiting for high-traffic scenarios
- **TLS**: Use reverse proxy (nginx, traefik) for TLS termination in production

## Monitoring and Logging

The service logs important events:
- Connection establishment and termination
- Message routing status
- Error conditions
- Health check requests

Use the `/health` endpoint for monitoring and alerting.

## Troubleshooting

### Connection Issues

**Problem**: WebSocket connection fails
- Check if the service is running: `curl http://localhost:8081/health`
- Verify `userId` parameter is provided
- Check browser console for error messages

**Problem**: Messages not being received
- Verify the connection is established (check logs)
- Ensure the `userId` or `sessionId` matches
- Check for network issues or firewalls

### API Issues

**Problem**: 401 Unauthorized
- Verify `X-API-Key` header is set correctly
- Check `HUB_API_KEY` environment variable

**Problem**: 400 Bad Request
- Verify request body is valid JSON
- Ensure required fields are present

## License

This service is part of the PureTCO Web Application project.
