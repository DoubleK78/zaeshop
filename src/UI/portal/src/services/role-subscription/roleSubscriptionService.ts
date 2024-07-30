import UserRoleSubcriptionRequest from "../../models/user/UserRoleSubcriptionRequest";
import { identityServer } from "../baseUrls";
import axiosApiInstance from "../interceptor";

export const updateRoleSubscription = async (userid: string, model: UserRoleSubcriptionRequest) => {
    const response = await axiosApiInstance.put<string>(identityServer + `/api/user/${userid}/role-subscription`, model);
    return response;
}