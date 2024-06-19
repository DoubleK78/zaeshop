import PagingRequest from "../common/PagingRequest";

export interface CommentManagementRequestModel extends PagingRequest {
    isReply: boolean;
    isDeleted: boolean;

    startDate: Date;
    endDate: Date;
}