import { EStorageType } from "../enums/EStorageType";

export default interface CollectionDetail {
    id: number;
    title: string;
    albumId: number;
    albumTitle?: string;
    albumFriendlyName: string;
    friendlyName: string;
    volume?: number;
    extendName?: string;
    description?: string;
    createdOnUtc: Date;
    updatedOnUtc?: Date;
    storageType: EStorageType;
}