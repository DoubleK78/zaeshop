export default interface CollectionPagingResponse {
    id: number;
    title: string;
    albumId: number;
    albumTitle?: string;
    volume?: number;
    extendName?: string;
    description?: string;
    createdOnUtc: Date;
    updatedOnUtc?: Date;
    views: number;
    levelPublic: number;
}