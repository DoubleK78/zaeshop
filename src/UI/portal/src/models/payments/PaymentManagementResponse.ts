export interface PaymentManagementResponse {
    id: number;
    description?: string | null;
    userId: number;
    identityUserId?: string | null;
    email?: string | null;
    createdOnUtc: Date;
}