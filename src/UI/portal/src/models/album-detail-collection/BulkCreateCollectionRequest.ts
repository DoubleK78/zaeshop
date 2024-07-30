export interface BulkCreateCollectionRequest {
    name: string;
    isPriority: boolean;
    contentItems: BulkCreateCollectionItemRequest[];
}

export interface BulkCreateCollectionItemRequest {
    name: string;
    size: number;
}