import { useEffect, useState } from "react";
import "./App.css";
import {
  createConversation,
  createMessage,
  getConversations,
  getMessages,
} from "./api";
import type { Conversation, Message } from "./types";

function App() {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [selectedConversationId, setSelectedConversationId] = useState<number | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [newConversationTitle, setNewConversationTitle] = useState("");
  const [newMessage, setNewMessage] = useState("");
  const [loadingConversations, setLoadingConversations] = useState(false);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [sendingMessage, setSendingMessage] = useState(false);
  const [error, setError] = useState("");

  async function loadConversations() {
    try {
      setLoadingConversations(true);
      setError("");
      const data = await getConversations();
      setConversations(data);

      if (data.length > 0 && selectedConversationId === null) {
        setSelectedConversationId(data[0].id);
      }
    } catch (err) {
      console.error("loadConversations error:", err);
      setError("Error loading conversations");
    } finally {
      setLoadingConversations(false);
    }
  }

  async function loadMessages(conversationId: number) {
    try {
      setLoadingMessages(true);
      setError("");
      const data = await getMessages(conversationId);
      setMessages(data);
    } catch (err) {
      console.error("loadMessages error:", err);
      setError("Error loading messages");
      setMessages([]);
    } finally {
      setLoadingMessages(false);
    }
  }

  async function handleCreateConversation() {
    try {
      setError("");

      const created = await createConversation({
        title: newConversationTitle.trim() || "New Chat",
      });

      setNewConversationTitle("");
      await loadConversations();
      setSelectedConversationId(created.id);
      await loadMessages(created.id);
    } catch (err) {
      console.error("handleCreateConversation error:", err);
      setError("Error creating conversation");
    }
  }

  async function handleSelectConversation(conversationId: number) {
    setSelectedConversationId(conversationId);
    await loadMessages(conversationId);
  }

  async function handleSendMessage() {
    if (!selectedConversationId) {
      setError("Please select a conversation first");
      return;
    }

    if (!newMessage.trim()) {
      return;
    }

    try {
      setSendingMessage(true);
      setError("");

      const updatedMessages = await createMessage(selectedConversationId, {
        role: "user",
        content: newMessage.trim(),
      });

      setMessages(updatedMessages);
      setNewMessage("");
    } catch (err) {
      console.error("handleSendMessage error:", err);
      setError("Error sending message");
    } finally {
      setSendingMessage(false);
    }
  }

  useEffect(() => {
    loadConversations();
  }, []);

  useEffect(() => {
    if (selectedConversationId !== null) {
      loadMessages(selectedConversationId);
    }
  }, [selectedConversationId]);

  return (
    <div className="app">
      <aside className="sidebar">
        <h2>DevMind Chats</h2>

        <div className="new-chat-box">
          <input
            type="text"
            placeholder="New chat title"
            value={newConversationTitle}
            onChange={(e) => setNewConversationTitle(e.target.value)}
          />
          <button onClick={handleCreateConversation}>Create Chat</button>
        </div>

        {loadingConversations ? (
          <p>Loading conversations...</p>
        ) : (
          <ul className="conversation-list">
            {conversations.map((conversation) => (
              <li
                key={conversation.id}
                className={
                  selectedConversationId === conversation.id ? "active" : ""
                }
                onClick={() => handleSelectConversation(conversation.id)}
              >
                <strong>{conversation.title}</strong>
                <small>{new Date(conversation.createdAt).toLocaleString()}</small>
              </li>
            ))}
          </ul>
        )}
      </aside>

      <main className="chat-area">
        <h2>Messages</h2>

        {error && <p className="error">{error}</p>}

        {!selectedConversationId ? (
          <p>Select a chat to view messages.</p>
        ) : loadingMessages ? (
          <p>Loading messages...</p>
        ) : (
          <div className="messages">
            {messages.length === 0 ? (
              <p>No messages yet.</p>
            ) : (
              messages.map((message) => (
                <div key={message.id} className={`message ${message.role}`}>
                  <div className="message-role">{message.role}</div>
                  <div>{message.content}</div>
                </div>
              ))
            )}
          </div>
        )}

        <div className="message-input-box">
          <input
            type="text"
            placeholder="Type your message..."
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && !sendingMessage) {
                handleSendMessage();
              }
            }}
          />
          <button onClick={handleSendMessage} disabled={sendingMessage}>
            {sendingMessage ? "Sending..." : "Send"}
          </button>
        </div>
      </main>
    </div>
  );
}

export default App;