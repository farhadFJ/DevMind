export type Conversation = {
    id: number;
    title: string;
    createdAt: string;
};

export type Message = {
    id: number;
    chatConversationId: number;
    role: string;
    content: string;
    createdAt: string;
};

export type CreateConversationDto = {
    title?:string;
};

export type CreateMessageDto = {
    role: string;
    content: string;
};
