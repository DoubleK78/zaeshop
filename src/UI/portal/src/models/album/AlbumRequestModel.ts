export default interface AlbumRequestModel {
    title: string;
    description?: string;
    albumAlertMessageId?: number;
    contentTypeIds?: number[];
    fileNameThumbnail?: string;
    isUpdateThumbnail: boolean;
    fileNameBackground?: string;
    isUpdateOriginalUrl: boolean;
    region: string;
}