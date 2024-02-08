export default interface ContentItemBulkUploadModel {
    items?: ContentItemBulkUploadItemModel[];
    existsItems?: ContentItemBulkUploadItemModel[];
}

export interface ContentItemBulkUploadItemModel {
    id?: number;
    fileName?: string;
    base64File?: string;
    isPublic: boolean;
    orderBy: number;
}

export interface ContentItemUploadLocalServer {
    id?: number;
    fileName?: string;
    isPublic: boolean;
}