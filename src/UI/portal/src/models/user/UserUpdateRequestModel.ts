export default interface UserUpdateRequestModel {
    fullName: string;
    password: string;
    roles?: string[];
    isBanned?: boolean;
}