export default interface CollectionRequestModel {
    title: string;
    albumId: number;
    volume?: number;
    extendName?: string;
    description?: string;
    isPriority: boolean;
}