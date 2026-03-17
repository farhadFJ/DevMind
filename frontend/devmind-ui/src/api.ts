import type {
  Conversation,
  Message,
  CreateConversationDto,
  CreateMessageDto,
} from "./types";

const API_BASE_URL = "http://localhost:5171/api";

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Request failed");
  }

  return response.json();
}

export async function getConversations(): Promise<Conversation[]> {
  const response = await fetch(`${API_BASE_URL}/conversations`);
  return handleResponse<Conversation[]>(response);
}

export async function createConversation(
  dto: CreateConversationDto
): Promise<Conversation> {
  const response = await fetch(`${API_BASE_URL}/conversations`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(dto),
  });

  return handleResponse<Conversation>(response);
}

export async function getMessages(conversationId: number): Promise<Message[]> {
  const response = await fetch(
    `${API_BASE_URL}/conversations/${conversationId}/messages`
  );

  return handleResponse<Message[]>(response);
}

export async function createMessage(
  conversationId: number,
  dto: CreateMessageDto
): Promise<Message[]> {
  const response = await fetch(
    `${API_BASE_URL}/conversations/${conversationId}/messages`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(dto),
    }
  );

  return handleResponse<Message[]>(response);
}