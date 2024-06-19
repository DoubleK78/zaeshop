export default interface CommentManagementResponse {
    id: number;
    text?: string | null;
    createdOnUtc: Date;
    isReply: boolean;
    userId: number;
    email?: string | null;
    albumFriendlyName?: string | null;
}