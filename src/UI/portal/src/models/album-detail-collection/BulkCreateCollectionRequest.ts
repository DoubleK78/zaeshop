export interface BulkCreateCollectionRequest {
    name: string;
    contentItems: BulkCreateCollectionItemRequest[];
}

export interface BulkCreateCollectionItemRequest {
    name: string;
    size: number;
}