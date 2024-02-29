export default interface User {
    id: string;
    fullName?: string;
    userName?: string;
    email?: string;
    emailConfirmed: boolean;
    verificationToken?: string;
    verifiedOnUtc?: Date;
    resetPasswordToken?: string;
    resetPasswordTokenExpiresOnUtc?: Date;
    resetPasswordOnUtc?: Date;
    createdOnUtc: Date;
    updatedOnUtc?: Date;
    roles?: string;
    isBanned?: boolean;
}