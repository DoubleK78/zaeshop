import { Dispatch } from "@reduxjs/toolkit";
import { fetchUsersFailure, fetchUsersStart, fetchUsersSuccess, userAdded, userDeleted, userUpdated } from "../reducers/userSlice";
import api from "../../services/interceptor";
import { identityServer } from "../../services/baseUrls";
import UserCreateRequestModel from "../../models/user/UserCreateRequestModel";
import UserUpdateRequestModel from "../../models/user/UserUpdateRequestModel";
import UserPagingRequest from "../../models/user/UserPagingRequest.";
import { AxiosRequestConfig } from "axios";

// GET list user
export const getUsers = (model: UserPagingRequest) => async (dispatch: Dispatch) => {
    try {
        dispatch(fetchUsersStart());
        const config: AxiosRequestConfig<UserPagingRequest> = {
            params: model
        };
        const response = await api.get(identityServer + `/api/user`, config);
        if (response.status === 200) {
            dispatch(fetchUsersSuccess(response.data));
        }
    } catch (err: any) {
        return dispatch(fetchUsersFailure(err.response));
    }
};

// Post create user
export const createUser = (userCreateRequestModel: UserCreateRequestModel) => async (dispatch: Dispatch) => {
    try {
        dispatch(fetchUsersStart());
        const response = await api.post(identityServer + `/api/user`, userCreateRequestModel);
        if (response.status === 200) {
            dispatch(userAdded(response.data));
        }
    } catch (err: any) {
        return dispatch(fetchUsersFailure(err.response));
    }
};

// PUT update user
export const updateUser = (id: string, userUpdateRequestModel: UserUpdateRequestModel) => async (dispatch: Dispatch) => {
    try {
        dispatch(fetchUsersStart());
        const response = await api.put(identityServer + `/api/user/${id}`, userUpdateRequestModel);
        if (response.status === 200) {
            dispatch(userUpdated({ id, updatedUser: response.data }));
        }
    } catch (err: any) {
        return dispatch(fetchUsersFailure(err.response));
    }
};

// DELETE delete user
export const deleteUser = (id: string) => async (dispatch: Dispatch) => {
    try {
        dispatch(fetchUsersStart());
        const response = await api.delete(identityServer + `/api/user/${id}`);
        if (response.status === 200) {
            dispatch(userDeleted(id));
        }
    } catch (err: any) {
        return dispatch(fetchUsersFailure(err.response));
    }
};