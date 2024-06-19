import { portalServer } from "../baseUrls";
import axiosApiInstance from "../interceptor";

export const deleteComment = async (commentId: number) => {
    const response = await axiosApiInstance.delete(portalServer + `/api/comment/${commentId}`);
    return response;
}