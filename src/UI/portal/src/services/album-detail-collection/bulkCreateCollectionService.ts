import { BulkCreateCollectionRequest } from "../../models/album-detail-collection/BulkCreateCollectionRequest";
import { portalServer } from "../baseUrls";
import axiosApiInstance from "../interceptor";

export const bulkCreateCollectionItemApi = async (albumId: string, model: BulkCreateCollectionRequest[]) => {
    const response = await axiosApiInstance.post(portalServer + `/api/album/${albumId}/collections/bulk-create`, model);
    return response;
}