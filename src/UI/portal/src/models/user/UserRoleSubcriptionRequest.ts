export default interface UserRoleSubcriptionRequest {
    userId: string;
    role: string;
    days?: number | null;
}